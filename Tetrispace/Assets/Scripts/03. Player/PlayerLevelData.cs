using System.Collections.Generic;
using UnityEngine;

public enum PlayerEnforcement
{
    Ammo,
    Damage,
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