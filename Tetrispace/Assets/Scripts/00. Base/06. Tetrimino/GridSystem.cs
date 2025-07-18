using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GridSystem
{
    [System.Serializable]
    public static class GridSettings
    {
        // === ���� �ػ� �� �׸��� ���� ===
        public const float REFERENCE_WIDTH = 1024f;   // ���� �ػ� ����
        public const float REFERENCE_HEIGHT = 784f;   // ���� �ػ� ����
        public const float GRID_SIZE = 28f;           // �׸��� �� ĭ ũ�� (�ȼ�)

        // === ���� �׸��� ���� ===
        public static int ActualGridWidth => 32;      // 1024 / 28 = 36.57 �� 36�� (0~35)
        public static int ActualGridHeight => 27;     // 784 / 28 = 28 �� 28�� (0~27)

        // === ���� ��� ===
        public static float HorizontalMargin => 128f;   // (1024 - 36*28) / 2 = 8px
        public static float VerticalMargin => 12f;     // (784 - 28*28) / 2 = 0px (��Ȯ�� ���������)
    }

    private static Vector2Int minVector = new Vector2Int(64, 6);
    private static Vector2Int maxVector = new Vector2Int(960, 762);

    // === ���� �׸��� ��ġ ���� ===
    public static Vector2Int GridPos = new Vector2Int(0, 0);
    public static Vector2Int GridMiddlePos = new Vector2Int(0, 0);

    /// <summary>
    /// PlayerController�� �׸��� ��ġ ���
    /// GridPos[0][0] = 8px, GridPos[35][35] = 988px
    /// </summary>
    public static Vector2Int GetGridPos(int gridX, int gridY)
    {
        // ���� üũ
        if (gridX < 0 || gridX >= GridSettings.ActualGridWidth ||
            gridY < 0 || gridY >= GridSettings.ActualGridHeight)
        {
            Debug.LogWarning($"�׸��� �ε��� ���� �ʰ�: [{gridX}][{gridY}] (�ִ�: {GridSettings.ActualGridWidth - 1}x{GridSettings.ActualGridHeight - 1})");
            return Vector2Int.zero;
        }

        // PlayerController ��ġ ���: ���� + (�ε��� * �׸���ũ��)
        float posX = minVector.x + (gridX * GridSettings.GRID_SIZE);
        float posY = minVector.x + (gridY * GridSettings.GRID_SIZE);

        if (posX > maxVector.x)
        {
            posX = maxVector.x;
        }
        
        if (posY > maxVector.y)
        {
            posY = maxVector.y;
        }

        // ���� �ػ󵵿� �°� �����ϸ� (���� �ػ󵵶�� ���� 1.0)
        float scaleX = Screen.width / GridSettings.REFERENCE_WIDTH;
        float scaleY = Screen.height / GridSettings.REFERENCE_HEIGHT;

        posX *= scaleX;
        posY *= scaleY;

        return new Vector2Int((int)posX, (int)posY);
    }

    /// <summary>
    /// Block�� �׸��� �߽��� ���
    /// GridMiddlePos[0][0] = 22px, GridMiddlePos[35][35] = 1002px
    /// </summary>
    public static Vector2Int GetGridMiddlePos(int gridX, int gridY)
    {
        // ���� üũ
        if (gridX < 0 || gridX >= GridSettings.ActualGridWidth ||
            gridY < 0 || gridY >= GridSettings.ActualGridHeight)
        {
            Debug.LogWarning($"�׸��� �ε��� ���� �ʰ�: [{gridX}][{gridY}] (�ִ�: {GridSettings.ActualGridWidth - 1}x{GridSettings.ActualGridHeight - 1})");
            return Vector2Int.zero;
        }

        // Block �߽��� ���: ���� + (�ε��� * �׸���ũ��) + (�׸���ũ�� / 2)
        float posX = GridSettings.HorizontalMargin + (gridX * GridSettings.GRID_SIZE) + (GridSettings.GRID_SIZE * 0.5f);
        float posY = GridSettings.VerticalMargin + (gridY * GridSettings.GRID_SIZE) + (GridSettings.GRID_SIZE * 0.5f);

        // ���� �ػ󵵿� �°� �����ϸ�
        float scaleX = Screen.width / GridSettings.REFERENCE_WIDTH;
        float scaleY = Screen.height / GridSettings.REFERENCE_HEIGHT;

        posX *= scaleX;
        posY *= scaleY;

        return new Vector2Int((int)posX, (int)posY);
    }

    /// <summary>
    /// PlayerController�� ���� ��ǥ ���
    /// </summary>
    public static Vector3 GetGridWorldPosition(int gridX, int gridY)
    {
        // ���� üũ
        if (gridX < 0 || gridX >= GridSettings.ActualGridWidth ||
            gridY < 0 || gridY >= GridSettings.ActualGridHeight)
        {
            Debug.LogWarning($"�׸��� �ε��� ���� �ʰ�: [{gridX}][{gridY}]");
            return Vector3.zero;
        }

        // ��ũ�� ��ǥ ���
        float screenX = GridSettings.HorizontalMargin + (gridX * GridSettings.GRID_SIZE);
        float screenY = GridSettings.VerticalMargin + (gridY * GridSettings.GRID_SIZE);

        // ���� �ػ󵵿� �°� �����ϸ�
        float scaleX = Screen.width / GridSettings.REFERENCE_WIDTH;
        float scaleY = Screen.height / GridSettings.REFERENCE_HEIGHT;

        screenX *= scaleX;
        screenY *= scaleY;

        // Unity ��ũ�� ��ǥ�� ���� (���� �Ʒ��� (0,0))
        screenY = Screen.height - screenY;

        // ��ũ�� ��ǥ�� ���� ��ǥ�� ��ȯ
        Vector3 screenPos = new Vector3(screenX, screenY, 0);
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(screenPos);
        worldPos.z = 0;

        return worldPos;
    }

    /// <summary>
    /// Block�� ���� ��ǥ �߽��� ���
    /// </summary>
    public static Vector3 GetGridMiddleWorldPosition(int gridX, int gridY)
    {
        // ���� üũ
        if (gridX < 0 || gridX >= GridSettings.ActualGridWidth ||
            gridY < 0 || gridY >= GridSettings.ActualGridHeight)
        {
            Debug.LogWarning($"�׸��� �ε��� ���� �ʰ�: [{gridX}][{gridY}]");
            return Vector3.zero;
        }

        // ��ũ�� ��ǥ ��� (�߽���)
        float screenX = GridSettings.HorizontalMargin + (gridX * GridSettings.GRID_SIZE) + (GridSettings.GRID_SIZE * 0.5f);
        float screenY = GridSettings.VerticalMargin + (gridY * GridSettings.GRID_SIZE) + (GridSettings.GRID_SIZE * 0.5f);

        // ���� �ػ󵵿� �°� �����ϸ�
        float scaleX = Screen.width / GridSettings.REFERENCE_WIDTH;
        float scaleY = Screen.height / GridSettings.REFERENCE_HEIGHT;

        screenX *= scaleX;
        screenY *= scaleY;

        // Unity ��ũ�� ��ǥ�� ����
        screenY = Screen.height - screenY;

        // ��ũ�� ��ǥ�� ���� ��ǥ�� ��ȯ
        Vector3 screenPos = new Vector3(screenX, screenY, 0);
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(screenPos);
        worldPos.z = 0;

        return worldPos;
    }

    /// <summary>
    /// ��ũ�� ��ǥ�� �׸��� �ε����� ��ȯ
    /// </summary>
    public static Vector2Int ScreenToGridIndex(Vector2 screenPos)
    {
        // ���� �ػ󵵿� �°� �������ϸ�
        float scaleX = Screen.width / GridSettings.REFERENCE_WIDTH;
        float scaleY = Screen.height / GridSettings.REFERENCE_HEIGHT;

        screenPos.x /= scaleX;
        screenPos.y /= scaleY;

        // ������ ���� �׸��� �ε��� ���
        screenPos.x -= GridSettings.HorizontalMargin;
        screenPos.y -= GridSettings.VerticalMargin;

        int gridX = Mathf.FloorToInt(screenPos.x / GridSettings.GRID_SIZE);
        int gridY = Mathf.FloorToInt(screenPos.y / GridSettings.GRID_SIZE);

        // ���� Ŭ����
        gridX = Mathf.Clamp(gridX, 0, GridSettings.ActualGridWidth - 1);
        gridY = Mathf.Clamp(gridY, 0, GridSettings.ActualGridHeight - 1);

        return new Vector2Int(gridX, gridY);
    }

    /// <summary>
    /// ���� ��ǥ�� �׸��� �ε����� ��ȯ
    /// </summary>
    public static Vector2Int WorldToGridIndex(Vector3 worldPos)
    {
        // ���� ��ǥ�� ��ũ�� ��ǥ�� ��ȯ
        Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPos);

        // Unity ��ũ�� ��ǥ�� ����
        screenPos.y = Screen.height - screenPos.y;

        return ScreenToGridIndex(new Vector2(screenPos.x, screenPos.y));
    }

    /// <summary>
    /// ��ȿ�� �׸��� �ε������� Ȯ��
    /// </summary>
    public static bool IsValidGridIndex(int gridX, int gridY)
    {
        return gridX >= 0 && gridX < GridSettings.ActualGridWidth &&
               gridY >= 0 && gridY < GridSettings.ActualGridHeight;
    }

    /// <summary>
    //�׸��� �ý��� ����� ���� ���
    /// </summary>
    public static void DebugGridInfo()
    {
        Debug.Log($"===  ���� �׸��� �ý��� ���� ===");
        Debug.Log($"�ػ�: {GridSettings.REFERENCE_WIDTH} x {GridSettings.REFERENCE_HEIGHT}");
        Debug.Log($"�׸��� ũ��: {GridSettings.GRID_SIZE}px");
        Debug.Log($"�׸��� ����: {GridSettings.ActualGridWidth} x {GridSettings.ActualGridHeight} (0~{GridSettings.ActualGridWidth - 1}, 0~{GridSettings.ActualGridHeight - 1})");
        Debug.Log($"����: ���� {GridSettings.HorizontalMargin}px, ���� {GridSettings.VerticalMargin}px");
        Debug.Log($"");
        Debug.Log($"=== PlayerController ��ġ (GetGridPos) ===");
        Debug.Log($"GridPos[0][0] = {GridSettings.HorizontalMargin}px");
        Debug.Log($"GridPos[35][27] = {GridSettings.HorizontalMargin + 35 * GridSettings.GRID_SIZE}px, {GridSettings.VerticalMargin + 27 * GridSettings.GRID_SIZE}px");
        Debug.Log($"");
        Debug.Log($"=== Block ��ġ (GetGridMiddlePos) ===");
        Debug.Log($"GridMiddlePos[0][0] = {GridSettings.HorizontalMargin + GridSettings.GRID_SIZE * 0.5f}px");
        Debug.Log($"GridMiddlePos[35][27] = {GridSettings.HorizontalMargin + 35 * GridSettings.GRID_SIZE + GridSettings.GRID_SIZE * 0.5f}px, {GridSettings.VerticalMargin + 27 * GridSettings.GRID_SIZE + GridSettings.GRID_SIZE * 0.5f}px");
    }

    /// <summary>
    /// �׸��� ��ǥ ���� (�׽�Ʈ��)
    /// </summary>
    public static void TestGridCalculation()
    {
        Debug.Log("===  �׸��� ��� �׽�Ʈ ===");

        // PlayerController ��ġ �׽�Ʈ
        Vector2Int playerPos0 = GetGridPos(0, 0);
        Vector2Int playerPos35 = GetGridPos(35, 27);
        Debug.Log($"Player [0,0]: {playerPos0} (����: 8, 0)");
        Debug.Log($"Player [35,27]: {playerPos35} (����: 988, 756)");

        // Block ��ġ �׽�Ʈ
        Vector2Int blockPos0 = GetGridMiddlePos(0, 0);
        Vector2Int blockPos35 = GetGridMiddlePos(35, 27);
        Debug.Log($"Block [0,0]: {blockPos0} (����: 22, 14)");
        Debug.Log($"Block [35,27]: {blockPos35} (����: 1002, 770)");
    }
}