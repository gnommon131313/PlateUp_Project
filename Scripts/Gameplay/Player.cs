using Cinemachine;
using DG.Tweening;
using System;
using UniRx;
using UnityEngine;
using Zenject;

[RequireComponent(typeof(CharacterController))]
public class Player : MonoBehaviour
{
    private CompositeDisposable _disposable = new CompositeDisposable();
    private CompositeDisposable _disposable1 = new CompositeDisposable();

    private Game _game;

    private CharacterController _characterController => GetComponent<CharacterController>();
    Vector3 _moveVelocity = Vector3.zero;
    private float _moveSpeed = 10.0f;
    [SerializeField] private float _moveSpeedMax = 10.0f;
    private float _rotateSpeed;
    [SerializeField] private float _rotateSpeedMax = 180.0f;
    private ReactiveProperty<Vector3> _moveDirection = new ReactiveProperty<Vector3>(Vector3.zero);
    private Vector3 _moveDirectionMoreZero;
    private float _currentVelocity; // only needed for SmoothDampAngle
    [SerializeField] Vector3 _gravity = new Vector3(0, -1.0f, 0);
    Vector3 _gravityVelocity = Vector3.zero;
    Vector3 _gravityMin = new Vector3(0, -0.1f, 0);
    [SerializeField] float _yPositionForGameOver = -50;

    private InputHandler _inputHandler;
    private CinemachineVirtualCamera _virtualCamera;
   
    [SerializeField] private ReactiveProperty<Product> _product = new ReactiveProperty<Product>(null);
    [SerializeField] private Transform _productPlace;

    private ReactiveProperty<Station> _nearStation = new ReactiveProperty<Station>(null);
    private Station _nearStationCached;
    private ReactiveProperty<float> ResetNearStationBuffer = new ReactiveProperty<float>(0);
    [SerializeField] private LayerMask _layerMaskForSearchNearInteractiveItem;
    
    private ReactiveProperty<float> _yPosition = new ReactiveProperty<float>();

    private ReactiveProperty<float> _blinkTimer = new ReactiveProperty<float>(0);
    [SerializeField] private float _blinkTime = 3;
    [SerializeField] private GameObject[] _eyelids;

    private bool _isGrounded => _characterController.isGrounded;

    [Inject]
    private void Construct(
        InputHandler inputHandler,
        CinemachineVirtualCamera virtualCamera,
        Game game)
    {
        _inputHandler = inputHandler;
        _virtualCamera = virtualCamera;
        _game = game;
    }

    private void Start()
    {
        _moveSpeed = _moveSpeedMax;
        _rotateSpeed = _rotateSpeedMax;

        SetupCamera();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
            _yPosition.Value = UnityEngine.Random.Range(-15f, 15f);
    }

    private void FixedUpdate()
    {
        DeterminMoveVelocity();
        DeterminGravityVelocity();
        Move();
        Rotate();
    }

    private void OnEnable()
    {
        SubscribeOnUniRx();
    }

    private void OnDisable()
    {
        UnSubscribeOnUniRx();
    }

    public void Refresh()
    {
        if (_product.Value)
            Destroy(_product.Value.gameObject);

        // отключение и включение _characterController потому что он не даст просто напрямую изменить transform.position
        _characterController.enabled = false;
        transform.position = Vector3.zero;
        _characterController.enabled = true;
    }

    private void SubscribeOnUniRx()
    {
        float step = 0.1f;
        Observable
            .Interval(TimeSpan.FromSeconds(step))
            .Subscribe(_ =>
            {
                SearchNearStation();
                CountdownTimer(step);

                _yPosition.Value = transform.position.y;
            })
            .AddTo(_disposable);

        _inputHandler.Axis
            .Subscribe(value => DeterminMoveDirection(value))
            .AddTo(_disposable);

        _inputHandler.Action1
            .Subscribe(_ => Action1WithNearStation())
            .AddTo(_disposable);

        _inputHandler.Action2
            .Subscribe(_ => Action2WithNearStation())
            .AddTo(_disposable);

        _inputHandler.Action3
            .Subscribe(value => Action3WithNearStation(value))
            .AddTo(_disposable);

        _product
            .Subscribe(value => SetupProduct(value))
            .AddTo(_disposable);

        _nearStation
            .Subscribe(value =>
            {
                SetupNearStationCached();

                _nearStationCached = value;

                if (value)
                    value.WasSelectedByPlayer();
            })
            .AddTo(_disposable);

        ResetNearStationBuffer
            .Subscribe(value =>
            {
                if (value <= 0 && _nearStation.Value)
                    _nearStation.Value = null;
            })
            .AddTo(_disposable);

        _moveDirection
            .Subscribe(value =>
            {
                if (value != Vector3.zero) 
                    _moveDirectionMoreZero = value;
            })
        .AddTo(_disposable);

        _yPosition
           .Subscribe(value => TryGameOver(value))
           .AddTo(_disposable);

        _blinkTimer
            .Subscribe(value =>
            {
                if (value > 0)
                    return;

                _blinkTimer.Value = _blinkTime;

                Blink();
            })
            .AddTo(_disposable);
    }

