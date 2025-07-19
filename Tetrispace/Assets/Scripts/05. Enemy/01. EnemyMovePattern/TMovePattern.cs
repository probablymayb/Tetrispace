using UnityEngine;

public class TMovePattern : IEnemyMovePattern
{
    private Rigidbody2D rigid;
    private Vector2[] points;
    private int currentIndex = 0;
    private float moveSpeed = 3f;

    public void Init(Vector2 startPosition, float speed, Rigidbody2D rigidBody)
    {
        moveSpeed = speed;
        rigid = rigidBody;

        float dx = GridSystem.WorldUnitsPerGridX * 10f;
        float dy = GridSystem.WorldUnitsPerGridY * 5f;

        // 이동 경로: 좌 → 중앙 → 하 → 중앙 → 우 → 중앙 → 반복
        points = new Vector2[]
        {
            startPosition + new Vector2(-dx, 0),     // 0: 왼쪽
            startPosition,                          // 1: 중앙
            startPosition + new Vector2(0, -dy),     // 2: 아래
            startPosition,                          // 3: 중앙
            startPosition + new Vector2(dx, 0),      // 4: 오른쪽
            startPosition,                           // 5: 중앙
            startPosition + new Vector2(0, -dy),     // 6: 아래
            startPosition,                          // 7: 중앙
        };
    }

    public void Tick(float deltaTime)
    {
        if (!rigid || points == null || points.Length == 0)
            return;

        Vector2 currentPos = rigid.position;
        Vector2 targetPos = points[currentIndex];

        Vector2 direction = (targetPos - currentPos).normalized;
        float distance = Vector2.Distance(currentPos, targetPos);
        float step = moveSpeed * deltaTime;

        if (distance < step)
        {
            rigid.MovePosition(targetPos);
            currentIndex = (currentIndex + 1) % points.Length;
        }
        else
        {
            rigid.MovePosition(currentPos + direction * step);
        }
    }
}