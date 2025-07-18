using UnityEngine;
using System.Collections.Generic;

public class PlayerGrid : MonoBehaviour
{
    [Header("플레이어 그리드 설정")]
    public int gridWidth = 4;           // 가로 4칸
    public int gridHeight = 5;          // 세로 5칸
    public float gridCellSize = 28f;     // 각 칸의 크기

    [Header("그리드 위치 설정")]
    public Vector2 gridOffset = new Vector2(0, 2f);  // 플레이어 기준 오프셋
    public bool followPlayer = true;    // 플레이어를 따라다닐지 여부

    [Header("참조")]
    public Transform player;            // 플레이어 Transform
    public GameObject blockPrefab;      // 테트리스 블록 프리팹

    // 그리드 시스템
    private Transform[,] grid;          // 4x5 그리드
    private Vector3 gridWorldCenter;    // 그리드의 월드 중심점

    // 현재 떨어지는 테트로미노
    private Transform currentTetromino;
    private List<Transform> tetrominoBlocks = new List<Transform>();

    void Start()
    {
        InitializeGrid();
        UpdateGridPosition();
    }

    void Update()
    {
        //test

        if (Input.GetKeyDown(KeyCode.Y))
        {
            CreateTestBlock();
        }
        if (followPlayer && player != null)
        {
            UpdateGridPosition();
        }
    }

    /// <summary>
    /// 플레이어 그리드 초기화
    /// </summary>
    void InitializeGrid()
    {
        grid = new Transform[gridWidth, gridHeight];

        // 그리드 시각화용 오브젝트 생성 (디버그용)
        CreateGridVisualizer();

        Debug.Log($"PlayerGrid 초기화: {gridWidth}x{gridHeight}");
    }

    /// <summary>
    /// 플레이어 위치에 따라 그리드 위치 업데이트
    /// </summary>
    void UpdateGridPosition()
    {
        if (player == null) return;

        // 플레이어 위치 + 오프셋으로 그리드 중심 계산
        gridWorldCenter = player.position + new Vector3(gridOffset.x, gridOffset.y, 0);
    }

    /// <summary>
    /// 그리드 좌표를 월드 좌표로 변환
    /// </summary>
    /// <param name="gridX">그리드 X 좌표 (0~3)</param>
    /// <param name="gridY">그리드 Y 좌표 (0~4)</param>
    /// <returns>월드 좌표</returns>
    public Vector3 GridToWorldPosition(int gridX, int gridY)
    {
        if (!IsValidGridPosition(gridX, gridY))
        {
            Debug.LogWarning($"잘못된 그리드 좌표: [{gridX}][{gridY}]");
            return Vector3.zero;
        }

        // 그리드 중심을 기준으로 계산
        float worldX = gridWorldCenter.x + (gridX - (gridWidth - 1) * 0.5f) * gridCellSize;
        float worldY = gridWorldCenter.y + (gridY - (gridHeight - 1) * 0.5f) * gridCellSize;

        return new Vector3(worldX, worldY, 0);
    }

    /// <summary>
    /// 월드 좌표를 그리드 좌표로 변환
    /// </summary>
    /// <param name="worldPos">월드 좌표</param>
    /// <returns>그리드 좌표</returns>
    public Vector2Int WorldToGridPosition(Vector3 worldPos)
    {
        float localX = (worldPos.x - gridWorldCenter.x) / gridCellSize + (gridWidth - 1) * 0.5f;
        float localY = (worldPos.y - gridWorldCenter.y) / gridCellSize + (gridHeight - 1) * 0.5f;

        int gridX = Mathf.RoundToInt(localX);
        int gridY = Mathf.RoundToInt(localY);

        return new Vector2Int(gridX, gridY);
    }

    /// <summary>
    /// 유효한 그리드 좌표인지 확인
    /// </summary>
    public bool IsValidGridPosition(int gridX, int gridY)
    {
        return gridX >= 0 && gridX < gridWidth && gridY >= 0 && gridY < gridHeight;
    }

