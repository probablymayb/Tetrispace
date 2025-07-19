using UnityEngine;
using System.Collections;

/// <summary>
/// 간단한 UselessBlock - 그리드에 등록되지만 라인 클리어에서만 제외
/// </summary>
public class UselessBlock : MonoBehaviour
{
    [Header("=== UselessBlock 설정 ===")]
    [SerializeField] private float lifetime = 10f;
    [SerializeField] private bool enableDebug = false;

    private bool isDestroying = false;

    void Start()
    {

        //// 플레이어를 부모로 설정 (플레이어 Transform 찾기)
        //Transform playerTransform = FindObjectOfType<PlayerController>().transform; // 또는 다른 방법으로 플레이어 참조
        //this.transform.SetParent(playerTransform);

        //// 로컬 위치로 설정 (부모 기준 상대 위치)
        //this.transform.localPosition = new Vector3(1.5f * 0.28f, 0.5f * 0.28f, 0f);

        // UselessBlock 태그 설정 (라인 클리어 제외용)
        gameObject.tag = "UselessBlock";

        // 자식 블록들도 UselessBlock 태그 설정
        SetupBlockParts();

        // 자동 삭제 타이머 시작
        StartCoroutine(LifetimeCoroutine());

        if (enableDebug)
        {
            Debug.Log($"UselessBlock 생성: {name} - {lifetime}초 후 삭제 예정");
        }
    }

    /// <summary>
    /// I자형 블록 구성 요소 설정
    /// </summary>
    void SetupBlockParts()
    {
        // 자식 오브젝트들도 UselessBlock 태그 설정
        Transform[] children = GetComponentsInChildren<Transform>();

        foreach (Transform child in children)
        {
            if (child != transform) // 자기 자신 제외
            {
                child.tag = "UselessBlock";

                // 콜라이더 설정
                Collider2D collider = child.GetComponent<Collider2D>();
                if (collider != null)
                {
                    collider.enabled = true;
                }
            }
        }

        if (enableDebug)
        {
            Debug.Log($"UselessBlock 자식 블록들 설정 완료");
        }
    }

    /// <summary>
    /// 수명 관리 코루틴
    /// </summary>
    private IEnumerator LifetimeCoroutine()
    {
        float timer = 0f;

        while (timer < lifetime && !isDestroying)
        {
            timer += Time.deltaTime;

            // 2초마다 남은 시간 로그
            if (enableDebug && Mathf.FloorToInt(timer) % 2 == 0 && timer >= 2f)
            {
                float remainingTime = lifetime - timer;
                if (remainingTime > 0)
                {
                    Debug.Log($"UselessBlock {name} - 남은 시간: {remainingTime:F1}초");
                }
            }

            yield return null;
        }

        // 시간 만료 시 삭제
        if (!isDestroying)
        {
            DestroyUselessBlock();
        }
    }

    /// <summary>
    /// UselessBlock 삭제 처리
    /// </summary>
    public void DestroyUselessBlock()
    {
        if (isDestroying) return;
        isDestroying = true;

        if (enableDebug)
        {
            Debug.Log($"UselessBlock {name} 삭제 시작");
        }

        // TetriminoManager에서 그리드 정리
        if (TetriminoManager.Instance != null)
        {
            TetriminoManager.Instance.RemoveUselessBlockFromGrid(this);
        }

        TetriminoManager.Instance.ClearLine(0);
        // 오브젝트 파괴
        Destroy(gameObject);
    }

    /// <summary>
    /// 즉시 삭제 (외부에서 호출 가능)
    /// </summary>
    public void ForceDestroy()
    {
        if (enableDebug)
        {
            Debug.Log($"UselessBlock {name} 강제 삭제");
        }

        StopAllCoroutines();
        DestroyUselessBlock();
    }

    /// <summary>
    /// 남은 수명 반환
    /// </summary>
    public float GetRemainingLifetime()
    {
        return Mathf.Max(0f, lifetime - (Time.time - Time.time)); // 정확한 계산 필요시 수정
    }

    void Update()
    {
        // 테스트용 회전
        if (Input.GetKeyDown(KeyCode.Q))
        {
            TurnLeft();
        }
        if (Input.GetKeyDown(KeyCode.E))
        {
            TurnRight();
        }
    }

    public void TurnLeft()
    {
        transform.Rotate(new Vector3(0f, 0f, 1f), -90);
    }

    public void TurnRight()
    {
        transform.Rotate(new Vector3(0f, 0f, 1f), 90);
    }

    void OnDestroy()
    {
        StopAllCoroutines();
    }
}