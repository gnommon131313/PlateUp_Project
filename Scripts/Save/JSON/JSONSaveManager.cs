using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using TMPro;
using UniRx;
using Zenject;
using UnityEngine.Playables;

public class JSONSaveManager : MonoBehaviour
{
    private CompositeDisposable _disposable = new CompositeDisposable();

    private string _filePath = Application.dataPath + Path.AltDirectorySeparatorChar + "/MyGameData.json";

    private GameData.Factory _gameDataFactory;
    private GameData _gameData;

    public GameData GameData => _gameData;

    [Inject]
    private void Construct(GameData.Factory gameDataFactory)
    {
        _gameDataFactory = gameDataFactory;
    }

    private void Awake()
    {
        SetupData();
    }

    private void Update()
    {
        // TEST
        if (Input.GetKeyDown(KeyCode.LeftControl))
            Save();
        if (Input.GetKeyDown(KeyCode.LeftAlt))
            Load();
    }

    private void OnEnable()
    {
        SubscribeOnUniRx();
    }

    private void OnDisable()
    {
        UnSubscribeOnUniRx();
    }

    [ContextMenu("Save")]
    public void Save()
    {
        string jsonData = JsonUtility.ToJson(_gameData);

        using (StreamWriter writer = new StreamWriter(_filePath))
            writer.WriteLine(jsonData);
    }

    [ContextMenu("Load")]
    public void Load()
    {
        string jsonData = string.Empty;

        using (StreamReader reader = new StreamReader(_filePath))
            jsonData = reader.ReadToEnd();

        GameData data = JsonUtility.FromJson<GameData>(jsonData);

        _gameData.LoadFromJSONFile(data);

    }

    private void SetupData()
    {
        _gameData = _gameDataFactory.Create();

        FirstCreateJSONFile();
        Load();

        _gameData.CheckAvailabilityOfAllIndexesInArrays();
    }

    private void FirstCreateJSONFile()
    {
        if (File.Exists(_filePath))
            return;

        string jsonData = JsonUtility.ToJson(_gameData);

        using (StreamWriter writer = new StreamWriter(_filePath))
            writer.WriteLine(jsonData);
    }

    private void SubscribeOnUniRx()
    {
        AutoSave();
    }

    private void UnSubscribeOnUniRx() => _disposable.Clear();

    private void AutoSave()
    {
        float step = 3.0f;
        Observable
            .Interval(TimeSpan.FromSeconds(step))
            .Subscribe(_ => Save())
            .AddTo(_disposable);
    }
}

