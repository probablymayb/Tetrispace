using UnityEngine;

public interface IObjectPool
{
    Component GetObject();
    void ReturnObject(Component obj);
}