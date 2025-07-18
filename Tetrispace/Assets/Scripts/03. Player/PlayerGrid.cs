using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 플레이어 그리드 (테트리미노 연동 버전)
/// 
/// 기능:
/// - 4x5 플레이어 중심 그리드
/// - 테트리미노 충돌 검사
/// - 라인 클리어 시스템
/// - 그리드 초기화
/// </summary>
public class PlayerGrid : MonoBehaviour
{
    [Header("=== 플레이어 그리드 설정 ===")]
    public int gridWidth = 4;           // 가로 4칸
    public int gridHeight = 5;          // 세로 5칸
    public float gridCellSize = 1f;     // 각 칸의 크기 (월드 단위)

    [Header("=== 그리드 위치 설정 ===")]
    public Vector2 gridOffset = new Vector2(0, 2f);  // 플레이어 기준 오프셋
    public bool followPlayer = true;    // 플레이어를 따라다닐지 여부

    [Header("=== 참조 ===")]
    public Transform player;            // 플레이어 Transform
    public GameObject blockPrefab;      // 테트리스 블록 프리팹

    [Header("=== 라인 클리어 이펙트 ===")]
    public float clearLineDelay = 0.5f; // 라인 클리어 지연 시간
    public Color clearLineColor = Color.yellow; // 클리어 라인 색상

    // === 그리드 시스템 ===
    private Transform[,] grid;          // 4x5 그리드
    private Vector3 gridWorldCenter;    // 그리드의 월드 중심점
    private List<int> linesToClear = new List<int>(); // 클리어할 라인 목록


    #region === 초기화 ===

    void Start()
    {
        InitializeGrid();
        UpdateGridPosition();
    }

    void Update()
    {
        // 테스트 코드 제거하고 플레이어 따라다니기만 유지
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
        Debug.Log($"PlayerGrid 초기화: {gridWidth}x{gridHeight}");
    }

