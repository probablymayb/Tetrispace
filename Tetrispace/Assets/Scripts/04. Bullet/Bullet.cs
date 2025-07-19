using System;
using UnityEngine;

[RequireComponent(typeof(CapsuleCollider2D))]
public class Bullet : MonoBehaviour
{
    private IBulletLogic logic;
    private BulletData data;
    private Transform effectTransform;
    private string targetTag;
    private bool isSetup = false;
    private float damage;
    public float Speed { private set; get; }
    public float LifeTime { private set; get; }
    public event Action OnDeath;
    private CapsuleCollider2D capsule;
    private const string EnemyTag = "Enemy";
    private const string PlayerTag = "Player";

    private void Awake()
    {
        capsule = GetComponent<CapsuleCollider2D>();
    }

    /// <summary>
    /// Bullet 생성 후 BulletData를 전달해 새로운 Bullet을 만드는 메서드
    /// </summary>
    /// <param name="bulletData">Bullet에 대한 정보가 담긴 ScriptableObject</param>
    /// <param name="bulletDamage">탄막 데미지</param>
    public void Setup(BulletData bulletData, float bulletDamage)
    {
        isSetup = true;
        data = bulletData;
        logic = BulletLogicFactory.Create(data.logicType);
        damage = bulletDamage;
        Speed = data.speed;
        LifeTime = data.lifeTime;
        PoolManager.Instance.CreatePool(data.effectPrefabTransform);
        effectTransform = PoolManager.Instance.Get(data.effectPrefabTransform);
        effectTransform.SetParent(transform);
        effectTransform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
        capsule.size = data.capsuleSize;
    }

    /// <summary>
    /// 총알 발사하는 메서드, Setup 안되었을 시 실행 안됨
    /// </summary>
    /// <param name="dir">이동시키는 방향</param>
    /// <param name="isTargetEnemy">대상이 적인지, 플레이어인지 true or false</param>
    public void Fire(Vector2 dir, bool isTargetEnemy)
    {
        if (!isSetup)
        {
            print("Bullet이 Setup되지 않은채로 Fire 시도");
            return;
        }
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle - 90f, Vector3.forward);
        targetTag = isTargetEnemy ? EnemyTag : PlayerTag;
        StartCoroutine(logic.Execute(this, dir));
    }

    private void OnDisable()
    {
        isSetup = false;
        OnDeath = null;
        if (effectTransform)
        {
            effectTransform.SetParent(null);
            PoolManager.Instance.Return(effectTransform);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!isSetup) return;
        if (other.CompareTag(PlayerSlowArea.Tag))
        {
            Speed = data.speed * 0.5f;
            return;
        }
        if (!other.CompareTag(targetTag)) return;

        print("Bullet Hit with " + other.name);
        IEntity entity = other.GetComponent<IEntity>();
        entity?.OnHit(damage, other.ClosestPoint(transform.position));
        Die();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!isSetup) return;
        if (other.CompareTag(PlayerSlowArea.Tag))
        {
            Speed = data.speed;
        }
    }

    public void Die()
    {
        OnDeath?.Invoke();
        StopAllCoroutines();
        PoolManager.Instance.Return(this);
    }
}
