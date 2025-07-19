using UnityEngine;
using System.Collections.Generic;
using System.Collections;

/// <summary>
/// 테트리스 매니저 개선 버전
/// 
/// 주요 개선사항:
/// 1. 블록 상태 동기화 강화
/// 2. 지연 파괴 문제 해결
/// 3. 그리드 이동 시 안전성 강화
/// </summary>
public class TetriminoManager : Singleton<TetriminoManager>
{
    private int width = 4;
    private int height = 5;

    private GameObject[,] gridArray;
    Vector3 lastPlayerPosition = Vector3.zero;

    [SerializeField] private Transform gridOriginTransform;
    private Vector3 gridOrigin;
    [SerializeField] private float cellSize = 1f;

    // 그리드 이동 중 트리거 이벤트 무시를 위한 플래그
    public bool IsGridMoving { get; private set; } = false;

    // 파괴 예정 블록들을 추적 (지연 파괴 문제 해결)
    private HashSet<GameObject> pendingDestroy = new HashSet<GameObject>();

    protected override void Awake()
    {
        base.Awake();
        gridArray = new GameObject[width, height];
        gridOrigin = gridOriginTransform.position;
        EventManager.Instance.onPlayerMove += OnPlayerMove;
    }

    /// <summary>
    /// 블록을 그리드에 등록 (중복 등록 방지)
    /// </summary>
    public void RegisterBlock(Vector2Int gridPos, GameObject block)
    {
        if (IsInsideGrid(gridPos) && !pendingDestroy.Contains(block))
        {
            // 기존 블록이 있다면 경고 출력
            if (gridArray[gridPos.x, gridPos.y] != null)
            {
                Debug.LogWarning($"Grid position {gridPos} already occupied! Replacing...");
            }

            Debug.Log($"Registering block at {gridPos}: {block.name}");
            gridArray[gridPos.x, gridPos.y] = block;

            // 블록 상태 확실히 설정
            block.tag = "LockedBlock";
        }
    }

    /// <summary>
    /// 해당 위치가 잠겨있는지 확인 (파괴 예정 블록 제외)
    /// </summary>
    public bool IsLocked(int x, int y)
    {
        if (x < 0 || x > width - 1 || y < 0 || y > height - 1)
        {
            return true;
        }

        GameObject block = gridArray[x, y];
        return block != null && !pendingDestroy.Contains(block);
    }

    /// <summary>
    /// 라인 체크 및 제거 (개선된 버전)
    /// </summary>
    public void CheckAndClearLines()
    {
        // 그리드 이동 중에는 라인 체크하지 않음
        if (IsGridMoving) return;

        List<int> linesToClear = new List<int>();

        // 먼저 제거할 라인들을 모두 찾기
        for (int y = 0; y < height; y++)
        {
            bool isLineFull = true;
            for (int x = 0; x < width; x++)
            {
                if (!IsLocked(x, y))
                {
                    isLineFull = false;
                    break;
                }
            }

            if (isLineFull)
            {
                linesToClear.Add(y);
            }
        }

        // 라인 제거 실행
        if (linesToClear.Count > 0)
        {
            StartCoroutine(ClearLinesCoroutine(linesToClear));
        }
    }

    /// <summary>
    /// 라인 제거를 코루틴으로 처리 (안전한 파괴)
    /// </summary>
    private IEnumerator ClearLinesCoroutine(List<int> linesToClear)
    {
        // 제거할 블록들을 먼저 비활성화하고 파괴 예정 목록에 추가
        foreach (int y in linesToClear)
        {
            for (int x = 0; x < width; x++)
            {
                GameObject block = gridArray[x, y];
                if (block != null)
                {
                    pendingDestroy.Add(block);
                    block.SetActive(false); // 즉시 비활성화로 충돌 방지
                    gridArray[x, y] = null;
                }
            }
        }

        // 한 프레임 대기 (다른 시스템들이 상태 변화를 인식할 수 있도록)
        yield return null;

        // 실제 파괴
        foreach (int y in linesToClear)
        {
            for (int x = 0; x < width; x++)
            {
                // 이미 gridArray에서는 제거되었으므로 pendingDestroy에서만 찾아서 파괴
            }
        }

        // 파괴 예정 블록들 정리
        foreach (GameObject block in pendingDestroy)
        {
            if (block != null)
            {
                Destroy(block);
            }
        }
        pendingDestroy.Clear();

        // 위쪽 라인들 드롭
        foreach (int clearedY in linesToClear)
        {
            DropLinesAbove(clearedY);
        }
    }

