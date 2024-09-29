using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;
using TMPro;
using System;
using UnityEngine.UI;
using DG.Tweening;
using UniRx;

public class SurvivalTime : UIBase
{
    [SerializeField] private TextMeshProUGUI _timeText;

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
    //    int minutesCurrent = Mathf.FloorToInt(_game.SurvivalTime.Value / 60);
    //    int secondsCurrent = Mathf.FloorToInt(_game.SurvivalTime.Value % 60);

    //    int minutesMax = 0;
    //    int secondsMax = 0;

    //    if (_game.Map)
    //    {
    //        minutesMax = Mathf.FloorToInt(_game.Map.SpentSurvivalTimeMax / 60);
    //        secondsMax = Mathf.FloorToInt(_game.Map.SpentSurvivalTimeMax % 60);
    //    }

    //    _timeText.text =
    //        $"{minutesCurrent.ToString()} : {secondsCurrent.ToString()}" +
    //        $" - {minutesMax.ToString()} : {secondsMax.ToString()} ";
    //}
}
