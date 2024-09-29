using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Zenject;
using UniRx;
using TMPro;
using DG.Tweening;
using UnityEngine.UI;

public class ProductView : MonoBehaviour
{
    private CompositeDisposable _disposable = new CompositeDisposable();
    
    private Product _product;

    [SerializeField] private Slider _sliderProgressConversion;
    [SerializeField] private Image _fillSliderProgressConversion;
    [SerializeField] private GameObject _warningFrameProgressConversion;
    [SerializeField] private List<GameObject> _iconsProgressConversion;
    [SerializeField] private GameObject _actionFrame;
    [SerializeField] private List<GameObject> _iconsAction;
    [SerializeField] private GameObject _chunkFrameAction;
    [SerializeField] private TextMeshProUGUI _chunkTextAction;

    private void Awake()
    {
        _product = GetComponent<Product>();
    }

    private void Start()
    {
        SetupIconActionConversion();
    }

    private void OnEnable()
    {
        SetupIconActionConversion();
        SubscribeOnUniRx();
    }

    private void OnDisable()
    {
        UnSubscribeOnUniRx();
    }

    private void SubscribeOnUniRx()
    {
        _product.ProgressConversion
           .Subscribe(value =>
           {
               _actionFrame.SetActive(value <= 0 && !_product.InPlate.Value);

               _sliderProgressConversion.gameObject.SetActive(value > 0);
               _sliderProgressConversion.DOValue(value / _product.ProgressConversionMax, 0.1f).SetEase(Ease.Linear);

               if (_product.AvailableConversionToInstanceIsGarbage)
                   _fillSliderProgressConversion.color = new Color(1, 1, 1 - (value / _product.ProgressConversionMax));
               else
                   _fillSliderProgressConversion.color = new Color(1 - (value / _product.ProgressConversionMax), 1, 1 - (value / _product.ProgressConversionMax));
           })
           .AddTo(_disposable);

        _product.CurrentConversionImpact
            .Subscribe(value =>
            {
                _warningFrameProgressConversion.SetActive(_product.AvailableConversionToInstanceIsGarbage);

                foreach (var icon in _iconsProgressConversion)
                    icon.SetActive(icon.name == value.ToString());
            })
            .AddTo(_disposable);

        _product.InPlate
            .Subscribe(value =>
            {
                _actionFrame.SetActive(!value);

                _chunkFrameAction.SetActive(!value && _product.Chunk.Value > 0);
            })
            .AddTo(_disposable);

        _product.Chunk
            .Subscribe(value =>
            {
                _chunkTextAction.text = value.ToString();
                _chunkFrameAction.SetActive(value > 0);
            })
            .AddTo(_disposable);
    }

    private void UnSubscribeOnUniRx() => _disposable.Clear();

    private void SetupIconActionConversion()
    {
        foreach (var icon in _iconsAction)
        {
            bool active = false;

            foreach (var Impact in _product.ConversionFromImpact)
                if (icon.name == Impact.ToString())
                    active = true;

            icon.SetActive(active);
        }
    }
}
