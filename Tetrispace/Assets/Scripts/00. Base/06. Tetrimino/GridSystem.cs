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
        public const float REFERENCE_WIDTH = 1024f;
        public const float REFERENCE_HEIGHT = 768f;
        public const float GRID_SIZE = 28f;

        // === ��Ȯ�� ��� ===
        public static int ActualGridWidth => 32;      // 896 / 28 = 32�� (0~31)
        public static int ActualGridHeight => 27;     // 768 / 28 = 27.43 �� 27�� (0~26)

        // === ���� ��� (����) ===
        public static float HorizontalMargin => 64f;    // (1024 - 896) / 2 = 64px
        public static float VerticalMargin => 12f;      // (768 - 27*28) / 2 = 12px

        // === ���� ���� ===
        public static float GameAreaWidth => 896f;      // ���� ���� ����
        public static float GameAreaStartX => 64f;      // ���� ���� ������
        public static float GameAreaEndX => 960f;       // ���� ���� ���� (64 + 896)
    }

    // === ���� �׸��� ��ġ ���� ===
    public static Vector2Int GridPos = new Vector2Int(5, 5);
    public static Vector2Int GridMiddlePos = new Vector2Int(0, 0);

    /// <summary>
    /// PlayerController�� �׸��� ��ġ ��� (�׸��� ���� �𼭸�)
    /// grid[0] = 64px, grid[1] = 92px, grid[31] = 932px
    /// </summary>
    public static Vector2Int GetGridPos(int gridX, int gridY)
    {
        gridX = Mathf.Clamp(gridX, 0, GridSettings.ActualGridWidth - 1);
        gridY = Mathf.Clamp(gridY, 0, GridSettings.ActualGridHeight - 1);
        // ���� üũ
        if (gridX < 0 || gridX >= GridSettings.ActualGridWidth ||
            gridY < 0 || gridY >= GridSettings.ActualGridHeight)
        {
            Debug.LogWarning($"�׸��� �ε��� ���� �ʰ�: [{gridX}][{gridY}] (�ִ�: {GridSettings.ActualGridWidth - 1}x{GridSettings.ActualGridHeight - 1})");
            return Vector2Int.zero;
        }

        // ��Ȯ�� ���: ���� ���� + (�׸��� �ε��� * �׸��� ũ��)
        float posX = GridSettings.HorizontalMargin + (gridX * GridSettings.GRID_SIZE);
        float posY = GridSettings.VerticalMargin + (gridY * GridSettings.GRID_SIZE);

        // ���� üũ (���� ���� ��)
        posX = Mathf.Clamp(posX, GridSettings.GameAreaStartX, GridSettings.GameAreaEndX - GridSettings.GRID_SIZE);
        posY = Mathf.Clamp(posY, GridSettings.VerticalMargin, GridSettings.VerticalMargin + (GridSettings.ActualGridHeight - 1) * GridSettings.GRID_SIZE);

        // �ػ� �����ϸ�
        float scaleX = Screen.width / GridSettings.REFERENCE_WIDTH;
        float scaleY = Screen.height / GridSettings.REFERENCE_HEIGHT;

        posX *= scaleX;
        posY *= scaleY;

        return new Vector2Int(Mathf.RoundToInt(posX), Mathf.RoundToInt(posY));
    }

    /// <summary>
    /// Block�� �׸��� �߽��� ��� (�׸��� �߾�)
    /// grid[0] = 78px, grid[1] = 106px, grid[31] = 946px
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

        // ��Ȯ�� ���: ���� ���� + (�׸��� �ε��� * �׸��� ũ��) + (�׸��� ũ�� / 2)
        float posX = GridSettings.HorizontalMargin + (gridX * GridSettings.GRID_SIZE) + (GridSettings.GRID_SIZE * 0.5f);
        float posY = GridSettings.VerticalMargin + (gridY * GridSettings.GRID_SIZE) + (GridSettings.GRID_SIZE * 0.5f);

        // ���� üũ
        posX = Mathf.Clamp(posX, GridSettings.HorizontalMargin + 14f, GridSettings.GameAreaEndX - 14f);
        posY = Mathf.Clamp(posY, GridSettings.VerticalMargin + 14f, GridSettings.VerticalMargin + GridSettings.ActualGridHeight * GridSettings.GRID_SIZE - 14f);

        // �ػ� �����ϸ�
        float scaleX = Screen.width / GridSettings.REFERENCE_WIDTH;
        float scaleY = Screen.height / GridSettings.REFERENCE_HEIGHT;

        posX *= scaleX;
        posY *= scaleY;

        return new Vector2Int(Mathf.RoundToInt(posX), Mathf.RoundToInt(posY));
    }

    /// <summary>
    /// PlayerController�� ���� ��ǥ ���
    /// </summary>
    public static Vector3 GetGridWorldPosition(int gridX, int gridY)
    {
        Vector2Int screenPos = GetGridPos(gridX, gridY);
        return ScreenToWorld(screenPos);
    }

    /// <summary>
    /// Block�� ���� ��ǥ �߽��� ���
    /// </summary>
    public static Vector3 GetGridMiddleWorldPosition(int gridX, int gridY)
    {
        Vector2Int screenPos = GetGridMiddlePos(gridX, gridY);
        return ScreenToWorld(screenPos);
    }

    /// <summary>
    /// ��ũ�� ��ǥ�� ���� ��ǥ�� ��ȯ
    /// </summary>
    private static Vector3 ScreenToWorld(Vector2Int screenPos)
    {
        Vector3 unityScreenPos = new Vector3(screenPos.x, Screen.height - screenPos.y, 10f);
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(unityScreenPos);
        worldPos.z = 0;
        return worldPos;
    }

    /// <summary>
    /// ��ũ�� ��ǥ�� �׸��� �ε����� ��ȯ
    /// </summary>
    public static Vector2Int ScreenToGridIndex(Vector2 screenPos)
    {
        // �ػ� �������ϸ�
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
        Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPos);
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
    /// �׸��� �ý��� ����� ���� ���
    /// </summary>
    public static void DebugGridInfo()
    {
        Debug.Log("=== ������ �׸��� �ý��� ���� ===");
        Debug.Log($"�ػ�: {GridSettings.REFERENCE_WIDTH} x {GridSettings.REFERENCE_HEIGHT}");
        Debug.Log($"���� ����: {GridSettings.GameAreaWidth}px (����: {GridSettings.GameAreaStartX}, ��: {GridSettings.GameAreaEndX})");
        Debug.Log($"�׸��� ũ��: {GridSettings.GRID_SIZE}px");
        Debug.Log($"�׸��� ����: {GridSettings.ActualGridWidth} x {GridSettings.ActualGridHeight}");
        Debug.Log($"����: �¿� �� {GridSettings.HorizontalMargin}px, ���� �� {GridSettings.VerticalMargin}px");
        Debug.Log("");
        Debug.Log("=== ��� ���� ===");
        Debug.Log($"�÷��̾� grid[0] = {GetGridPos(0, 0)} (����: 64)");
        Debug.Log($"�÷��̾� grid[1] = {GetGridPos(1, 0)} (����: 92)");
        Debug.Log($"�÷��̾� grid[31] = {GetGridPos(31, 0)} (����: 932)");
        Debug.Log($"��� grid[0] = {GetGridMiddlePos(0, 0)} (����: 78)");
        Debug.Log($"��� grid[1] = {GetGridMiddlePos(1, 0)} (����: 106)");
        Debug.Log($"��� grid[31] = {GetGridMiddlePos(31, 0)} (����: 946)");
    }

    /// <summary>
    /// �׸��� ��� �׽�Ʈ
    /// </summary>
    public static void TestGridCalculation()
    {
        Debug.Log("=== �׸��� ��� ��Ȯ�� �׽�Ʈ ===");

        // �÷��̾� ��ġ �׽�Ʈ
        for (int i = 0; i <= 31; i += 10)
        {
            Vector2Int pos = GetGridPos(i, 0);
            int expected = 64 + i * 28;
            Debug.Log($"�÷��̾� grid[{i}] = {pos.x} (����: {expected}) - {(pos.x == expected ? "o" : "x")}");
        }

        // ��� ��ġ �׽�Ʈ
        for (int i = 0; i <= 31; i += 10)
        {
            Vector2Int pos = GetGridMiddlePos(i, 0);
            int expected = 64 + i * 28 + 14;
            Debug.Log($"��� grid[{i}] = {pos.x} (����: {expected}) - {(pos.x == expected ? "o" : "x")}");
        }
    }
}