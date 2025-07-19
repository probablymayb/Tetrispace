using UnityEngine;

public class LMovePattern : IEnemyMovePattern
{
    private Rigidbody2D rigid;
    private Vector2[] points;
    private int currentIndex = 0;
    private float moveSpeed;

    public void Init(Vector2 startPosition, float speed, Rigidbody2D rigidBody)
    {
        rigid = rigidBody;
        moveSpeed = speed;

        float dx = 7f;
        float dy = 3f;

        // L자 삼각형 루트
        points = new Vector2[]
        {
            startPosition + new Vector2(dx, dy),    // 0: 오른쪽 위
            startPosition + new Vector2(dx, 0),    // 1: 오른쪽
            startPosition,                          // 2: 원점
        };
    }

    public void Tick(float dt)
    {
        if (!rigid || points == null || points.Length == 0)
            return;

        Vector2 currentPos = rigid.position;
        Vector2 targetPos = points[currentIndex];

        Vector2 dir = (targetPos - currentPos).normalized;
        float dist = Vector2.Distance(currentPos, targetPos);
        float step = moveSpeed * dt;

        if (dist < step)
        {
            rigid.MovePosition(targetPos);
            currentIndex = (currentIndex + 1) % points.Length;
        }
        else
        {
            rigid.MovePosition(currentPos + dir * step);
        }
    }
}