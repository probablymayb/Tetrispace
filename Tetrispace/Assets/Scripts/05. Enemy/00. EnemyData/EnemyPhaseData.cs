using UnityEngine;

[CreateAssetMenu(fileName = "EnemyPhaseData", menuName = "Enemy", order = 0)]
public class EnemyPhaseData : ScriptableObject
{
    public int enemyCount;
    public EnemyType enemyTypes;
    public bool isTetrisEnemyCountFixed;
    public int tetrisCount;
}
