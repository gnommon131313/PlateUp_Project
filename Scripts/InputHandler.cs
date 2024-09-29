using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using UnityEngine.InputSystem;
using Zenject;

public class InputHandler : IInitializable, IDisposable
{
    private InputControls _inputControls = new InputControls();

    public ReactiveProperty<Vector3> Axis { get; private set; }
    public ReactiveCommand Action1 { get; private set; }
    public ReactiveCommand Action2 { get; private set; }
    public ReactiveProperty<bool> Action3 { get; private set; }

    public void Initialize()
    {
        _inputControls.Enable();

        Axis = new ReactiveProperty<Vector3>();
        _inputControls.Player.Axis.performed += x => Axis.Value = new Vector3(x.ReadValue<Vector2>().x, 0, x.ReadValue<Vector2>().y);
        _inputControls.Player.Axis.canceled += x => Axis.Value = Vector3.zero;

        Action1 = new ReactiveCommand();
        _inputControls.Player.Action1.started += x => Action1.Execute();

        Action2 = new ReactiveCommand();
        _inputControls.Player.Action2.started += x => Action2.Execute();

        Action3 = new ReactiveProperty<bool>();
        _inputControls.Player.Action3.performed += x => Action3.Value = true;
        _inputControls.Player.Action3.canceled += x => Action3.Value = false;
    }

    public void Dispose()
    {
        _inputControls.Disable();
    }
}