    private void UnSubscribeOnUniRx() => _disposable.Clear();

    private void Action1WithNearStation()
    {
        if (!_nearStation.Value)
            return;

        if (_product.Value)
        {
            Product newProduct = _nearStation.Value.PutProduct(
                Instantiate(_product.Value, 
                _productPlace.transform.position, 
                _productPlace.transform.rotation));
           
             Destroy(_product.Value.gameObject);

            _product.Value = newProduct;
        }
        else
        {
            _product.Value = _nearStation.Value.PickUpProduct();
        }
    }

    private void Action2WithNearStation()
    {
        if (!_nearStation.Value || _product.Value)
            return;

        _product.Value = _nearStation.Value.ChunkingProduct();
    }

    private void Action3WithNearStation(bool value)
    {
        if (!_nearStation.Value)
            return;

        if (value)
            _nearStation.Value.Use();
        else
            _nearStation.Value.Unuse();
    }

    private void SetupNearStationCached()
    {
        if (!_nearStationCached)
            return;

        _nearStationCached.WasUnselectedByPlayer();
        _nearStationCached.Unuse();
    }

    private void DeterminMoveDirection(Vector3 direction)
    {
        Vector3 cameraForward = _virtualCamera.transform.forward;
        Vector3 cameraRight = _virtualCamera.transform.right;
        cameraForward.y = 0; cameraRight.y = 0;

        Vector3 directionForward = cameraForward.normalized * direction.z;
        Vector3 directionRight = cameraRight.normalized * direction.x;

        _moveDirection.Value = (directionForward + directionRight);
    }

    private void DeterminMoveVelocity()
    {
        _moveVelocity = _moveDirection.Value * _moveSpeed * Time.deltaTime;
    }

    private void DeterminGravityVelocity()
    {
        if (_isGrounded)
            _gravityVelocity = _gravityMin;
        else
            if (transform.position.y > _yPositionForGameOver)
                _gravityVelocity += _gravity * Time.deltaTime;
    }

    private void Move()
    {
        _characterController.Move(_moveVelocity + _gravityVelocity);
    }

    private void Rotate()
    {
        if (_moveDirection.Value == Vector3.zero)
            return;

        float targetAngle = Mathf.Atan2(_moveDirection.Value.x, _moveDirection.Value.z) * Mathf.Rad2Deg; // Mathf.Atan2(...) * Mathf.Rad2Deg возвращает преобразованую координату на оси полученную из вектора2 (который равен только -1 до 1)
        float smoothAngle = Mathf.SmoothDampAngle(
            transform.eulerAngles.y,
            targetAngle,
            ref _currentVelocity,
            0.1f,
            _rotateSpeed);

        transform.rotation = Quaternion.Euler(0, smoothAngle, 0);
    }

    private void SetupProduct(Product product)
    {
        if (!product) 
            return;

        product.Setup(_productPlace);
    }

    private void SetupCamera()
    {
        _virtualCamera.Follow = transform;
        _virtualCamera.LookAt = transform;
    }

    private void SearchNearStation()
    {
        float rayLength = 2;
        Vector3 rayOrigin = transform.position;
        Vector3 rayDirection = _moveDirectionMoreZero * rayLength;
        Ray ray = new Ray(rayOrigin, rayDirection);

        Debug.DrawRay(rayOrigin, rayDirection, Color.red, 0.1f);

        if (Physics.Raycast(ray, out RaycastHit hit, rayLength, _layerMaskForSearchNearInteractiveItem))
        {
            //Debug.Log("Hit object: " + hit.collider.gameObject.transform.parent.transform.parent.name);
            //Debug.Log("Hit object: " + hit.collider.name);

            Station station = hit.collider.transform.parent.transform.parent.GetComponent<Station>();

            if (!station)
                return;

            _nearStation.Value = station;

            ResetNearStationBuffer.Value = 0.2f;
        }
    }

    private void CountdownTimer(float step)
    {
        if (ResetNearStationBuffer.Value > 0)
            ResetNearStationBuffer.Value -= step;

        if (_blinkTimer.Value > 0)
            _blinkTimer.Value -= step;
    }

    private void TryGameOver(float value)
    {
        //if (value < _yPositionForGameOver && _game)
        //    if (_game.GameModeCurrent.Value == GameMode.World)
        //        _game.GameRestart();
        //    else
        //        _game.GameOver();
    }

    private void Blink()
    {
        _eyelids[0].SetActive(true);

        Observable.Timer(TimeSpan.FromSeconds(0.1))
            .Subscribe(_ =>
            {
                _eyelids[1].SetActive(true);

                Observable.Timer(TimeSpan.FromSeconds(0.1))
                    .Subscribe(_ =>
                    {
                        _eyelids[0].SetActive(false);

                        Observable.Timer(TimeSpan.FromSeconds(0.1))
                            .Subscribe(_ =>
                            {
                                _eyelids[1].SetActive(false);

                                _disposable1.Clear();
                            })
                            .AddTo(_disposable1);
                    })
                    .AddTo(_disposable1);
            })
            .AddTo(_disposable1);
    }

    public class Factory : PlaceholderFactory<Player> { }
}
