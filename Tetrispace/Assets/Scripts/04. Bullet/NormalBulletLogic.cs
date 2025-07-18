using System.Collections;
using UnityEngine;

// 한 방향으로 이동하는 일반 총알
public class NormalBulletLogic : IBulletLogic
{
    public IEnumerator Execute(Bullet bullet, Vector3 dir)
    {
        float timer = 0f;
        while (timer < bullet.Data.lifeTime)
        {
            bullet.transform.position += dir * (bullet.Data.speed * Time.deltaTime);
            timer += Time.deltaTime;
            yield return null;
        }

        PoolManager.Instance.Return(bullet);
    }
}
