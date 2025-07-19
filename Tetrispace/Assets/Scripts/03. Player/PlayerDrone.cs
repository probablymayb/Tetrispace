using System;
using System.Collections;
using UnityEngine;

public class PlayerDrone : MonoBehaviour
{
    [SerializeField] private float speed;
    [SerializeField] private float attackSpeed;
    [SerializeField] private Vector2[] PatrolPos;
    [SerializeField] private Bullet bulletPrefab;
    [SerializeField] private BulletData bulletData;
    private float damage;
    private PlayerStat playerStat;
    
    public void Init(PlayerStat stat)
    {
        playerStat = stat;
        damage = playerStat.GetStat(PlayerEnforcement.Drone);
        playerStat.OnLevelUp -= OnDroneLevelUp;
        playerStat.OnLevelUp += OnDroneLevelUp;
        StartCoroutine(AutoFire());
        StartCoroutine(Patrol());
    }

    private void OnDroneLevelUp(PlayerEnforcement enforcement)
    {
        if (enforcement != PlayerEnforcement.Drone) return;
        damage = playerStat.GetStat(PlayerEnforcement.Drone);
    }
    
    
    private IEnumerator AutoFire()
    {
        float fireInterval = 1f / attackSpeed;

        while (true)
        {
            FireBullet();
            yield return new WaitForSeconds(fireInterval);
        }
    }

    private void FireBullet()
    {
        Vector2 direction = Vector2.up;
        Bullet bullet = PoolManager.Instance.Get(bulletPrefab);
        bullet.transform.position = transform.position;
        bullet.Setup(bulletData, damage);
        bullet.Fire(direction, true);
    }

    private IEnumerator Patrol()
    {
        if (PatrolPos == null || PatrolPos.Length == 0) yield break;

        int index = 0;

        while (true)
        {
            Vector2 target = PatrolPos[index];
            while (Vector2.Distance(transform.position, target) > 0.05f)
            {
                transform.position = Vector2.MoveTowards(transform.position, target, speed * Time.deltaTime);
                yield return null;
            }

            index = (index + 1) % PatrolPos.Length;
            yield return null;
        }
    }
}
