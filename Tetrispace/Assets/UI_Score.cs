using UnityEngine;

public class UI_Score : MonoBehaviour
{
    [Header("=== UI 컴포넌트 ===")]
    [SerializeField] private TextMesh currentScoreText;
    [SerializeField] private TextMesh highScoreText;
    [SerializeField] private TextMesh newRecordText;  // 신기록 텍스트 (옵션)

    [Header("=== 표시 설정 ===")]
    [SerializeField] private string currentScorePrefix = "Score: ";
    [SerializeField] private string highScorePrefix = "Best: ";
    [SerializeField] private string newRecordMessage = "NEW RECORD!";

    /// <summary>
    /// 게임오버 팝업이 활성화될 때 호출
    /// </summary>
    public void DisplayGameOverScores()
    {
        if (GameManager.Instance == null) return;

        int currentScore = GameManager.Instance.CurrentScore;
        int highScore = GameManager.Instance.HighScore;
        bool isNewRecord = currentScore >= highScore;

        // 현재 점수 표시
        if (currentScoreText != null)
        {
            currentScoreText.text = currentScorePrefix + currentScore.ToString("N0");
        }

        // 최고점수 표시
        if (highScoreText != null)
        {
            highScoreText.text = highScorePrefix + highScore.ToString("N0");
        }

        // 신기록 메시지 표시
        if (newRecordText != null)
        {
            newRecordText.gameObject.SetActive(isNewRecord);
            if (isNewRecord)
            {
                newRecordText.text = newRecordMessage;
            }
        }

        Debug.Log($"게임오버 점수 표시: 현재 {currentScore}, 최고 {highScore}, 신기록: {isNewRecord}");
    }
}
