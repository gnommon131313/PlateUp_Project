using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using Zenject;
using UnityEditor;
using System;

public class Station : MonoBehaviour
{
    protected SignalBus _signalBus;
    protected CompositeDisposable _disposable = new CompositeDisposable();

    [SerializeField] protected bool _lock = false;

    protected Game _game;

    [Header("Conversion")]
    [SerializeField] protected ReactiveProperty<Product> _product = new ReactiveProperty<Product>(null);
    [SerializeField] private Product _acceptableProduct;

    [SerializeField] protected ReactiveProperty<int> _productAmount = new ReactiveProperty<int>(0);
    [SerializeField] private int _productAmountMax = 1;
    [SerializeField] private bool _fillInAtStart = false;

    [SerializeField] private Conversion _conversionImpactOnProduct;
    [SerializeField] private float _conversionMultiplier = 1;
    [SerializeField] private ReactiveProperty<bool> _inConversion = new ReactiveProperty<bool>(false);
    [SerializeField] private bool _alwaysInConversion;

    [SerializeField] protected Transform _productPlace;

    [Space(25), Header("Other")]
    [SerializeField] private GameObject _selectModel;
    [SerializeField] protected GameObject _errorModel;

    public ReactiveProperty<int> ProductAmount => _productAmount;
    public int ProductAmountMax => _productAmountMax;
    private bool Storage => _productAmountMax == -1;
    private bool Filled => _productAmount.Value >= _productAmountMax && !Storage;

    [Inject]
    private void Construct(
        SignalBus signalBus,
        Game game)
    {
        _signalBus = signalBus;
        _game = game;
    }

    protected void Awake()
    {
    }

    protected void Start()
    {
        ReplaceProductPrefabWithInstance();
        FillInAmountAtStart();
    }

    protected void FixedUpdate()
    {
        TryConvertingProduct();
    }

    protected void OnEnable()
    {
        SubscribeOnUniRx();
    }

    protected void OnDisable()
    {
        UnSubscribeOnUniRx();
    }
   
    public void WasSelectedByPlayer()
    {
        _selectModel.SetActive(true);
    }

    public void WasUnselectedByPlayer()
    {
        _selectModel.SetActive(false);
    }

    protected virtual void SubscribeOnUniRx()
    {
        _product
            .Subscribe(value => SetupProduct(value))
            .AddTo(_disposable);

        _productAmount
            .Subscribe(value =>
            {
                if (value == 0 && _product.Value && !_product.Value.IsPrefab)
                    DestroyProductGameObject();
            })
            .AddTo(_disposable);

        _inConversion
            .Subscribe(value => ApplyConversionImpactForProduct(value))
            .AddTo(_disposable);

    }

    private void UnSubscribeOnUniRx() => _disposable.Clear();

    private void ApplyConversionImpactForProduct(bool value)
    {
        if (!_product.Value)
            return;

        if (value)
            _product.Value.SetCurrentConversionImpact(_conversionImpactOnProduct);
        else
            _product.Value.SetCurrentConversionImpact(Conversion.None);
    }

    private void TryConvertingProduct()
    {
        if (!_product.Value || !_product.Value.ProcessConversionIsReachedMax) 
            return;

        _product.Value = _product.Value.Converting();

        DestroySomeProductAfterConverting();
    }

    private void DestroySomeProductAfterConverting()
    {
        if (_product.Value.Type != "Trash")
            return;

        DestroyProductGameObject();
        _productAmount.Value -= 1;
    }

    private void ReplaceProductPrefabWithInstance()
    {
        if (!_product.Value || !_product.Value.IsPrefab)
            return;

        Product newInstant = Instantiate(_product.Value, 
            _productPlace.transform.position,
            _productPlace.transform.rotation);

        _product.Value = newInstant;
    }

    private void SetupProduct(Product product)
    {
        if (product && product.IsPrefab)
            return;

        if (!product)
            return;

        product.Setup(_productPlace);
        product.SetConversionMultiplier(_conversionMultiplier);

        if (_inConversion.Value || _alwaysInConversion)
            product.SetCurrentConversionImpact(_conversionImpactOnProduct);
    }

    private void FillInAmountAtStart()
    {
        if (_fillInAtStart)
            _productAmount.Value = _productAmountMax;
    }

    public Product PickUpProduct()
    {
        if (!_product.Value || _lock)
            return null;

        if (Storage)
            return Instantiate(_product.Value,
                _productPlace.transform.position,
                _productPlace.transform.rotation);

        Product newProduct = Instantiate(_product.Value,
            _productPlace.transform.position, 
            _productPlace.transform.rotation);

        _productAmount.Value -= 1;

        return newProduct;
    }

    public virtual Product PutProduct(Product product) // product должен заходить новым обьекта
    {
        if(_lock)
            return product;

        if (_product.Value)
            return PutProductInBusyPlace(product);
        else
            return PutProductInEmptyPlace(product);
    }

