using UnityEngine;

public class TetrisBlock : MonoBehaviour
{
    [Header("��� ũ�� ����")]
    public float blockPixelSize = 28f;  // ���ϴ� ��� ũ�� (�ȼ�)

    void Start()
    {
        //SetBlockSize();
    }

    void Update()
    {
        // ��ũ�� ��ǥ�� ���� ��� (ī�޶� ������)
        Vector2Int gridPos = GridSystem.GetGridMiddlePos(GridSystem.GridMiddlePos.x, GridSystem.GridMiddlePos.y);

        // ��ũ�� ��ǥ�� ���� ��ǥ�� ��ȯ
        Vector3 screenPos = new Vector3(gridPos.x, gridPos.y, 10f); // Z�� ī�޶���� �Ÿ�
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(screenPos);
        worldPos.z = 0; // 2D ���ӿ�

        transform.position = worldPos;
    }

    /// <summary>
    /// ��� ũ�⸦ 28x28 �ȼ��� ����
    /// </summary>
    //void SetBlockSize()
    //{
    //    // ���� �׸��� ũ�� (32�ȼ�)�� ���ϴ� ũ�� (28�ȼ�)�� ���� ���
    //    float scaleRatio = blockPixelSize / GridSystem.GridSettings.GRID_SIZE;

    //    // ��� ������ ����
    //    transform.localScale = Vector3.one * scaleRatio;

    //    Debug.Log($"��� ũ�� ����: {GridSystem.GridSettings.GRID_SIZE}px �� {blockPixelSize}px (����: {scaleRatio:F2})");
    //}
}