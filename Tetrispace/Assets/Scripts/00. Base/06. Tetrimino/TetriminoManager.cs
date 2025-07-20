using System;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;

/// <summary>
/// 테트리미노 매니저 - 게임오버 조건 수정 버전
/// 
/// 수정 사항:
/// - 4x5 그리드에서 올바른 게임오버 조건 적용
/// - 5칸째(Y=4, 최상단)에 블록이 있으면서 6칸째(Y=5+) 위에도 블록이 있으면 게임오버
/// - 인덱스 기반: 4칸째(Y=3)과 5칸째(Y=4) 체크하는 것이 아니라, 5칸째(Y=4)와 6칸째(Y=5+) 체크
/// 
/// 주요 기능:
/// 1. UselessBlock도 그리드에 정상 등록
/// 2. 라인 클리어 체크에서만 UselessBlock 제외
/// 3. 플레이어 움직임에 자동으로 따라다님
/// 4. 10초 후 자동 삭제
/// 5. 게임오버 조건: 5칸째(Y=4, 최상단)에 블록이 있으면서 동시에 6칸째(Y=5+) 위에도 블록이 있어야 함
/// </summary>
public class TetriminoManager : Singleton<TetriminoManager>
{
    public event Action OnLineClear;
    private int width = 4;
    private int height = 5;  // 0,1,2,3,4 인덱스 = 총 5칸

    private GameObject[,] gridArray;
    Vector3 lastPlayerPosition = Vector3.zero;

    [SerializeField] private Transform gridOriginTransform;
    private Vector3 gridOrigin;
    [SerializeField] private float cellSize = 1f;

    // 그리드 이동 중 트리거 이벤트 무시를 위한 플래그
    public bool IsGridMoving { get; private set; } = false;

    // 파괴 예정 블록들을 추적 (중복 파괴 문제 해결)
    private HashSet<GameObject> pendingDestroy = new HashSet<GameObject>();
    // UselessBlock 위치 추적 배열 추가
    private bool[,] uselessBlockPositions = new bool[4, 5];

    // 전역 게임오버 상태 관리
    private bool _globalGameOver = false;
    public bool IsGameOver
    {
        get { return _globalGameOver; }
        private set
        {
            if (_globalGameOver != value)
            {
                _globalGameOver = value;
                Debug.Log($"전역 게임오버 상태 변경: {value}");

                if (value)
                {
                    OnGameOver();
                }
            }
        }
    }

    [Header("=== UselessBlock 시스템 ===")]
    [SerializeField] private GameObject uselessBlockPrefab; // I자형 UselessBlock 프리팹
    [SerializeField] private float uselessBlockLifetime = 10f;
    [SerializeField] private bool showUselessBlockWarning = true;

    // UselessBlock 관리 (간단하게)
    private List<UselessBlock> activeUselessBlocks = new List<UselessBlock>();

    public AudioClip clearSound;
    public UI_GameOverPopup gameOverPopup;

    protected override void Awake()
    {
        base.Awake();
        gridArray = new GameObject[width, height];
        gridOrigin = gridOriginTransform.position;
        EventManager.Instance.onPlayerMove += OnPlayerMove;
    }

    public void ResetGrid()
    {
        foreach (var elem in gridArray)
        {
            Destroy(elem);
        }
    }

    /// <summary>
    /// 블록을 그리드에 등록 (UselessBlock도 포함)
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

            Debug.Log($"Registering block at {gridPos}: {block.name} (태그: {block.tag})");
            gridArray[gridPos.x, gridPos.y] = block;

            // 블록 상태 설정 (UselessBlock이든 일반 블록이든 동일하게)
            if (block.CompareTag("UselessBlock"))
            {
                // UselessBlock은 태그 유지
                Debug.Log($"UselessBlock 그리드 등록: {block.name}");
            }
            else
            {
                block.tag = "LockedBlock";
            }

            // 콜라이더 활성화
            Collider2D collider = block.GetComponent<Collider2D>();
            if (collider != null)
            {
                collider.enabled = true;
            }

