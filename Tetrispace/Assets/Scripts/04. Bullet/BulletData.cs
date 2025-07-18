using UnityEngine;

[CreateAssetMenu(menuName = "Bullet/BulletData")]
public class BulletData : ScriptableObject
{
    public float speed;
    public float lifeTime;
    public BulletLogicType logicType;
}
