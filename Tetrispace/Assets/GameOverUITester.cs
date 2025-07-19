// GameOverTester.cs - 테스트용 스크립트
using UnityEngine;

/// <summary>
/// 게임오버 UI 테스트용 스크립트
/// 다양한 점수 상황을 빠르게 테스트할 수 있음
/// </summary>
public class GameOverTester : MonoBehaviour
{
    [Header("=== 테스트 설정 ===")]
    [SerializeField] private UI_GameOverPopup gameOverPopup;
    [SerializeField] private int[] testScores = { 1000, 5000, 10000, 15000 };
    [SerializeField] private int currentTestIndex = 0;

    [Header("=== 디버그 정보 ===")]
    [SerializeField] private bool showDebugInfo = true;

    void Update()
    {
        HandleTestInputs();
    }

    /// <summary>
    /// 테스트 입력 처리
    /// </summary>
    private void HandleTestInputs()
    {
        // G키: 게임오버 테스트 (현재 테스트 점수로)
        if (Input.GetKeyDown(KeyCode.G))
        {
            TestGameOverWithScore(testScores[currentTestIndex]);
        }

        // H키: 하이스코어보다 높은 점수로 테스트 (신기록)
        if (Input.GetKeyDown(KeyCode.H))
        {
            int highScore = GameManager.Instance.HighScore;
            TestGameOverWithScore(highScore + 1000);
        }

        // L키: 하이스코어보다 낮은 점수로 테스트
        if (Input.GetKeyDown(KeyCode.L))
        {
            int highScore = GameManager.Instance.HighScore;
            TestGameOverWithScore(Mathf.Max(0, highScore - 500));
        }

        // 숫자키 1-4: 미리 설정된 점수로 테스트
        for (int i = 0; i < 4; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                if (i < testScores.Length)
                {
                    TestGameOverWithScore(testScores[i]);
                }
            }
        }

        // R키: 하이스코어 리셋
        if (Input.GetKeyDown(KeyCode.R))
        {
            ResetHighScore();
        }

        // T키: 테스트 점수 순환
        if (Input.GetKeyDown(KeyCode.T))
        {
            CycleTestScore();
        }
    }

    /// <summary>
    /// 특정 점수로 게임오버 테스트
    /// </summary>
    private void TestGameOverWithScore(int score)
    {
        Debug.Log($"=== 게임오버 테스트 시작 ===");
        Debug.Log($"테스트 점수: {score}");
        Debug.Log($"현재 하이스코어: {GameManager.Instance.HighScore}");
        Debug.Log($"신기록 여부: {score > GameManager.Instance.HighScore}");

        // GameManager에 점수 설정
        GameManager.Instance.SetScore(score);

        // 게임오버 실행
        if (gameOverPopup != null)
        {
            // 프리팹이 할당된 경우
            gameOverPopup.gameObject.SetActive(true);
            gameOverPopup.Init();
        }
        else
        {
            // GameManager를 통한 일반적인 게임오버
            GameManager.Instance.GameOver();
        }
    }

    /// <summary>
    /// 하이스코어 리셋
    /// </summary>
    private void ResetHighScore()
    {
        GameManager.Instance.ResetAllData();
        Debug.Log("하이스코어가 리셋되었습니다!");
    }

    /// <summary>
    /// 테스트 점수 순환
    /// </summary>
    private void CycleTestScore()
    {
        currentTestIndex = (currentTestIndex + 1) % testScores.Length;
        Debug.Log($"테스트 점수 변경: {testScores[currentTestIndex]}");
    }

    /// <summary>
    /// GUI 디버그 정보 표시
    /// </summary>
    private void OnGUI()
    {
        if (!showDebugInfo) return;

        GUILayout.BeginArea(new Rect(10, 10, 400, 300));
        GUILayout.Label("=== 게임오버 UI 테스트 ===");
        GUILayout.Space(10);

        GUILayout.Label($"현재 점수: {GameManager.Instance.CurrentScore}");
        GUILayout.Label($"최고 점수: {GameManager.Instance.HighScore}");
        GUILayout.Label($"테스트 점수: {testScores[currentTestIndex]}");
        GUILayout.Space(10);

        GUILayout.Label("=== 테스트 키 ===");
        GUILayout.Label("G: 게임오버 (현재 테스트 점수)");
        GUILayout.Label("H: 게임오버 (신기록)");
        GUILayout.Label("L: 게임오버 (낮은 점수)");
        GUILayout.Label("1-4: 미리 설정된 점수");
        GUILayout.Label("R: 하이스코어 리셋");
        GUILayout.Label("T: 테스트 점수 변경");
        GUILayout.Space(10);

        GUILayout.Label("=== 네비게이션 키 ===");
        GUILayout.Label("↑↓ / WS: 버튼 선택");
        GUILayout.Label("Enter / Space / Z: 확인");
        GUILayout.Label("ESC: 타이틀로");

        GUILayout.EndArea();
    }
}
