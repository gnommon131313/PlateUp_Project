using ModestTree;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Zenject;

// Чтобы поля были авто записаны в JSON файл они должны быть public или [SerializeField] + рекомендуется ставить у класса [System.Serializable]
// Свойтсва не записиваются в файл ( PROP => _field )
// Поля записыватся в момент создания JSON файла (это происходит при JsonUtility.ToJson(DATA))
// Вернее они не записыватся а каждый раз переписыватся в файл, если в файле поля с таким именем нету то оно создается, а если в файле есть поля он в классе его уже нет то оно авто удалится

[System.Serializable]
public class GameData
{
    private Map[] _mapsPool;
    [SerializeField] private List<float> _spentSurvivalTimeMaxOnMaps = new List<float>();

    public float Time1;

    public List<float> SpentSurvivalTimeMaxOnMaps => _spentSurvivalTimeMaxOnMaps;

    public GameData(Map[] maps = null)
    {
        _mapsPool = maps;
    }

    public void CheckAvailabilityOfAllIndexesInArrays()
    {
        // До любого взаимодействия с массивами (и списани и ...) из базы данных нужно убедится что у них нужный размер (есть нужные индексы).
        // Когда массив загружается их фала в этом фале может не быть информации от этом масиве и тогда его длина будет 0, а части программы могут обратится к определенному индексу, потому что предпологаеся что он ТОЧНО должен быть.
        // Или информация в файле от этом масиве может быть, но старая, где у него мало индексов
        
        //Debug.Log(_spentSurvivalTimeMaxOnMaps.Count);

        if (_spentSurvivalTimeMaxOnMaps.Count < _mapsPool.Length)
        {
            int difference = _mapsPool.Length - _spentSurvivalTimeMaxOnMaps.Count;

            for (int i = 0; i < difference; i++)
                _spentSurvivalTimeMaxOnMaps.Add(0);

            //Debug.Log("difference " + difference);
        }

        //Debug.Log(_spentSurvivalTimeMaxOnMaps.Count);
    }


    public void LoadFromJSONFile(GameData data)
    {
        _spentSurvivalTimeMaxOnMaps = new List<float>(data._spentSurvivalTimeMaxOnMaps);
    }

    public void AddSpentSurvivalTimeMax(int index, float value)
    {
        if(_spentSurvivalTimeMaxOnMaps[index] < value)
            _spentSurvivalTimeMaxOnMaps[index] = value;
    }

    public class Factory : PlaceholderFactory<GameData> { }
}
