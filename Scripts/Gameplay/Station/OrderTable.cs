using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using Zenject;
using System;
using DG.Tweening;

public class OrderTable : Station
{
    [Space(25), Header("Order")]
    [SerializeField] private ReactiveProperty<Product> _order = new ReactiveProperty<Product>();
    [SerializeField] private List<Product> _orders;
    [SerializeField] private ReactiveProperty<float> _orderTimer;
    [SerializeField] private float _orderTime;
    [SerializeField] private float _orderTimeDefault = 3;

    [SerializeField] private ReactiveProperty<float> _patienceTimer = new ReactiveProperty<float>();
    [SerializeField] private float _patienceTime;
    [SerializeField] private float _patienceTimeDefault = 10;

    [SerializeField] private ReactiveProperty<float> _eatTimer = new ReactiveProperty<float>();
    [SerializeField] private float _eatTime;
    [SerializeField] private float _eatTimeDefault = 3;

    [SerializeField] private Transform _orderPlace;

    [Space(25), Header("Other")]
    [SerializeField] private Product _plateDirty;

    [SerializeField] private List<GameObject> _customers;
    private GameObject _currentCustomer;
    private Vector3 _customerTargetPosition;

    [SerializeField] private GameObject _orderExpiredModel;
    [SerializeField] private GameObject _eatingModel;

    public ReactiveProperty<float> PatienceTimer => _patienceTimer;
    public float PatienceTime => _patienceTime;
    public ReactiveProperty<float> EatTimer => _eatTimer;

    private new void Awake()
    {
        base.Awake();

        SetupTime();

        _customerTargetPosition = transform.position + transform.forward * -2;
    }

    private new void Start()
    {
        base.Start();
    }

    private new void FixedUpdate()
    {
        base.FixedUpdate();

        DefineFieldsDependingOnComplexity();
    }

    public override Product PutProduct(Product product)
    {
        if (!_order.Value)
        {
            Debug.Log("order is not ready");

            ErrorNotification();

            return product;
        }

        if (product.Type != _order.Value.Type)
        {
            Debug.Log("order is not suitable");

            ErrorNotification();

            return product;
        }

        if (!product.InPlate.Value)
        {
            Debug.Log("plate missing");

            ErrorNotification();

            return product;
        }

        return base.PutProduct(product);
    }

    protected override void SubscribeOnUniRx()
    {
        base.SubscribeOnUniRx();

        float step = 0.1f;
        Observable
            .Interval(TimeSpan.FromSeconds(step))
            .Subscribe(_ =>
            {
                CountdownTimers(step);
            })
            .AddTo(_disposable);

        _product
            .Subscribe(value => TryGiveAwayTheOrder())
            .AddTo(_disposable);

        _order
            .Subscribe(value => {
                SetupOrder(value);

                if (value)
                    return;

                _orderTimer.Value = _orderTime;

                CustomerCreate();
            })
            .AddTo(_disposable);

        _orderTimer
            .Subscribe(value =>
            {
                if (value > 0)
                    return;

                _patienceTimer.Value = _patienceTime;

                _order.Value = Instantiate(
                    _orders[UnityEngine.Random.Range(0, _orders.Count)],
                    _productPlace.transform.position,
                    _productPlace.transform.rotation);
            })
            .AddTo(_disposable);

        _patienceTimer
            .Subscribe(value => 
            {
                if (value <= 0 && _order.Value && _eatTimer.Value <= 0)
                    OrderExpired();
            })
            .AddTo(_disposable);

        _eatTimer
            .Subscribe(value =>
            {
                _eatingModel.SetActive(value > 0);

                if (value <= 0 && _product.Value && _order.Value)
                    CustomerEatCompleted();
            })
            .AddTo(_disposable);
    }

    private void CountdownTimers(float step)
    {
        if (_orderTimer.Value > 0)
            _orderTimer.Value -= step;

        if (_patienceTimer.Value > 0)
            _patienceTimer.Value -= step;

        if (_eatTimer.Value > 0)
            _eatTimer.Value -= step;
    }

    private void OrderExpired()
    {
        Debug.Log($"{_order.Value} ПРОСРОЧЕН");

        _lock = true;

        _signalBus.TryFire(new OrderExpired());

        _orderExpiredModel.SetActive(true);

        //_game.GameOver();
    }

    private void DefineFieldsDependingOnComplexity()
    {
        float complexity = 1;
        //float complexity = _game.Complexity;

        _patienceTime = _patienceTimeDefault / complexity;
        _orderTime = _orderTimeDefault / complexity;
        //_eatTime = _eatTimeDefault / complexity;
    }

    private void CustomerCreate()
    {
        CustomerRemove();

        _currentCustomer = Instantiate(
            _customers[UnityEngine.Random.Range(0, _customers.Count)],
            _customerTargetPosition + transform.up * -5,
            transform.rotation,
            transform);

        CustomerMove();
    }

    private void CustomerRemove()
    {
        if (!_currentCustomer)
            return;

        GameObject customerCached = _currentCustomer;
        customerCached.transform
            .DOMove(_customerTargetPosition + transform.up * 50, 1.0f)
            .SetEase(Ease.Linear)
            .OnComplete(() => Destroy(customerCached))
            .SetLink(_currentCustomer);
    }

    private void CustomerMove()
    {
        _currentCustomer.transform
            .DOMove(_customerTargetPosition, _orderTime)
            //.SetEase(Ease.OutCubic)
            .SetEase(Ease.OutElastic)
            .SetLink(_currentCustomer);
    }

    private void SetupOrder(Product product)
    {
        if (!product)
            return;

        product.Setup(_orderPlace.transform);
        product.PlaceOnAPlate(_productPlace.transform);
    }

    private void TryGiveAwayTheOrder()
    {
        if (!_product.Value || !_order.Value)
            return;

        Debug.Log($"{_product.Value} ОТДАН");

        _lock = true;
        _eatTimer.Value = _eatTime;
        _patienceTimer.Value = 0;
    }

    private void CustomerEatCompleted()
    {
        _lock = false;

        Destroy(_order.Value.gameObject);
        _order.Value = null; // если просто уничтожить обьект то значение станет = Missing, это не тоже самое что null

        DestroyProductGameObject();
        _product.Value = Instantiate(_plateDirty, 
            _orderPlace.transform.position,
            _orderPlace.transform.rotation);
    }

    private void SetupTime()
    {
        _patienceTime = _patienceTimeDefault;
        _orderTime = _orderTimeDefault;
        _eatTime = _eatTimeDefault;
    }

    private void ErrorNotification()
    {
        _errorModel.SetActive(true);

        CompositeDisposable disposable = new CompositeDisposable();

        Observable.Timer(TimeSpan.FromSeconds(0.25))
            .Subscribe(_ =>
            {
                _errorModel.SetActive(false);

                Observable.Timer(TimeSpan.FromSeconds(0.25))
                    .Subscribe(_ =>
                    {
                        _errorModel.SetActive(true);

                        Observable.Timer(TimeSpan.FromSeconds(0.25))
                            .Subscribe(_ =>
                            {
                                _errorModel.SetActive(false);

                                disposable.Clear();
                            })
                            .AddTo(disposable).AddTo(this);
                    })
                    .AddTo(disposable).AddTo(this);
            })
            .AddTo(disposable).AddTo(this);
    }
}
