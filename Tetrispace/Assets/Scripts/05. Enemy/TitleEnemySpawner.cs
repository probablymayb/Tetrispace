using System;
using UnityEngine;

public class TitleEnemySpawner : MonoBehaviour
{
    private void Start()
    {
        Array values = Enum.GetValues(typeof(EnemyType));
        Debug.Log($"[Init] 자식 개수: {transform.childCount}");

        foreach (Transform child in transform)
        {
            Debug.Log($"[Init] 처리 중: {child.name}");

            EnemyType randomType = (EnemyType)values.GetValue(UnityEngine.Random.Range(0, values.Length));
            var enemy = child.GetComponent<Enemy>();

            if (enemy != null)
            {
                Debug.Log($"[Init] {child.name}에 Enemy 있음. 타입: {randomType}");
                enemy.Init(child.position, randomType, true);
            }
            else
            {
                Debug.LogWarning($"[Init] {child.name}에는 Enemy 컴포넌트가 없음");
            }
        }

        GameManager.OnGameStart -= OnGameStart;
        GameManager.OnGameStart += OnGameStart;
    }

    private void OnGameStart()
    {
        Destroy(gameObject);
    }
}
