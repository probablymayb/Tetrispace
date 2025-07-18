using System;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    private IBulletLogic logic;
    public BulletData Data { get; private set; }
    private bool isSetup = false;
    
    /// <summary>
    /// Bullet 생성 후 BulletData를 전달해 새로운 Bullet을 만드는 메서드
    /// </summary>
    /// <param name="bulletData">Bullet에 대한 정보가 담긴 ScriptableObject</param>
    public void Setup(BulletData bulletData)
    {
        isSetup = true;
        Data = bulletData;
        logic = BulletLogicFactory.Create(Data.logicType);
    }

    /// <summary>
    /// 총알 발사하는 메서드, Setup 안되었을 시 실행 안됨
    /// </summary>
    /// <param name="dir">이동시키는 방향</param>
    public void Fire(Vector2 dir)
    {
        if (!isSetup)
        {
            print("Bullet이 Setup되지 않은채로 Fire 시도");
            return;
        }
        StartCoroutine(logic.Execute(this, dir));
    }

    private void OnDisable()
    {
        isSetup = false;
    }
}
