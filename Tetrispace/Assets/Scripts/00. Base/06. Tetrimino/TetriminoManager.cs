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
    [Header("=== 테트리미노 설정 ===")]
    [SerializeField] private GameObject tetrominoPrefab;     // 테트리미노 프리팹
    [SerializeField] private Transform spawnPoint;          // 생성 위치
    [SerializeField] private float initialFallSpeed = 1f;   // 초기 하강 속도
    [SerializeField] private float speedIncreaseRate = 0.1f; // 속도 증가율

    [Header("=== 참조 ===")]
    [SerializeField] private PlayerGrid playerGrid;        // 플레이어 그리드
    [SerializeField] private Transform nextBlockPreview;   // 다음 블록 미리보기 위치

    [Header("=== 게임 상태 ===")]
    [SerializeField] private bool autoSpawn = false;        // 자동 생성 여부
    [SerializeField] private float spawnDelay = 0.5f;      // 생성 지연 시간
    [SerializeField] private int maxLevel = 20;            // 최대 레벨

    // === 상태 변수 ===
    private Tetrimino currentTetrimino;                    // 현재 테트리미노
    private Tetrimino.TetrominoType nextTetrominoType;     // 다음 블록 타입
    private Queue<Tetrimino.TetrominoType> tetrominoQueue; // 블록 대기열
    private float currentFallSpeed;                        // 현재 하강 속도
    private int currentLevel = 1;                          // 현재 레벨
    private int linesCleared = 0;                          // 클리어된 라인 수
    private bool isGameOver = false;                       // 게임 오버 상태

    // === 테트리미노 타입 배열 (6가지) ===
    private readonly Tetrimino.TetrominoType[] allTetrominoTypes = {
        Tetrimino.TetrominoType.O,
        Tetrimino.TetrominoType.I,
        Tetrimino.TetrominoType.T,
        Tetrimino.TetrominoType.L,
        Tetrimino.TetrominoType.J,
        Tetrimino.TetrominoType.S
    };

    // === 이벤트 시스템 ===
    public static System.Action<int> OnLevelUp;
    public static System.Action OnGameOver;

    #region === 초기화 ===

    protected override void Awake()
    {
        base.Awake();

        // 초기화
        tetrominoQueue = new Queue<Tetrimino.TetrominoType>();
        currentFallSpeed = initialFallSpeed;

        // 큐 초기화
        FillTetrominoQueue();
    }

    void Start()
    {
        InitializeManager();
    }

    /// <summary>
    /// 매니저 초기화
    /// </summary>
    void InitializeManager()
    {
        // PlayerGrid 자동 찾기
        if (playerGrid == null)
            playerGrid = FindFirstObjectByType<PlayerGrid>();
        //
        if (playerGrid == null)
        {
            Debug.LogError("PlayerGrid를 찾을 수 없습니다!");
            return;
        }

        // 생성 위치 설정
        if (spawnPoint == null)
        {
            GameObject spawnPointObj = new GameObject("TetrominoSpawnPoint");
            spawnPoint = spawnPointObj.transform;
            spawnPoint.SetParent(transform);
        }

        // 게임 시작
        if (autoSpawn)
        {
            StartCoroutine(StartGameSequence());
        }

        Debug.Log("TetrominoManager 초기화 완료");
    }

    /// <summary>
    /// 게임 시작 시퀀스
    /// </summary>
    IEnumerator StartGameSequence()
    {
        yield return new WaitForSeconds(0.1f);

        // 첫 번째 테트리미노 생성
        SpawnNextTetrimino();

        // 다음 블록 미리보기 업데이트
        UpdateNextBlockPreview();
    }

    #endregion

    #region === 테트리미노 생성 ===

    /// <summary>
    /// 다음 테트리미노 생성
    /// </summary>
    public void SpawnNextTetrimino()
    {
        if (isGameOver) return;

        // 게임 오버 체크
        if (CheckGameOver())
        {
            TriggerGameOver();
            return;
        }

        // 다음 블록 타입 가져오기
        Tetrimino.TetrominoType typeToSpawn = GetNextTetrominoType();

        // 테트리미노 생성
        GameObject tetrominoObj = Instantiate(tetrominoPrefab, spawnPoint.position, Quaternion.identity);
        currentTetrimino = tetrominoObj.GetComponent<Tetrimino>();

        if (currentTetrimino == null)
        {
            Debug.LogError("생성된 오브젝트에 Tetrimino 컴포넌트가 없습니다!");
            return;
        }

        // 테트리미노 설정
        currentTetrimino.SetTetrominoType(typeToSpawn);
        currentTetrimino.SetFallSpeed(currentFallSpeed);

        // 이벤트 발생
        EventManager.Instance.TetrominoSpawned(currentTetrimino.GetTetrominoType());

        // 다음 블록 미리보기 업데이트
        UpdateNextBlockPreview();

        Debug.Log($"테트리미노 생성: {typeToSpawn}, 속도: {currentFallSpeed}");
    }

    /// <summary>
    /// 테트리미노 큐에서 다음 타입 가져오기
    /// </summary>
    /// <returns>다음 블록 타입</returns>
    Tetrimino.TetrominoType GetNextTetrominoType()
    {
        // 큐가 비어있으면 새로 채우기
        if (tetrominoQueue.Count == 0)
        {
            FillTetrominoQueue();
        }

        return tetrominoQueue.Dequeue();
    }

    /// <summary>
    /// 테트리미노 큐 채우기 (7-bag 시스템)
    /// </summary>
    void FillTetrominoQueue()
    {
        // 모든 타입을 리스트에 추가
        List<Tetrimino.TetrominoType> tempList = new List<Tetrimino.TetrominoType>(allTetrominoTypes);

        // 셔플
        for (int i = 0; i < tempList.Count; i++)
        {
            int randomIndex = Random.Range(i, tempList.Count);
            var temp = tempList[i];
            tempList[i] = tempList[randomIndex];
            tempList[randomIndex] = temp;
        }

        // 큐에 추가
        foreach (var type in tempList)
        {
            tetrominoQueue.Enqueue(type);
        }

        Debug.Log($"테트리미노 큐 채움: {tetrominoQueue.Count}개");
    }

    #endregion

    #region === 테트리미노 콜백 ===

    /// <summary>
    /// 테트리미노가 고정되었을 때 호출
    /// </summary>
    /// <param name="tetrimino">고정된 테트리미노</param>
    public void OnTetrominoLocked(Tetrimino tetrimino)
    {
        if (tetrimino == currentTetrimino)
        {
            currentTetrimino = null;
        }

        // 이벤트 발생
        EventManager.Instance.TetrominoLocked(tetrimino.GetTetrominoType(), tetrimino.GetGridPosition());
        //OnTetrominoLocked?.Invoke(tetrimino);

        // 라인 클리어 체크는 PlayerGrid에서 처리

        // 다음 테트리미노 생성 (지연)
        if (autoSpawn && !isGameOver)
        {
            StartCoroutine(SpawnNextTetrominoDelayed());
        }
    }

    /// <summary>
    /// 지연된 테트리미노 생성
    /// </summary>
    IEnumerator SpawnNextTetrominoDelayed()
    {
        yield return new WaitForSeconds(spawnDelay);
        SpawnNextTetrimino();
    }

    /// <summary>
    /// 라인 클리어 완료 콜백
    /// </summary>
    /// <param name="clearedLines">클리어된 라인 수</param>
    public void OnLinesCleared(int clearedLines)
    {
        linesCleared += clearedLines;

        // 레벨업 체크 (10라인마다)
        int newLevel = (linesCleared / 10) + 1;
        if (newLevel > currentLevel && newLevel <= maxLevel)
        {
            currentLevel = newLevel;
            UpdateFallSpeed();
            OnLevelUp?.Invoke(currentLevel);

            Debug.Log($"레벨업! 현재 레벨: {currentLevel}");
        }

        // 점수 추가
        if (GameManager.Instance != null)
        {
            int score = clearedLines * 100 * currentLevel;
            GameManager.Instance.AddScore(score);
        }
    }

    #endregion

    #region === 게임 상태 관리 ===

    /// <summary>
    /// 게임 오버 체크
    /// </summary>
    /// <returns>게임 오버 여부</returns>
    bool CheckGameOver()
    {
        // 상단 2줄에 블록이 있는지 확인
        for (int x = 0; x < playerGrid.gridWidth; x++)
        {
            if (playerGrid.IsGridOccupied(x, playerGrid.gridHeight - 1) ||
                playerGrid.IsGridOccupied(x, playerGrid.gridHeight - 2))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// 게임 오버 처리
    /// </summary>
    void TriggerGameOver()
    {
        isGameOver = true;

        // 현재 테트리미노 정지
        if (currentTetrimino != null)
        {
            currentTetrimino.SetCanMove(false);
        }

        // 이벤트 발생
        OnGameOver?.Invoke();

        // 게임 매니저에 알림
        if (GameManager.Instance != null)
        {
            GameManager.Instance.GameOver();
        }

        Debug.Log("게임 오버!");
    }

    /// <summary>
    /// 하강 속도 업데이트
    /// </summary>
    void UpdateFallSpeed()
    {
        currentFallSpeed = Mathf.Max(0.1f, initialFallSpeed - (currentLevel - 1) * speedIncreaseRate);

        // 현재 테트리미노 속도도 업데이트
        if (currentTetrimino != null)
        {
            currentTetrimino.SetFallSpeed(currentFallSpeed);
        }
    }

    #endregion

    #region === 미리보기 시스템 ===

    /// <summary>
    /// 다음 블록 미리보기 업데이트
    /// </summary>
    void UpdateNextBlockPreview()
    {
        if (nextBlockPreview == null) return;

        // 기존 미리보기 제거
        foreach (Transform child in nextBlockPreview)
        {
            Destroy(child.gameObject);
        }

        // 다음 블록 타입 가져오기
        if (tetrominoQueue.Count > 0)
        {
            nextTetrominoType = tetrominoQueue.Peek();
            // 여기서 미리보기 블록 생성 로직을 구현할 수 있음
        }
    }

    #endregion

    #region === 공개 메서드 ===

    /// <summary>
    /// 게임 시작
    /// </summary>
    public void StartGame()
    {
        isGameOver = false;
        currentLevel = 1;
        linesCleared = 0;
        currentFallSpeed = initialFallSpeed;

        // 큐 초기화
        tetrominoQueue.Clear();
        FillTetrominoQueue();

        // 첫 번째 테트리미노 생성
        SpawnNextTetrimino();

        Debug.Log("게임 시작!");
    }

    /// <summary>
    /// 게임 재시작
    /// </summary>
    public void RestartGame()
    {
        // 현재 테트리미노 제거
        if (currentTetrimino != null)
        {
            Destroy(currentTetrimino.gameObject);
            currentTetrimino = null;
        }

        // 그리드 초기화
        if (playerGrid != null)
        {
            // PlayerGrid의 초기화 메서드 필요
        }

        // 게임 재시작
        StartGame();
    }

    /// <summary>
    /// 게임 일시정지
    /// </summary>
    public void PauseGame()
    {
        if (currentTetrimino != null)
        {
            currentTetrimino.SetCanMove(false);
        }
    }

    /// <summary>
    /// 게임 재개
    /// </summary>
    public void ResumeGame()
    {
        if (currentTetrimino != null && !isGameOver)
        {
            currentTetrimino.SetCanMove(true);
        }
    }

    /// <summary>
    /// 수동 테트리미노 생성
    /// </summary>
    /// <param name="type">생성할 블록 타입</param>
    public void SpawnTetrimino(Tetrimino.TetrominoType type)
    {
        if (isGameOver) return;

        // 현재 테트리미노가 있으면 제거
        if (currentTetrimino != null)
        {
            Destroy(currentTetrimino.gameObject);
        }

        // 지정된 타입으로 생성
        GameObject tetrominoObj = Instantiate(tetrominoPrefab, spawnPoint.position, Quaternion.identity);
        currentTetrimino = tetrominoObj.GetComponent<Tetrimino>();
        currentTetrimino.SetTetrominoType(type);
        currentTetrimino.SetFallSpeed(currentFallSpeed);

        EventManager.Instance.TetrominoSpawned(currentTetrimino.GetTetrominoType());
    }

    #endregion

    #region === 속성 ===

    public int CurrentLevel => currentLevel;
    public int LinesCleared => linesCleared;
    public float CurrentFallSpeed => currentFallSpeed;
    public bool IsGameOver => isGameOver;
    public Tetrimino CurrentTetrimino => currentTetrimino;
    public Tetrimino.TetrominoType NextTetrominoType => nextTetrominoType;

    #endregion

    #region === 디버그 ===

    /// <summary>
    /// 매니저 정보 출력
    /// </summary>
    [ContextMenu("매니저 정보")]
    void PrintManagerInfo()
    {
        Debug.Log($"=== TetrominoManager 정보 ===");
        Debug.Log($"현재 레벨: {currentLevel}");
        Debug.Log($"클리어된 라인: {linesCleared}");
        Debug.Log($"하강 속도: {currentFallSpeed}");
        Debug.Log($"게임 오버: {isGameOver}");
        Debug.Log($"큐 크기: {tetrominoQueue.Count}");
        Debug.Log($"현재 테트리미노: {(currentTetrimino != null ? currentTetrimino.GetTetrominoType().ToString() : "없음")}");
    }

    /// <summary>
    /// 테스트용 레벨업
    /// </summary>
    [ContextMenu("테스트 레벨업")]
    void TestLevelUp()
    {
        OnLinesCleared(10);
    }

    #endregion
}