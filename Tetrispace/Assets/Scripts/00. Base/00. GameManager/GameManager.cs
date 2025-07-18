
using System;
using UnityEngine;

public enum EGameState
{
    Title,      // 타이틀 화면
    Playing,    // 게임 진행 중
    Paused,     // 일시정지
    GameOver,   // 게임 오버
    Victory,    // 게임 클리어
    Finish      // 결과 화면
}

public class GameManager : Singleton<GameManager>
{
    public EGameState CurrentGameState { get; private set; } = EGameState.Title;

    [Header("=== 점수 시스템 ===")]
    [SerializeField] private int currentScore = 0;
    [SerializeField] private int highScore = 0;
    public int CurrentScore => currentScore;
    public int HighScore => highScore;

    // === 이벤트 시스템 ===
    public static event Action<EGameState> OnGameStateChanged;
    public static event Action<int> OnScoreChanged;
    public static event Action OnGameStart;
    public static event Action OnGameOver;
    public static event Action OnGameClear;

    protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(gameObject);

        // 최고 점수 불러오기
        LoadHighScore();
    }

    #region === 게임 상태 관리 ===

    /// <summary>
    /// 게임 상태를 변경합니다
    /// </summary>
    /// <param name="newState">변경할 게임 상태</param>
    public void ChangeState(EGameState newState)
    {
        if (CurrentGameState == newState) return;

        CurrentGameState = newState;
        OnGameStateChanged?.Invoke(newState);

        // 상태별 처리
        switch (newState)
        {
            case EGameState.Title:
                Time.timeScale = 0f;
                break;

            case EGameState.Playing:
                Time.timeScale = 1f;
                break;

            case EGameState.Paused:
                Time.timeScale = 0f;
                break;

            case EGameState.GameOver:
                Time.timeScale = 0f;
                OnGameOver?.Invoke();
                SaveHighScore();
                break;

            case EGameState.Victory:
                Time.timeScale = 0f;
                OnGameClear?.Invoke();
                SaveHighScore();
                break;

            case EGameState.Finish:
                Time.timeScale = 0f;
                break;
        }
    }

    /// <summary>
    /// 게임을 시작합니다
    /// </summary>
    public void StartGame()
    {
        ResetScore();
        ChangeState(EGameState.Playing);
        OnGameStart?.Invoke();
    }

    /// <summary>
    /// 게임을 재시작합니다
    /// </summary>
    public void RestartGame()
    {
        StartGame();
    }

    /// <summary>
    /// 게임을 일시정지/재개합니다
    /// </summary>
    public void TogglePause()
    {
        if (CurrentGameState == EGameState.Playing)
        {
            ChangeState(EGameState.Paused);
        }
        else if (CurrentGameState == EGameState.Paused)
        {
            ChangeState(EGameState.Playing);
        }
    }

    /// <summary>
    /// 게임 오버 처리
    /// </summary>
    public void GameOver()
    {
        ChangeState(EGameState.GameOver);
    }

    /// <summary>
    /// 게임 클리어 처리
    /// </summary>
    public void GameClear()
    {
        ChangeState(EGameState.Victory);
    }

    #endregion

    #region === 점수 시스템 ===

    /// <summary>
    /// 점수를 추가합니다
    /// </summary>
    /// <param name="points">추가할 점수</param>
    public void AddScore(int points)
    {
        currentScore += points;
        OnScoreChanged?.Invoke(currentScore);

        // 최고 점수 갱신
        if (currentScore > highScore)
        {
            highScore = currentScore;
        }
    }

    /// <summary>
    /// 점수를 설정합니다
    /// </summary>
    /// <param name="score">설정할 점수</param>
    public void SetScore(int score)
    {
        currentScore = score;
        OnScoreChanged?.Invoke(currentScore);
    }

    /// <summary>
    /// 점수를 초기화합니다
    /// </summary>
    public void ResetScore()
    {
        currentScore = 0;
        OnScoreChanged?.Invoke(currentScore);
    }

    #endregion

    #region === 씬 관리 ===

    /// <summary>
    /// 타이틀 화면으로 이동
    /// </summary>
    public void GoToTitle()
    {
        ChangeState(EGameState.Title);
        SceneLoader.LoadScene(SceneLoader.SceneName.Title);
    }

    /// <summary>
    /// 메인 게임으로 이동
    /// </summary>
    public void GoToMain()
    {
        SceneLoader.LoadScene(SceneLoader.SceneName.Main);
    }

    /// <summary>
    /// 결과 화면으로 이동
    /// </summary>
    public void GoToResult()
    {
        ChangeState(EGameState.Finish);
        SceneLoader.LoadScene(SceneLoader.SceneName.Result);
    }

    #endregion

    #region === 저장/로드 ===

    /// <summary>
    /// 최고 점수를 저장합니다
    /// </summary>
    private void SaveHighScore()
    {
        PlayerPrefs.SetInt("HighScore", highScore);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// 최고 점수를 불러옵니다
    /// </summary>
    private void LoadHighScore()
    {
        highScore = PlayerPrefs.GetInt("HighScore", 0);
    }

    /// <summary>
    /// 모든 저장 데이터를 삭제합니다
    /// </summary>
    public void ResetAllData()
    {
        PlayerPrefs.DeleteAll();
        highScore = 0;
        ResetScore();
    }

    #endregion

    #region === 유틸리티 ===

    /// <summary>
    /// 게임이 진행 중인지 확인
    /// </summary>
    public bool IsGamePlaying()
    {
        return CurrentGameState == EGameState.Playing;
    }

    /// <summary>
    /// 게임이 일시정지 중인지 확인
    /// </summary>
    public bool IsGamePaused()
    {
        return CurrentGameState == EGameState.Paused;
    }

    /// <summary>
    /// 게임이 종료되었는지 확인
    /// </summary>
    public bool IsGameEnded()
    {
        return CurrentGameState == EGameState.GameOver ||
               CurrentGameState == EGameState.Victory ||
               CurrentGameState == EGameState.Finish;
    }

    #endregion

    #region === 인풋 처리 ===

    private void Update()
    {
        // ESC 키로 일시정지 토글 (게임 진행 중에만)
        if (Input.GetKeyDown(KeyCode.Escape) && IsGamePlaying())
        {
            TogglePause();
        }
    }

    #endregion

    #region === 디버그 (빌드 시 제거 가능) ===

    private void OnGUI()
    {
        if (!Application.isEditor) return;

        GUILayout.BeginArea(new Rect(10, 10, 200, 150));
        GUILayout.Label($"상태: {CurrentGameState}");
        GUILayout.Label($"점수: {currentScore}");
        GUILayout.Label($"최고점수: {highScore}");

        if (GUILayout.Button("게임 시작"))
            StartGame();

        if (GUILayout.Button("점수 +100"))
            AddScore(100);

        if (GUILayout.Button("게임 오버"))
            GameOver();

        GUILayout.EndArea();
    }

    #endregion
}