using UnityEngine;
using System.Collections.Generic;
using System.Collections;

/// <summary>
/// 수정된 PlayerGridFloor - 그리드 외부 블록 처리 개선
/// 
/// 주요 수정사항:
/// 1. 락 처리 전에 유효한 자식이 있는지 미리 체크
/// 2. 모든 자식이 그리드 외부면 전체 오브젝트 파괴
/// 3. 일부만 유효해도 락 처리 진행
/// </summary>
public class PlayerGridFloor : MonoBehaviour
{
    [SerializeField] private bool _isLocked = false;
    private bool _isProcessing = false;
    private bool _isFading = false;

    private bool _isGameOver = false;

    private HashSet<Transform> registeredChildren = new HashSet<Transform>();

    [Header("페이드 아웃 설정")]
    [SerializeField] private float fadeOutDuration = 2f;

    [Header("커스텀 Fade 설정")]
    [SerializeField] private string fadePropertyName = "_Fade";

    void OnTriggerEnter2D(Collider2D other)
    {
        if (_isFading) return;
        if (_isProcessing || _isLocked) return;

        if (TetriminoManager.Instance != null && TetriminoManager.Instance.IsGridMoving)
        {
            return;
        }

        if (other.CompareTag("Wall") && !gameObject.CompareTag("UselessBlock"))
        {
            Debug.Log("*** Wall 충돌 - 커스텀 Fade 페이드아웃 시작! ***");
            StartCustomFadeOut();
        }
        else if (other.CompareTag("Floor"))
        {
            StartLockProcess();
        }
        else if (other.CompareTag("LockedBlock") || other.CompareTag("UselessBlock"))
        {
            CheckAndLock();
        }
    }

    #region === 페이드 시스템 ===

    /// <summary>
    /// 간단한 페이드아웃 시작
    /// </summary>
    private void StartCustomFadeOut()
    {
        if (_isFading) return;
        _isFading = true;

        // 물리 비활성화
        SafeDisablePhysics();

        // TetriminoController 비활성화
        TetriminoController tc = GetComponent<TetriminoController>();
        if (tc != null)
        {
            tc.enabled = false;
        }

        StartCoroutine(SimpleFadeOutCoroutine());
    }