    private Product PutProductInBusyPlace(Product product)
    {
        if (product.InPlate.Value)
            return product;

        Product productResult = TryPutOnAPlate(product);
        if(productResult)
            return productResult;

        productResult = TryMixing(product);
        if (productResult)
            return productResult;

        if (TryPutOnNoFilled(product))
            return null;

        return product;
    }

    private Product TryPutOnAPlate(Product product)
    {
        if (_product.Value.Class == "Plate" && product.Class == "Food" && !product.InPlate.Value)
        {
            product.PlaceOnAPlate(_productPlace.transform);

            if (!Storage)
                _productAmount.Value -= 1;

            return product;
        }

        if (product.Class == "Plate" && _product.Value.Class == "Food" && !_product.Value.InPlate.Value)
        {
            Destroy(product.gameObject);

            Product newProduct = Instantiate(_product.Value, 
                _productPlace.transform.position, 
                _productPlace.transform.rotation);
            newProduct.PlaceOnAPlate(_productPlace.transform);

            if (!Storage)
                _productAmount.Value -= 1;

            return newProduct;
        }

        return null;
    }

    private Product TryMixing(Product product)
    {
        Product mixProduct = null;

        // Двумя цыклами пройтись сначало по одному обьекту а потом по другому (так можно задавать связь только для одного обьекта)
        for (int i = 0; i < _product.Value.MixWithInstance.Count; i++)
            if (_product.Value.MixWithInstance[i].Type == product.Type)
                mixProduct = _product.Value.MixResultInstance[i];
        for (int i = 0; i < product.MixWithInstance.Count; i++)
            if (product.MixWithInstance[i].Type == _product.Value.Type)
                mixProduct = product.MixResultInstance[i];

        if (!mixProduct)
            return null;

        Product mixInstance = Instantiate(mixProduct,
            _productPlace.transform.position, 
            _productPlace.transform.rotation);

        Destroy(product.gameObject);

        if (!Storage)
            _productAmount.Value -= 1;

        return mixInstance;
    }

    private bool TryPutOnNoFilled(Product product)
    {
        if (Filled)
            return false;

        if (_acceptableProduct && _acceptableProduct.Type != product.Type)
            return false;

        if (!Storage)
            _productAmount.Value += 1;

        DestroyProductGameObject();
        _product.Value = product;

        return true;

    }

    private Product PutProductInEmptyPlace(Product product)
    {
        if (_acceptableProduct && _acceptableProduct.Type != product.Type)
            return product;

        _product.Value = product;

        _productAmount.Value += 1;

        return null;
    }


    public Product ChunkingProduct()
    {
        if (_lock)
            return null;

        Product productResult = TryTakeOutFromAPlate();
        if (productResult)
            return productResult;

        if (!_product.Value
            || Storage
            || _productAmount.Value > 1
            || _product.Value.Chunk.Value <= 0)
            return null;

        Product productCached = _product.Value;
        Product turnIntoInstance = null;
        if (_product.Value.TurnIntoAfterChunking) // обьект при отламывании от него куска может просто исчезнуть
        {
            turnIntoInstance = Instantiate(productCached.TurnIntoAfterChunking, 
                _productPlace.transform.position,
                _productPlace.transform.rotation);
            turnIntoInstance.SetChunk(productCached.Chunk.Value - 1);
        }
        else
        {
            _productAmount.Value -= 1; // обьект не во что не превратится и его нельзя будет взять, так что сразу вычесть
        }

        Product resultInstance = null;
        resultInstance = Instantiate(productCached.ChunkingResult,
            _productPlace.transform.position,
            _productPlace.transform.rotation);

        DestroyProductGameObject();
        _product.Value = turnIntoInstance;

        Destroy(productCached);

        return resultInstance;
    }

    private Product TryTakeOutFromAPlate()
    {
        if (!_product.Value || !_product.Value.InPlate.Value)
            return null;

        Product newProduct = Instantiate(_product.Value, 
            _productPlace.transform.position,
            _productPlace.transform.rotation);

        DestroyProductGameObject();
        _product.Value = Instantiate(newProduct.InPlate.Value,
            _productPlace.transform.position,
            _productPlace.transform.rotation);

        newProduct.TakeOutFromAPlate();

        return newProduct;
    }

    public void Use()
    {
        if (_lock || _alwaysInConversion)
            return;

        _inConversion.Value = true;
    }

    public void Unuse()
    {
        if (_lock || _alwaysInConversion)
            return;

        _inConversion.Value = false;
    }

    protected void DestroyProductGameObject()
    {
        if (!_product.Value)
            return;

        Destroy(_product.Value.gameObject);
        _product.Value = null; // если просто уничтожить обьект то значение станет = Missing, это не тоже самое что null
    }

    public class Factory : PlaceholderFactory<Station> { }
}
