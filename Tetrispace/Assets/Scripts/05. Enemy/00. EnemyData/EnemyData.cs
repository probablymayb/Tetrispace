using UnityEngine;

[CreateAssetMenu(fileName = "EnemyData", menuName = "Enemy", order = 1)]
public class EnemyData : ScriptableObject
{
    public float hp;
    public float damage;
    public Texture2D enemySprite;
    public bool isEnforced;
}
