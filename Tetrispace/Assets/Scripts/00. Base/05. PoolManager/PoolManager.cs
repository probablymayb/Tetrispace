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
    private Dictionary<string, IObjectPool> poolDict = new();

    /// <summary>
    /// 오브젝트 풀 생성
    /// </summary>
    /// <param name="prefab">풀링할 프리팹</param>
    /// <param name="initialSize">초기 생성 개수</param>
    public void CreatePool<T>(T prefab, int size = 10) where T : Component
    {
        string key = prefab.name;
        if (poolDict.ContainsKey(key)) return;

        var pool = new ObjectPool<T>(prefab, size);
        poolDict[key] = pool;
    }

    /// <summary>
    /// 풀에서 오브젝트 가져오기
    /// </summary>
    /// <param name="prefab">가져올 프리팹</param>
    /// <returns>활성화된 게임 오브젝트</returns>
    public T Get<T>(T prefab) where T : Component
    {
        string key = prefab.name;
        if (!poolDict.ContainsKey(key))
            CreatePool(prefab, 5);

        return ((ObjectPool<T>)poolDict[key]).GetObject();
    }

    /// <summary>
    /// 개선! 오브젝트를 풀에 반환 (프리팹 참조 불필요)
    /// </summary>
    /// <param name="obj">반환할 게임 오브젝트</param>
    public void Return<T>(T obj) where T : Component
    {
        string key = obj.name.Replace("(Clone)", "").Trim();

        if (poolDict.ContainsKey(key))
        {
            poolDict[key].ReturnObject(obj);
        }
        else
        {
            Debug.LogWarning($"[PoolManager] {key} 풀 없음 → 파괴");
            Destroy(obj.gameObject);
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