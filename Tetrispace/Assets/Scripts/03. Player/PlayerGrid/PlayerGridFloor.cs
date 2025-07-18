using UnityEngine;
using System.Collections.Generic;

/// <summary>
// 블록 - 플레이어 그리드 연동 스크립트
/// </summary>
public class PlayerGridFloor : MonoBehaviour
{
    [SerializeField] private bool _isLocked = false;


    void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"Trigger Enter: {other.name} [{other.tag}], isLocked={_isLocked}");

        if (!_isLocked && (other.CompareTag("Floor") || other.CompareTag("LockedBlock")))
        {
            Debug.Log("Before LockToGrid()");
            LockToGrid();
        }
    }

    //void OnCollisionEnter2D(Collision2D collision)
    //{
    //    Debug.Log("Collision ENter");

    //    //// 바닥 혹은 이미 고정된 블록에 닿으면 착지
    //    //if (!_isLocked && (collision.CompareTag("Floor") || collision.CompareTag("LockedBlock")))
    //    //{
    //    //    Debug.Log("Before lcoked");

    //    //    LockToGrid();
    //    //}
    //}

    void LockToGrid()
    {

        _isLocked = true;
        // Rigidbody 정지
        var rb = GetComponent<Rigidbody2D>();
        if (rb) Destroy(rb);

        // 자식 블록(각 사각형) 들고 하나씩 셀에 고정
        foreach (Transform child in transform)
        {
            Vector2 worldPos = child.position;

            Debug.Log($"Checking block at {worldPos}");

            // 이 부분은 CellTrigger 스크립트에서 좌표를 알려줘도 되고,
            // 간단하게 가장 가까운 셀 트리거를 Physics2D.OverlapPoint로 찾을 수도 있습니다.
            Collider2D cell = Physics2D.OverlapPoint(worldPos, LayerMask.GetMask("Cell"));
            if (cell != null)
            {
                Debug.Log("Z pos of child: " + child.position.z);
                // 셀의 중심으로 자식 블록 스냅
                child.position = cell.transform.position;
                // 고정 블록으로 태그 변경
                child.gameObject.tag = "LockedBlock";
                Debug.Log(child.gameObject.tag);

                Vector2Int gridPos = TetriminoManager.Instance.WorldToGrid(child.position);
                TetriminoManager.Instance.RegisterBlock(gridPos, child.gameObject);

                // 필요하다면 Rigidbody2D/Collider2D 세팅(Static Collider) 추가
            }
            else
            {
                Debug.LogWarning($"No Cell found at position {worldPos}");
                // 그리드 밖에 튕겨나간 블록은 그냥 삭제
                //Destroy(child.gameObject);
            }
        }

        // 부모(테트로미노) 오브젝트는 더 이상 필요 없으니 삭제
        //Destroy(gameObject);
        Debug.Log("Locked!!!!!");

        // 줄이 가득 찼는지 체크
        TetriminoManager.Instance.CheckAndClearLines();

    }
}