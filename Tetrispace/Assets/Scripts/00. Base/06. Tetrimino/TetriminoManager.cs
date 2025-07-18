using UnityEngine;
using System.Collections.Generic;
using System.Collections;

/// <summary>
/// ?? 테트리미노 매니저 (게임잼용)
/// 
/// 기능:
/// - 테트리미노 생성 및 관리
/// - 다음 블록 시스템
/// - 게임 오버 체크
/// - 레벨 및 속도 관리
/// </summary>
/// 
public class TetriminoManager : Singleton<TetriminoManager>
{
    private int width = 4;
    private int height = 5;

    private GameObject[,] gridArray;
    Vector3 lastPlayerPosition = Vector3.zero;

    protected override void Awake()
    {
        base.Awake();
        gridArray = new GameObject[width, height];
        EventManager.Instance.onPlayerMove += OnPlayerMove;
    }

    public void RegisterBlock(Vector2Int gridPos, GameObject block)
    {
        if (IsInsideGrid(gridPos))
        {
            gridArray[gridPos.x, gridPos.y] = block;
        }
    }

    public void CheckAndClearLines()
    {
        for (int y = 0; y < height; y++)
        {
            bool isLineFull = true;
            for (int x = 0; x < width; x++)
            {
                if (gridArray[x, y] == null)
                {
                    isLineFull = false;
                    break;
                }
            }

            if (isLineFull)
            {
                ClearLine(y);
                DropLinesAbove(y);
                y--; // 라인 제거 후 다시 검사
            }
        }
    }

    private void ClearLine(int y)
    {
        for (int x = 0; x < width; x++)
        {
            Destroy(gridArray[x, y]);
            gridArray[x, y] = null;
        }
    }

    private void DropLinesAbove(int clearedY)
    {
        for (int y = clearedY + 1; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                GameObject block = gridArray[x, y];
                if (block != null)
                {
                    gridArray[x, y - 1] = block;
                    gridArray[x, y] = null;
                    block.transform.position += Vector3.down;
                }
            }
        }
    }

    public void MoveEntireGrid(Vector3 moveDelta)
    {
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                GameObject block = gridArray[x, y];
                if (block != null)
                {
                    block.transform.position += moveDelta;
                }
            }
        }
    }

    public void ClearAll()
    {
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (gridArray[x, y] != null)
                    Destroy(gridArray[x, y]);
            }
        }
    }

    private bool IsInsideGrid(Vector2Int pos)
    {
        return pos.x >= 0 && pos.x < width && pos.y >= 0 && pos.y < height;
    }

    public Vector2Int WorldToGrid(Vector3 worldPos)
    {
        int x = Mathf.RoundToInt(worldPos.x);
        int y = Mathf.RoundToInt(worldPos.y);
        return new Vector2Int(x + width / 2, y);  // 필요 시 x 좌표 오프셋 조절
    }

    private void OnPlayerMove(Transform playerTransform)
    {
        if (lastPlayerPosition == Vector3.zero)
        {
            lastPlayerPosition = playerTransform.position;
            return;
        }

        Vector3 delta = playerTransform.position - lastPlayerPosition;
        lastPlayerPosition = playerTransform.position;
        Debug.Log("플레이어 포지션 차이값"+delta);
        MoveEntireGrid(delta);
    }
}
