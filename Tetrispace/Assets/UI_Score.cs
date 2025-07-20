using UnityEngine;
using TMPro; // TextMeshPro ����� ���� using �߰�

/// <summary>
/// TextMeshPro ��� ���� UI
/// 
/// ����:
/// 1. TextMeshPro ������Ʈ���� Inspector���� ����
/// 2. ���ӿ��� �� DisplayGameOverScores() ȣ��
/// 3. UI Canvas���� TextMeshProUGUI, 3D ������� TextMeshPro ���
/// </summary>
public class UI_Score : MonoBehaviour
{
    [Header("=== UI ������Ʈ (TextMeshPro) ===")]
    [SerializeField] private TextMeshProUGUI currentScoreText;  // UI Canvas��
    [SerializeField] private TextMeshProUGUI highScoreText;     // UI Canvas��
    [SerializeField] private TextMeshProUGUI newRecordText;     // �ű�� �ؽ�Ʈ (�ɼ�)

    // ���� 3D ���� �������� ����Ѵٸ� �Ʒ� �ּ��� �����ϰ� ���� UGUI ������ �ּ�ó��
    // [SerializeField] private TextMeshPro currentScoreText;   // 3D �����
    // [SerializeField] private TextMeshPro highScoreText;      // 3D �����
    // [SerializeField] private TextMeshPro newRecordText;      // 3D �����

    [Header("=== ǥ�� ���� ===")]
    [SerializeField] private string currentScorePrefix = "Score: ";
    [SerializeField] private string highScorePrefix = "Best: ";
    [SerializeField] private string newRecordMessage = "NEW RECORD!";
    [SerializeField] private bool autoUpdateCurrentScore = false; // �ǽð� ������Ʈ ����

    void Start()
    {
        // �ǽð� ���� ������Ʈ�� ���ϴ� ���
        if (autoUpdateCurrentScore && GameManager.Instance != null)
        {
            GameManager.OnScoreChanged += OnScoreChanged;
            // �ʱ� ���� ǥ��
            OnScoreChanged(GameManager.Instance.CurrentScore);
        }

        // ���� ���� �� �ʱ�ȭ
        InitializeScoreDisplay();
    }

    /// <summary>
    /// ���� ���� �� �ǽð� ������Ʈ (�ɼ�)
    /// </summary>
    private void OnScoreChanged(int newScore)
    {
        if (currentScoreText != null)
        {
            currentScoreText.text = currentScorePrefix + newScore.ToString("N0");
        }
    }

    /// <summary>
    /// ���ӿ��� �˾��� Ȱ��ȭ�� �� ȣ��
    /// </summary>
    public void DisplayGameOverScores()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogWarning("UI_Score: GameManager.Instance�� null�Դϴ�!");
            return;
        }

        int currentScore = GameManager.Instance.CurrentScore;
        int highScore = GameManager.Instance.HighScore;
        bool isNewRecord = currentScore > highScore; // ���� �ְ��������� ���ƾ� �ű��

        // ���� ���� ǥ��
        if (currentScoreText != null)
        {
            currentScoreText.text = currentScorePrefix + currentScore.ToString("N0");
            Debug.Log($"���� ���� ǥ��: {currentScoreText.text}");
        }
        else
        {
            Debug.LogWarning("UI_Score: currentScoreText(TextMeshPro)�� ������� �ʾҽ��ϴ�!");
        }

        // �ְ����� ǥ�� (�ű���� ��� ���� ������ ������Ʈ��)
        if (highScoreText != null)
        {
            int displayHighScore = isNewRecord ? currentScore : highScore;
            highScoreText.text = highScorePrefix + displayHighScore.ToString("N0");
            Debug.Log($"�ְ� ���� ǥ��: {highScoreText.text}");
        }
        else
        {
            Debug.LogWarning("UI_Score: highScoreText(TextMeshPro)�� ������� �ʾҽ��ϴ�!");
        }

        // �ű�� �޽��� ǥ��
        if (newRecordText != null)
        {
            newRecordText.gameObject.SetActive(isNewRecord);
            if (isNewRecord)
            {
                newRecordText.text = newRecordMessage;
                Debug.Log($"�ű��! {newRecordMessage}");

                // �ű�� �ؽ�Ʈ�� ȿ�� �߰� (�ɼ�)
                StartCoroutine(AnimateNewRecordText());
            }
        }

        Debug.Log($"���ӿ��� ���� ǥ��: ���� {currentScore}, �ְ� {highScore}, �ű��: {isNewRecord}");
    }

    /// <summary>
    /// �ű�� �ؽ�Ʈ �ִϸ��̼� (�ɼ�)
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
            float alpha = Mathf.PingPong(elapsedTime * 3f, 1f); // �����̴� ȿ��
            newRecordText.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
            yield return null;
        }

        // ���� �������� ����
        newRecordText.color = originalColor;
    }

    /// <summary>
    /// ���� ������ ������Ʈ (�ǽð� �뵵)
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
    /// �ְ������� ������Ʈ
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
    /// ��� ���� �ؽ�Ʈ �ʱ�ȭ
    /// </summary>
    public void InitializeScoreDisplay()
    {
        if (GameManager.Instance == null) return;

        UpdateCurrentScore();
        UpdateHighScore();

        // �ű�� �ؽ�Ʈ�� �⺻������ ����
        if (newRecordText != null)
        {
            newRecordText.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// TextMeshPro ������Ʈ���� ����� ����Ǿ����� Ȯ��
    /// </summary>
    [ContextMenu("Check TextMeshPro Connections")]
    public void CheckConnections()
    {
        Debug.Log("=== TextMeshPro ���� ���� Ȯ�� ===");
        Debug.Log($"currentScoreText: {(currentScoreText != null ? "�����" : "���� �ȵ�")}");
        Debug.Log($"highScoreText: {(highScoreText != null ? "�����" : "���� �ȵ�")}");
        Debug.Log($"newRecordText: {(newRecordText != null ? "�����" : "���� �ȵ�")}");
        Debug.Log($"GameManager.Instance: {(GameManager.Instance != null ? "�����" : "���� �ȵ�")}");

        // TextMeshPro ������Ʈ Ÿ�� Ȯ��
        if (currentScoreText != null)
        {
            Debug.Log($"currentScoreText Ÿ��: {currentScoreText.GetType()}");
        }
    }

    /// <summary>
    /// �������� ���� ���ΰ�ħ (��ư ��� ȣ�� ����)
    /// </summary>
    public void RefreshAllScores()
    {
        DisplayGameOverScores();
    }

    private void OnDestroy()
    {
        // �̺�Ʈ ���� ���� (�޸� ���� ����)
        if (GameManager.Instance != null)
        {
            GameManager.OnScoreChanged -= OnScoreChanged;
        }
    }
}

/// <summary>
/// ������ �ǽð� ���� ǥ�ÿ� (���� �� ���)
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
            UpdateScore(GameManager.Instance.CurrentScore); // �ʱⰪ
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