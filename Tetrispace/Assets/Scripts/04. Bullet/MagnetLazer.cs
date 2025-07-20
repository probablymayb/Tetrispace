using UnityEngine;

/// <summary>
/// MagnetLazer 간단 버전
/// 자식 VFX_Magnet 객체를 활성화/비활성화만 처리
/// </summary>
public class MagnetLazer : MonoBehaviour
{
    [Header("=== VFX 설정 ===")]
    [SerializeField] private GameObject vfxMagnet;  // VFX_Magnet 자식 객체

    [Header("=== 디버그 ===")]
    [SerializeField] private bool showDebugLogs = true;

    void Start()
    {
        // VFX_Magnet 자식 객체 자동 찾기
        if (vfxMagnet == null)
        {
            Transform vfxTransform = transform.Find("VFX_Magnet");
            if (vfxTransform != null)
            {
                vfxMagnet = vfxTransform.gameObject;
                DebugLog("VFX_Magnet 자동 발견: " + vfxMagnet.name);
            }
            else
            {
                DebugLog("VFX_Magnet 자식 객체를 찾을 수 없습니다!");
            }
        }

        // 초기에는 VFX 비활성화
        if (vfxMagnet != null)
        {
            vfxMagnet.SetActive(false);
            DebugLog("VFX_Magnet 초기 비활성화 완료");
        }
    }

    /// <summary>
    /// MagnetLazer가 활성화될 때 VFX도 활성화
    /// </summary>
    void OnEnable()
    {
        DebugLog("=== MagnetLazer 활성화 ===");

        if (vfxMagnet != null)
        {
            vfxMagnet.SetActive(true);
            DebugLog("VFX_Magnet 활성화 완료");
        }
    }

    /// <summary>
    /// MagnetLazer가 비활성화될 때 VFX도 비활성화
    /// </summary>
    void OnDisable()
    {
        DebugLog("=== MagnetLazer 비활성화 ===");

        if (vfxMagnet != null)
        {
            vfxMagnet.SetActive(false);
            DebugLog("VFX_Magnet 비활성화 완료");
        }
    }

    /// <summary>
    /// 디버그 로그 (토글 가능)
    /// </summary>
    private void DebugLog(string message)
    {
        if (showDebugLogs)
        {
            Debug.Log($"[MagnetLazer] {message}");
        }
    }

    /// <summary>
    /// 수동으로 VFX 활성화/비활성화 (외부 제어용)
    /// </summary>
    public void SetVFXActive(bool active)
    {
        if (vfxMagnet != null)
        {
            vfxMagnet.SetActive(active);
            DebugLog($"VFX_Magnet 수동 {(active ? "활성화" : "비활성화")}");
        }
    }

    /// <summary>
    /// VFX 활성화 상태 확인
    /// </summary>
    public bool IsVFXActive()
    {
        return vfxMagnet != null && vfxMagnet.activeSelf;
    }

    /// <summary>
    /// Context Menu 테스트
    /// </summary>
    [ContextMenu("Toggle VFX")]
    public void ToggleVFX()
    {
        if (vfxMagnet != null)
        {
            bool newState = !vfxMagnet.activeSelf;
            vfxMagnet.SetActive(newState);
            DebugLog($"VFX 토글: {newState}");
        }
    }
}