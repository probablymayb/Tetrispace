// UI_GameOverPopup.cs (안전한 버전 - 널체크 완비)
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// 안전한 게임오버 팝업 UI (널체크 완비)
/// 일부 요소가 없어도 오류 없이 동작
/// </summary>
public class UI_GameOverPopup : UI_Base
{
    #region UI 요소 정의
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

    #region 설정값들
    [Header("=== 페이드 설정 ===")]
    [SerializeField] private float backgroundFadeDuration = 1.0f;
    [SerializeField] private float panelFadeDuration = 0.8f;
    [SerializeField] private float textFadeDuration = 0.5f;

    [Header("=== 애니메이션 설정 ===")]
    [SerializeField] private float gameOverTypingSpeed = 0.1f;
    [SerializeField] private float scoreCountUpDuration = 1.5f;
    [SerializeField] private float buttonAppearDelay = 1.0f;

    [Header("=== 색상 설정 ===")]
    [SerializeField] private Color gameOverColor = new Color(1f, 0.2f, 0.2f, 1f);
    [SerializeField] private Color normalScoreColor = Color.white;
    [SerializeField] private Color newRecordColor = Color.yellow;
    [SerializeField] private Color selectedButtonColor = Color.yellow;
    [SerializeField] private Color normalButtonColor = Color.white;

    [Header("=== 수동 할당 (선택사항) ===")]
    [SerializeField] private TextMeshProUGUI manualGameOverText;
    [SerializeField] private TextMeshProUGUI manualScoreText;
    [SerializeField] private Button manualRestartButton;
    [SerializeField] private Button manualTitleButton;
    #endregion

    #region 내부 변수들
    private int currentScore;
    private int previousHighScore;
    private int newHighScore;
    private bool isNewRecord = false;
    private bool isAnimating = true;
    private bool canNavigate = false;

    // 버튼 네비게이션
    private int selectedButtonIndex = 0;
    private Button[] navigationButtons;
    private TextMeshProUGUI[] buttonTexts;

    // 안전한 UI 참조
    private TextMeshProUGUI safeGameOverText;
    private TextMeshProUGUI safeScoreText;
    private TextMeshProUGUI safeHighScoreText;
    private Button safeRestartButton;
    private Button safeTitleButton;
    #endregion

