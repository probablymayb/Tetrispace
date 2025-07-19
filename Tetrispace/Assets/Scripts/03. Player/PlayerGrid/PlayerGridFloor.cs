using UnityEngine;
using System.Collections.Generic;
using System.Collections;

/// <summary>
/// 커스텀 Fade 변수를 사용한 페이드아웃
/// 아트 디자이너가 만든 Material의 Fade 값을 조절
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
    [SerializeField] private string fadePropertyName = "_Fade"; // Material의 Fade 변수 이름

    void OnTriggerEnter2D(Collider2D other)
    {
        if (_isFading) return;
        if (_isProcessing || _isLocked) return;

        if (TetriminoManager.Instance != null && TetriminoManager.Instance.IsGridMoving)
        {
            return;
        }

        if (other.CompareTag("Wall"))
        {
            Debug.Log("*** Wall 충돌 - 커스텀 Fade 페이드아웃 시작! ***");
            StartCustomFadeOut();
        }
        else if (other.CompareTag("Floor"))
        {
            StartLockProcess();
        }
        else if (other.CompareTag("LockedBlock"))
        {
            CheckAndLock();
        }
    }

    /// <summary>
    /// 커스텀 Fade 변수를 사용한 페이드아웃 시작
    /// </summary>
    private void StartCustomFadeOut()
    {
        if (_isFading) return;
        _isFading = true;

        // 물리 비활성화
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

        // TetriminoController 비활성화
        TetriminoController tc = GetComponent<TetriminoController>();
        if (tc != null)
        {
            tc.enabled = false;
        }

        StartCoroutine(CustomFadeOutCoroutine());
    }

    /// <summary>
    /// 커스텀 Fade 변수를 사용한 페이드아웃 코루틴
    /// </summary>
    private IEnumerator CustomFadeOutCoroutine()
    {
        Debug.Log("=== 커스텀 Fade 페이드아웃 시작 ===");

        // 렌더러와 머티리얼 정보 수집
        List<MaterialData> materialDataList = new List<MaterialData>();

        // 자신의 렌더러
        SpriteRenderer mainRenderer = GetComponent<SpriteRenderer>();
        if (mainRenderer != null)
        {
            MaterialData data = new MaterialData(mainRenderer, fadePropertyName);
            if (data.isValid)
            {
                materialDataList.Add(data);
            }
        }

        // 자식들의 렌더러
        SpriteRenderer[] childRenderers = GetComponentsInChildren<SpriteRenderer>(true);
        foreach (SpriteRenderer renderer in childRenderers)
        {
            if (renderer != mainRenderer && renderer != null)
            {
                MaterialData data = new MaterialData(renderer, fadePropertyName);
                if (data.isValid)
                {
                    materialDataList.Add(data);
                }
            }
        }

        Debug.Log($"총 {materialDataList.Count}개 Material에서 Fade 변수 발견");

        if (materialDataList.Count == 0)
        {
            Debug.LogWarning("Fade 변수를 가진 Material이 없어서 즉시 파괴");
            Destroy(gameObject);
            yield break;
        }

        // 초기 상태 로그
        foreach (MaterialData data in materialDataList)
        {
            Debug.Log($"{data.renderer.gameObject.name}: " +
                     $"원본 Fade={data.originalFadeValue:F2}");
        }

        // 커스텀 Fade 페이드아웃 실행
        float fadeTimer = 0f;

        while (fadeTimer < fadeOutDuration)
        {
            fadeTimer += Time.deltaTime;
            float fadeProgress = fadeTimer / fadeOutDuration;

            // 1에서 0으로 페이드 (아트 디자이너가 0이 완전 투명이라고 했으니)
            float currentFadeValue = Mathf.Lerp(1f, 0f, fadeProgress);

            // 30프레임마다 로그
            if (Time.frameCount % 30 == 0)
            {
                Debug.Log($"Fade 진행: {fadeTimer:F2}s / {fadeOutDuration:F2}s, Fade값: {currentFadeValue:F3}");
            }

            // 모든 Material에 Fade 값 적용
            foreach (MaterialData data in materialDataList)
            {
                if (data.renderer != null && data.renderer.material != null)
                {
                    data.renderer.material.SetFloat(data.fadePropertyName, currentFadeValue);
                }
            }

            yield return null;
        }

        // 최종 완전 투명 처리
        Debug.Log("최종 Fade 값 0 적용");
        foreach (MaterialData data in materialDataList)
        {
            if (data.renderer != null && data.renderer.material != null)
            {
                data.renderer.material.SetFloat(data.fadePropertyName, 0f);
                Debug.Log($"최종: {data.renderer.gameObject.name}: Fade=0");
            }
        }

        Debug.Log("커스텀 Fade 페이드아웃 완료 - 파괴 대기");
        yield return new WaitForSeconds(0.1f);

        Debug.Log("*** 오브젝트 파괴 ***");
        Destroy(gameObject);
    }

    /// <summary>
    /// Material 데이터 클래스
    /// </summary>
    private class MaterialData
    {
        public SpriteRenderer renderer;
        public string fadePropertyName;
        public float originalFadeValue;
        public bool isValid;

        public MaterialData(SpriteRenderer r, string propertyName)
        {
            renderer = r;
            fadePropertyName = propertyName;
            isValid = false;

            if (r.material != null && r.material.HasProperty(propertyName))
            {
                originalFadeValue = r.material.GetFloat(propertyName);
                isValid = true;
                Debug.Log($"{r.gameObject.name}: Fade 변수 발견 - 현재값: {originalFadeValue:F2}");
            }
            else
            {
                Debug.Log($"{r.gameObject.name}: Fade 변수 없음");
                originalFadeValue = 1f;
            }
        }
    }

    /// <summary>
    /// 테스트용 메서드들
    /// </summary>
    [ContextMenu("Test Custom Fade Out")]
    public void TestCustomFadeOut()
    {
        StartCustomFadeOut();
    }

    [ContextMenu("Check Fade Properties")]
    public void CheckFadeProperties()
    {
        Debug.Log("=== Fade 변수 확인 ===");
        SpriteRenderer[] allRenderers = GetComponentsInChildren<SpriteRenderer>(true);

        foreach (SpriteRenderer renderer in allRenderers)
        {
            Debug.Log($"{renderer.gameObject.name}:");
            Debug.Log($"  - Material: {renderer.material?.name}");

            if (renderer.material != null && renderer.material.HasProperty(fadePropertyName))
            {
                float fadeValue = renderer.material.GetFloat(fadePropertyName);
                Debug.Log($"  - {fadePropertyName}: {fadeValue:F2} ✅");
            }
            else
            {
                Debug.Log($"  - {fadePropertyName}: 없음 ❌");
            }
        }
    }

    [ContextMenu("Set Fade to 0")]
    public void SetFadeToZero()
    {
        SpriteRenderer[] allRenderers = GetComponentsInChildren<SpriteRenderer>(true);

        foreach (SpriteRenderer renderer in allRenderers)
        {
            if (renderer.material != null && renderer.material.HasProperty(fadePropertyName))
            {
                renderer.material.SetFloat(fadePropertyName, 0f);
                Debug.Log($"{renderer.gameObject.name}: Fade를 0으로 설정");
            }
        }
    }

    [ContextMenu("Set Fade to 1")]
    public void SetFadeToOne()
    {
        SpriteRenderer[] allRenderers = GetComponentsInChildren<SpriteRenderer>(true);

        foreach (SpriteRenderer renderer in allRenderers)
        {
            if (renderer.material != null && renderer.material.HasProperty(fadePropertyName))
            {
                renderer.material.SetFloat(fadePropertyName, 1f);
                Debug.Log($"{renderer.gameObject.name}: Fade를 1로 설정");
            }
        }
    }

    [ContextMenu("Set Fade to 0.5")]
    public void SetFadeToHalf()
    {
        SpriteRenderer[] allRenderers = GetComponentsInChildren<SpriteRenderer>(true);

        foreach (SpriteRenderer renderer in allRenderers)
        {
            if (renderer.material != null && renderer.material.HasProperty(fadePropertyName))
            {
                renderer.material.SetFloat(fadePropertyName, 0.5f);
                Debug.Log($"{renderer.gameObject.name}: Fade를 0.5로 설정");
            }
        }
    }

    // === 기존 락 처리 메서드들 (간소화) ===
    private void StartLockProcess()
    {
        if (_isProcessing || _isLocked || _isFading) return;
        _isProcessing = true;
        LockToGrid();
    }

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

    void LockToGrid()
    {
        if (_isLocked) return;
        _isLocked = true;

        var rb = GetComponent<Rigidbody2D>();
        if (rb) Destroy(rb);

        var tc = GetComponent<TetriminoController>();
        if (tc) Destroy(tc);

        List<Transform> childrenToProcess = new List<Transform>();
        foreach (Transform child in transform)
        {
            childrenToProcess.Add(child);
        }

        int successCount = 0;
        foreach (Transform child in childrenToProcess)
        {
            if (ProcessChildBlock(child))
            {
                successCount++;
            }
        }

        Invoke(nameof(CheckLinesDelayed), 0.1f);
        _isProcessing = false;
    }

    private bool ProcessChildBlock(Transform child)
    {
        if (registeredChildren.Contains(child)) return false;

        Vector3 worldPos = child.position;

        if (TetriminoManager.Instance != null)
        {
            Vector2Int gridPos = TetriminoManager.Instance.WorldToGrid(worldPos);

            Collider2D cell = Physics2D.OverlapPoint(worldPos, LayerMask.GetMask("Cell"));
            if (cell != null)
            {
                Vector3 cellPosition = cell.transform.position;
                child.position = new Vector3(cellPosition.x, cellPosition.y, child.position.z);
                child.gameObject.tag = "LockedBlock";

                Collider2D childCollider = child.GetComponent<Collider2D>();
                if (childCollider != null)
                {
                    childCollider.enabled = true;
                }

                Vector2Int finalGridPos = TetriminoManager.Instance.WorldToGrid(child.position);
                TetriminoManager.Instance.RegisterBlock(finalGridPos, child.gameObject);
                registeredChildren.Add(child);

                return true;
            }
            else
            {
               if (gridPos.x < 0 || gridPos.x >= 4 || gridPos.y < 0)
                {
                    Destroy(child.gameObject);
                }
                return false;
            }
        }
        return false;
    }

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
    }
}