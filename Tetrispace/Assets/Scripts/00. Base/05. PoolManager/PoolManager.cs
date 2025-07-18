using System.Collections.Generic;
using UnityEngine;

/// <summary>
///오브젝트 풀 매니저
/// 
/// 개선점:
/// - String key 사용으로 안정성 향상
/// - Return 시 프리팹 참조 불필요
/// - Singleton 패턴 일관성 적용
/// 
/// 사용법:
/// 1. CreatePool(prefab, size) - 풀 생성
/// 2. Get(prefab) - 오브젝트 가져오기
/// 3. Return(obj) - 오브젝트 반환 (프리팹 참조 불필요!)
/// </summary>
public class PoolManager : Singleton<PoolManager>
{
    private Dictionary<string, ObjectPool> poolDict = new Dictionary<string, ObjectPool>();

    protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// 오브젝트 풀 생성
    /// </summary>
    /// <param name="prefab">풀링할 프리팹</param>
    /// <param name="initialSize">초기 생성 개수</param>
    public void CreatePool(GameObject prefab, int initialSize = 10)
    {
        string key = prefab.name;  // 프리팹 이름을 key로 사용

        if (poolDict.ContainsKey(key))
        {
            Debug.LogWarning($"[PoolManager] {key} 풀이 이미 존재합니다.");
            return;
        }

        Debug.Log($"[PoolManager] {key} 풀 생성됨 (크기: {initialSize})");

        ObjectPool pool = new ObjectPool(prefab, initialSize);
        poolDict.Add(key, pool);
    }

    /// <summary>
    /// 풀에서 오브젝트 가져오기
    /// </summary>
    /// <param name="prefab">가져올 프리팹</param>
    /// <returns>활성화된 게임 오브젝트</returns>
    public GameObject Get(GameObject prefab)
    {
        string key = prefab.name;

        // 풀이 없으면 자동 생성
        if (!poolDict.ContainsKey(key))
        {
            Debug.LogWarning($"[PoolManager] {key} 풀이 없습니다. 자동 생성합니다.");
            CreatePool(prefab, 5);
        }

        return poolDict[key].GetObject();
    }

    /// <summary>
    /// 개선! 오브젝트를 풀에 반환 (프리팹 참조 불필요)
    /// </summary>
    /// <param name="obj">반환할 게임 오브젝트</param>
    public void Return(GameObject obj)
    {
        // "(Clone)" 제거해서 원본 프리팹 이름 추출
        string prefabName = obj.name.Replace("(Clone)", "").Trim();

        if (poolDict.ContainsKey(prefabName))
        {
            poolDict[prefabName].ReturnObject(obj);
        }
        else
        {
            Debug.LogWarning($"[PoolManager] {prefabName}에 해당하는 풀을 찾을 수 없습니다. 오브젝트를 파괴합니다.");
            Destroy(obj);
        }
    }

    /// <summary>
    /// 특정 풀의 정보 확인 (디버깅용)
    /// </summary>
    /// <param name="prefabName">확인할 프리팹 이름</param>
    public void GetPoolInfo(string prefabName)
    {
        if (poolDict.ContainsKey(prefabName))
        {
            Debug.Log($"[PoolManager] {prefabName} 풀 존재함");
        }
        else
        {
            Debug.Log($"[PoolManager] {prefabName} 풀 없음");
        }
    }

    /// <summary>
    /// 모든 풀 정리 (씬 전환 시 사용)
    /// </summary>
    public void ClearAllPools()
    {
        foreach (var pool in poolDict.Values)
        {
            // ObjectPool에 정리 메서드가 있다면 호출
            // pool.Clear();
        }
        poolDict.Clear();
        Debug.Log("[PoolManager] 모든 풀이 정리되었습니다.");
    }

    // 디버그 정보 표시
    void OnGUI()
    {
        if (Application.isPlaying)
        {
            GUILayout.BeginArea(new Rect(10, 100, 300, 200));
            GUILayout.Label($"활성 풀 개수: {poolDict.Count}");

            foreach (var kvp in poolDict)
            {
                GUILayout.Label($"- {kvp.Key}");
            }

            GUILayout.EndArea();
        }
    }
}