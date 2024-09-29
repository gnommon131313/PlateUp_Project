using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class MapSwitcherTrigger : MonoBehaviour
{
    protected SignalBus _signalBus;

    [SerializeField] private int _index;

    [Inject]
    private void Construct(SignalBus signalBus)
    {
        _signalBus = signalBus;
    }

    private void OnTriggerEnter(Collider other)
    {
        Player player = other.GetComponent<Player>();

        if (!player)
            return;

        _signalBus.TryFire(new MapSwitcherOpen(_index));
    }

    private void OnTriggerExit(Collider other)
    {
        Player player = other.GetComponent<Player>();

        if (!player)
            return;

        _signalBus.TryFire(new MapSwitcherClose());
    }
}
