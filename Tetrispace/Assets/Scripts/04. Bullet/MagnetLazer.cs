using UnityEngine;

/// <summary>
/// MagnetLazer ���� ����
/// �ڽ� VFX_Magnet ��ü�� Ȱ��ȭ/��Ȱ��ȭ�� ó��
/// </summary>
public class MagnetLazer : MonoBehaviour
{
    [Header("=== VFX ���� ===")]
    [SerializeField] private GameObject vfxMagnet;  // VFX_Magnet �ڽ� ��ü

    [Header("=== ����� ===")]
    [SerializeField] private bool showDebugLogs = true;

    void Start()
    {
        // VFX_Magnet �ڽ� ��ü �ڵ� ã��
        if (vfxMagnet == null)
        {
            Transform vfxTransform = transform.Find("VFX_Magnet");
            if (vfxTransform != null)
            {
                vfxMagnet = vfxTransform.gameObject;
                DebugLog("VFX_Magnet �ڵ� �߰�: " + vfxMagnet.name);
            }
            else
            {
                DebugLog("VFX_Magnet �ڽ� ��ü�� ã�� �� �����ϴ�!");
            }
        }

        // �ʱ⿡�� VFX ��Ȱ��ȭ
        if (vfxMagnet != null)
        {
            vfxMagnet.SetActive(false);
            DebugLog("VFX_Magnet �ʱ� ��Ȱ��ȭ �Ϸ�");
        }
    }

    /// <summary>
    /// MagnetLazer�� Ȱ��ȭ�� �� VFX�� Ȱ��ȭ
    /// </summary>
    void OnEnable()
    {
        DebugLog("=== MagnetLazer Ȱ��ȭ ===");

        if (vfxMagnet != null)
        {
            vfxMagnet.SetActive(true);
            DebugLog("VFX_Magnet Ȱ��ȭ �Ϸ�");
        }
    }

    /// <summary>
    /// MagnetLazer�� ��Ȱ��ȭ�� �� VFX�� ��Ȱ��ȭ
    /// </summary>
    void OnDisable()
    {
        DebugLog("=== MagnetLazer ��Ȱ��ȭ ===");

        if (vfxMagnet != null)
        {
            vfxMagnet.SetActive(false);
            DebugLog("VFX_Magnet ��Ȱ��ȭ �Ϸ�");
        }
    }

    /// <summary>
    /// ����� �α� (��� ����)
    /// </summary>
    private void DebugLog(string message)
    {
        if (showDebugLogs)
        {
            Debug.Log($"[MagnetLazer] {message}");
        }
    }

    /// <summary>
    /// �������� VFX Ȱ��ȭ/��Ȱ��ȭ (�ܺ� �����)
    /// </summary>
    public void SetVFXActive(bool active)
    {
        if (vfxMagnet != null)
        {
            vfxMagnet.SetActive(active);
            DebugLog($"VFX_Magnet ���� {(active ? "Ȱ��ȭ" : "��Ȱ��ȭ")}");
        }
    }

    /// <summary>
    /// VFX Ȱ��ȭ ���� Ȯ��
    /// </summary>
    public bool IsVFXActive()
    {
        return vfxMagnet != null && vfxMagnet.activeSelf;
    }

    /// <summary>
    /// Context Menu �׽�Ʈ
    /// </summary>
    [ContextMenu("Toggle VFX")]
    public void ToggleVFX()
    {
        if (vfxMagnet != null)
        {
            bool newState = !vfxMagnet.activeSelf;
            vfxMagnet.SetActive(newState);
            DebugLog($"VFX ���: {newState}");
        }
    }
}