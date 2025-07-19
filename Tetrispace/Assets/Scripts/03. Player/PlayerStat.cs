using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerStat : MonoBehaviour
{
    public event Action<PlayerEnforcement> OnLevelUp;
    private Dictionary<PlayerEnforcement, int> levelsByEnforcement = new();
    private Dictionary<PlayerEnforcement, List<EnforcementValue>> valuesByEnforcement = new();
    [SerializeField] private PlayerLevelData playerLevelData;
    
    private PlayerSlowArea slowArea;
    private PlayerDrone drone;
    private PlayerAutoShooter autoShooter;
    [SerializeField] private Vector2 dronePos;
    
    private void Awake()
    {
        foreach (var pair in playerLevelData.enforcements)
        {
            valuesByEnforcement.Add(pair.enforcement, pair.valuesByLevel);
        }
        
        foreach (PlayerEnforcement enforcement in Enum.GetValues(typeof(PlayerEnforcement)))
        {
            levelsByEnforcement.Add(enforcement, 0);
        }
        
        slowArea = GetComponentInChildren<PlayerSlowArea>(true);
        drone = GetComponentInChildren<PlayerDrone>(true);
        autoShooter = GetComponentInChildren<PlayerAutoShooter>();

        OnLevelUp -= InitSlowArea;
        OnLevelUp += InitSlowArea;
        OnLevelUp -= InitDrone;
        OnLevelUp += InitDrone;
        
        EventManager.Instance.onPlayerEnforcementLevelUp -= LevelUp;
        EventManager.Instance.onPlayerEnforcementLevelUp += LevelUp;
        
        // Test
        // EventManager.Instance.PlayerEnforcementLevelUp(PlayerEnforcement.Bomb);
    }

    private void Start()
    {
        autoShooter.Init(this);
    }

    public List<float> GetStat(PlayerEnforcement enforcement)
    { 
        int level = levelsByEnforcement[enforcement];
        if (valuesByEnforcement[enforcement].Count <= level)
        {
            Debug.Log($"PlayerEnforcement {enforcement}가 Level Data에 없는 레벨{level}을 가리키고 있음");
            return new List<float>();
        }
        return valuesByEnforcement[enforcement][level].innerList;
    }

    public int GetLevel(PlayerEnforcement enforcement)
    {
        return levelsByEnforcement[enforcement];
    }

    private void LevelUp(PlayerEnforcement enforcement)
    {
        levelsByEnforcement[enforcement]++;
        OnLevelUp?.Invoke(enforcement);
    }

    private void InitSlowArea(PlayerEnforcement enforcement)
    {
        if (enforcement != PlayerEnforcement.Area) return;
        if (GetLevel(PlayerEnforcement.Area) < 1) return;

        OnLevelUp -= InitSlowArea;
        slowArea.gameObject.SetActive(true);
        slowArea.Init(this);
    }
    
    private void InitDrone(PlayerEnforcement enforcement)
    {
        if (enforcement != PlayerEnforcement.Drone) return;
        if (GetLevel(PlayerEnforcement.Drone) < 1) return;

        OnLevelUp -= InitDrone;
        drone.gameObject.SetActive(true);
        drone.transform.SetParent(null);
        drone.transform.position = dronePos;
        drone.Init(this);
    }
}