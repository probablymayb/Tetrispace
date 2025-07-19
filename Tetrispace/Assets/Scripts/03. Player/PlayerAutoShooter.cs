using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAutoShooter : MonoBehaviour
{
    [SerializeField] private float attackSpeed;
    [SerializeField] private Bullet bulletPrefab;
    [SerializeField] private List<BulletData> bulletDatas;
    private List<List<Transform>> shootPositions = new();
    private float damage;
    private int ammo;
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
        ammo = (int)playerStat.GetStat(PlayerEnforcement.AutoAmmo);
        damage = playerStat.GetStat(PlayerEnforcement.AutoDamage);
        playerStat.OnLevelUp -= OnAutoDamageLevelUp;
        playerStat.OnLevelUp += OnAutoDamageLevelUp;
        playerStat.OnLevelUp -= OnAutoAmmoLevelUp;
        playerStat.OnLevelUp += OnAutoAmmoLevelUp;
        StartCoroutine(AutoFire());
    }

    private void OnAutoDamageLevelUp(PlayerEnforcement enforcement)
    {
        if (enforcement != PlayerEnforcement.AutoDamage) return;
        damage = playerStat.GetStat(PlayerEnforcement.AutoDamage);
    }

    private void OnAutoAmmoLevelUp(PlayerEnforcement enforcement)
    {
        if (enforcement != PlayerEnforcement.AutoAmmo) return;
        ammo = (int)playerStat.GetStat(PlayerEnforcement.AutoAmmo);
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
        for (int i = 0; i < ammo; i++)
        {
            Vector2 direction = Vector2.up;
            Bullet bullet = PoolManager.Instance.Get(bulletPrefab);
            bullet.transform.position = shootPositions[ammo-1][i].position;
            bullet.Setup(bulletDatas[i], damage);
            bullet.Fire(direction, true);
        }
    }
}
