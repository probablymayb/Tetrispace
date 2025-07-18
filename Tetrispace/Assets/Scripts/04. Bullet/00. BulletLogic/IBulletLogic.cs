using System.Collections;
using UnityEngine;

// 실질적 총알의 이동로직을 구현하는 인터페이스
public interface IBulletLogic
{
    IEnumerator Execute(Bullet bullet, Vector3 dir);
}