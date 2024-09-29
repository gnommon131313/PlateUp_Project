using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class Map : MonoBehaviour
{
    [SerializeField] private int _currencyNecessary;
    [SerializeField] private float _spentSurvivalTimeMax;

    [SerializeField] private AnimationCurve _complexityCurve;
    [SerializeField] private AnimationCurve _currencyCurve;

    [SerializeField] private GameObject _gameplay;
    [SerializeField] private GameObject _environment;

    private GameObject _gameplayContentFolder;
    private GameObject _environmentContentFolder;

    public float SpentSurvivalTimeMax => _spentSurvivalTimeMax;
    public int ReceivedCurrency => Mathf.FloorToInt(_currencyCurve.Evaluate(_spentSurvivalTimeMax));
    public int CurrencyNecessary => _currencyNecessary;
    public AnimationCurve ComplexityCurve => _complexityCurve;
    public AnimationCurve CurrencyCurve => _currencyCurve;

    [Inject]
    private void Construct(
        [Inject(Id = "GameplayContentFolder")] GameObject gameplayContentFolder,
        [Inject(Id = "EnvironmentContentFolder")] GameObject environmentContentFolder)
    {
        _gameplayContentFolder = gameplayContentFolder;
        _environmentContentFolder = environmentContentFolder;
    }

    private void Start()
    {
        SetupOnStart();
    }

    public void AddSpentSurvivalTimeMax(float time)
    {
        if (_spentSurvivalTimeMax < time)
            _spentSurvivalTimeMax = time;
    }

    public void Clear()
    {
        Destroy(_gameplay);
        Destroy(_environment);
        Destroy(gameObject);
    }

    private void SetupOnStart()
    {
        _gameplay.name = gameObject.name;
        _environment.name = gameObject.name;

        _complexityCurve.preWrapMode = WrapMode.Clamp;
        _complexityCurve.postWrapMode = WrapMode.Clamp;
        _currencyCurve.preWrapMode = WrapMode.Clamp;
        _currencyCurve.postWrapMode = WrapMode.Clamp;

        _gameplay.transform.SetParent(_gameplayContentFolder.transform);
        _environment.transform.SetParent(_environmentContentFolder.transform);
    }

    public class Factory : PlaceholderFactory<Map> { }
}
