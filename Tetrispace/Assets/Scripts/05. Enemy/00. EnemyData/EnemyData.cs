using UnityEngine;

[CreateAssetMenu(menuName = "Enemy/EnemyData")]
public class EnemyData : ScriptableObject
{
    public float hp;
    public float damage;
    public Texture2D enemySprite;
    public bool isEnforced;
}