    /// <summary>
    /// 특정 그리드 위치에 블록이 있는지 확인
    /// </summary>
    public bool IsGridOccupied(int gridX, int gridY)
    {
        if (!IsValidGridPosition(gridX, gridY)) return true;
        return grid[gridX, gridY] != null;
    }

    /// <summary>
    /// 그리드에 블록 배치
    /// </summary>
    public void PlaceBlock(int gridX, int gridY, Transform block)
    {
        if (IsValidGridPosition(gridX, gridY))
        {
            grid[gridX, gridY] = block;

            // 블록을 실제 월드 위치로 이동
            Vector3 worldPos = GridToWorldPosition(gridX, gridY);
            block.position = worldPos;
        }
    }

    /// <summary>
    /// 그리드에서 블록 제거
    /// </summary>
    public void RemoveBlock(int gridX, int gridY)
    {
        if (IsValidGridPosition(gridX, gridY))
        {
            if (grid[gridX, gridY] != null)
            {
                Destroy(grid[gridX, gridY].gameObject);
                grid[gridX, gridY] = null;
            }
        }
    }

    /// <summary>
    /// 테트로미노가 유효한 위치에 있는지 확인
    /// </summary>
    public bool IsValidTetrominoPosition(Transform tetromino)
    {
        foreach (Transform block in tetromino)
        {
            Vector2Int gridPos = WorldToGridPosition(block.position);

            // 경계 체크
            if (!IsValidGridPosition(gridPos.x, gridPos.y))
            {
                return false;
            }

            // 다른 블록과 충돌 체크
            if (IsGridOccupied(gridPos.x, gridPos.y))
            {
                // 같은 테트로미노의 블록이 아닌 경우만 충돌로 간주
                if (grid[gridPos.x, gridPos.y].parent != tetromino)
                {
                    return false;
                }
            }
        }

        return true;
    }

    /// <summary>
    /// 테트로미노를 그리드에 고정
    /// </summary>
    public void LockTetromino(Transform tetromino)
    {
        foreach (Transform block in tetromino)
        {
            Vector2Int gridPos = WorldToGridPosition(block.position);

            if (IsValidGridPosition(gridPos.x, gridPos.y))
            {
                grid[gridPos.x, gridPos.y] = block;

                // 부모 관계 해제하여 개별 블록으로 만들기
                block.SetParent(transform);
            }
        }

        // 테트로미노 오브젝트 파괴
        Destroy(tetromino.gameObject);

        // 라인 클리어 체크
        CheckForLines();
    }

    /// <summary>
    /// 완성된 라인 체크 및 제거
    /// </summary>
    void CheckForLines()
    {
        for (int y = 0; y < gridHeight; y++)
        {
            if (IsLineFull(y))
            {
                ClearLine(y);
                DropLinesAbove(y);
                y--; // 같은 줄 다시 체크
            }
        }
    }

    /// <summary>
    /// 해당 줄이 가득 찼는지 확인
    /// </summary>
    bool IsLineFull(int y)
    {
        for (int x = 0; x < gridWidth; x++)
        {
            if (grid[x, y] == null)
            {
                return false;
            }
        }
        return true;
    }

    /// <summary>
    /// 줄 제거
    /// </summary>
    void ClearLine(int y)
    {
        for (int x = 0; x < gridWidth; x++)
        {
            if (grid[x, y] != null)
            {
                Destroy(grid[x, y].gameObject);
                grid[x, y] = null;
            }
        }

        Debug.Log($"라인 클리어: Y={y}");
    }

