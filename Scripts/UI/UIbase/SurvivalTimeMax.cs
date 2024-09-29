using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;
using TMPro;
using System;
using UnityEngine.UI;
using DG.Tweening;
using UniRx;

public class SurvivalTimeMax : UIBase
{
    [SerializeField] private TextMeshProUGUI _timeText;

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
    //    int minutesMax = Mathf.FloorToInt(_game.MapDesiredPrefab.SpentSurvivalTimeMax / 60);
    //    int secondsMax = Mathf.FloorToInt(_game.MapDesiredPrefab.SpentSurvivalTimeMax % 60);

    //    _timeText.text = $"{minutesMax.ToString()} : {secondsMax.ToString()} ";
    //}
}
