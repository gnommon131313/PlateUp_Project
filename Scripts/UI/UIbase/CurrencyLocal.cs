using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;
using TMPro;
using System;
using UnityEngine.UI;
using DG.Tweening;
using UniRx;

public class CurrencyLocal : UIBase
{
    [SerializeField] private Image[] _currentImage;
    [SerializeField] private Image[] _receivedImage;

    //protected override void SubscribeOnUniRx()
    //{
    //    base.SubscribeOnUniRx();

    //    _game.SurvivalTime
    //        .Subscribe(value =>
    //        {
    //            Visualize();
    //        })
    //        .AddTo(_disposable);
    //}

    //private void Visualize()
    //{
    //    for (int i = 0; i < _currentImage.Length; i++)
    //        if (i < _game.CurrencyLocal)
    //            _currentImage[i].color = new Color(1.0f, 1.0f, 1.0f);
    //        else
    //            _currentImage[i].color = new Color(0.0f, 0.0f, 0.0f);

    //    if (!_game.Map)
    //        return;

    //    for (int i = 0; i < _receivedImage.Length; i++)
    //        if (i < _game.Map.ReceivedCurrency)
    //            _receivedImage[i].color = new Color(1.0f, 1.0f, 1.0f);
    //        else
    //            _receivedImage[i].color = new Color(0.0f, 0.0f, 0.0f);
    //}
}
