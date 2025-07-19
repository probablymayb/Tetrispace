using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class EnforcementIconPair
{
    public PlayerEnforcement enforcement;
    public Sprite sprite;
}

public class UI_EnforcementIcon : UI_Base
{
    enum Images
    {
        IconOne,
        IconTwo,
        IconThree,
        IconFour,
        IconFive,
        IconSix,
    }
    
    [SerializeField] private List<EnforcementIconPair> enforcementIcons = new();
    private Dictionary<PlayerEnforcement, Sprite> spriteDict;

    private void Awake()
    {
        spriteDict = new Dictionary<PlayerEnforcement, Sprite>();
        foreach (var pair in enforcementIcons)
        {
            if (!spriteDict.ContainsKey(pair.enforcement))
                spriteDict.Add(pair.enforcement, pair.sprite);
        }
        Init();
    }

    public override void Init()
    {
        Bind<Image>(typeof(Images));
    }

    public void ShowIcons(List<PlayerEnforcement> levelUpHistory)
    {
        for (int i = 0; i < 6; i++)
        {
            if(i < levelUpHistory.Count) GetImage((int)Images.IconOne + i).sprite = spriteDict[levelUpHistory[i]];
            else GetImage((int)Images.IconOne + i).sprite = null;
        }
    }
}