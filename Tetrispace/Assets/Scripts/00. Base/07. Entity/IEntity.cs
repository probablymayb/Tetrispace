using UnityEngine;

public interface IEntity
{
    void OnHit(float Damage, Vector2 hitPosition);
}