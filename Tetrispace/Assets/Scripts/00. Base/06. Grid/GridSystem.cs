using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GridSystem
{
    [System.Serializable]
    // �׸��� �ý��� ����
    public static class GridSettings
    {
        public const float GRID_SIZE = 32f;          // �׸��� �� ĭ�� ũ�� (�ȼ�)
        public const int GRID_WIDTH = 32;            // ���� �׸��� ���� (1024 / 32)
        public const int GRID_HEIGHT = 24;           // ���� �׸��� ���� (768 / 32)
        public const float REFERENCE_WIDTH = 1024f;  // ���� �ػ� ����
        public const float REFERENCE_HEIGHT = 768f;  // ���� �ػ� ����
    }

    public float MinY { get; private set; }
    public float MaxY { get; private set; }
    public float SpawnX { get; private set; }
    public static Vector2Int GridPos = new Vector2Int(0,0);

    public static Vector2Int GetGridPos(int gridX, int gridY)
    {
        if (gridX < 0 || gridX >= GridSettings.GRID_WIDTH ||
            gridY < 0 || gridY >= GridSettings.GRID_HEIGHT)
        {
            Debug.LogWarning($"�׸��� �ε��� ���� �ʰ�: [{gridX}][{gridY}]");
            return Vector2Int.zero;
        }

        float posX = (gridX * GridSettings.GRID_SIZE);
        float posY = (gridY * GridSettings.GRID_SIZE);

        //// �׸��� �߽��� ��� (���� �ػ� ����)
        //float posX = (gridX * GridSettings.GRID_SIZE);
        //float posY = (gridY * GridSettings.GRID_SIZE));

        // ���� �ػ󵵿� �°� �����ϸ�
        float scaleX = Screen.width / GridSettings.REFERENCE_WIDTH;
        float scaleY = Screen.height / GridSettings.REFERENCE_HEIGHT;

        posX *= scaleX;
        posY *= scaleY;

        return new Vector2Int((int)posX, (int)posY);
    }


    /// <summary>
    /// �׸��� �ε����� �޾Ƽ� �ش� �׸����� ���� ��ǥ�� ��ȯ
    /// </summary>
    /// <param name="gridX">�׸��� X �ε��� (0 ~ 31)</param>
    /// <param name="gridY">�׸��� Y �ε��� (0 ~ 23)</param>
    /// <returns>�׸��� �߽����� ���� ��ǥ</returns>
    public static Vector3 GetGridWorldPosition(int gridX, int gridY)
    {
        // �ε��� ���� üũ
        if (gridX < 0 || gridX >= GridSettings.GRID_WIDTH ||
            gridY < 0 || gridY >= GridSettings.GRID_HEIGHT)
        {
            Debug.LogWarning($"�׸��� �ε��� ���� �ʰ�: [{gridX}][{gridY}]");
            return Vector3.zero;
        }

        // �׸��� �߽��� ��� (��ũ�� ��ǥ)
        float screenX = (gridX * GridSettings.GRID_SIZE) + (GridSettings.GRID_SIZE * 0.5f);
        float screenY = (gridY * GridSettings.GRID_SIZE) + (GridSettings.GRID_SIZE * 0.5f);

        // ���� �ػ󵵿� �°� �����ϸ�
        float scaleX = Screen.width / GridSettings.REFERENCE_WIDTH;
        float scaleY = Screen.height / GridSettings.REFERENCE_HEIGHT;

        screenX *= scaleX;
        screenY *= scaleY;

        // Unity ��ũ�� ��ǥ��� ���� �Ʒ��� (0,0)�̹Ƿ� Y�� ����
        screenY = Screen.height - screenY;

        // ��ũ�� ��ǥ�� ���� ��ǥ�� ��ȯ
        Vector3 screenPos = new Vector3(screenX, screenY, 0);
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(screenPos);
        worldPos.z = 0;

        return worldPos;
    }

    /// <summary>
    /// �׸��� �ε����� �޾Ƽ� �ش� �׸����� ��ũ�� ��ǥ�� ��ȯ
    /// </summary>
    /// <param name="gridX">�׸��� X �ε��� (0 ~ 31)</param>
    /// <param name="gridY">�׸��� Y �ε��� (0 ~ 23)</param>
    /// <returns>�׸��� �߽����� ��ũ�� ��ǥ</returns>
    public static Vector2 GetGridScreenPosition(int gridX, int gridY)
    {
        // �ε��� ���� üũ
        if (gridX < 0 || gridX >= GridSettings.GRID_WIDTH ||
            gridY < 0 || gridY >= GridSettings.GRID_HEIGHT)
        {
            Debug.LogWarning($"�׸��� �ε��� ���� �ʰ�: [{gridX}][{gridY}]");
            return Vector2.zero;
        }

        // �׸��� �߽��� ��� (��ũ�� ��ǥ)
        float screenX = (gridX * GridSettings.GRID_SIZE) + (GridSettings.GRID_SIZE * 0.5f);
        float screenY = (gridY * GridSettings.GRID_SIZE) + (GridSettings.GRID_SIZE * 0.5f);

        // ���� �ػ󵵿� �°� �����ϸ�
        float scaleX = Screen.width / GridSettings.REFERENCE_WIDTH;
        float scaleY = Screen.height / GridSettings.REFERENCE_HEIGHT;

        screenX *= scaleX;
        screenY *= scaleY;

        // Unity ��ũ�� ��ǥ��� ���� �Ʒ��� (0,0)�̹Ƿ� Y�� ����
        screenY = Screen.height - screenY;

        return new Vector2(screenX, screenY);
    }

    /// <summary>
    /// ���� ��ǥ�� �׸��� �ε����� ��ȯ
    /// </summary>
    /// <param name="worldPos">���� ��ǥ</param>
    /// <returns>�׸��� �ε��� (x, y)</returns>
    public static Vector2Int WorldToGridIndex(Vector3 worldPos)
    {
        // ���� ��ǥ�� ��ũ�� ��ǥ�� ��ȯ
        Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPos);

        // Unity ��ũ�� ��ǥ�� ����
        screenPos.y = Screen.height - screenPos.y;

        // ���� �ػ󵵿� �°� �������ϸ�
        float scaleX = Screen.width / GridSettings.REFERENCE_WIDTH;
        float scaleY = Screen.height / GridSettings.REFERENCE_HEIGHT;

        screenPos.x /= scaleX;
        screenPos.y /= scaleY;

        // �׸��� �ε��� ���
        int gridX = Mathf.FloorToInt(screenPos.x / GridSettings.GRID_SIZE);
        int gridY = Mathf.FloorToInt(screenPos.y / GridSettings.GRID_SIZE);

        // ���� Ŭ����
        gridX = Mathf.Clamp(gridX, 0, GridSettings.GRID_WIDTH - 1);
        gridY = Mathf.Clamp(gridY, 0, GridSettings.GRID_HEIGHT - 1);

        return new Vector2Int(gridX, gridY);
    }

    /// <summary>
    /// �׸��� ���� ���� ��ȿ�� �ε������� Ȯ��
    /// </summary>
    /// <param name="gridX">�׸��� X �ε���</param>
    /// <param name="gridY">�׸��� Y �ε���</param>
    /// <returns>��ȿ�� �ε������� ����</returns>
    public static bool IsValidGridIndex(int gridX, int gridY)
    {
        return gridX >= 0 && gridX < GridSettings.GRID_WIDTH &&
               gridY >= 0 && gridY < GridSettings.GRID_HEIGHT;
    }

    /// <summary>
    /// �׸��� �ý��� ����� ���� ���
    /// </summary>
    public static void DebugGridInfo()
    {
        Debug.Log($"�׸��� �ý��� ����:");
        Debug.Log($"- �׸��� ũ��: {GridSettings.GRID_SIZE}px");
        Debug.Log($"- �׸��� ����: {GridSettings.GRID_WIDTH} x {GridSettings.GRID_HEIGHT}");
        Debug.Log($"- ���� �ػ�: {GridSettings.REFERENCE_WIDTH} x {GridSettings.REFERENCE_HEIGHT}");
        Debug.Log($"- ���� �ػ�: {Screen.width} x {Screen.height}");
        Debug.Log($"- ������ ����: {Screen.width / GridSettings.REFERENCE_WIDTH:F2} x {Screen.height / GridSettings.REFERENCE_HEIGHT:F2}");
    }

}