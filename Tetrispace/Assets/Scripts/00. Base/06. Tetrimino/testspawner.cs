using UnityEngine;
using System.Collections.Generic;
using System.Collections;

/// <summary>
/// ?? 테트리미노 매니저 (게임잼용)
/// 
/// 기능:
/// - 테트리미노 생성 및 관리
/// - 다음 블록 시스템
/// - 게임 오버 체크
/// - 레벨 및 속도 관리
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