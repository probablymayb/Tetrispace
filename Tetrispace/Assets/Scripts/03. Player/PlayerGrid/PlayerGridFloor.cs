using UnityEngine;
using System.Collections.Generic;
using System.Collections;

/// <summary>
/// Emission까지 포함한 완전한 페이드아웃
/// </summary>
public class PlayerGridFloor : MonoBehaviour
{
    [SerializeField] private bool _isLocked = false;
    private bool _isProcessing = false;
    private bool _isFading = false;

    private HashSet<Transform> registeredChildren = new HashSet<Transform>();

    [Header("페이드 아웃 설정")]
    [SerializeField] private float fadeOutDuration = 2f;

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
            Debug.Log("*** Wall 충돌 - Emission 포함 페이드아웃 시작! ***");
            StartEmissionFadeOut();
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
    /// Emission까지 포함한 페이드아웃 시작
    /// </summary>
    private void StartEmissionFadeOut()
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

        StartCoroutine(EmissionFadeOutCoroutine());
    }

    /// <summary>
    /// Emission까지 포함한 페이드아웃 코루틴
    /// </summary>
    private IEnumerator EmissionFadeOutCoroutine()
    {
        Debug.Log("=== Emission 포함 페이드아웃 시작 ===");

        // 렌더러와 머티리얼 정보 수집
        List<RendererData> rendererDataList = new List<RendererData>();

        // 자신의 렌더러
        SpriteRenderer mainRenderer = GetComponent<SpriteRenderer>();
        if (mainRenderer != null)
        {
            rendererDataList.Add(new RendererData(mainRenderer));
        }

        // 자식들의 렌더러
        SpriteRenderer[] childRenderers = GetComponentsInChildren<SpriteRenderer>(true);
        foreach (SpriteRenderer renderer in childRenderers)
        {
            if (renderer != mainRenderer && renderer != null)
            {
                rendererDataList.Add(new RendererData(renderer));
            }
        }

        Debug.Log($"총 {rendererDataList.Count}개 렌더러로 Emission 페이드아웃 진행");

        if (rendererDataList.Count == 0)
        {
            Debug.LogWarning("렌더러가 없어서 즉시 파괴");
            Destroy(gameObject);
            yield break;
        }

        // 초기 상태 로그
        foreach (RendererData data in rendererDataList)
        {
            Debug.Log($"{data.renderer.gameObject.name}: " +
                     $"색상={data.originalColor}, " +
                     $"Emission={data.originalEmission}");
        }

        // 페이드아웃 실행
        float fadeTimer = 0f;

        while (fadeTimer < fadeOutDuration)
        {
            fadeTimer += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, fadeTimer / fadeOutDuration);

            // 30프레임마다 로그
            if (Time.frameCount % 30 == 0)
            {
                Debug.Log($"페이드 진행: {fadeTimer:F2}s / {fadeOutDuration:F2}s, 알파: {alpha:F3}");
            }

            // 모든 렌더러에 알파와 Emission 적용
            foreach (RendererData data in rendererDataList)
            {
                if (data.renderer != null)
                {
                    // 알파 적용
                    Color newColor = data.originalColor;
                    newColor.a = data.originalColor.a * alpha;
                    data.renderer.color = newColor;

                    // Emission 적용 (Material이 있는 경우)
                    if (data.renderer.material != null && data.hasEmission)
                    {
                        Color newEmission = data.originalEmission * alpha;
                        data.renderer.material.SetColor("_EmissionColor", newEmission);
                    }
                }
            }

            yield return null;
        }

        // 최종 완전 투명 처리
        Debug.Log("최종 투명 처리 (Emission 포함)");
        foreach (RendererData data in rendererDataList)
        {
            if (data.renderer != null)
            {
                // 완전 투명
                Color finalColor = data.originalColor;
                finalColor.a = 0f;
                data.renderer.color = finalColor;

                // Emission 완전 제거
                if (data.renderer.material != null && data.hasEmission)
                {
                    data.renderer.material.SetColor("_EmissionColor", Color.black);
                }

                Debug.Log($"최종: {data.renderer.gameObject.name}: 색상={finalColor}, Emission=Black");
            }
        }

        Debug.Log("Emission 포함 페이드아웃 완료 - 파괴 대기");
        yield return new WaitForSeconds(0.1f);

        Debug.Log("*** 오브젝트 파괴 ***");
        Destroy(gameObject);
    }

    /// <summary>
    /// 렌더러 데이터 클래스
    /// </summary>
    private class RendererData
    {
        public SpriteRenderer renderer;
        public Color originalColor;
        public Color originalEmission;
        public bool hasEmission;

        public RendererData(SpriteRenderer r)
        {
            renderer = r;
            originalColor = r.color;

            // Emission 정보 확인
            if (r.material != null && r.material.HasProperty("_EmissionColor"))
            {
                originalEmission = r.material.GetColor("_EmissionColor");
                hasEmission = true;
                Debug.Log($"{r.gameObject.name}: Emission 감지 - {originalEmission}");
            }
            else
            {
                originalEmission = Color.black;
                hasEmission = false;
            }
        }
    }

    /// <summary>
    /// 테스트용 메서드들
    /// </summary>
    [ContextMenu("Test Emission Fade Out")]
    public void TestEmissionFadeOut()
    {
        StartEmissionFadeOut();
    }

    [ContextMenu("Check Emission Values")]
    public void CheckEmissionValues()
    {
        Debug.Log("=== Emission 값 확인 ===");
        SpriteRenderer[] allRenderers = GetComponentsInChildren<SpriteRenderer>(true);

        foreach (SpriteRenderer renderer in allRenderers)
        {
            Debug.Log($"{renderer.gameObject.name}:");
            Debug.Log($"  - 색상: {renderer.color}");
            Debug.Log($"  - Material: {renderer.material?.name}");

            if (renderer.material != null && renderer.material.HasProperty("_EmissionColor"))
            {
                Color emission = renderer.material.GetColor("_EmissionColor");
                Debug.Log($"  - Emission: {emission} (강도: {emission.maxColorComponent:F2})");
            }
            else
            {
                Debug.Log($"  - Emission: 없음");
            }
        }
    }

    [ContextMenu("Force Remove Emission")]
    public void ForceRemoveEmission()
    {
        SpriteRenderer[] allRenderers = GetComponentsInChildren<SpriteRenderer>(true);

        foreach (SpriteRenderer renderer in allRenderers)
        {
            if (renderer.material != null && renderer.material.HasProperty("_EmissionColor"))
            {
                renderer.material.SetColor("_EmissionColor", Color.black);
                Debug.Log($"{renderer.gameObject.name}: Emission 제거");
            }
        }
    }

    [ContextMenu("Reset Emission to Original")]
    public void ResetEmissionToOriginal()
    {
        SpriteRenderer[] allRenderers = GetComponentsInChildren<SpriteRenderer>(true);

        foreach (SpriteRenderer renderer in allRenderers)
        {
            if (renderer.material != null && renderer.material.HasProperty("_EmissionColor"))
            {
                // 원본 Emission 복원 (예: 흰색 * 2.2)
                Color originalEmission = Color.white * 2.2f;
                renderer.material.SetColor("_EmissionColor", originalEmission);
                Debug.Log($"{renderer.gameObject.name}: Emission 복원 - {originalEmission}");
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