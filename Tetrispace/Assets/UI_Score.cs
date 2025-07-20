using UnityEngine;
using TMPro; // TextMeshPro 사용을 위한 using 추가

/// <summary>
/// TextMeshPro 기반 점수 UI
/// 
/// 사용법:
/// 1. TextMeshPro 컴포넌트들을 Inspector에서 연결
/// 2. 게임오버 시 DisplayGameOverScores() 호출
/// 3. UI Canvas용은 TextMeshProUGUI, 3D 월드용은 TextMeshPro 사용
/// </summary>
public class UI_Score : MonoBehaviour
{
    [Header("=== UI 컴포넌트 (TextMeshPro) ===")]
    [SerializeField] private TextMeshProUGUI currentScoreText;  // UI Canvas용
    [SerializeField] private TextMeshProUGUI highScoreText;     // UI Canvas용
    [SerializeField] private TextMeshProUGUI newRecordText;     // 신기록 텍스트 (옵션)

    // 만약 3D 월드 공간에서 사용한다면 아래 주석을 해제하고 위의 UGUI 버전은 주석처리
    // [SerializeField] private TextMeshPro currentScoreText;   // 3D 월드용
    // [SerializeField] private TextMeshPro highScoreText;      // 3D 월드용
    // [SerializeField] private TextMeshPro newRecordText;      // 3D 월드용

    [Header("=== 표시 설정 ===")]
    [SerializeField] private string currentScorePrefix = "Score: ";
    [SerializeField] private string highScorePrefix = "Best: ";
    [SerializeField] private string newRecordMessage = "NEW RECORD!";
    [SerializeField] private bool autoUpdateCurrentScore = false; // 실시간 업데이트 여부

    void Start()
    {
        // 실시간 점수 업데이트를 원하는 경우
        if (autoUpdateCurrentScore && GameManager.Instance != null)
        {
            GameManager.OnScoreChanged += OnScoreChanged;
            // 초기 점수 표시
            OnScoreChanged(GameManager.Instance.CurrentScore);
        }

        // 게임 시작 시 초기화
        InitializeScoreDisplay();
    }

    /// <summary>
    /// 점수 변경 시 실시간 업데이트 (옵션)
    /// </summary>
    private void OnScoreChanged(int newScore)
    {
        if (currentScoreText != null)
        {
            currentScoreText.text = currentScorePrefix + newScore.ToString("N0");
        }
    }

