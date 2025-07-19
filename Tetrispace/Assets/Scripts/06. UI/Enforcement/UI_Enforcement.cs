using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class UI_Enforcement : UI_Base
{
    enum IconGroups
    {
        EnforcementIconGroup
    }
    enum CardLists
    {
        EnforcementCardList
    }
    enum Texts
    {
        SelectText
    }
    
    private Dictionary<PlayerEnforcement, int> levels = new Dictionary<PlayerEnforcement, int>();
    private List<PlayerEnforcement> earnedEnforcement = new();
    private Dictionary<PlayerEnforcement, bool> isEnforcementShown = new();
    [SerializeField] private List<EnforcementCardData> cardDatas = new();
    [SerializeField] private float blinkSpeed = 1.5f;
    private Coroutine blinkCoroutine;
    
    private InputAction cardMoveAction;
    private InputAction cardSelectAction;
    private const string CardMoveActionName = "CardMove";
    private const string CardSelectActionName = "Select";
    private const string PlayerActionMap = "PlayerActions";
    private const string UIActionMap = "UIActions";
    
    private void Awake()
    {
        cardMoveAction = InputSystem.actions.FindAction(CardMoveActionName);
        cardSelectAction = InputSystem.actions.FindAction(CardSelectActionName);
        earnedEnforcement = new();
        foreach (PlayerEnforcement playerEnforcement in Enum.GetValues(typeof(PlayerEnforcement)))
        {
            levels.Add(playerEnforcement, 0);
            isEnforcementShown.Add(playerEnforcement, false);
        }
        Init();
    }
    
    public override void Init()
    {
        Bind<UI_EnforcementIcon>(typeof(IconGroups));
        Bind<UI_EnforcementCardList>(typeof(CardLists));
        Bind<TextMeshProUGUI>(typeof(Texts));
        
        Get<UI_EnforcementIcon>((int)IconGroups.EnforcementIconGroup).Init();
        Get<UI_EnforcementCardList>((int)CardLists.EnforcementCardList).Init();
    }

    public void OnEnable()
    {
        if (!CanEnforcement())
        {
            gameObject.SetActive(false);
            return;
        }
        GameManager.Instance.ChangeState(EGameState.Paused);
        InputSystem.actions.FindActionMap(PlayerActionMap)?.Disable();
        InputSystem.actions.FindActionMap(UIActionMap)?.Enable();
        
        cardMoveAction.started += OnMove;
        cardSelectAction.started += OnSelect;
        Get<UI_EnforcementIcon>((int)IconGroups.EnforcementIconGroup).ShowIcons(earnedEnforcement);
        Get<UI_EnforcementCardList>((int)CardLists.EnforcementCardList).ShowCards(GetRandomCardInfos());
        StartBlinking();
    }

    private bool CanEnforcement()
    {
        return levels.Values.Any(level => level < 3);
    }
    
    private void OnMove(InputAction.CallbackContext context)
    {
        Vector2 input = context.ReadValue<Vector2>();
        if (input.x == 0) return;
        Get<UI_EnforcementCardList>((int)CardLists.EnforcementCardList).MoveHover((int)Mathf.Sign(input.x));
    }
    
    private void OnSelect(InputAction.CallbackContext context)
    {
        PlayerEnforcement enforcement = Get<UI_EnforcementCardList>((int)CardLists.EnforcementCardList).SelectNowHovered();
        levels[enforcement]++;
        if(!earnedEnforcement.Contains(enforcement)) earnedEnforcement.Add(enforcement);
        gameObject.SetActive(false);
    }

    public void OnDisable()
    {
        GameManager.Instance.ChangeState(EGameState.Playing);
        InputSystem.actions.FindActionMap(PlayerActionMap)?.Enable();
        InputSystem.actions.FindActionMap(UIActionMap)?.Disable();
        
        cardMoveAction.started -= OnMove;
        cardSelectAction.started -= OnSelect;
        StopBlinking();
    }
    
    private List<EnforcementCardInfo> GetRandomCardInfos(int count = 3)
    {
        List<PlayerEnforcement> candidates = new();

        // 레벨 3 미만만
        foreach (var kvp in levels)
        {
            if (kvp.Value < 3)
                candidates.Add(kvp.Key);
        }

        // 후보가 3개 미만이면 제한
        int finalCount = Mathf.Min(count, candidates.Count);

        // 랜덤 셔플 후 선택
        for (int i = 0; i < candidates.Count; i++)
        {
            int rand = UnityEngine.Random.Range(i, candidates.Count);
            (candidates[i], candidates[rand]) = (candidates[rand], candidates[i]);
        }

        List<EnforcementCardInfo> result = new();

        for (int i = 0; i < finalCount; i++)
        {
            PlayerEnforcement enf = candidates[i];

            // CardData 매칭
            EnforcementCardData cardData = cardDatas.Find(data => data.enforcement == enf);
            if (cardData == null)
            {
                Debug.LogWarning($"CardData not found for {enf}");
                continue;
            }

            bool isNew = !isEnforcementShown[enf];
            int level = levels[enf];

            result.Add(new EnforcementCardInfo(cardData, isNew, level));
            isEnforcementShown[enf] = true;
        }

        return result;
    }

    private void StartBlinking()
    {
        if (blinkCoroutine == null)
            blinkCoroutine = StartCoroutine(BlinkTMPText());
    }

    private void StopBlinking()
    {
        if (blinkCoroutine != null)
        {
            StopCoroutine(blinkCoroutine);
            blinkCoroutine = null;

            // 깜빡임 중지 후 알파 원상복구
            var color = GetText((int)Texts.SelectText).color;
            color.a = 1f;
            GetText((int)Texts.SelectText).color = color;
        }
    }

    private IEnumerator BlinkTMPText()
    {
        while (true)
        {
            float t = Mathf.PingPong(Time.unscaledTime * blinkSpeed, 1f);
            var color = GetText((int)Texts.SelectText).color;
            color.a = t;
            GetText((int)Texts.SelectText).color = color;
            yield return new WaitForSecondsRealtime(0.1f);
        }
    }
}
