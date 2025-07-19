using UnityEngine;


public class DataManager : Singleton<DataManager>
{
    
    #region EnemyData
    private EnemyDataTable enemyDataTable;

    /// <summary>
    /// EnemyType에 맞는 EnemyData를 반환해주는 함수
    /// </summary>
    /// <param name="type">EnemyType은 Flags지만, 하나의 Enum만 선택해 넘겨줘야 올바른 Data 전달 가능</param>
    /// <returns></returns>
    public EnemyData GetEnemyData(EnemyType type)
    {
        return enemyDataTable.GetData(type);
    }
    #endregion

    protected override void Awake()
    {
        base.Awake();
        if (!enemyDataTable)
        {
            enemyDataTable = Resources.Load<EnemyDataTable>("Data/EnemyDataTable");
        }
    }
}