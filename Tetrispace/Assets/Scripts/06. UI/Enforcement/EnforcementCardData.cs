using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "UIData/EnforcementCardData")]
public class EnforcementCardData : ScriptableObject
{
    public PlayerEnforcement enforcement;
    public string name;
    public Sprite icon;
    public List<string> desc;
}
