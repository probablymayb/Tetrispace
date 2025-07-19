using System.Collections.Generic;
using UnityEngine;

public enum PlayerEnforcement
{
    AutoAmmo,
    AutoDamage,
    Speed,
    Drone,
    Bomb,
    Area,
}

[System.Serializable]
public class EnforcementData
{
    public PlayerEnforcement enforcement;
    public List<float> valueByLevel;
}

[CreateAssetMenu(menuName = "Player/PlayerLevelData")]
public class PlayerLevelData : ScriptableObject
{
    public List<EnforcementData> enforcements;
}