using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;
using TMPro;
using System;
using UnityEngine.UI;
using DG.Tweening;
using UniRx;

public class CurrencyReceived : UIBase
{
    [SerializeField] private Image[] _currencyImage;

    //protected override void SubscribeOnUniRx()
    //{
    //    base.SubscribeOnUniRx();

    //    _game.MapIndexDesired
    //        .Subscribe(value =>
    //        {
    //            Visualize();
    //        })
    //        .AddTo(_disposable);
    //}

    //private void Visualize()
    //{
    //    for (int i = 0; i < _currencyImage.Length; i++)
    //        _currencyImage[i].gameObject.SetActive(i < _game.MapDesiredPrefab.ReceivedCurrency);
    //}
}
