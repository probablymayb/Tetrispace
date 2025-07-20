using UnityEngine;

public class UI_Score : MonoBehaviour
{
    [Header("=== UI ������Ʈ ===")]
    [SerializeField] private TextMesh currentScoreText;
    [SerializeField] private TextMesh highScoreText;
    [SerializeField] private TextMesh newRecordText;  // �ű�� �ؽ�Ʈ (�ɼ�)

    [Header("=== ǥ�� ���� ===")]
    [SerializeField] private string currentScorePrefix = "Score: ";
    [SerializeField] private string highScorePrefix = "Best: ";
    [SerializeField] private string newRecordMessage = "NEW RECORD!";

    /// <summary>
    /// ���ӿ��� �˾��� Ȱ��ȭ�� �� ȣ��
    /// </summary>
    public void DisplayGameOverScores()
    {
        if (GameManager.Instance == null) return;

        int currentScore = GameManager.Instance.CurrentScore;
        int highScore = GameManager.Instance.HighScore;
        bool isNewRecord = currentScore >= highScore;

        // ���� ���� ǥ��
        if (currentScoreText != null)
        {
            currentScoreText.text = currentScorePrefix + currentScore.ToString("N0");
        }

        // �ְ����� ǥ��
        if (highScoreText != null)
        {
            highScoreText.text = highScorePrefix + highScore.ToString("N0");
        }

        // �ű�� �޽��� ǥ��
        if (newRecordText != null)
        {
            newRecordText.gameObject.SetActive(isNewRecord);
            if (isNewRecord)
            {
                newRecordText.text = newRecordMessage;
            }
        }

        Debug.Log($"���ӿ��� ���� ǥ��: ���� {currentScore}, �ְ� {highScore}, �ű��: {isNewRecord}");
    }
}
