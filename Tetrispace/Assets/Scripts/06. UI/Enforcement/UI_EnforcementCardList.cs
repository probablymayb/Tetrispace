using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_EnforcementCardList : UI_Base
{
    private int nowSelect;
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
        Get<UI_EnforcementCard>((int)Cards.CardOne).ShowCard(cardInfos[0]);
        Get<UI_EnforcementCard>((int)Cards.CardTwo).ShowCard(cardInfos[1]);
        Get<UI_EnforcementCard>((int)Cards.CardThree).ShowCard(cardInfos[2]);
        
        nowSelect = 1;
        HoverCard(nowSelect);
    }

    public void MoveHover(int dir)
    {
        nowSelect = Mathf.Clamp(nowSelect + dir, 0, 2);
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
