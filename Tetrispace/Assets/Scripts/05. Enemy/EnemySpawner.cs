using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;


/// <summary>
/// 자식으로 SpawnPointParent 오브젝트(인덱스 0)를 하나 두고,
/// 그 아래에 Phase 개수만큼 자식을 두어 각 Phase에 알맞게 스폰포인트를 그 자식으로 둔다.
/// EnemySpawner > SpawnPointParent > Phase (0) > SpawnPoint(0), SpawnPoint(1)...
/// </summary>
public class EnemySpawner : MonoBehaviour
{
    private List<List<Transform>> phaseSpawnPoints = new();
    [SerializeField] private List<EnemyPhaseData> phaseDatas;
    [SerializeField] private Enemy enemyPrefab;
    [SerializeField] private Bounds spawnBounds;

    private int currentPhase;
    
    private void Awake()
    {
        FindSpawnPoints();
        currentPhase = 0;
        PoolManager.Instance.CreatePool(enemyPrefab);
    }

    private void FindSpawnPoints()
    {
        Transform spawnPointParent = transform.GetChild(0);
        foreach (Transform phasePoints in spawnPointParent)
        {
            phaseSpawnPoints.Add(new List<Transform>());
            foreach (Transform spawnPoint in phasePoints)
            {
                phaseSpawnPoints[^1].Add(spawnPoint);
            }
        }
    }
    
    private void Start()
    {
        StartNextPhase();
    }

    private void StartNextPhase()
    {
        EnemyPhaseData phaseData = phaseDatas[currentPhase];
        List<Transform> spawnPoints = phaseSpawnPoints[currentPhase];
        
        SpawnEnemies(phaseData, spawnPoints);
    }

    private void SpawnEnemies(EnemyPhaseData phaseData, List<Transform> spawnPoints)
    {
        int enemyCount = phaseData.enemyCount;
        // 블럭화 되는 적 수 결정
        int tetrisCount = phaseData.isTetrisEnemyCountFixed
            ? phaseData.tetrisCount : Random.Range(0, enemyCount + 1);

        // 랜덤한 블럭 인덱스 생성
        HashSet<int> tetrisIndices = new HashSet<int>();
        while (tetrisIndices.Count < tetrisCount)
        {
            tetrisIndices.Add(Random.Range(0, enemyCount));
        }

        for (int i = 0; i < enemyCount; i++)
        {
            EnemyType selectedType = phaseData.enemyTypes[Random.Range(0, phaseData.enemyTypes.Count)];
            Enemy enemy = PoolManager.Instance.Get(enemyPrefab);

            // 스폰 위치 결정
            Vector3 spawnPos;
            if (i < spawnPoints.Count && spawnPoints[i] != null)
            {
                spawnPos = spawnPoints[i].position;
            }
            else
            {
                spawnPos = new Vector3(
                    Random.Range(spawnBounds.min.x, spawnBounds.max.x),
                    Random.Range(spawnBounds.min.y, spawnBounds.max.y),
                    0f
                );
            }

            enemy.transform.position = spawnPos + Vector3.up * 10f;

            // 이 적이 블럭화 가능인지
            bool isTetris = tetrisIndices.Contains(i);
            enemy.Init(spawnPos, selectedType);
        }
    }
}
