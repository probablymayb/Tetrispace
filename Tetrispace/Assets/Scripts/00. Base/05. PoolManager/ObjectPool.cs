using System.Collections.Generic;
using UnityEngine;

public class ObjectPool<T> : IObjectPool where T : Component
{
    private T prefab;
    private Queue<T> pool;
    private Transform poolParent;

    public ObjectPool(T prefab, int initialSize = 10)
    {
        this.prefab = prefab;
        pool = new Queue<T>(initialSize);

        GameObject poolObj = new GameObject($"{prefab.name}Pool");
        poolParent = poolObj.transform;

        for (int i = 0; i < initialSize; i++)
        {
            T obj = Object.Instantiate(prefab, poolParent);
            obj.gameObject.SetActive(false);
            pool.Enqueue(obj);
        }
    }

    public T GetObject()
    {
        T obj = pool.Count > 0 ? pool.Dequeue() : Object.Instantiate(prefab, poolParent);
        obj.transform.SetParent(null);
        obj.gameObject.SetActive(true);
        return obj;
    }

    public void ReturnObject(T obj)
    {
        obj.gameObject.SetActive(false);
        obj.transform.SetParent(poolParent);
        pool.Enqueue(obj);
    }
    
    // IObjectPool Interface
    Component IObjectPool.GetObject() => GetObject();
    void IObjectPool.ReturnObject(Component obj) => ReturnObject(obj as T);
}
