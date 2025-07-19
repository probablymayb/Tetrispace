using UnityEngine;

public interface IEnemyMovePattern
{
    void Init(Vector2 startPosition, float speed, Rigidbody2D rigidBody);
    void Tick(float deltaTime);
}
