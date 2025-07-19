using UnityEngine;

public class OMovePattern : IEnemyMovePattern
{
    private Rigidbody2D rigid;
    private Vector2 center;
    private float radius = 5f;
    private float angle = 0f; // 현재 각도 (라디안)
    private float angleSpeed = 2f; // 초당 회전속도
    
    public void Init(Vector2 startPosition, float speed, Rigidbody2D rigidBody)
    {
        rigid = rigidBody;
        angleSpeed = speed;

        center = startPosition + new Vector2(0, -radius); // 시작점 기준 아래로 중심점 설정
        angle = Mathf.PI / 2f; // 처음에는 정점 (0, radius)
    }

    public void Tick(float dt)
    {
        if (!rigid) return;

        angle += angleSpeed * dt;

        float x = center.x + Mathf.Cos(angle) * radius;
        float y = center.y + Mathf.Sin(angle) * radius;

        rigid.MovePosition(new Vector2(x, y));
    }
}