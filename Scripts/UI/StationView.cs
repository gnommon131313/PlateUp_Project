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

public class StationView : MonoBehaviour
{
    protected SignalBus _signalBus;
    protected CompositeDisposable _disposable = new CompositeDisposable();

    protected Station _station;

    [SerializeField] private TextMeshProUGUI _productAmountText;

    [Inject]
    private void Construct(SignalBus signalBus)
    {
        _signalBus = signalBus;
    }

    protected void Awake()
    {
        _station = GetComponent<Station>();
    }

    private void OnEnable()
    {
        SubscribeOnUniRx();
    }

    private void OnDisable()
    {
        UnSubscribeOnUniRx();
    }

    protected virtual void SubscribeOnUniRx()
    {
        _station.ProductAmount
            .Subscribe(value =>
            {
                _productAmountText.gameObject.SetActive(value > 0 && _station.ProductAmountMax > 1);
                _productAmountText.text = $"{value} / {_station.ProductAmountMax}";
            })
            .AddTo(_disposable);
    }

    private void UnSubscribeOnUniRx() => _disposable.Clear();
}