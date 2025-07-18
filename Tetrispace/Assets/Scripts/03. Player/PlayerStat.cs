using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStat : MonoBehaviour
{
    private Dictionary<PlayerEnforcement, int> levelsByEnforcement = new();
    private Dictionary<PlayerEnforcement, List<float>> valuesByEnforcement = new();
    [SerializeField] private PlayerLevelData playerLevelData;
    
    private void Awake()
    {
        foreach (var pair in playerLevelData.enforcements)
        {
            valuesByEnforcement.Add(pair.enforcement, pair.valueByLevel);
        }
        
        foreach (PlayerEnforcement enforcement in Enum.GetValues(typeof(PlayerEnforcement)))
        {
            levelsByEnforcement.Add(enforcement, 0);
        }
        
        EventManager.Instance.onPlayerEnforcementLevelUp -= LevelUp;
        EventManager.Instance.onPlayerEnforcementLevelUp += LevelUp;
        
        EventManager.Instance.PlayerEnforcementLevelUp(PlayerEnforcement.Speed);
        EventManager.Instance.PlayerEnforcementLevelUp(PlayerEnforcement.Speed);
        EventManager.Instance.PlayerEnforcementLevelUp(PlayerEnforcement.Speed);
    }

    public float GetStat(PlayerEnforcement enforcement)
    { 
        int level = levelsByEnforcement[enforcement];
        if (valuesByEnforcement[enforcement].Count <= level)
        {
            Debug.Log($"PlayerEnforcement {enforcement}가 Level Data에 없는 레벨{level}을 가리키고 있음");
            return 0;
        }
        return valuesByEnforcement[enforcement][level];
    }

    public int GetLevel(PlayerEnforcement enforcement)
    {
        return levelsByEnforcement[enforcement];
    }

    private void LevelUp(PlayerEnforcement enforcement)
    {
        levelsByEnforcement[enforcement]++;
    }
}