    /// <summary>
    /// 위쪽 라인들을 아래로 떨어뜨리기 (개선된 버전)
    /// </summary>
    private void DropLinesAbove(int clearedY)
    {
        for (int y = clearedY + 1; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                GameObject block = gridArray[x, y];
                if (block != null && !pendingDestroy.Contains(block))
                {
                    // 그리드 배열 업데이트
                    gridArray[x, y - 1] = block;
                    gridArray[x, y] = null;

                    // 위치 이동
                    block.transform.position += Vector3.down * cellSize;

                    // 블록 상태 재확인 및 업데이트
                    UpdateBlockState(block);
                }
            }
        }
    }

    /// <summary>
    /// 블록 상태 업데이트 (태그, 콜라이더 등)
    /// </summary>
    private void UpdateBlockState(GameObject block)
    {
        if (block != null)
        {
            // 락된 블록으로 태그 설정
            block.tag = "LockedBlock";

            // 필요시 콜라이더 재활성화
            Collider2D collider = block.GetComponent<Collider2D>();
            if (collider != null)
            {
                collider.enabled = true;
            }
        }
    }

    /// <summary>
    /// 전체 그리드 이동 (안전성 강화)
    /// </summary>
    public void MoveEntireGrid(Vector3 moveDelta)
    {
        // 그리드 이동 플래그 설정
        IsGridMoving = true;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                GameObject block = gridArray[x, y];
                if (block != null && !pendingDestroy.Contains(block))
                {
                    block.transform.position += moveDelta;
                }
            }
        }

        // 한 프레임 후 플래그 해제
        StartCoroutine(ResetGridMovingFlag());
    }

    /// <summary>
    /// 그리드 이동 플래그 리셋
    /// </summary>
    private IEnumerator ResetGridMovingFlag()
    {
        yield return null;
        IsGridMoving = false;
    }

    public void ClearAll()
    {
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (gridArray[x, y] != null)
                {
                    pendingDestroy.Add(gridArray[x, y]);
                    Destroy(gridArray[x, y]);
                    gridArray[x, y] = null;
                }
            }
        }
        pendingDestroy.Clear();
    }

    private bool IsInsideGrid(Vector2Int pos)
    {
        return pos.x >= 0 && pos.x < width && pos.y >= 0 && pos.y < height;
    }

    public Vector2Int WorldToGrid(Vector3 worldPos)
    {
        Vector3 origin = gridOriginTransform.position;
        Vector3 local = worldPos - origin;

        int x = Mathf.FloorToInt(local.x / cellSize);
        int y = Mathf.FloorToInt(local.y / cellSize);

        // 클램프 대신 유효 범위 체크 후 경고
        if (x < 0 || x >= width || y < 0 || y >= height)
        {
            Debug.LogWarning($"WorldToGrid: Position {worldPos} is outside grid bounds");
        }

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
        Debug.Log($"플레이어 위치 변화: {delta}");
        MoveEntireGrid(delta);
    }

    private void Test()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            int count = 0;
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (IsLocked(x, y))
                    {
                        count++;
                    }
                }
            }
            Debug.Log($"Locked 그리드 개수: {count}");
        }
    }

    public void Update()
    {
        Test();
    }
}