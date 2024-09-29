using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Zenject;
using UniRx;
using TMPro;
using DG.Tweening;
using Unity.VisualScripting;
using static UnityEngine.Rendering.DebugUI;

public class Product : MonoBehaviour
{
    private CompositeDisposable _disposable = new CompositeDisposable();

    private bool _isPrefab = true;
    [SerializeField] private string _type;
    [SerializeField] private string _class;
    [SerializeField] private ReactiveProperty<Product> _inPlate = new ReactiveProperty<Product>(null);
    [SerializeField] private Product _plate;
    [SerializeField] private Transform _model;

    [Header("Conversion")]
    [SerializeField] private ReactiveProperty<Conversion> _currentConversionImpact = new ReactiveProperty<Conversion>(Conversion.None);
    [SerializeField] private Product _availableConversionToInstance;
    [SerializeField] private ReactiveProperty<float> _progressConversion = new ReactiveProperty<float>(0);
    [SerializeField] private float _progressConversionMax = 0;
    [SerializeField] private float _conversionMultiplier = 1;
    [SerializeField] private List<Product> _conversionToInstance;
    [SerializeField] private List<Conversion> _conversionToInstanceFromImpact;

    [Space(25), Header("Mix")]
    [SerializeField] private List<Product> _mixWithInstance;
    [SerializeField] private List<Product> _mixResultInstance;

    [Space(25), Header("Chunking")] 
    [SerializeField] private ReactiveProperty<int> _chunk = new ReactiveProperty<int>(0);
    [SerializeField] private Product _turnIntoAfterChunking;
    [SerializeField] private Product _chunkingResult;

    public string Type => _type;
    public string Class => _class;
    public ReactiveProperty<Product> InPlate => _inPlate;
    public ReactiveProperty<Conversion> CurrentConversionImpact => _currentConversionImpact;
    public ReactiveProperty<float> ProgressConversion => _progressConversion;
    public float ProgressConversionMax => _progressConversionMax;
    public bool ProcessConversionIsReachedMax => _progressConversion.Value >= _progressConversionMax && _progressConversionMax > 0;
    public bool AvailableConversionToInstanceIsGarbage => 
        _availableConversionToInstance && _availableConversionToInstance.Class == "Garbage";
    public List<Product> MixWithInstance => _mixWithInstance;
    public List<Product> MixResultInstance => _mixResultInstance;
    public ReactiveProperty<int> Chunk => _chunk;
    public Product TurnIntoAfterChunking => _turnIntoAfterChunking;
    public Product ChunkingResult => _chunkingResult;
    public bool IsPrefab => _isPrefab;
    public List<Conversion> ConversionFromImpact => _conversionToInstanceFromImpact;

    private void Awake()
    {
        // Awake ÒÓ·‡Ú˚‚‡ÂÚ Ò‡ÁÛ ÔÓÒÎÂ ÒÓÁ‰‡ÌËˇ Ó·¸ÂÍÚ‡, ‰‡ÊÂ ‰Ó ReactiveProperty.Subscribe(value => ...(value)) ‚ ÒÍËÔÚÂ ÍÓÚÓ˚È ÒÓÁ‰‡Î this Ó·¸ÂÍÚ, ‡ ‚ÓÚ Start ÒÓ·‡Ú˚‚‡ÚÂ ÔÓÒÎÂ Â‡ÍˆËË Ì‡ ËÁÏÂÌÂÌËˇ Â‡ÍÚË‚ÌÓ„Ó Ò‚ÓÈÒÚ‚‡
        //Debug.Log("Awake");

        _isPrefab = false;
        name = $"{_type} (Clone)";
    }

    private void OnEnable()
    {
        Refresh();
        SubscribeOnUniRx();
    }

    private void OnDisable()
    {
        UnSubscribeOnUniRx();
    }

    public void Setup(Transform parent)
    {
        SetupTransform(parent);
        DOResizeModel();
        DOPlatePosition();
    }

    public Product Converting()
    {
        Product newInstance = Instantiate(
            _availableConversionToInstance,
            transform.position,
            transform.rotation,
            transform.parent);

        Destroy(gameObject);

        return newInstance;
    }

    public void PlaceOnAPlate(Transform startTransform)
    {
        if (_inPlate.Value)
            return;

        _inPlate.Value = Instantiate(
            _plate,
            startTransform.position,
            startTransform.rotation,
            transform);
    }

    public void TakeOutFromAPlate()
    {
        Destroy(_inPlate.Value.gameObject);
        _inPlate.Value = null;
    }

    public void SetCurrentConversionImpact(Conversion value) => _currentConversionImpact.Value = value;

    public void SetConversionMultiplier(float value) => _conversionMultiplier = value;

    public void SetChunk(int value) => _chunk.Value = value;

    private void SubscribeOnUniRx()
    {
        float step = 0.1f;
        Observable
            .Interval(TimeSpan.FromSeconds(step))
            .Subscribe(_ =>
            {
                ProcessConversion(step);
            })
            .AddTo(_disposable);

        _currentConversionImpact
            .Subscribe(value =>
            {
                DefineAvailable—onversionToInstance();
            })
            .AddTo(_disposable);

        _progressConversion
            .Subscribe(value =>
            {
                DOShakeModel(value, step);
            })
            .AddTo(_disposable);
    }

    private void UnSubscribeOnUniRx() => _disposable.Clear();

    private void DefineAvailable—onversionToInstance()
    {
        for (int i = 0; i < _conversionToInstanceFromImpact.Count; i++)
            if (_conversionToInstanceFromImpact[i] == _currentConversionImpact.Value)
            {
                _availableConversionToInstance = _conversionToInstance[i];

                return;
            }

        _availableConversionToInstance = null;
    }

    private void ProcessConversion(float step)
    {
        if (_progressConversionMax <= 0 || _inPlate.Value)
        {
            _progressConversion.Value = 0;

            return;
        }

        if (_availableConversionToInstance)
            _progressConversion.Value = Mathf.Clamp(_progressConversion.Value + step * _conversionMultiplier, 0, _progressConversionMax);
        else
            _progressConversion.Value = Mathf.Clamp(_progressConversion.Value - step * 3, 0, _progressConversionMax);
    }

    private void Refresh()
    {
        _currentConversionImpact.Value = Conversion.None;
        _progressConversion.Value = 0;
    }

    private void SetupTransform(Transform parent)
    {
        transform.parent = parent;
        transform.localRotation = Quaternion.identity;
        transform
            .DOLocalMove(Vector3.zero, 2.0f)
            .SetEase(Ease.OutElastic)
            .SetLink(gameObject);
    }

    private void DOResizeModel()
    {
        _model.localScale = new Vector3(0.5f, 0.5f, 0.5f);
        _model
            .DOScale(1, 0.5f)
            .SetEase(Ease.OutElastic)
            .SetLink(gameObject);
    }

    private void DOShakeModel(float value, float step)
    {
        if (value <= 0)
            return;

        _model
            .DOShakePosition(step, (value / _progressConversionMax) / 5)
            .SetLink(gameObject);
    }

    private void DOPlatePosition()
    {
        if (!_inPlate.Value)
            return;

        _inPlate.Value.transform
            .DOLocalMove(Vector3.zero, 2.0f)
            .SetEase(Ease.OutElastic)
            .SetLink(_inPlate.Value.gameObject);
    }

    public class Factory : PlaceholderFactory<Product> { }
}