    /// <summary>
    /// 그리드 완전 초기화 (게임 재시작용)
    /// </summary>
    public void ResetGrid()
    {
        // 모든 블록 제거
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                if (grid[x, y] != null)
                {
                    Destroy(grid[x, y].gameObject);
                    grid[x, y] = null;
                }
            }
        }

        Debug.Log("그리드 초기화 완료");
    }

    #endregion

    #region === 위치 관리 ===

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

    #endregion

    #region === 그리드 검사 ===

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
    /// 여러 위치가 모두 비어있는지 확인
    /// </summary>
    /// <param name="positions">확인할 위치 목록</param>
    /// <returns>모두 비어있는지 여부</returns>
    public bool ArePositionsEmpty(List<Vector2Int> positions)
    {
        foreach (Vector2Int pos in positions)
        {
            if (IsGridOccupied(pos.x, pos.y))
            {
                return false;
            }
        }
        return true;
    }

    #endregion

    #region === 블록 배치 ===

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

            // 블록 부모 설정
            block.SetParent(transform);

            Debug.Log($"블록 배치: [{gridX}][{gridY}] at {worldPos}");
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
    /// 여러 블록을 한 번에 배치
    /// </summary>
    /// <param name="blocks">배치할 블록들</param>
    /// <param name="positions">배치할 위치들</param>
    public void PlaceBlocks(List<Transform> blocks, List<Vector2Int> positions)
    {
        for (int i = 0; i < blocks.Count && i < positions.Count; i++)
        {
            PlaceBlock(positions[i].x, positions[i].y, blocks[i]);
        }
    }

    #endregion

    #region === 라인 클리어 시스템 ===

    /// <summary>
    /// 완성된 라인 체크 및 제거
    /// </summary>
    public void CheckAndClearLines()
    {
        linesToClear.Clear();

        // 완성된 라인 찾기
        for (int y = 0; y < gridHeight; y++)
        {
            if (IsLineFull(y))
            {
                linesToClear.Add(y);
            }
        }

        // 라인이 있으면 클리어
        if (linesToClear.Count > 0)
        {
            StartCoroutine(ClearLinesSequence());
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
    /// 라인 클리어 시퀀스 (코루틴)
    /// </summary>
    System.Collections.IEnumerator ClearLinesSequence()
    {
        // 클리어될 라인 하이라이트
        HighlightLines(linesToClear);

        // 지연 시간
        yield return new WaitForSeconds(clearLineDelay);

        // 실제 라인 제거
        int clearedCount = linesToClear.Count;
        foreach (int y in linesToClear)
        {
            ClearLine(y);
        }

        // 블록들 아래로 이동
        DropLinesAbove();

        // 이벤트 발생
        EventManager.Instance.LinesCleared(clearedCount, linesToClear.ToArray());

        // 테트리미노 매니저에 알림
        if (TetriminoManager.Instance != null)
        {
            TetriminoManager.Instance.OnLinesCleared(clearedCount);
        }

        Debug.Log($"라인 클리어 완료: {clearedCount}개");
    }

    /// <summary>
    /// 라인 하이라이트 (시각 효과)
    /// </summary>
    /// <param name="lines">하이라이트할 라인 목록</param>
    void HighlightLines(List<int> lines)
    {
        foreach (int y in lines)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                if (grid[x, y] != null)
                {
                    // 블록 색상 변경 (렌더러가 있다면)
                    Renderer renderer = grid[x, y].GetComponent<Renderer>();
                    if (renderer != null)
                    {
                        renderer.material.color = clearLineColor;
                    }
                }
            }
        }
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
    }

    /// <summary>
    /// 제거된 줄 위의 블록들을 아래로 이동
    /// </summary>
    void DropLinesAbove()
    {
        // 아래쪽부터 위로 올라가면서 처리
        for (int y = 0; y < gridHeight; y++)
        {
            // 현재 줄이 비어있는지 확인
            if (IsLineEmpty(y))
            {
                // 위쪽에서 블록 찾아서 내리기
                for (int upperY = y + 1; upperY < gridHeight; upperY++)
                {
                    if (!IsLineEmpty(upperY))
                    {
                        // 한 줄 전체를 아래로 이동
                        MoveLineDown(upperY, y);
                        break;
                    }
                }
            }
        }
    }

    /// <summary>
    /// 해당 줄이 비어있는지 확인
    /// </summary>
    bool IsLineEmpty(int y)
    {
        for (int x = 0; x < gridWidth; x++)
        {
            if (grid[x, y] != null)
            {
                return false;
            }
        }
        return true;
    }

    /// <summary>
    /// 한 줄을 아래로 이동
    /// </summary>
    void MoveLineDown(int fromY, int toY)
    {
        for (int x = 0; x < gridWidth; x++)
        {
            if (grid[x, fromY] != null)
            {
                // 블록 이동
                grid[x, toY] = grid[x, fromY];
                grid[x, fromY] = null;

                // 실제 위치 업데이트
                Vector3 newWorldPos = GridToWorldPosition(x, toY);
                grid[x, toY].position = newWorldPos;
            }
        }
    }

    #endregion

    #region === 테트리미노 관련 ===

    /// <summary>
    /// 테트리미노가 유효한 위치에 있는지 확인 (충돌 검사)
    /// </summary>
    /// <param name="positions">확인할 위치 목록</param>
    /// <param name="ignoreTetrimino">무시할 테트리미노 (현재 테트리미노)</param>
    /// <returns>유효한 위치인지 여부</returns>
    public bool IsValidTetrominoPosition(List<Vector2Int> positions, Transform ignoreTetrimino = null)
    {
        foreach (Vector2Int pos in positions)
        {
            // 경계 체크
            if (!IsValidGridPosition(pos.x, pos.y))
            {
                return false;
            }

            // 충돌 체크
            if (grid[pos.x, pos.y] != null)
            {
                // 같은 테트리미노의 블록은 무시
                if (ignoreTetrimino != null && grid[pos.x, pos.y].IsChildOf(ignoreTetrimino))
                {
                    continue;
                }
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// 테트리미노 고정 후 라인 클리어 체크
    /// </summary>
    public void OnTetrominoLocked()
    {
        // 라인 클리어 체크
        CheckAndClearLines();
    }

    #endregion

    #region === 디버그 및 테스트 ===

    /// <summary>
    /// 테스트용 블록 생성 (수정된 버전)
    /// </summary>
    [ContextMenu("테스트 블록 생성")]
    public void CreateTestBlock()
    {
        if (blockPrefab == null)
        {
            Debug.LogWarning("blockPrefab이 설정되지 않았습니다!");
            return;
        }

        // 랜덤한 위치에 블록 생성
        int randomX = Random.Range(0, gridWidth);
        int randomY = Random.Range(0, gridHeight);

        if (!IsGridOccupied(randomX, randomY))
        {
            GameObject block = Instantiate(blockPrefab, transform);
            PlaceBlock(randomX, randomY, block.transform);
            Debug.Log($"테스트 블록 생성: [{randomX}][{randomY}]");
        }
        else
        {
            Debug.Log($"위치 [{randomX}][{randomY}]는 이미 점유되어 있습니다.");
        }
    }

    /// <summary>
    /// 테스트용 라인 생성
    /// </summary>
    [ContextMenu("테스트 라인 생성")]
    public void CreateTestLine()
    {
        if (blockPrefab == null)
        {
            Debug.LogWarning("blockPrefab이 설정되지 않았습니다!");
            return;
        }

        // 맨 아래 줄에 블록 생성
        int y = 0;
        for (int x = 0; x < gridWidth; x++)
        {
            if (!IsGridOccupied(x, y))
            {
                GameObject block = Instantiate(blockPrefab, transform);
                PlaceBlock(x, y, block.transform);
            }
        }

        Debug.Log("테스트 라인 생성 완료");
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
        int totalBlocks = 0;
        for (int y = gridHeight - 1; y >= 0; y--)
        {
            string line = $"Y{y}: ";
            for (int x = 0; x < gridWidth; x++)
            {
                if (grid[x, y] != null)
                {
                    line += "[■]";
                    totalBlocks++;
                }
                else
                {
                    line += "[□]";
                }
            }
            Debug.Log(line);
        }

        Debug.Log($"총 블록 개수: {totalBlocks}");
    }

    #endregion

    #region === 씬 뷰 시각화 ===

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

        // 점유된 그리드 셀 표시
        if (Application.isPlaying && grid != null)
        {
            Gizmos.color = Color.red;
            for (int x = 0; x < gridWidth; x++)
            {
                for (int y = 0; y < gridHeight; y++)
                {
                    if (IsGridOccupied(x, y))
                    {
                        Vector3 cellCenter = GridToWorldPosition(x, y);
                        Gizmos.DrawCube(cellCenter, Vector3.one * gridCellSize * 0.8f);
                    }
                }
            }
        }
    }

    #endregion
}