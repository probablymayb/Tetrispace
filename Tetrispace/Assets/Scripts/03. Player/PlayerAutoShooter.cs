using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAutoShooter : MonoBehaviour
{
    [SerializeField] private float attackSpeed;
    [SerializeField] private Bullet bulletPrefab;
    [SerializeField] private List<BulletData> bulletDatas;
    [SerializeField] private BulletData bombData;
    [SerializeField] private List<Transform> bombHitEffects;
    private List<List<Transform>> shootPositions = new();
    private float damage;
    private int ammo;
    private float bombCoolTime;
    private PlayerStat playerStat;

    private void Awake()
    {
        FindShootPos();
    }

    private void FindShootPos()
    {
        foreach (Transform shootPoses in transform)
        {
            shootPositions.Add(new List<Transform>());
            foreach (Transform shootPos in shootPoses)
            {
                shootPositions[^1].Add(shootPos);
            }
        }
    }
    
    public void Init(PlayerStat stat)
    {
        playerStat = stat;
        ammo = (int)playerStat.GetStat(PlayerEnforcement.AutoAmmo)[0];
        damage = playerStat.GetStat(PlayerEnforcement.AutoDamage)[0];
        bombCoolTime = playerStat.GetStat(PlayerEnforcement.Bomb)[0];
        
        playerStat.OnLevelUp -= OnAutoDamageLevelUp;
        playerStat.OnLevelUp += OnAutoDamageLevelUp;
        playerStat.OnLevelUp -= OnAutoAmmoLevelUp;
        playerStat.OnLevelUp += OnAutoAmmoLevelUp;
        playerStat.OnLevelUp -= OnBombLevelUp;
        playerStat.OnLevelUp += OnBombLevelUp;
        StartCoroutine(AutoFire());
    }

    private void OnAutoDamageLevelUp(PlayerEnforcement enforcement)
    {
        if (enforcement != PlayerEnforcement.AutoDamage) return;
        damage = playerStat.GetStat(PlayerEnforcement.AutoDamage)[0];
    }

    private void OnAutoAmmoLevelUp(PlayerEnforcement enforcement)
    {
        if (enforcement != PlayerEnforcement.AutoAmmo) return;
        ammo = (int)playerStat.GetStat(PlayerEnforcement.AutoAmmo)[0];
    }

    private void OnBombLevelUp(PlayerEnforcement enforcement)
    {
        if (enforcement != PlayerEnforcement.Bomb) return;
        bombCoolTime = playerStat.GetStat(PlayerEnforcement.Bomb)[0];
        if (playerStat.GetLevel(PlayerEnforcement.Bomb) == 1) StartCoroutine(AutoBomb());
    }

    private IEnumerator AutoFire()
    {
        print("AutoFire");
        float fireInterval = 1f / attackSpeed;

        while (true)
        {
            FireBullet();
            yield return new WaitForSeconds(fireInterval);
        }
    }

    private void FireBullet()
    {
        for (int i = 0; i < ammo; i++)
        {
            Vector2 direction = Vector2.up;
            Bullet bullet = PoolManager.Instance.Get(bulletPrefab);
            bullet.transform.position = shootPositions[ammo-1][i].position;
            bullet.Setup(bulletDatas[ammo-1], damage);
            bullet.Fire(direction, "Enemy");
        }
    }
    
    private IEnumerator AutoBomb()
    {
        while (true)
        {
            FireBomb();
            yield return new WaitForSeconds(bombCoolTime);
        }
    }
    
    private void FireBomb()
    {
        Vector2 direction = Vector2.up;
        Bullet bomb = PoolManager.Instance.Get(bulletPrefab);
        bomb.OnDeath += () => { Bomb(bomb.transform); };
        bomb.transform.position = transform.position;
        bomb.Setup(bombData, damage);
        bomb.Fire(direction, "Enemy");
    }

    private void Bomb(Transform bombTransform)
    {
        Instantiate(bombHitEffects[playerStat.GetLevel(PlayerEnforcement.Bomb) - 1], bombTransform.position, bombTransform.rotation);
        Vector2 bombCenter = bombTransform.position;
        float bombRadius = playerStat.GetStat(PlayerEnforcement.Bomb)[1];
        Collider2D[] hits = Physics2D.OverlapCircleAll(bombCenter, bombRadius);
        foreach (var hit in hits)
        {
            if (!hit.CompareTag("Enemy")) continue;
            print("Bomb Hit with " + hit.name);
            IEntity entity = hit.GetComponent<IEntity>();
            entity?.OnHit(damage, hit.ClosestPoint(transform.position));
        }
        
        #if UNITY_EDITOR
        float angleStep = 360f / 32;
        for (int i = 0; i < 32; i++)
        {
            float angle1 = Mathf.Deg2Rad * (angleStep * i);
            float angle2 = Mathf.Deg2Rad * (angleStep * (i + 1));

            Vector3 point1 = bombCenter + new Vector2(Mathf.Cos(angle1), Mathf.Sin(angle1)) * bombRadius;
            Vector3 point2 = bombCenter + new Vector2(Mathf.Cos(angle2), Mathf.Sin(angle2)) * bombRadius;

            Debug.DrawLine(point1, point2, Color.red, 1.0f); // 마지막 인자는 지속 시간
        }
        #endif
    }
}