    /// <summary>
    /// 물리 비활성화
    /// </summary>
    private void SafeDisablePhysics()
    {
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
#if UNITY_2023_2_OR_NEWER
            rb.linearVelocity = Vector2.zero;
#else
            rb.velocity = Vector2.zero;
#endif
            rb.isKinematic = true;
        }
    }

    /// <summary>
    /// 간단한 페이드 코루틴
    /// </summary>
    private IEnumerator SimpleFadeOutCoroutine()
    {
        Debug.Log("페이드아웃 시작");

        float fadeTimer = 0f;
        SpriteRenderer[] renderers = GetComponentsInChildren<SpriteRenderer>(true);

        while (fadeTimer < fadeOutDuration && !_isLocked)
        {
            fadeTimer += Time.deltaTime;
            float fadeValue = Mathf.Lerp(1f, 0f, fadeTimer / fadeOutDuration);

            // 모든 렌더러에 적용
            foreach (SpriteRenderer renderer in renderers)
            {
                if (renderer != null && renderer.material != null && renderer.material.HasProperty(fadePropertyName))
                {
                    renderer.material.SetFloat(fadePropertyName, fadeValue);
                }
            }

            yield return null;
        }

        // 완전 투명
        foreach (SpriteRenderer renderer in renderers)
        {
            if (renderer != null && renderer.material != null && renderer.material.HasProperty(fadePropertyName))
            {
                renderer.material.SetFloat(fadePropertyName, 0f);
            }
        }

        Debug.Log("페이드아웃 완료 - 오브젝트 파괴");
        Destroy(gameObject);
    }

    #endregion

    #region === 개선된 락 시스템 ===

    /// <summary>
    /// 락 프로세스 시작 (유효성 검사 추가)
    /// </summary>
    private void StartLockProcess()
    {
        if (_isProcessing || _isLocked || _isFading) return;

        _isProcessing = true;
        Debug.Log($"락 프로세스 시작: {gameObject.name}");

        // 유효한 자식이 있는지 미리 체크
        if (HasValidChildren())
        {
            Debug.Log("모든 자식이 그리드 내부에 있음 - 락 처리 진행");
            LockToGrid();
        }
        else
        {
            Debug.Log("일부 또는 모든 자식이 그리드 외부에 있음 - 전체 오브젝트 파괴");
            DestroyEntireObject();
        }
    }

    /// <summary>
    /// 유효한 자식이 있는지 확인 (모든 자식이 그리드 내부에 있어야 함)
    /// </summary>
    private bool HasValidChildren()
    {
        if (TetriminoManager.Instance == null) return false;

        int totalChildren = 0;
        int validChildren = 0;

        foreach (Transform child in transform)
        {
            if (registeredChildren.Contains(child)) continue;

            totalChildren++;
            Vector2Int gridPos = TetriminoManager.Instance.WorldToGrid(child.position);

            // 그리드 내부에 있거나, Cell이 있는 위치라면 유효
            if (IsValidGridPosition(gridPos, child.position))
            {
                validChildren++;
                Debug.Log($"유효한 자식: {child.name} at Grid({gridPos.x}, {gridPos.y})");
            }
            else
            {
                Debug.Log($"무효한 자식: {child.name} at Grid({gridPos.x}, {gridPos.y}) - 그리드 외부");
            }
        }

        Debug.Log($"자식 검사 결과: {validChildren}/{totalChildren} 유효");

        // 모든 자식이 유효해야만 true 반환
        bool allValid = (totalChildren > 0) && (validChildren == totalChildren);

        if (!allValid && totalChildren > 0)
        {
            Debug.Log("일부 자식이 그리드 외부에 있음 - 전체 파괴 필요");
        }

        return allValid;
    }

    /// <summary>
    /// 그리드 위치가 유효한지 확인
    /// </summary>
    private bool IsValidGridPosition(Vector2Int gridPos, Vector3 worldPos)
    {
        // 1. 그리드 범위 체크 (Y=0 허용, Y=-1부터 무효)
        if (gridPos.x < 0 || gridPos.x >= 4 || gridPos.y < 0)
        {
            return false;
        }

        // 2. Cell 오버랩 체크
        Collider2D cell = Physics2D.OverlapPoint(worldPos, LayerMask.GetMask("Cell"));
        if (cell != null)
        {
            return true;
        }

        // 3. Y=0에서는 UselessBlock 위라도 허용
        if (gridPos.y == 0)
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// 전체 오브젝트 파괴
    /// </summary>
    private void DestroyEntireObject()
    {
        Debug.Log($"전체 오브젝트 파괴: {gameObject.name} (유효한 자식 없음)");

        // 코루틴 정지
        StopAllCoroutines();

        // 즉시 파괴
        Destroy(gameObject);
    }

    /// <summary>
    /// 체크 후 락 (기존 로직)
    /// </summary>
    private void CheckAndLock()
    {
        if (_isProcessing || _isLocked || _isFading) return;

        bool shouldLock = false;
        foreach (Transform child in transform)
        {
            if (TetriminoManager.Instance != null)
            {
                Vector2Int gridPos = TetriminoManager.Instance.WorldToGrid(child.position);
                if (gridPos.y <= 0 || TetriminoManager.Instance.IsLocked(gridPos.x, gridPos.y - 1))
                {
                    shouldLock = true;
                    break;
                }
            }
        }

        if (shouldLock)
        {
            StartLockProcess();
        }
    }

    /// <summary>
    /// 개선된 락 처리 (등록 중에도 전체 유효성 재검사)
    /// </summary>
    void LockToGrid()
    {
        if (_isLocked) return;

        Debug.Log($"락 처리 시작: {gameObject.name}");

        _isLocked = true;

        // 컴포넌트 제거는 자식 처리 후에
        List<Transform> childrenToProcess = new List<Transform>();
        foreach (Transform child in transform)
        {
            childrenToProcess.Add(child);
        }

        int totalChildren = childrenToProcess.Count;
        Debug.Log($"처리할 자식 수: {totalChildren}");

        // 1단계: 모든 자식의 최종 유효성 검사
        if (!ValidateAllChildrenForRegistration(childrenToProcess))
        {
            Debug.Log("등록 전 최종 검사 실패 - 전체 오브젝트 파괴");
            Destroy(gameObject);
            return;
        }

        // 2단계: 모든 자식이 유효하므로 등록 진행
        int successCount = ProcessAllChildren(childrenToProcess);

        Debug.Log($"자식 처리 결과: {successCount}/{totalChildren} 성공");

        // 3단계: 등록 중 실패가 발생했다면 전체 파괴
        if (successCount != totalChildren)
        {
            Debug.Log($"등록 중 실패 발생 ({successCount}/{totalChildren}) - 전체 오브젝트 파괴");
            // 이미 등록된 블록들도 정리
            CleanupRegisteredBlocks();
            Destroy(gameObject);
            return;
        }

        // 4단계: 모든 등록 성공 시 컴포넌트 정리
        CleanupComponents();

        // 라인 체크
        Invoke(nameof(CheckLinesDelayed), 0.1f);
        _isProcessing = false;

        Debug.Log($"락 처리 완료: {gameObject.name} - 모든 자식 등록 성공");
    }

    /// <summary>
    /// 등록 전 모든 자식의 최종 유효성 검사
    /// </summary>
    private bool ValidateAllChildrenForRegistration(List<Transform> children)
    {
        if (TetriminoManager.Instance == null)
        {
            Debug.Log("TetriminoManager 없음");
            return false;
        }

        int validCount = 0;
        int totalCount = 0;

        foreach (Transform child in children)
        {
            if (registeredChildren.Contains(child)) continue;

            totalCount++;
            Vector3 worldPos = child.position;
            Vector2Int gridPos = TetriminoManager.Instance.WorldToGrid(worldPos);

            if (IsValidGridPosition(gridPos, worldPos))
            {
                validCount++;
                Debug.Log($"등록 전 검사 - 유효: {child.name} at Grid({gridPos.x}, {gridPos.y})");
            }
            else
            {
                Debug.Log($"등록 전 검사 - 무효: {child.name} at Grid({gridPos.x}, {gridPos.y})");
                break; // 하나라도 무효하면 즉시 중단
            }
        }

        bool allValid = (totalCount > 0) && (validCount == totalCount);
        Debug.Log($"등록 전 최종 검사: {validCount}/{totalCount} 유효 - {(allValid ? "통과" : "실패")}");

        return allValid;
    }

    /// <summary>
    /// 모든 자식 처리 (유효성이 보장된 상태)
    /// </summary>
    private int ProcessAllChildren(List<Transform> children)
    {
        int successCount = 0;

        foreach (Transform child in children)
        {
            if (ProcessChildBlock(child))
            {
                successCount++;
            }
            else
            {
                // 이미 유효성이 확인된 상태에서 실패했다면 심각한 문제
                Debug.LogError($"유효성 확인된 자식의 등록 실패: {child.name}");
                break; // 즉시 중단
            }
        }

        return successCount;
    }

    /// <summary>
    /// 이미 등록된 블록들 정리
    /// </summary>
    private void CleanupRegisteredBlocks()
    {
        Debug.Log("이미 등록된 블록들 정리 시작");

        foreach (Transform child in registeredChildren)
        {
            if (child != null && TetriminoManager.Instance != null)
            {
                Vector2Int gridPos = TetriminoManager.Instance.WorldToGrid(child.position);
                // 그리드에서 제거 (TetriminoManager에 제거 메서드가 있다면 사용)
                Debug.Log($"등록된 블록 정리: {child.name} from Grid({gridPos.x}, {gridPos.y})");
                Destroy(child.gameObject);
            }
        }

        registeredChildren.Clear();
        Debug.Log("등록된 블록들 정리 완료");
    }

    /// <summary>
    /// 컴포넌트 정리 (분리된 메서드)
    /// </summary>
    private void CleanupComponents()
    {
        // Rigidbody2D 제거
        var rb = GetComponent<Rigidbody2D>();
        if (rb)
        {
            Debug.Log("Rigidbody2D 제거");
            Destroy(rb);
        }

        // TetriminoController 제거
        var tc = GetComponent<TetriminoController>();
        if (tc)
        {
            Debug.Log("TetriminoController 제거");
            Destroy(tc);
        }
    }

    /// <summary>
    /// 자식 블록 처리 (유효성이 보장된 상태에서 호출)
    /// </summary>
    private bool ProcessChildBlock(Transform child)
    {
        if (registeredChildren.Contains(child))
        {
            Debug.Log($"이미 등록된 자식: {child.name}");
            return true; // 이미 등록됨 = 성공으로 간주
        }

        Vector3 worldPos = child.position;

        if (TetriminoManager.Instance != null)
        {
            Vector2Int gridPos = TetriminoManager.Instance.WorldToGrid(worldPos);

            // 이미 유효성이 확인된 상태이므로 추가 검사는 선택적
            if (!IsValidGridPosition(gridPos, worldPos))
            {
                Debug.LogError($"[심각] 유효성 보장된 자식이 무효함: {child.name} at Grid({gridPos.x}, {gridPos.y})");
                return false;
            }

            // Cell에 스냅
            Collider2D cell = Physics2D.OverlapPoint(worldPos, LayerMask.GetMask("Cell"));
            if (cell != null)
            {
                Vector3 cellPosition = cell.transform.position;
                child.position = new Vector3(cellPosition.x, cellPosition.y, child.position.z);
                Debug.Log($"Cell에 스냅: {child.name} to {cellPosition}");
            }

            // 블록 설정
            child.gameObject.tag = "LockedBlock";

            Collider2D childCollider = child.GetComponent<Collider2D>();
            if (childCollider != null)
            {
                childCollider.enabled = true;
            }

            // 최종 위치로 그리드 등록
            Vector2Int finalGridPos = TetriminoManager.Instance.WorldToGrid(child.position);

            try
            {
                TetriminoManager.Instance.RegisterBlock(finalGridPos, child.gameObject);
                registeredChildren.Add(child);
                Debug.Log($"자식 블록 등록 성공: {child.name} at Grid({finalGridPos.x}, {finalGridPos.y})");
                return true;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"그리드 등록 실패: {child.name} - {e.Message}");
                return false;
            }
        }

        Debug.LogError($"TetriminoManager 없음 - 자식 처리 실패: {child.name}");
        return false;
    }

    #endregion

    #region === 기존 메서드들 ===

    private void CheckLinesDelayed()
    {
        if (TetriminoManager.Instance != null && !TetriminoManager.Instance.IsGridMoving)
        {
            TetriminoManager.Instance.CheckAndClearLines();
        }
        else
        {
            Invoke(nameof(CheckLinesDelayed), 0.1f);
        }
    }

    public bool IsFading()
    {
        return _isFading;
    }

    void OnDestroy()
    {
        registeredChildren.Clear();
        StopAllCoroutines();
        Debug.Log($"PlayerGridFloor 파괴됨: {gameObject.name}");
    }

    #endregion

    #region === 디버그 메서드들 ===

    [ContextMenu("Test Custom Fade Out")]
    public void TestCustomFadeOut()
    {
        StartCustomFadeOut();
    }

    [ContextMenu("Check All Children Valid")]
    public void CheckAllChildrenValid()
    {
        Debug.Log("=== 모든 자식 유효성 검사 ===");

        if (TetriminoManager.Instance == null)
        {
            Debug.Log("TetriminoManager 없음");
            return;
        }

        int totalChildren = 0;
        int validChildren = 0;

        foreach (Transform child in transform)
        {
            if (registeredChildren.Contains(child)) continue;

            totalChildren++;
            Vector2Int gridPos = TetriminoManager.Instance.WorldToGrid(child.position);
            bool isValid = IsValidGridPosition(gridPos, child.position);

            if (isValid)
            {
                validChildren++;
            }

            Debug.Log($"{child.name}: Grid({gridPos.x}, {gridPos.y}) - {(isValid ? "✅ 유효" : "❌ 무효")}");
        }

        bool allValid = (totalChildren > 0) && (validChildren == totalChildren);
        Debug.Log($"결과: {validChildren}/{totalChildren} 유효 - {(allValid ? "✅ 락 가능" : "❌ 전체 파괴 필요")}");
    }

    [ContextMenu("Force Lock Process")]
    public void ForceStartLockProcess()
    {
        StartLockProcess();
    }

    #endregion
}