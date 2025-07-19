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

    [SerializeField] private Transform gridOriginTransform;
    private Vector3 gridOrigin;

    [SerializeField] private float cellSize = 1f;  // 셀 하나 너비/높이


    protected override void Awake()
    {
        base.Awake();
        gridArray = new GameObject[width, height];
        gridOrigin = gridOriginTransform.position;
        EventManager.Instance.onPlayerMove += OnPlayerMove;
    }

    public void RegisterBlock(Vector2Int gridPos, GameObject block)
    {
        if (IsInsideGrid(gridPos))
        {
            Debug.Log(" Now Registered" + block);
            gridArray[gridPos.x, gridPos.y] = block;
        }
    }

    public bool IsLocked(int x, int y)
    {
        if (x < 0 || x > width - 1 || y < 0 || y > height - 1)
        {
            return true;
        }

        return gridArray[x, y] != null;
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
                    block.transform.position += Vector3.down * cellSize;
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
        //Debug.Log("is Inside Grid ??" + pos);
        return pos.x >= 0 && pos.x < width && pos.y >= 0 && pos.y < height;
    }

    public Vector2Int WorldToGrid(Vector3 worldPos)
    {
            // 1) 그리드 원점
            Vector3 origin = gridOriginTransform.position;

            // 2) 월드위치 → 원점 기준로컬 위치
            Vector3 local = worldPos - origin;

            // 3) 셀 크기로 나누고 Floor
            int x = Mathf.FloorToInt(local.x / cellSize);
            int y = Mathf.FloorToInt(local.y / cellSize);

            // 4) 범위 보정
            x = Mathf.Clamp(x, 0, width - 1);
            y = Mathf.Clamp(y, 0, height - 1);

            return new Vector2Int(x, y);
    }


    private void OnPlayerMove(Transform playerTransform)
    {
        Debug.Log("OnPlayerMove on TetriminoManager");
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

    private void test()
    {
        int count = 0;
        if (Input.GetKeyDown(KeyCode.P))
        {
            foreach (GameObject a in gridArray)
            {
                if (a != null)
                {
                    count += 1;
                }
            }
            Debug.Log("LOcked 그리드 개수!!!@@ " +count);
        }
       
    }

    public void Update()
    {
        test();
    }
}
