using UnityEngine;

public class IMovePattern : IEnemyMovePattern
{
    private Rigidbody2D rigid;
    private Vector2[] points;
    private int currentIndex = 0;
    private float moveSpeed;

    public void Init(Vector2 startPosition, float speed, Rigidbody2D rigidBody)
    {
        rigid = rigidBody;
        moveSpeed = speed;

        float dx = 10f;

        points = new Vector2[]
        {
            startPosition + new Vector2(-dx, 0),   // 0: 왼쪽
            startPosition,                         // 1: 중앙
            startPosition + new Vector2(dx, 0),    // 2: 오른쪽
            startPosition                          // 3: 다시 중앙
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
    