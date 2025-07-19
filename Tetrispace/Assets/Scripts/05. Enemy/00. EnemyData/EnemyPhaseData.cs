using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Enemy/EnemyPhaseData")]
public class EnemyPhaseData : ScriptableObject
{
    public int enemyCount;
    public List<EnemyType> enemyTypes;
    public bool isTetrisEnemyCountFixed;
    public int tetrisCount;
}
