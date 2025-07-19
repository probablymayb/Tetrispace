using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public struct EnforcementCardInfo
{
    public EnforcementCardData data;
    public bool isNew;
    public int currentLevel;

    public EnforcementCardInfo(EnforcementCardData data, bool isNew, int currentLevel)
    {
        this.data = data;
        this.isNew = isNew;
        this.currentLevel = currentLevel;
    }
}

public class UI_EnforcementCard : UI_Base
{
    enum Texts
    {
        EnforcementName,
        EnforcementDesc,
    }

    enum Images
    {
        NewImage,
        EnforcementIcon,
        EnforcementStat,
    }

    [SerializeField] private List<Sprite> statImages = new();
    private EnforcementCardInfo info;
    
    void Awake()
    {
        Init();
    }

    public override void Init()
    {
        Bind<TextMeshProUGUI>(typeof(Texts));
        Bind<Image>(typeof(Images));
    }

    public void ShowCard(EnforcementCardInfo cardInfo)
    {
        info = cardInfo;
        GetImage((int)Images.NewImage).gameObject.SetActive(cardInfo.isNew);
        GetText((int)Texts.EnforcementName).SetText(cardInfo.data.name);
        GetText((int)Texts.EnforcementDesc).SetText(cardInfo.data.desc[cardInfo.currentLevel]);
        GetImage((int)Images.EnforcementIcon).sprite = cardInfo.data.icon;
        GetImage((int)Images.EnforcementStat).sprite = statImages[cardInfo.currentLevel];
    }

    public PlayerEnforcement SelectCard()
    {
        EventManager.Instance.PlayerEnforcementLevelUp(info.data.enforcement);
        return info.data.enforcement;
    }
}
    