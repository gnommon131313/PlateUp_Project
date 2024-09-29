using ModestTree;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Zenject;

// ����� ���� ���� ���� �������� � JSON ���� ��� ������ ���� public ��� [SerializeField] + ������������� ������� � ������ [System.Serializable]
// �������� �� ������������ � ���� ( PROP => _field )
// ���� ����������� � ������ �������� JSON ����� (��� ���������� ��� JsonUtility.ToJson(DATA))
// ������ ��� �� ����������� � ������ ��� ������������� � ����, ���� � ����� ���� � ����� ������ ���� �� ��� ���������, � ���� � ����� ���� ���� �� � ������ ��� ��� ��� �� ��� ���� ��������

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
        // �� ������ �������������� � ��������� (� ������� � ...) �� ���� ������ ����� �������� ��� � ��� ������ ������ (���� ������ �������).
        // ����� ������ ����������� �� ���� � ���� ���� ����� �� ���� ���������� �� ���� ������ � ����� ��� ����� ����� 0, � ����� ��������� ����� ��������� � ������������� �������, ������ ��� ������������� ��� �� ����� ������ ����.
        // ��� ���������� � ����� �� ���� ������ ����� ����, �� ������, ��� � ���� ���� ��������
        
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
