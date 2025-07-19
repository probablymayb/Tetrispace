using UnityEngine;

public class MainCamera : MonoBehaviour
{
    [Header("Target Settings")]
    [SerializeField] private Transform target;  // 플레이어의 Transform
    [SerializeField] private Vector3 offset = new Vector3(0f, 5f, -7f);  // 카메라와 플레이어 간의 거리

    [Header("Movement Settings")]
    [SerializeField] private float smoothSpeed = 5f;  // 카메라 이동 부드러움 정도

    private void LateUpdate()
    {

    }
}