    /// <summary>
    /// 게임오버 팝업이 활성화될 때 호출
    /// </summary>
    public void DisplayGameOverScores()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogWarning("UI_Score: GameManager.Instance가 null입니다!");
            return;
        }

        int currentScore = GameManager.Instance.CurrentScore;
        int highScore = GameManager.Instance.HighScore;
        bool isNewRecord = currentScore > highScore; // 기존 최고점수보다 높아야 신기록

        // 현재 점수 표시
        if (currentScoreText != null)
        {
            currentScoreText.text = currentScorePrefix + currentScore.ToString("N0");
            Debug.Log($"현재 점수 표시: {currentScoreText.text}");
        }
        else
        {
            Debug.LogWarning("UI_Score: currentScoreText(TextMeshPro)가 연결되지 않았습니다!");
        }

        // 최고점수 표시 (신기록인 경우 현재 점수로 업데이트됨)
        if (highScoreText != null)
        {
            int displayHighScore = isNewRecord ? currentScore : highScore;
            highScoreText.text = highScorePrefix + displayHighScore.ToString("N0");
            Debug.Log($"최고 점수 표시: {highScoreText.text}");
        }
        else
        {
            Debug.LogWarning("UI_Score: highScoreText(TextMeshPro)가 연결되지 않았습니다!");
        }

        // 신기록 메시지 표시
        if (newRecordText != null)
        {
            newRecordText.gameObject.SetActive(isNewRecord);
            if (isNewRecord)
            {
                newRecordText.text = newRecordMessage;
                Debug.Log($"신기록! {newRecordMessage}");

                // 신기록 텍스트에 효과 추가 (옵션)
                StartCoroutine(AnimateNewRecordText());
            }
        }

        Debug.Log($"게임오버 점수 표시: 현재 {currentScore}, 최고 {highScore}, 신기록: {isNewRecord}");
    }

    /// <summary>
    /// 신기록 텍스트 애니메이션 (옵션)
    /// </summary>
    private System.Collections.IEnumerator AnimateNewRecordText()
    {
        if (newRecordText == null) yield break;

        float duration = 1f;
        float elapsedTime = 0f;
        Color originalColor = newRecordText.color;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.PingPong(elapsedTime * 3f, 1f); // 깜빡이는 효과
            newRecordText.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
            yield return null;
        }

        // 원래 색상으로 복구
        newRecordText.color = originalColor;
    }

    /// <summary>
    /// 현재 점수만 업데이트 (실시간 용도)
    /// </summary>
    public void UpdateCurrentScore()
    {
        if (GameManager.Instance != null && currentScoreText != null)
        {
            int currentScore = GameManager.Instance.CurrentScore;
            currentScoreText.text = currentScorePrefix + currentScore.ToString("N0");
        }
    }

    /// <summary>
    /// 최고점수만 업데이트
    /// </summary>
    public void UpdateHighScore()
    {
        if (GameManager.Instance != null && highScoreText != null)
        {
            int highScore = GameManager.Instance.HighScore;
            highScoreText.text = highScorePrefix + highScore.ToString("N0");
        }
    }

    /// <summary>
    /// 모든 점수 텍스트 초기화
    /// </summary>
    public void InitializeScoreDisplay()
    {
        if (GameManager.Instance == null) return;

        UpdateCurrentScore();
        UpdateHighScore();

        // 신기록 텍스트는 기본적으로 숨김
        if (newRecordText != null)
        {
            newRecordText.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// TextMeshPro 컴포넌트들이 제대로 연결되었는지 확인
    /// </summary>
    [ContextMenu("Check TextMeshPro Connections")]
    public void CheckConnections()
    {
        Debug.Log("=== TextMeshPro 연결 상태 확인 ===");
        Debug.Log($"currentScoreText: {(currentScoreText != null ? "연결됨" : "연결 안됨")}");
        Debug.Log($"highScoreText: {(highScoreText != null ? "연결됨" : "연결 안됨")}");
        Debug.Log($"newRecordText: {(newRecordText != null ? "연결됨" : "연결 안됨")}");
        Debug.Log($"GameManager.Instance: {(GameManager.Instance != null ? "연결됨" : "연결 안됨")}");

        // TextMeshPro 컴포넌트 타입 확인
        if (currentScoreText != null)
        {
            Debug.Log($"currentScoreText 타입: {currentScoreText.GetType()}");
        }
    }

    /// <summary>
    /// 수동으로 점수 새로고침 (버튼 등에서 호출 가능)
    /// </summary>
    public void RefreshAllScores()
    {
        DisplayGameOverScores();
    }

    private void OnDestroy()
    {
        // 이벤트 구독 해제 (메모리 누수 방지)
        if (GameManager.Instance != null)
        {
            GameManager.OnScoreChanged -= OnScoreChanged;
        }
    }
}

/// <summary>
/// 간단한 실시간 점수 표시용 (게임 중 사용)
/// </summary>
public class SimpleScoreDisplayTMP : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private string prefix = "Score: ";

    void Start()
    {
        if (scoreText == null)
        {
            scoreText = GetComponent<TextMeshProUGUI>();
        }

        if (GameManager.Instance != null)
        {
            GameManager.OnScoreChanged += UpdateScore;
            UpdateScore(GameManager.Instance.CurrentScore); // 초기값
        }
    }

    private void UpdateScore(int score)
    {
        if (scoreText != null)
        {
            scoreText.text = prefix + score.ToString("N0");
        }
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.OnScoreChanged -= UpdateScore;
        }
    }
}