using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using System;

public class LoadMapWorldButton : CustomButton, IPointerClickHandler
{
    public void OnPointerClick(PointerEventData eventData)
    {
        _game.GameGoToWolrd();
    }
}
