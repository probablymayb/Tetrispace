using System.Collections;
using UnityEngine;

// 한 방향으로 이동하는 일반 총알
public class NormalBulletLogic : IBulletLogic
{
    public IEnumerator Execute(Bullet bullet, Vector3 dir)
    {
        while (true)
        {
            bullet.transform.position += dir * (bullet.Speed * Time.deltaTime);
            yield return null;
        }
    }
}
