using UnityEngine;

public class TetrisBlock : MonoBehaviour
{
    void Start()
    {
        
    }

    void Update()
    {
        Transform trans = GetComponent<Transform>();
        Vector2 gridPos = GridSystem.GetGridMiddleWorldPosition(GridSystem.GridMiddlePos.x, GridSystem.GridMiddlePos.y);
        trans.position = new Vector3(gridPos.x, gridPos.y, 0);
    }
}
