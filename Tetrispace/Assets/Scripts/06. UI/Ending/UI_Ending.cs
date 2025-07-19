// UI_GameOverPopup.cs (������ ���� - ��üũ �Ϻ�)
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// ������ ���ӿ��� �˾� UI (��üũ �Ϻ�)
/// �Ϻ� ��Ұ� ��� ���� ���� ����
/// </summary>
public class UI_GameOverPopup : UI_Base
{
    #region UI ��� ����
    enum Texts
    {
        GameOverText,
        ScoreLabel,
        ScoreValue,
        HighScoreLabel,
        HighScoreValue,
        NewRecordText,
        RestartText,
        TitleText,
    }

    enum Buttons
    {
        RestartButton,
        TitleButton,
    }

    enum Images
    {
        Background,
        GameOverPanel,
        ScorePanel,
        ButtonPanel,
    }

    enum GameObjects
    {
        ScoreContainer,
        ButtonContainer,
        NewRecordEffect,
    }
    #endregion

    #region ��������
    [Header("=== ���̵� ���� ===")]
    [SerializeField] private float backgroundFadeDuration = 1.0f;
    [SerializeField] private float panelFadeDuration = 0.8f;
    [SerializeField] private float textFadeDuration = 0.5f;

    [Header("=== �ִϸ��̼� ���� ===")]
    [SerializeField] private float gameOverTypingSpeed = 0.1f;
    [SerializeField] private float scoreCountUpDuration = 1.5f;
    [SerializeField] private float buttonAppearDelay = 1.0f;

    [Header("=== ���� ���� ===")]
    [SerializeField] private Color gameOverColor = new Color(1f, 0.2f, 0.2f, 1f);
    [SerializeField] private Color normalScoreColor = Color.white;
    [SerializeField] private Color newRecordColor = Color.yellow;
    [SerializeField] private Color selectedButtonColor = Color.yellow;
    [SerializeField] private Color normalButtonColor = Color.white;

    [Header("=== ���� �Ҵ� (���û���) ===")]
    [SerializeField] private TextMeshProUGUI manualGameOverText;
    [SerializeField] private TextMeshProUGUI manualScoreText;
    [SerializeField] private Button manualRestartButton;
    [SerializeField] private Button manualTitleButton;
    #endregion

    #region ���� ������
    private int currentScore;
    private int previousHighScore;
    private int newHighScore;
    private bool isNewRecord = false;
    private bool isAnimating = true;
    private bool canNavigate = false;

    // ��ư �׺���̼�
    private int selectedButtonIndex = 0;
    private Button[] navigationButtons;
    private TextMeshProUGUI[] buttonTexts;

    // ������ UI ����
    private TextMeshProUGUI safeGameOverText;
    private TextMeshProUGUI safeScoreText;
    private TextMeshProUGUI safeHighScoreText;
    private Button safeRestartButton;
    private Button safeTitleButton;
    #endregion

    #region �ʱ�ȭ (���� ����)
    public override void Init()
    {
        Debug.Log("=== ���ӿ��� UI �ʱ�ȭ ���� ===");

        try
        {
            BindUIElementsSafely();
            LoadGameData();
            SetupInitialStateSafely();
            SetupNavigationSafely();
            RegisterEventsSafely();

            StartCoroutine(PlayCompleteAnimationSafely());
        }
        catch (System.Exception e)
        {
            Debug.LogError($"���ӿ��� UI �ʱ�ȭ ����: {e.Message}");
            // �ּ����� ����̶� �����ϵ���
            SetupFallbackUI();
        }
    }

    /// <summary>
    /// ������ UI ���ε� (���� ����)
    /// </summary>
    private void BindUIElementsSafely()
    {
        try
        {
            // ���� ���ε� �õ�
            Bind<TextMeshProUGUI>(typeof(Texts));
            Bind<Button>(typeof(Buttons));
            Bind<Image>(typeof(Images));
            Bind<GameObject>(typeof(GameObjects));

            Debug.Log("�ڵ� ���ε� ����!");
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"�ڵ� ���ε� ����: {e.Message}. ���� ���ε����� ��ȯ�մϴ�.");
        }

