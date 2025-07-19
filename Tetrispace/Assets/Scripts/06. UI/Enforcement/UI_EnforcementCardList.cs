using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_EnforcementCardList : UI_Base
{
    private int nowSelect;
    private int maxCardIdx;
    [SerializeField] private List<Sprite> cardPanels = new();
    enum Cards
    {
        CardOne,
        CardTwo,
        CardThree,
    }
    
    enum Images
    {
        CardOne,
        CardTwo,
        CardThree,
    }
    
    void Awake()
    {
        Init();
    }

    public override void Init()
    {
        Bind<UI_EnforcementCard>(typeof(Cards));
        Bind<Image>(typeof(Images));
        Get<UI_EnforcementCard>((int)Cards.CardOne).Init();
        Get<UI_EnforcementCard>((int)Cards.CardTwo).Init();
        Get<UI_EnforcementCard>((int)Cards.CardThree).Init();
    }

    public void ShowCards(List<EnforcementCardInfo> cardInfos)
    {
        if (cardInfos.Count == 0) return;
        nowSelect = Mathf.Min(cardInfos.Count - 1, 1);
        maxCardIdx = cardInfos.Count - 1;

        for (int i = 0; i < 3; i++)
        {
            if (i >= cardInfos.Count)
            {
                Get<UI_EnforcementCard>((int)Cards.CardOne+i).gameObject.SetActive(false);
            }
            else
            {
                Get<UI_EnforcementCard>((int)Cards.CardOne+i).gameObject.SetActive(true);
                Get<UI_EnforcementCard>((int)Cards.CardOne+i).ShowCard(cardInfos[i]);
            }
        }
        
        HoverCard(nowSelect);
    }

    public void MoveHover(int dir)
    {
        nowSelect = Mathf.Clamp(nowSelect + dir, 0, maxCardIdx);
        HoverCard(nowSelect);
    }

    private void HoverCard(int idx)
    {
        GetImage((int)Images.CardOne).sprite = cardPanels[0];
        GetImage((int)Images.CardTwo).sprite = cardPanels[0];
        GetImage((int)Images.CardThree).sprite = cardPanels[0];
        GetImage((int)Images.CardOne + idx).sprite = cardPanels[1];
    }

    public PlayerEnforcement SelectNowHovered()
    {
        return Get<UI_EnforcementCard>(nowSelect).SelectCard();
    }
}