    /// <summary>
    /// 제거된 줄 위의 블록들을 아래로 이동
    /// </summary>
    void DropLinesAbove(int clearedY)
    {
        for (int y = clearedY + 1; y < gridHeight; y++)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                if (grid[x, y] != null)
                {
                    // 한 칸 아래로 이동
                    grid[x, y - 1] = grid[x, y];
                    grid[x, y] = null;

                    // 실제 위치도 이동
                    Vector3 newWorldPos = GridToWorldPosition(x, y - 1);
                    grid[x, y - 1].position = newWorldPos;
                }
            }
        }
    }

    /// <summary>
    /// 테스트용 블록 생성
    /// </summary>
    [ContextMenu("테스트 블록 생성")]
    public void CreateTestBlock()
    {
        if (blockPrefab == null)
        {
            Debug.LogWarning("blockPrefab이 설정되지 않았습니다!");
            return;
        }

        //// 랜덤한 위치에 블록 생성
        //int randomX = Random.Range(0, gridWidth);
        //int randomY = Random.Range(0, gridHeight);

        for(int i = 0; i < gridWidth; ++i)
        {
            for (int j = 0; j < gridHeight; ++j)
            {
                if (!IsGridOccupied(i, j))
                {
                    GameObject block = Instantiate(blockPrefab, transform);
                    PlaceBlock(i, j, block.transform);

                    Debug.Log($"테스트 블록 생성: [{i}][{j}]");
                }

            }

        }
    }

    /// <summary>
    /// 그리드 시각화 (디버그용)
    /// </summary>
    void CreateGridVisualizer()
    {
        GameObject visualizer = new GameObject("GridVisualizer");
        visualizer.transform.SetParent(transform);

        // 나중에 LineRenderer나 Gizmos로 그리드 라인 그리기
    }

    /// <summary>
    /// 그리드 정보 출력
    /// </summary>
    [ContextMenu("그리드 정보 출력")]
    public void PrintGridInfo()
    {
        Debug.Log($"=== PlayerGrid 정보 ===");
        Debug.Log($"크기: {gridWidth}x{gridHeight}");
        Debug.Log($"셀 크기: {gridCellSize}");
        Debug.Log($"월드 중심: {gridWorldCenter}");
        Debug.Log($"플레이어 위치: {(player != null ? player.position : Vector3.zero)}");

        // 그리드 상태 출력
        for (int y = gridHeight - 1; y >= 0; y--)
        {
            string line = $"Y{y}: ";
            for (int x = 0; x < gridWidth; x++)
            {
                line += (grid[x, y] != null ? "[■]" : "[□]");
            }
            Debug.Log(line);
        }
    }

    /// <summary>
    /// 씬 뷰에서 그리드 시각화
    /// </summary>
    void OnDrawGizmos()
    {
        if (player == null) return;

        // 그리드 중심점 계산
        Vector3 center = player.position + new Vector3(gridOffset.x, gridOffset.y, 0);

        // 그리드 경계 그리기
        Gizmos.color = Color.yellow;
        float totalWidth = gridWidth * gridCellSize;
        float totalHeight = gridHeight * gridCellSize;
        Gizmos.DrawWireCube(center, new Vector3(totalWidth, totalHeight, 0));

        // 그리드 라인 그리기
        Gizmos.color = Color.gray;

        // 세로 라인
        for (int x = 0; x <= gridWidth; x++)
        {
            float worldX = center.x + (x - gridWidth * 0.5f) * gridCellSize;
            Vector3 start = new Vector3(worldX, center.y - totalHeight * 0.5f, 0);
            Vector3 end = new Vector3(worldX, center.y + totalHeight * 0.5f, 0);
            Gizmos.DrawLine(start, end);
        }

        // 가로 라인
        for (int y = 0; y <= gridHeight; y++)
        {
            float worldY = center.y + (y - gridHeight * 0.5f) * gridCellSize;
            Vector3 start = new Vector3(center.x - totalWidth * 0.5f, worldY, 0);
            Vector3 end = new Vector3(center.x + totalWidth * 0.5f, worldY, 0);
            Gizmos.DrawLine(start, end);
        }

        // 그리드 셀 번호 표시 (에디터에서)
        if (Application.isPlaying)
        {
            Gizmos.color = Color.red;
            for (int x = 0; x < gridWidth; x++)
            {
                for (int y = 0; y < gridHeight; y++)
                {
                    Vector3 cellCenter = GridToWorldPosition(x, y);
                    if (IsGridOccupied(x, y))
                    {
                        Gizmos.DrawCube(cellCenter, Vector3.one * 0.3f);
                    }
                }
            }
        }
    }
}