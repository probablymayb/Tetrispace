using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class Enemy
{
    private IEnumerator AutoFire()
    {
        float fireInterval = 1f / data.attackSpeed;

        while (true)
        {
            FireBullet();
            yield return new WaitForSeconds(fireInterval);
        }
    }

    private void FireBullet()
    {
        List<Vector2> directions = new List<Vector2>();
        switch (type)
        {
            case EnemyType.I:
                directions.Add(Vector2.down);
                break;
            case EnemyType.S:
                directions.Add(new Vector2(0.5f, -1).normalized);
                break;
            case EnemyType.L:
                directions.Add(Vector2.down);
                break;
            case EnemyType.Z:
                directions.Add(new Vector2(-0.5f, -1).normalized);
                break;
            case EnemyType.O:
                directions.Add(new Vector2(-0.5f, -1).normalized);
                directions.Add(new Vector2(0.5f, -1).normalized);
                break;
            case EnemyType.T:
                directions.Add(Vector2.down);
                break;
            default:
                directions.Add(Vector2.down);
                break;
        }

        foreach (var dir in directions)
        {
            Bullet bullet = PoolManager.Instance.Get(bulletPrefab);
            bullet.transform.position = transform.position;
            bullet.Setup(data.bulletData, data.damage);
            bullet.Fire(dir, false);
        }
    }
}