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

public class OrderTableView : StationView
{
    private OrderTable _orderTable;

    [SerializeField] private Slider _sliderPatience;
    [SerializeField] private Image _fillSliderPatience;
    [SerializeField] private GameObject _eatIcon;
    [SerializeField] private GameObject _orderExpiredIcon;

    private new void Awake()
    {
        base.Awake();

        _orderTable = (OrderTable)_station;
    }

    protected override void SubscribeOnUniRx()
    {
        base.SubscribeOnUniRx();

        _orderTable.PatienceTimer
            .Subscribe(value =>
            {
                _sliderPatience.gameObject.SetActive(value > 0);
                _sliderPatience.DOValue(value / _orderTable.PatienceTime, 0.1f).SetEase(Ease.Linear);
            })
            .AddTo(_disposable);

        _orderTable.EatTimer
            .Subscribe(value =>
            {
                _eatIcon.SetActive(value > 0);
            })
            .AddTo(_disposable);

        _signalBus.GetStream<OrderExpired>()
            .Subscribe(data =>
            {
                _orderExpiredIcon.SetActive(true);
            }).AddTo(_disposable);
    }
}
