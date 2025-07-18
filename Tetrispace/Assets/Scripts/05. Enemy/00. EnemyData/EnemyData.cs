using UnityEngine;

[CreateAssetMenu(menuName = "Enemy/EnemyData")]
public class EnemyData : ScriptableObject
{
    public float hp;
    public float damage;
    public float speed;
    public float attackSpeed;
    public BulletData bulletData;
    public Texture2D enemySprite;
    public bool isEnforced;
}
