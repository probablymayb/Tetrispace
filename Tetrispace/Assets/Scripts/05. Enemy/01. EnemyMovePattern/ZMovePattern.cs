using UnityEngine;

public class ZMovePattern : IEnemyMovePattern
{
    private Rigidbody2D rigid;
    private Vector2[] points;
    private int currentIndex = 0;
    private float moveSpeed;

    public void Init(Vector2 startPosition, float speed, Rigidbody2D rigidBody)
    {
        rigid = rigidBody;
        moveSpeed = speed;

        float dx = GridSystem.WorldUnitsPerGridX * 7f;
        float dy = GridSystem.WorldUnitsPerGridY * 3f;

        // 시작 지점 중심으로 평행사변형 형태 경로 정의
        points = new Vector2[]
        {
            startPosition + new Vector2(-dx, dy),   // 0. 왼쪽 위
            startPosition + new Vector2(-dx, 0),    // 1. 왼쪽
            startPosition + new Vector2(dx, -dy),   // 2. 오른쪽 아래
            startPosition + new Vector2(dx, 0)      // 3. 오른쪽
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