using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using Zenject;

public class UIBase : MonoBehaviour
{
    protected SignalBus _signalBus;
    protected CompositeDisposable _disposable = new CompositeDisposable();

    protected Game _game = null;

    [SerializeField] protected UIVisibleCondition _contentVisible;
    [SerializeField] GameObject _content;

    [Inject]
    private void Construct(
        SignalBus signalBus,
        Game game)
    {
        _signalBus = signalBus;
        _game = game;
    }

    protected virtual void OnEnable()
    {
        SubscribeOnUniRx();
    }

    protected virtual void OnDisable()
    {
        UnSubscribeOnUniRx();
    }

    protected virtual void SubscribeOnUniRx()
    {
        _game.GameStateCurrent
            .Subscribe(value => DefineContentVisible(value))
            .AddTo(_disposable);
    }

    private void UnSubscribeOnUniRx() => _disposable.Clear();

    private void DefineContentVisible(GameState gameState)
    {
        _content.SetActive(
            ((int)_contentVisible == (int)gameState
            || _contentVisible == UIVisibleCondition.Always)
            && _contentVisible != UIVisibleCondition.Never);
    }
}
