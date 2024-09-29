using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using System;
using Zenject;

public class MapSwitcherApplyButton : CustomButton, IPointerClickHandler
{
    private int _index;

    public int Index => _index;

    public void OnPointerClick(PointerEventData eventData)
    {
        _game.GameStart(_index);

        _signalBus.TryFire(new MapSwitcherClose());
    }

    public void SetIndex(int index) => _index = index;
}
