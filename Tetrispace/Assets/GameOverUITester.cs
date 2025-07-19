// GameOverTester.cs - �׽�Ʈ�� ��ũ��Ʈ
using UnityEngine;

/// <summary>
/// ���ӿ��� UI �׽�Ʈ�� ��ũ��Ʈ
/// �پ��� ���� ��Ȳ�� ������ �׽�Ʈ�� �� ����
/// </summary>
public class GameOverTester : MonoBehaviour
{
    [Header("=== �׽�Ʈ ���� ===")]
    [SerializeField] private UI_GameOverPopup gameOverPopup;
    [SerializeField] private int[] testScores = { 1000, 5000, 10000, 15000 };
    [SerializeField] private int currentTestIndex = 0;

    [Header("=== ����� ���� ===")]
    [SerializeField] private bool showDebugInfo = true;

    void Update()
    {
        HandleTestInputs();
    }

    /// <summary>
    /// �׽�Ʈ �Է� ó��
    /// </summary>
    private void HandleTestInputs()
    {
        // GŰ: ���ӿ��� �׽�Ʈ (���� �׽�Ʈ ������)
        if (Input.GetKeyDown(KeyCode.G))
        {
            TestGameOverWithScore(testScores[currentTestIndex]);
        }

        // HŰ: ���̽��ھ�� ���� ������ �׽�Ʈ (�ű��)
        if (Input.GetKeyDown(KeyCode.H))
        {
            int highScore = GameManager.Instance.HighScore;
            TestGameOverWithScore(highScore + 1000);
        }

        // LŰ: ���̽��ھ�� ���� ������ �׽�Ʈ
        if (Input.GetKeyDown(KeyCode.L))
        {
            int highScore = GameManager.Instance.HighScore;
            TestGameOverWithScore(Mathf.Max(0, highScore - 500));
        }

        // ����Ű 1-4: �̸� ������ ������ �׽�Ʈ
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

        // RŰ: ���̽��ھ� ����
        if (Input.GetKeyDown(KeyCode.R))
        {
            ResetHighScore();
        }

        // TŰ: �׽�Ʈ ���� ��ȯ
        if (Input.GetKeyDown(KeyCode.T))
        {
            CycleTestScore();
        }
    }

    /// <summary>
    /// Ư�� ������ ���ӿ��� �׽�Ʈ
    /// </summary>
    private void TestGameOverWithScore(int score)
    {
        Debug.Log($"=== ���ӿ��� �׽�Ʈ ���� ===");
        Debug.Log($"�׽�Ʈ ����: {score}");
        Debug.Log($"���� ���̽��ھ�: {GameManager.Instance.HighScore}");
        Debug.Log($"�ű�� ����: {score > GameManager.Instance.HighScore}");

        // GameManager�� ���� ����
        GameManager.Instance.SetScore(score);

        // ���ӿ��� ����
        if (gameOverPopup != null)
        {
            // �������� �Ҵ�� ���
            gameOverPopup.gameObject.SetActive(true);
            gameOverPopup.Init();
        }
        else
        {
            // GameManager�� ���� �Ϲ����� ���ӿ���
            GameManager.Instance.GameOver();
        }
    }

    /// <summary>
    /// ���̽��ھ� ����
    /// </summary>
    private void ResetHighScore()
    {
        GameManager.Instance.ResetAllData();
        Debug.Log("���̽��ھ ���µǾ����ϴ�!");
    }

    /// <summary>
    /// �׽�Ʈ ���� ��ȯ
    /// </summary>
    private void CycleTestScore()
    {
        currentTestIndex = (currentTestIndex + 1) % testScores.Length;
        Debug.Log($"�׽�Ʈ ���� ����: {testScores[currentTestIndex]}");
    }

    /// <summary>
    /// GUI ����� ���� ǥ��
    /// </summary>
    private void OnGUI()
    {
        if (!showDebugInfo) return;

        GUILayout.BeginArea(new Rect(10, 10, 400, 300));
        GUILayout.Label("=== ���ӿ��� UI �׽�Ʈ ===");
        GUILayout.Space(10);

        GUILayout.Label($"���� ����: {GameManager.Instance.CurrentScore}");
        GUILayout.Label($"�ְ� ����: {GameManager.Instance.HighScore}");
        GUILayout.Label($"�׽�Ʈ ����: {testScores[currentTestIndex]}");
        GUILayout.Space(10);

        GUILayout.Label("=== �׽�Ʈ Ű ===");
        GUILayout.Label("G: ���ӿ��� (���� �׽�Ʈ ����)");
        GUILayout.Label("H: ���ӿ��� (�ű��)");
        GUILayout.Label("L: ���ӿ��� (���� ����)");
        GUILayout.Label("1-4: �̸� ������ ����");
        GUILayout.Label("R: ���̽��ھ� ����");
        GUILayout.Label("T: �׽�Ʈ ���� ����");
        GUILayout.Space(10);

        GUILayout.Label("=== �׺���̼� Ű ===");
        GUILayout.Label("��� / WS: ��ư ����");
        GUILayout.Label("Enter / Space / Z: Ȯ��");
        GUILayout.Label("ESC: Ÿ��Ʋ��");

        GUILayout.EndArea();
    }
}
