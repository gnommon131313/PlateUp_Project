using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class CustomButton : MonoBehaviour
{
    protected SignalBus _signalBus;

    protected Game _game;

    [Inject]
    private void Construct(
        SignalBus signalBus,
        Game game)
    {
        _signalBus = signalBus;
        _game = game;
    }
}