    #region 초기화 (안전 버전)
    public override void Init()
    {
        Debug.Log("=== 게임오버 UI 초기화 시작 ===");

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
            Debug.LogError($"게임오버 UI 초기화 실패: {e.Message}");
            // 최소한의 기능이라도 동작하도록
            SetupFallbackUI();
        }
    }

    /// <summary>
    /// 안전한 UI 바인딩 (오류 방지)
    /// </summary>
    private void BindUIElementsSafely()
    {
        try
        {
            // 기존 바인딩 시도
            Bind<TextMeshProUGUI>(typeof(Texts));
            Bind<Button>(typeof(Buttons));
            Bind<Image>(typeof(Images));
            Bind<GameObject>(typeof(GameObjects));

            Debug.Log("자동 바인딩 성공!");
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"자동 바인딩 실패: {e.Message}. 수동 바인딩으로 전환합니다.");
        }

        // 안전한 참조 설정
        SetupSafeReferences();
    }

    /// <summary>
    /// 안전한 참조 설정
    /// </summary>
    private void SetupSafeReferences()
    {
        // 1순위: 수동 할당된 것들
        safeGameOverText = manualGameOverText;
        safeScoreText = manualScoreText;
        safeRestartButton = manualRestartButton;
        safeTitleButton = manualTitleButton;

        // 2순위: 자동 바인딩된 것들
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

        // 3순위: 이름으로 찾기
        if (safeGameOverText == null)
            safeGameOverText = GameObject.Find("GameOverText")?.GetComponent<TextMeshProUGUI>();

        if (safeScoreText == null)
            safeScoreText = GameObject.Find("ScoreValue")?.GetComponent<TextMeshProUGUI>();

    }

    /// <summary>
    /// 안전한 GetText (널체크 포함)
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
    /// 안전한 GetButton (널체크 포함)
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
    /// 게임 데이터 로드
    /// </summary>
    private void LoadGameData()
    {
        // GameManager가 있으면 데이터 가져오기
        if (GameManager.Instance != null)
        {
            currentScore = GameManager.Instance.CurrentScore;
            previousHighScore = GameManager.Instance.HighScore;
        }
        else
        {
            // 테스트용 기본값
            currentScore = 4000;
            previousHighScore = 8000;
            Debug.LogWarning("GameManager가 없어서 테스트 데이터 사용");
        }

        // 신기록 체크
        if (currentScore > previousHighScore)
        {
            isNewRecord = true;
            newHighScore = currentScore;
        }
        else
        {
            newHighScore = previousHighScore;
        }

        Debug.Log($"현재점수: {currentScore}, 최고점수: {previousHighScore}, 신기록: {isNewRecord}");
    }

    /// <summary>
    /// 안전한 초기 상태 설정
    /// </summary>
    private void SetupInitialStateSafely()
    {
        // 텍스트 설정
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

        // 이미지들 투명하게 (있다면)
        for (int i = 0; i < System.Enum.GetValues(typeof(Images)).Length; i++)
        {
            try
            {
                var image = GetImage(i);
                if (image != null) SetAlpha(image, 0f);
            }
            catch { }
        }

        Debug.Log("초기 상태 설정 완료");
    }

    /// <summary>
    /// 안전한 네비게이션 설정
    /// </summary>
    private void SetupNavigationSafely()
    {
        var buttons = new System.Collections.Generic.List<Button>();
        var texts = new System.Collections.Generic.List<TextMeshProUGUI>();

        // 버튼들 수집
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

        Debug.Log($"네비게이션 설정 완료: {navigationButtons.Length}개 버튼");
    }

    /// <summary>
    /// 안전한 이벤트 등록
    /// </summary>
    private void RegisterEventsSafely()
    {
        // 버튼 이벤트 등록
        if (safeRestartButton != null)
        {
            safeRestartButton.onClick.AddListener(() => OnRestartSelectedSafely());
        }

        if (safeTitleButton != null)
        {
            safeTitleButton.onClick.AddListener(() => OnTitleSelectedSafely());
        }

        Debug.Log("이벤트 등록 완료");
    }

    /// <summary>
    /// 폴백 UI 설정 (최악의 경우)
    /// </summary>
    private void SetupFallbackUI()
    {
        Debug.Log("폴백 UI 설정 시작");

        // 기본 텍스트 생성
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

        Debug.Log("폴백 UI 설정 완료");
    }
    #endregion

    #region 안전한 애니메이션
    /// <summary>
    /// 안전한 애니메이션 재생
    /// </summary>
    private IEnumerator PlayCompleteAnimationSafely()
    {
        isAnimating = true;
        canNavigate = false;

        Debug.Log("애니메이션 시작");

        // 게임오버 텍스트 표시
        if (safeGameOverText != null)
        {
            yield return StartCoroutine(TypeGameOverTextSafely());
        }

        yield return new WaitForSeconds(0.5f);

        // 점수 표시
        if (safeScoreText != null)
        {
            yield return StartCoroutine(CountUpScoreSafely());
        }

        yield return new WaitForSeconds(0.5f);

        // 하이스코어 표시
        if (safeHighScoreText != null)
        {
            yield return StartCoroutine(FadeInTextSafely(safeHighScoreText, textFadeDuration));
        }

        // 신기록 효과
        if (isNewRecord)
        {
            yield return StartCoroutine(PlayNewRecordEffectSafely());
        }

        yield return new WaitForSeconds(buttonAppearDelay);

        // 버튼 표시
        yield return StartCoroutine(ShowButtonsSafely());

        // 네비게이션 활성화
        canNavigate = true;
        isAnimating = false;

        Debug.Log("애니메이션 완료! 네비게이션 활성화");
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

        // 깜빡임 효과
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

        Debug.Log("신기록 효과 재생!");

        // 점수 텍스트 깜빡임
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

        // 하이스코어 업데이트
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
        // 버튼 텍스트들 페이드인
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

    #region 안전한 네비게이션
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
            Debug.Log($"버튼 선택: {selectedButtonIndex}");
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

    #region 안전한 버튼 이벤트
    private void OnRestartSelectedSafely()
    {
        Debug.Log("재시작 선택됨!");

        if (GameManager.Instance != null)
        {
            GameManager.Instance.RestartGame();
        }
        else
        {
            Debug.Log("GameManager가 없어서 재시작 시뮬레이션");
        }

        ClosePopupSafely();
    }

    private void OnTitleSelectedSafely()
    {
        Debug.Log("타이틀로 이동 선택됨!");

        if (GameManager.Instance != null)
        {
            GameManager.Instance.GoToTitle();
        }
        else
        {
            Debug.Log("GameManager가 없어서 타이틀 이동 시뮬레이션");
        }

        ClosePopupSafely();
    }

    private void ClosePopupSafely()
    {
        StopAllCoroutines();
        gameObject.SetActive(false);
    }
    #endregion

    #region 헬퍼 메서드
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