        // ������ ���� ����
        SetupSafeReferences();
    }

    /// <summary>
    /// ������ ���� ����
    /// </summary>
    private void SetupSafeReferences()
    {
        // 1����: ���� �Ҵ�� �͵�
        safeGameOverText = manualGameOverText;
        safeScoreText = manualScoreText;
        safeRestartButton = manualRestartButton;
        safeTitleButton = manualTitleButton;

        // 2����: �ڵ� ���ε��� �͵�
        if (safeGameOverText == null)
            safeGameOverText = GetTextSafely((int)Texts.GameOverText);

        if (safeScoreText == null)
            safeScoreText = GetTextSafely((int)Texts.ScoreValue);

        if (safeHighScoreText == null)
            safeHighScoreText = GetTextSafely((int)Texts.HighScoreValue);

        if (safeRestartButton == null)
            safeRestartButton = GetButtonSafely((int)Buttons.RestartButton);

        if (safeTitleButton == null)
            safeTitleButton = GetButtonSafely((int)Buttons.TitleButton);

        // 3����: �̸����� ã��
        if (safeGameOverText == null)
            safeGameOverText = GameObject.Find("GameOverText")?.GetComponent<TextMeshProUGUI>();

        if (safeScoreText == null)
            safeScoreText = GameObject.Find("ScoreValue")?.GetComponent<TextMeshProUGUI>();

    }

    /// <summary>
    /// ������ GetText (��üũ ����)
    /// </summary>
    private TextMeshProUGUI GetTextSafely(int index)
    {
        try
        {
            return GetText(index);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// ������ GetButton (��üũ ����)
    /// </summary>
    private Button GetButtonSafely(int index)
    {
        try
        {
            return GetButton(index);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// ���� ������ �ε�
    /// </summary>
    private void LoadGameData()
    {
        // GameManager�� ������ ������ ��������
        if (GameManager.Instance != null)
        {
            currentScore = GameManager.Instance.CurrentScore;
            previousHighScore = GameManager.Instance.HighScore;
        }
        else
        {
            // �׽�Ʈ�� �⺻��
            currentScore = 4000;
            previousHighScore = 8000;
            Debug.LogWarning("GameManager�� ��� �׽�Ʈ ������ ���");
        }

        // �ű�� üũ
        if (currentScore > previousHighScore)
        {
            isNewRecord = true;
            newHighScore = currentScore;
        }
        else
        {
            newHighScore = previousHighScore;
        }

        Debug.Log($"��������: {currentScore}, �ְ�����: {previousHighScore}, �ű��: {isNewRecord}");
    }

    /// <summary>
    /// ������ �ʱ� ���� ����
    /// </summary>
    private void SetupInitialStateSafely()
    {
        // �ؽ�Ʈ ����
        if (safeGameOverText != null)
        {
            safeGameOverText.text = "GAME\nOVER";
            safeGameOverText.color = gameOverColor;
            SetAlpha(safeGameOverText, 0f);
        }

        if (safeScoreText != null)
        {
            safeScoreText.text = "0";
            safeScoreText.color = isNewRecord ? newRecordColor : normalScoreColor;
            SetAlpha(safeScoreText, 0f);
        }

        if (safeHighScoreText != null)
        {
            safeHighScoreText.text = previousHighScore.ToString();
            safeHighScoreText.color = normalScoreColor;
            SetAlpha(safeHighScoreText, 0f);
        }

        // �̹����� �����ϰ� (�ִٸ�)
        for (int i = 0; i < System.Enum.GetValues(typeof(Images)).Length; i++)
        {
            try
            {
                var image = GetImage(i);
                if (image != null) SetAlpha(image, 0f);
            }
            catch { }
        }

        Debug.Log("�ʱ� ���� ���� �Ϸ�");
    }

    /// <summary>
    /// ������ �׺���̼� ����
    /// </summary>
    private void SetupNavigationSafely()
    {
        var buttons = new System.Collections.Generic.List<Button>();
        var texts = new System.Collections.Generic.List<TextMeshProUGUI>();

        // ��ư�� ����
        if (safeRestartButton != null)
        {
            buttons.Add(safeRestartButton);
            var restartText = safeRestartButton.GetComponentInChildren<TextMeshProUGUI>();
            texts.Add(restartText);
        }

        if (safeTitleButton != null)
        {
            buttons.Add(safeTitleButton);
            var titleText = safeTitleButton.GetComponentInChildren<TextMeshProUGUI>();
            texts.Add(titleText);
        }

        navigationButtons = buttons.ToArray();
        buttonTexts = texts.ToArray();

        selectedButtonIndex = 0;
        UpdateButtonSelectionSafely();

        Debug.Log($"�׺���̼� ���� �Ϸ�: {navigationButtons.Length}�� ��ư");
    }

    /// <summary>
    /// ������ �̺�Ʈ ���
    /// </summary>
    private void RegisterEventsSafely()
    {
        // ��ư �̺�Ʈ ���
        if (safeRestartButton != null)
        {
            safeRestartButton.onClick.AddListener(() => OnRestartSelectedSafely());
        }

        if (safeTitleButton != null)
        {
            safeTitleButton.onClick.AddListener(() => OnTitleSelectedSafely());
        }

        Debug.Log("�̺�Ʈ ��� �Ϸ�");
    }

    /// <summary>
    /// ���� UI ���� (�־��� ���)
    /// </summary>
    private void SetupFallbackUI()
    {
        Debug.Log("���� UI ���� ����");

        // �⺻ �ؽ�Ʈ ����
        if (safeGameOverText == null)
        {
            GameObject textObj = new GameObject("GameOverText");
            textObj.transform.SetParent(transform, false);
            safeGameOverText = textObj.AddComponent<TextMeshProUGUI>();
            safeGameOverText.text = "GAME OVER";
            safeGameOverText.fontSize = 48;
            safeGameOverText.color = gameOverColor;
            safeGameOverText.alignment = TextAlignmentOptions.Center;

            RectTransform rect = textObj.GetComponent<RectTransform>();
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = new Vector2(400, 100);
        }

        canNavigate = true;
        isAnimating = false;

        Debug.Log("���� UI ���� �Ϸ�");
    }
    #endregion

    #region ������ �ִϸ��̼�
    /// <summary>
    /// ������ �ִϸ��̼� ���
    /// </summary>
    private IEnumerator PlayCompleteAnimationSafely()
    {
        isAnimating = true;
        canNavigate = false;

        Debug.Log("�ִϸ��̼� ����");

        // ���ӿ��� �ؽ�Ʈ ǥ��
        if (safeGameOverText != null)
        {
            yield return StartCoroutine(TypeGameOverTextSafely());
        }

        yield return new WaitForSeconds(0.5f);

        // ���� ǥ��
        if (safeScoreText != null)
        {
            yield return StartCoroutine(CountUpScoreSafely());
        }

        yield return new WaitForSeconds(0.5f);

        // ���̽��ھ� ǥ��
        if (safeHighScoreText != null)
        {
            yield return StartCoroutine(FadeInTextSafely(safeHighScoreText, textFadeDuration));
        }

        // �ű�� ȿ��
        if (isNewRecord)
        {
            yield return StartCoroutine(PlayNewRecordEffectSafely());
        }

        yield return new WaitForSeconds(buttonAppearDelay);

        // ��ư ǥ��
        yield return StartCoroutine(ShowButtonsSafely());

        // �׺���̼� Ȱ��ȭ
        canNavigate = true;
        isAnimating = false;

        Debug.Log("�ִϸ��̼� �Ϸ�! �׺���̼� Ȱ��ȭ");
    }

    private IEnumerator TypeGameOverTextSafely()
    {
        if (safeGameOverText == null) yield break;

        string fullText = "GAME\nOVER";
        safeGameOverText.text = "";
        SetAlpha(safeGameOverText, 1f);

        for (int i = 0; i <= fullText.Length; i++)
        {
            safeGameOverText.text = fullText.Substring(0, i);
            yield return new WaitForSeconds(gameOverTypingSpeed);
        }

        // ������ ȿ��
        for (int i = 0; i < 3; i++)
        {
            SetAlpha(safeGameOverText, 0.3f);
            yield return new WaitForSeconds(0.15f);
            SetAlpha(safeGameOverText, 1f);
            yield return new WaitForSeconds(0.15f);
        }
    }

    private IEnumerator CountUpScoreSafely()
    {
        if (safeScoreText == null) yield break;

        SetAlpha(safeScoreText, 1f);

        float elapsed = 0f;
        int displayScore = 0;

        while (elapsed < scoreCountUpDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / scoreCountUpDuration;
            displayScore = Mathf.RoundToInt(Mathf.Lerp(0, currentScore, progress));
            safeScoreText.text = displayScore.ToString();
            yield return null;
        }

        safeScoreText.text = currentScore.ToString();
    }

    private IEnumerator PlayNewRecordEffectSafely()
    {
        if (!isNewRecord) yield break;

        Debug.Log("�ű�� ȿ�� ���!");

        // ���� �ؽ�Ʈ ������
        if (safeScoreText != null)
        {
            for (int i = 0; i < 5; i++)
            {
                safeScoreText.color = Color.red;
                yield return new WaitForSeconds(0.2f);
                safeScoreText.color = newRecordColor;
                yield return new WaitForSeconds(0.2f);
            }
        }

        // ���̽��ھ� ������Ʈ
        if (safeHighScoreText != null)
        {
            yield return StartCoroutine(CountUpHighScoreSafely(safeHighScoreText, previousHighScore, newHighScore, 1f));
        }
    }

    private IEnumerator CountUpHighScoreSafely(TextMeshProUGUI text, int from, int to, float duration)
    {
        if (text == null) yield break;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            int currentValue = Mathf.RoundToInt(Mathf.Lerp(from, to, elapsed / duration));
            text.text = currentValue.ToString();
            yield return null;
        }
        text.text = to.ToString();
    }

    private IEnumerator ShowButtonsSafely()
    {
        // ��ư �ؽ�Ʈ�� ���̵���
        foreach (var text in buttonTexts)
        {
            if (text != null)
            {
                yield return StartCoroutine(FadeInTextSafely(text, textFadeDuration));
                yield return new WaitForSeconds(0.2f);
            }
        }

        UpdateButtonSelectionSafely();
    }

    private IEnumerator FadeInTextSafely(TextMeshProUGUI text, float duration)
    {
        if (text == null) yield break;

        float elapsed = 0f;
        Color color = text.color;
        color.a = 0f;
        text.color = color;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            color.a = Mathf.Lerp(0f, 1f, elapsed / duration);
            text.color = color;
            yield return null;
        }

        color.a = 1f;
        text.color = color;
    }
    #endregion

    #region ������ �׺���̼�
    private void Update()
    {
        if (!canNavigate || isAnimating) return;

        HandleNavigationSafely();
        HandleSelectionSafely();
        HandleCloseSafely();
    }

    private void HandleNavigationSafely()
    {
        if (navigationButtons == null || navigationButtons.Length == 0) return;

        bool moved = false;

        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
        {
            selectedButtonIndex = (selectedButtonIndex - 1 + navigationButtons.Length) % navigationButtons.Length;
            moved = true;
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
        {
            selectedButtonIndex = (selectedButtonIndex + 1) % navigationButtons.Length;
            moved = true;
        }

        if (moved)
        {
            UpdateButtonSelectionSafely();
            Debug.Log($"��ư ����: {selectedButtonIndex}");
        }
    }

    private void HandleSelectionSafely()
    {
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Z))
        {
            ExecuteSelectedButtonSafely();
        }
    }

    private void HandleCloseSafely()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            OnTitleSelectedSafely();
        }
    }

    private void UpdateButtonSelectionSafely()
    {
        if (buttonTexts == null) return;

        for (int i = 0; i < buttonTexts.Length; i++)
        {
            if (buttonTexts[i] != null)
            {
                if (i == selectedButtonIndex)
                {
                    buttonTexts[i].color = selectedButtonColor;
                    buttonTexts[i].text = i == 0 ? " RESTART" : " TITLE";
                }
                else
                {
                    buttonTexts[i].color = normalButtonColor;
                    buttonTexts[i].text = i == 0 ? "  RESTART" : "  TITLE";
                }
            }
        }
    }

    private void ExecuteSelectedButtonSafely()
    {
        if (navigationButtons == null || selectedButtonIndex >= navigationButtons.Length) return;

        switch (selectedButtonIndex)
        {
            case 0:
                OnRestartSelectedSafely();
                break;
            case 1:
                OnTitleSelectedSafely();
                break;
        }
    }
    #endregion

    #region ������ ��ư �̺�Ʈ
    private void OnRestartSelectedSafely()
    {
        Debug.Log("����� ���õ�!");

        if (GameManager.Instance != null)
        {
            GameManager.Instance.RestartGame();
        }
        else
        {
            Debug.Log("GameManager�� ��� ����� �ùķ��̼�");
        }

        ClosePopupSafely();
    }

    private void OnTitleSelectedSafely()
    {
        Debug.Log("Ÿ��Ʋ�� �̵� ���õ�!");

        if (GameManager.Instance != null)
        {
            GameManager.Instance.GoToTitle();
        }
        else
        {
            Debug.Log("GameManager�� ��� Ÿ��Ʋ �̵� �ùķ��̼�");
        }

        ClosePopupSafely();
    }

    private void ClosePopupSafely()
    {
        StopAllCoroutines();
        gameObject.SetActive(false);
    }
    #endregion

    #region ���� �޼���
    private void SetAlpha(Image image, float alpha)
    {
        if (image == null) return;
        Color color = image.color;
        color.a = alpha;
        image.color = color;
    }

    private void SetAlpha(TextMeshProUGUI text, float alpha)
    {
        if (text == null) return;
        Color color = text.color;
        color.a = alpha;
        text.color = color;
    }
    #endregion
}