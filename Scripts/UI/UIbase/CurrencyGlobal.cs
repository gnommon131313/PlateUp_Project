using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;
using TMPro;
using System;
using UnityEngine.UI;
using DG.Tweening;
using UniRx;

public class CurrencyGlobal : UIBase
{
    [SerializeField] private TextMeshProUGUI _valueText;

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
    //    _valueText.text = _game.CurrencyGlobal.Value.ToString();
    //}
}
