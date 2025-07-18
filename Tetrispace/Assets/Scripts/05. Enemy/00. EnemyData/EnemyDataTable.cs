using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Enemy/EnemyDataTable")]
public class EnemyDataTable : ScriptableObject
{
    [SerializeField] private List<EnemyDataEntry> entries;
    private Dictionary<EnemyType, EnemyData> dict;

    public EnemyData GetData(EnemyType type)
    {
        if (dict == null)
        {
            dict = new Dictionary<EnemyType, EnemyData>();
            foreach (var e in entries)
                dict[e.type] = e.data;
        }

        return dict.GetValueOrDefault(type);
    }
}

[System.Serializable]
public class EnemyDataEntry
{
    public EnemyType type;
    public EnemyData data;
}
