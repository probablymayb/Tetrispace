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
    float posx = 0.14f;
    [SerializeField] private GameObject test;
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.U))
        {
        Instantiate(test, new Vector3(posx, 4.83f, 0), Quaternion.identity);
        posx -= 0.28f;
        }
    }
}