using UnityEngine;
using System.Collections.Generic;
using System.Collections;

/// <summary>
/// ?? ��Ʈ���̳� �Ŵ��� (�������)
/// 
/// ���:
/// - ��Ʈ���̳� ���� �� ����
/// - ���� ��� �ý���
/// - ���� ���� üũ
/// - ���� �� �ӵ� ����
/// </summary>
/// 
public class testspawner : MonoBehaviour
{
    [SerializeField] private GameObject test;
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.U))
        {
        Instantiate(test, new Vector3(-0.15f, 4.83f, 0), Quaternion.identity);
        }
    }
}