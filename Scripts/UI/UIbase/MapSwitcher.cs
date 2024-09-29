using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;
using TMPro;
using System;
using UnityEngine.UI;
using DG.Tweening;
using UniRx;

public class MapSwitcher : UIBase
{
    [SerializeField] private GameObject _availableContent;
    [SerializeField] private GameObject _unavailableContent;
    [SerializeField] private MapSwitcherApplyButton _applyButton;
    [SerializeField] private TextMeshProUGUI _necessaryCurrencyText;
    [SerializeField] private GameObject[] _otherElementsForDisplay;

    protected override void SubscribeOnUniRx()
    {
        base.SubscribeOnUniRx();

        _signalBus.GetStream<MapSwitcherOpen>()
            .Subscribe(observer =>
            {

                _applyButton.SetIndex(observer.Index);
                _applyButton.gameObject.SetActive(true);
            })
            .AddTo(_disposable);

        _signalBus.GetStream<MapSwitcherClose>()
            .Subscribe(observer =>
            {

                _applyButton.gameObject.SetActive(false);
            })
            .AddTo(_disposable);

        //_game.MapIndexDesired
        //    .Subscribe(value =>
        //    {
        //        Visualize(value);
        //    })
        //    .AddTo(_disposable);
    }

    //private void Visualize(int value)
    //{
    //    _availableContent.SetActive(
    //        _game.CurrencyGlobal.Value >= _game.MapDesiredPrefab.CurrencyNecessary
    //        && value != _game.MapIndex.Value
    //        && value != 0);

    //    _unavailableContent.SetActive(
    //        _game.CurrencyGlobal.Value < _game.MapDesiredPrefab.CurrencyNecessary
    //        && value != _game.MapIndex.Value
    //        && value != 0);

    //    _applyButton.SetActive(_availableContent.activeSelf);

    //    _necessaryCurrencyText.text = $"{_game.MapDesiredPrefab.CurrencyNecessary}";

    //    foreach (var element in _otherElementsForDisplay)
    //        element.SetActive(_availableContent.activeSelf || _unavailableContent.activeSelf);
    //}
}