            // 5칸째(최상단, Y=4)에 블록이 등록되면 게임오버 상태 체크
            if (gridPos.y == height - 1) // Y=4 (5칸째, 최상단)
            {
                CheckAndUpdateGameOverState();
            }
        }
        // 그리드 밖(6칸째 위, Y=5+)에 블록이 등록되려고 하면 게임오버 체크
        else if (gridPos.y >= height) // Y=5 이상 (6칸째 위)
        {
            Debug.Log($"6칸째 위에 블록 등록 시도: Grid({gridPos.x}, {gridPos.y}) - 게임오버 조건 체크");
            CheckAndUpdateGameOverState();
        }
    }

    /// <summary>
    /// 게임오버 상태 체크 및 업데이트 (4x5 그리드 기준 수정)
    /// 조건: 5칸째(Y=4, 최상단)에 블록이 있으면서 동시에 6칸째(Y=5+) 위에도 블록이 있어야 게임오버
    /// </summary>
    private void CheckAndUpdateGameOverState()
    {
        bool hasTopRowBlocks = false;  // 5칸째(Y=4, 최상단)에 블록 있는지
        bool hasOverflowBlocks = false; // 6칸째(Y=5+) 위에 블록 있는지

        // 1단계: 5칸째(최상단, Y=4) 체크
        for (int x = 0; x < width; x++)
        {
            if (IsLocked(x, height - 1)) // Y=4 체크 (5칸째, 최상단)
            {
                hasTopRowBlocks = true;
                Debug.Log($"5칸째(최상단) Grid({x}, {height - 1})에 블록 발견");
                break;
            }
        }

        // 2단계: 6칸째 위(Y=5 이상) 체크 (실제 월드에 존재하는 모든 블록 검사)
        GameObject[] allBlocks = FindObjectsOfType<GameObject>();
        foreach (GameObject obj in allBlocks)
        {
            if (obj.CompareTag("LockedBlock") || obj.CompareTag("UselessBlock") || obj.CompareTag("TetriminoBlock"))
            {
                Vector2Int gridPos = WorldToGrid(obj.transform.position);
                if (gridPos.y >= height) // Y=5 이상 (6칸째 위)
                {
                    hasOverflowBlocks = true;
                    Debug.Log($"6칸째 위 블록 발견: {obj.name} at Grid({gridPos.x}, {gridPos.y})");
                    break;
                }
            }
        }

        // 3단계: 게임오버 조건 판정 (둘 다 있어야 게임오버)
        bool shouldGameOver = hasTopRowBlocks && hasOverflowBlocks;

        // 게임오버 상태 업데이트
        IsGameOver = shouldGameOver;

        // 상태별 로그
        if (shouldGameOver)
        {
            // GameManager에 점수 설정
            //GameManager.Instance.SetScore(6000);
            CameraShake.Instance.ShakeCamera(2f,1f);
            GameManager.Instance.ChangeState(EGameState.Paused);
            // 게임오버 실행
            if (gameOverPopup != null)
            {
                // 프리팹이 할당된 경우
                gameOverPopup.gameObject.SetActive(true);
                gameOverPopup.Init();
            }
        }
        else if (hasTopRowBlocks && !hasOverflowBlocks)
        {
            Debug.LogWarning("5칸째(최상단)에 블록 있지만 6칸째 위는 비어있음 - 아직 게임 계속");
        }
        else if (!hasTopRowBlocks && hasOverflowBlocks)
        {
            Debug.LogWarning("6칸째 위에 블록 있지만 5칸째(최상단)는 비어있음 - 아직 게임 계속");
        }
        else
        {
            Debug.Log("5칸째(최상단)과 6칸째 위 모두 비어있음 - 게임 정상");
        }
    }

    /// <summary>
    /// 게임오버 발생 시 처리
    /// </summary>
    private void OnGameOver()
    {
        Debug.Log("=== 게임오버 발생 ===");
        //GameManager.Instance.ChangeState(EGameState.GameOver);
        // 추가 게임오버 처리 로직 여기에 구현
    }

    /// <summary>
    /// 해당 위치가 잠겨있는지 확인 (UselessBlock도 잠긴 것으로 인식)
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
    /// 라인 클리어용 잠김 체크 (UselessBlock 제외)
    /// </summary>
    private bool IsLockedForLineClear(int x, int y)
    {
        if (x < 0 || x > width - 1 || y < 0 || y > height - 1)
            return true;

        // 해당 위치에 UselessBlock이 있으면 라인 클리어 제외
        if (uselessBlockPositions[x, y])
        {
            return false;
        }

        GameObject block = gridArray[x, y];
        return block != null && !pendingDestroy.Contains(block);
    }

    /// <summary>
    /// 라인 체크 및 제거 (UselessBlock 제외)
    /// </summary>
    public void CheckAndClearLines()
    {
        List<int> linesToClear = new List<int>();

        // 모든 제거할 라인들을 먼저 찾기 (UselessBlock 제외)
        for (int y = 0; y < height; y++)
        {
            bool isLineFull = true;
            for (int x = 0; x < width; x++)
            {
                if (!IsLockedForLineClear(x, y))
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
            Debug.Log($"라인 클리어 발생: {linesToClear.Count}줄 (UselessBlock 제외)");
            OnLineClear?.Invoke();
            StartCoroutine(ClearLinesCoroutine(linesToClear));
        }
    }

    public void ClearLine(int i)
    {
        List<int> linesToClear = new List<int>();
        linesToClear.Add(i);
        StartCoroutine(ClearLinesCoroutine(linesToClear));
    }

    /// <summary>
    /// 라인 제거를 코루틴으로 처리 (UselessBlock 제외)
    /// </summary>
    private IEnumerator ClearLinesCoroutine(List<int> linesToClear)
    {
        Debug.Log("=== 라인 클리어 시작 ===");

        // 제거할 블록들을 먼저 비활성화 (UselessBlock 제외)
        foreach (int y in linesToClear)
        {
            for (int x = 0; x < width; x++)
            {
                GameObject block = gridArray[x, y];
                if (block != null && !block.CompareTag("UselessBlock"))
                {
                    pendingDestroy.Add(block);
                    block.SetActive(false);
                    gridArray[x, y] = null;
                    Debug.Log($"라인 클리어: Grid({x}, {y}) 블록 제거");
                }
                else if (block != null && block.CompareTag("UselessBlock"))
                {
                    Debug.Log($"UselessBlock 유지: Grid({x}, {y}) - {block.name}");
                }
            }
        }

        // 한 프레임 대기
        yield return null;

        // 파괴 예정 블록들 정리
        foreach (GameObject block in pendingDestroy)
        {
            if (block != null)
            {
                Destroy(block);
            }
        }
        pendingDestroy.Clear();

        // 위쪽 라인들 드롭 (UselessBlock도 함께)
        foreach (int clearedY in linesToClear)
        {
            DropLinesAbove(clearedY);
        }

        Debug.Log("=== 라인 클리어 완료 (UselessBlock은 영향 없음) ===");
        GameManager.Instance.AddScore(1000);
        SFXManager.Instance.PlaySFX(clearSound);
        CameraShake.Instance.ShakeCamera();

        // 라인 클리어 후 게임오버 상태 재체크
        CheckAndUpdateGameOverState();
    }

    /// <summary>
    /// 위쪽 라인들을 아래로 떨어뜨리기 (UselessBlock도 함께)
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

                    if (block.CompareTag("UselessBlock"))
                    {
                        Debug.Log($"UselessBlock 드롭: {block.name} to Grid({x}, {y - 1})");
                    }
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
            // UselessBlock이 아닌 경우만 태그 변경
            if (!block.CompareTag("UselessBlock"))
            {
                block.tag = "LockedBlock";
            }

            // 콜라이더 재활성화
            Collider2D collider = block.GetComponent<Collider2D>();
            if (collider != null)
            {
                collider.enabled = true;
            }
        }
    }

    /// <summary>
    /// 전체 그리드 이동 (UselessBlock도 자동으로 함께 이동)
    /// </summary>
    public void MoveEntireGrid(Vector3 moveDelta)
    {
        IsGridMoving = true;

        // 이미 이동한 오브젝트 추적
        HashSet<GameObject> movedObjects = new HashSet<GameObject>();

        // 1. 그리드 배열의 블록들 이동
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                GameObject block = gridArray[x, y];
                if (block != null && !pendingDestroy.Contains(block) && !movedObjects.Contains(block))
                {
                    block.transform.position += moveDelta;
                    movedObjects.Add(block);
                }
            }
        }
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

    /// <summary>
    /// UselessBlock 소환 (I자형, 맨 아래줄)
    /// </summary>
    public void SpawnUselessBlock()
    {
        if (uselessBlockPrefab == null)
        {
            Debug.LogError("uselessBlockPrefab이 설정되지 않았습니다!");
            return;
        }
        if (IsGridMoving)
        {
            Debug.LogWarning("그리드 이동 중에는 UselessBlock을 생성할 수 없습니다.");
            return;
        }

        // 맨 아래줄이 비어있는지 확인
        bool canSpawn = true;
        for (int x = 0; x < width; x++)
        {
            if (IsLocked(x, 0))
            {
                canSpawn = false;
                break;
            }
        }

        if (!canSpawn)
        {
            Debug.Log("맨 아래줄이 차서 기존 라인들을 위로 올립니다!");
            StartCoroutine(PushUpAndSpawnUselessBlockCoroutine());
        }
        else
        {
            Debug.Log("=== UselessBlock 소환 시작 ===");
            StartCoroutine(SpawnUselessBlockCoroutine());
        }
    }

    /// <summary>
    /// 기존 라인들을 위로 올리고 UselessBlock 소환
    /// </summary>
    private IEnumerator PushUpAndSpawnUselessBlockCoroutine()
    {
        Debug.Log("=== 라인 위로 올리기 시작 ===");

        // 5칸째(최상단, Y=4) 체크 (게임오버 방지)
        bool topRowHasBlocks = false;
        for (int x = 0; x < width; x++)
        {
            if (IsLocked(x, height - 1)) // Y=4 (5칸째, 최상단)
            {
                topRowHasBlocks = true;
                break;
            }
        }

        if (topRowHasBlocks)
        {
            Debug.LogWarning("5칸째(최상단)에 블록이 있어서 더 이상 올릴 수 없습니다! 게임오버!");
            // 게임오버 처리
            CheckAndUpdateGameOverState();
            yield break;
        }

        // 위에서부터 아래로 모든 라인을 한 칸씩 위로 이동
        for (int y = height - 1; y >= 1; y--) // 최상단(height-1)부터 Y=1까지
        {
            for (int x = 0; x < width; x++)
            {
                GameObject blockBelow = gridArray[x, y - 1]; // 아래쪽 블록

                if (blockBelow != null && !pendingDestroy.Contains(blockBelow))
                {
                    // 그리드 배열 업데이트
                    gridArray[x, y] = blockBelow;
                    gridArray[x, y - 1] = null;

                    // 실제 위치 이동
                    blockBelow.transform.position += Vector3.up * cellSize;

                    // UselessBlock 위치 마킹도 함께 이동
                    if (uselessBlockPositions[x, y - 1])
                    {
                        uselessBlockPositions[x, y] = true;
                        uselessBlockPositions[x, y - 1] = false;
                    }

                    Debug.Log($"블록 위로 이동: {blockBelow.name} from Grid({x}, {y - 1}) to Grid({x}, {y})");
                }
                else
                {
                    // 빈 공간은 그대로 빈 공간으로
                    gridArray[x, y] = null;
                    uselessBlockPositions[x, y] = false;
                }
            }
        }

        // 맨 아래줄(Y=0) 완전히 비우기
        for (int x = 0; x < width; x++)
        {
            gridArray[x, 0] = null;
            uselessBlockPositions[x, 0] = false;
        }

        Debug.Log("=== 라인 위로 올리기 완료 ===");

        // 한 프레임 대기
        yield return null;

        // 이제 맨 아래줄이 비었으므로 UselessBlock 소환
        Debug.Log("=== UselessBlock 소환 시작 (라인 올리기 후) ===");
        yield return StartCoroutine(SpawnUselessBlockCoroutine());
    }

    /// <summary>
    /// UselessBlock 소환 코루틴 (기존과 동일)
    /// </summary>
    private IEnumerator SpawnUselessBlockCoroutine()
    {
        if (showUselessBlockWarning)
        {
            Debug.Log("UselessBlock이 소환됩니다!");
            yield return new WaitForSeconds(0.5f);
        }

        gridOrigin = gridOriginTransform.position;

        // I자형 블록 정확한 중앙 위치
        Vector3 spawnPosition = gridOrigin + new Vector3(2f * cellSize, 0.5f * cellSize, 0f);
        Debug.Log("UselessBlock 포지션 " + spawnPosition);

        // UselessBlock 생성
        GameObject uselessBlockObj = Instantiate(uselessBlockPrefab, spawnPosition, Quaternion.identity);
        uselessBlockObj.name = $"UselessBlock_I_{Time.time:F1}";

        // UselessBlock 컴포넌트 가져오기
        UselessBlock uselessBlockComponent = uselessBlockObj.GetComponent<UselessBlock>();
        if (uselessBlockComponent == null)
        {
            uselessBlockComponent = uselessBlockObj.AddComponent<UselessBlock>();
        }

        // 즉시 그리드 등록 완료
        for (int x = 0; x < width; x++)
        {
            Vector2Int gridPos = new Vector2Int(x, 0);
            RegisterBlock(gridPos, uselessBlockObj);
            uselessBlockPositions[x, 0] = true;
        }

        // 그리드 등록 완료 후 관리 목록에 추가
        activeUselessBlocks.Add(uselessBlockComponent);

        // 한 프레임 대기로 확실한 등록 보장
        yield return null;

        Debug.Log($"UselessBlock 생성 및 그리드 등록 완료: {uselessBlockObj.name}");
        Debug.Log("=== UselessBlock 소환 완료 ===");

        // 라인 올리기 후 게임오버 상태 체크
        CheckAndUpdateGameOverState();
    }

    /// <summary>
    /// UselessBlock이 삭제될 때 그리드에서 제거
    /// </summary>
    public void RemoveUselessBlockFromGrid(UselessBlock uselessBlock)
    {
        if (uselessBlock == null) return;

        GameObject uselessBlockObj = uselessBlock.gameObject;

        // UselessBlock 위치 해제
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (gridArray[x, y] == uselessBlockObj)
                {
                    gridArray[x, y] = null;
                    uselessBlockPositions[x, y] = false; // 위치 마킹 해제
                }
            }
        }

        // 관리 목록에서 제거
        activeUselessBlocks.Remove(uselessBlock);

        Debug.Log($"UselessBlock 완전 제거: {uselessBlock.name}");

        // 라인 클리어 체크 (빈 공간이 생겼을 수 있음)
        CheckAndClearLines();

        // 게임오버 상태 재체크
        CheckAndUpdateGameOverState();
    }

    /// <summary>
    /// 모든 UselessBlock 즉시 제거
    /// </summary>
    public void RemoveAllUselessBlocks()
    {
        Debug.Log("=== 모든 UselessBlock 즉시 제거 ===");

        List<UselessBlock> blocksToRemove = new List<UselessBlock>(activeUselessBlocks);
        foreach (UselessBlock uselessBlock in blocksToRemove)
        {
            if (uselessBlock != null)
            {
                uselessBlock.ForceDestroy();
            }
        }

        activeUselessBlocks.Clear();
    }

    /// <summary>
    /// 활성 UselessBlock 개수 반환
    /// </summary>
    public int GetUselessBlockCount()
    {
        return activeUselessBlocks.Count;
    }

    /// <summary>
    /// 모든 블록 제거
    /// </summary>
    public void ClearAll()
    {
        // UselessBlock 먼저 정리
        RemoveAllUselessBlocks();

        // 모든 그리드 블록 제거
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

        // 게임오버 상태 리셋
        IsGameOver = false;
        Debug.Log("모든 블록 제거 - 게임 상태 완전 리셋");
    }

    /// <summary>
    /// 개선된 WorldToGrid (게임오버 조건 고려)
    /// </summary>
    public Vector2Int WorldToGrid(Vector3 worldPos)
    {
        Vector3 origin = gridOriginTransform.position;
        Vector3 local = worldPos - origin;

        int x = Mathf.FloorToInt(local.x / cellSize);
        int y = Mathf.FloorToInt(local.y / cellSize);

        // 범위 체크는 하지만 클램핑하지 않음 (게임오버 감지를 위해)
        if (x < 0 || x >= width || y >= height)
        {
            if (y >= height) // Y=5 이상 (6칸째 위)
            {
                Debug.LogWarning($"WorldToGrid: 블록이 6칸째 위에 위치 - Grid({x}, {y})");
                // 게임오버 조건이므로 실제 값 반환
                return new Vector2Int(Mathf.Clamp(x, 0, width - 1), y);
            }
            else if (y < 0)
            {
                Debug.LogWarning($"WorldToGrid: 블록이 그리드 아래에 위치 - Grid({x}, {y})");
            }
        }

        // X는 클램핑, Y는 음수만 클램핑 (위쪽 오버플로우는 그대로 반환)
        x = Mathf.Clamp(x, 0, width - 1);
        y = Mathf.Max(y, 0); // 아래쪽만 클램핑

        return new Vector2Int(x, y);
    }

    private bool IsInsideGrid(Vector2Int pos)
    {
        return pos.x >= 0 && pos.x < width && pos.y >= 0 && pos.y < height;
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

    #region === 디버그 및 테스트 메서드들 ===

    /// <summary>
    /// 게임오버 조건 상세 체크 (디버깅용) - 4x5 그리드 기준 수정
    /// </summary>
    [ContextMenu("Check GameOver Condition Detail")]
    public void CheckGameOverConditionDetail()
    {
        Debug.Log("=== 게임오버 조건 상세 체크 (4x5 그리드) ===");

        // 5칸째(최상단, Y=4) 상태 체크
        bool hasTopRowBlocks = false;
        int topRowBlockCount = 0;

        Debug.Log("5칸째(Y=4) 최상단 상태:");
        for (int x = 0; x < width; x++)
        {
            if (IsLocked(x, height - 1)) // Y=4 체크 (5칸째, 최상단)
            {
                hasTopRowBlocks = true;
                topRowBlockCount++;
                GameObject block = gridArray[x, height - 1];
                string blockType = block != null && block.CompareTag("UselessBlock") ? "(UselessBlock)" : "(일반블록)";
                Debug.Log($"  Grid({x}, 4): ■ {blockType}");
            }
            else
            {
                Debug.Log($"  Grid({x}, 4): □ (비어있음)");
            }
        }

        // 6칸째 위(Y=5 이상) 상태 체크
        bool hasOverflowBlocks = false;
        int overflowBlockCount = 0;

        Debug.Log("6칸째 위(Y=5+) 오버플로우 영역:");
        string[] targetTags = { "LockedBlock", "UselessBlock", "TetriminoBlock" };

        foreach (string tag in targetTags)
        {
            GameObject[] blocks = GameObject.FindGameObjectsWithTag(tag);
            foreach (GameObject block in blocks)
            {
                Vector2Int gridPos = WorldToGrid(block.transform.position);
                if (gridPos.y >= height) // Y=5 이상 (6칸째 위)
                {
                    hasOverflowBlocks = true;
                    overflowBlockCount++;
                    Debug.Log($"  오버플로우 블록: {block.name} at Grid({gridPos.x}, {gridPos.y}) [{tag}]");
                }
            }
        }

        if (!hasOverflowBlocks)
        {
            Debug.Log("  6칸째 위에는 블록 없음");
        }

        // 최종 판정
        bool shouldGameOver = hasTopRowBlocks && hasOverflowBlocks;

        Debug.Log("=== 최종 판정 (4x5 그리드) ===");
        Debug.Log($"5칸째(최상단)에 블록 있음: {hasTopRowBlocks} ({topRowBlockCount}개)");
        Debug.Log($"6칸째 위에 블록 있음: {hasOverflowBlocks} ({overflowBlockCount}개)");
        Debug.Log($"게임오버 조건 충족: {shouldGameOver}");
        Debug.Log($"현재 게임오버 상태: {IsGameOver}");

        if (shouldGameOver)
        {
            Debug.LogError("▶ 게임오버! (5칸째 AND 6칸째 위 둘 다 블록 있음)");
        }
        else if (hasTopRowBlocks)
        {
            Debug.LogWarning("▶ 위험! 5칸째(최상단)에 블록 있음 (6칸째 위에만 블록 오면 게임오버)");
        }
        else if (hasOverflowBlocks)
        {
            Debug.LogWarning("▶ 6칸째 위에 블록 있지만 5칸째(최상단)는 비어있어서 아직 안전");
        }
        else
        {
            Debug.Log("▶ 안전! 5칸째(최상단)과 6칸째 위 모두 비어있음");
        }
    }

    /// <summary>
    /// 최상단 상태 로그 (4x5 그리드 기준)
    /// </summary>
    [ContextMenu("Log Top Row Status")]
    public void LogTopRowStatus()
    {
        Debug.Log("=== 게임오버 조건 체크 (4x5 그리드) ===");

        // 조건 설명
        Debug.Log("게임오버 조건: 5칸째(Y=4, 최상단)에 블록이 있으면서 AND 6칸째(Y=5+) 위에도 블록이 있어야 함");

        // 상세 체크 실행
        CheckGameOverConditionDetail();
    }

    /// <summary>
    /// 디버그 테스트 메서드들
    /// </summary>
    private void Test()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            int totalCount = 0;
            int uselessCount = 0;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (IsLocked(x, y))
                    {
                        totalCount++;
                        GameObject block = gridArray[x, y];
                        if (block != null && block.CompareTag("UselessBlock"))
                        {
                            uselessCount++;
                        }
                    }
                }
            }
            Debug.Log($"전체 Locked 블록: {totalCount}개 (UselessBlock: {uselessCount}개)");
        }

        if (Input.GetKeyDown(KeyCode.G))
        {
            CheckAndClearLines();
        }

        if (Input.GetKeyDown(KeyCode.U))
        {
            Debug.Log("=== 수동 UselessBlock 소환 ===");
            SpawnUselessBlock();
        }

        if (Input.GetKeyDown(KeyCode.I))
        {
            Debug.Log("=== 수동 UselessBlock 제거 ===");
            RemoveAllUselessBlocks();
        }

        if (Input.GetKeyDown(KeyCode.O))
        {
            Debug.Log($"=== UselessBlock 상태 ===");
            Debug.Log($"활성 UselessBlock: {GetUselessBlockCount()}개");

            foreach (UselessBlock uselessBlock in activeUselessBlocks)
            {
                if (uselessBlock != null)
                {
                    Vector2Int pos = WorldToGrid(uselessBlock.transform.position);
                    Debug.Log($"  - {uselessBlock.name} at Grid({pos.x}, {pos.y})");
                }
            }
        }
    }

    [ContextMenu("Force Check Game Over")]
    public void ForceCheckGameOver()
    {
        CheckAndUpdateGameOverState();
    }

    [ContextMenu("Test Spawn UselessBlock")]
    public void TestSpawnUselessBlock()
    {
        SpawnUselessBlock();
    }

    [ContextMenu("Remove All UselessBlocks")]
    public void TestRemoveAllUselessBlocks()
    {
        RemoveAllUselessBlocks();
    }

    [ContextMenu("Show Grid State")]
    public void ShowGridState()
    {
        Debug.Log("=== 전체 그리드 상태 (4x5) ===");
        for (int y = height - 1; y >= 0; y--)
        {
            string row = $"Y={y}: ";
            for (int x = 0; x < width; x++)
            {
                GameObject block = gridArray[x, y];
                if (block == null)
                {
                    row += "□ ";
                }
                else if (block.CompareTag("UselessBlock"))
                {
                    row += "U "; // UselessBlock
                }
                else
                {
                    row += "■ "; // 일반 블록
                }
            }
            Debug.Log(row);
        }

        Debug.Log($"활성 UselessBlock: {GetUselessBlockCount()}개");
        Debug.Log("그리드 설명: Y=0(맨아래) ~ Y=4(최상단), 6칸째 위는 Y=5+");
    }

    #endregion

    public void Update()
    {
        Test();
    }

    private void OnDestroy()
    {
        // 모든 UselessBlock 정리
        RemoveAllUselessBlocks();
    }
}