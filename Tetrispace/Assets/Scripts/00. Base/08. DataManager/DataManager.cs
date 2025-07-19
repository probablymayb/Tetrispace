using UnityEngine;


public class DataManager : Singleton<DataManager>
{
    
    #region EnemyData
    private EnemyDataTable enemyDataTable;

    /// <summary>
    /// EnemyType에 맞는 EnemyData를 반환해주는 함수
    /// </summary>
    /// <param name="type">EnemyType 전달</param>
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