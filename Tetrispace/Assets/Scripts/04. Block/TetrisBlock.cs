using UnityEngine;

/// <summary>
/// �Ϻ� ���� ��Ʈ���� ���
/// 
/// PlayerController�� ��Ȯ�� ���� �׸��� �ý��� ���
/// - PlayerController: GetGridPos() ��� (�׸��� �𼭸�)
/// - TetrisBlock: GetGridMiddlePos() ��� (�׸��� �߽�)
/// </summary>
public class TetrisBlock : MonoBehaviour
{
    [Header("=== ��� ���� ===")]
    [SerializeField] private float blockPixelSize = 28f;  // ��� ũ��

    [Header("=== �׸��� ��ġ ===")]
    [SerializeField] private bool useStaticPosition = true;  // ���� ��ġ ��� ����
    [SerializeField] private int customGridX = 0;           // Ŀ���� �׸��� X
    [SerializeField] private int customGridY = 0;           // Ŀ���� �׸��� Y

    void Start()
    {
        // ��� ũ�� ����
        SetBlockSize();

        // �׸��� ���� ��� (�� ����)
        if (useStaticPosition)
        {
            GridSystem.DebugGridInfo();
            GridSystem.TestGridCalculation();
        }
    }

    void Update()
    {
        //  PlayerController�� ������ ������� ��ġ ���
        Vector2Int gridPos;

        if (useStaticPosition)
        {
            // ���� GridMiddlePos ��� (PlayerController�� GridPos�� ����)
            gridPos = GridSystem.GetGridMiddlePos(GridSystem.GridMiddlePos.x, GridSystem.GridMiddlePos.y);
        }
        else
        {
            // Ŀ���� ��ġ ���
            gridPos = GridSystem.GetGridMiddlePos(customGridX, customGridY);
        }

        //  PlayerController�� ������ ��ũ��-���� ��ȯ
        Vector3 screenPos = new Vector3(gridPos.x, gridPos.y, 10f);
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(screenPos);
        worldPos.z = 0;

        transform.position = worldPos;
    }

    /// <summary>
    /// ��� ũ�� ����
    /// </summary>
    void SetBlockSize()
    {
        // �׸��� ũ��� ��� ũ�� ���� ���
        float scaleRatio = blockPixelSize / GridSystem.GridSettings.GRID_SIZE;
        transform.localScale = Vector3.one * scaleRatio;

        Debug.Log($"��� ũ�� ����: {GridSystem.GridSettings.GRID_SIZE}px �� {blockPixelSize}px (����: {scaleRatio:F2})");
    }

    /// <summary>
    /// Ư�� �׸��� ��ġ�� �̵�
    /// </summary>
    public void SetGridPosition(int gridX, int gridY)
    {
        if (GridSystem.IsValidGridIndex(gridX, gridY))
        {
            customGridX = gridX;
            customGridY = gridY;
            useStaticPosition = false;

            Debug.Log($"��� ��ġ ����: [{gridX}][{gridY}]");
        }
        else
        {
            Debug.LogWarning($"��ȿ���� ���� �׸��� ��ġ: [{gridX}][{gridY}]");
        }
    }

    /// <summary>
    /// ���� ��ġ ������� ����
    /// </summary>
    public void UseStaticPosition()
    {
        useStaticPosition = true;
        Debug.Log("���� ��ġ ��� ���� ����");
    }

    /// <summary>
    /// ���� ��ġ ���� ���
    /// </summary>
    [ContextMenu("���� ��ġ ���")]
    public void PrintCurrentPosition()
    {
        Vector3 worldPos = transform.position;
        Vector2Int gridIndex = GridSystem.WorldToGridIndex(worldPos);

        Debug.Log($"=== TetrisBlock ��ġ ���� ===");
        Debug.Log($"���� ��ǥ: {worldPos}");
        Debug.Log($"�׸��� �ε���: [{gridIndex.x}][{gridIndex.y}]");
        Debug.Log($"��� ���� �׸��� ��ġ: [{(useStaticPosition ? GridSystem.GridMiddlePos.x : customGridX)}][{(useStaticPosition ? GridSystem.GridMiddlePos.y : customGridY)}]");
        Debug.Log($"���: {(useStaticPosition ? "���� ��ġ" : "Ŀ���� ��ġ")}");
    }

    /// <summary>
    /// �׸��� ���� �׽�Ʈ
    /// </summary>
    [ContextMenu("���� �׽�Ʈ")]
    public void TestAlignment()
    {
        Debug.Log("=== ���� �׽�Ʈ ===");

        // ���� ��ġ���� �׽�Ʈ
        int[] testPositions = { 0, 1, 17, 35 };

        foreach (int pos in testPositions)
        {
            if (GridSystem.IsValidGridIndex(pos, 0))
            {
                Vector2Int playerPos = GridSystem.GetGridPos(pos, 0);
                Vector2Int blockPos = GridSystem.GetGridMiddlePos(pos, 0);

                Debug.Log($"��ġ [{pos}][0] - Player: {playerPos}, Block: {blockPos}, ����: {blockPos.x - playerPos.x}px");
            }
        }
    }
}