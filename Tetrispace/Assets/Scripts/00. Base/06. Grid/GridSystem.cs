using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GridSystem
{
    [System.Serializable]
    // 그리드 시스템 설정
    public static class GridSettings
    {
        public const float GRID_SIZE = 32f;          // 그리드 한 칸의 크기 (픽셀)
        public const int GRID_WIDTH = 32;            // 가로 그리드 개수 (1024 / 32)
        public const int GRID_HEIGHT = 24;           // 세로 그리드 개수 (768 / 32)
        public const float REFERENCE_WIDTH = 1024f;  // 기준 해상도 가로
        public const float REFERENCE_HEIGHT = 768f;  // 기준 해상도 세로
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
            Debug.LogWarning($"그리드 인덱스 범위 초과: [{gridX}][{gridY}]");
            return Vector2Int.zero;
        }

        float posX = (gridX * GridSettings.GRID_SIZE);
        float posY = (gridY * GridSettings.GRID_SIZE);

        //// 그리드 중심점 계산 (기준 해상도 기준)
        //float posX = (gridX * GridSettings.GRID_SIZE);
        //float posY = (gridY * GridSettings.GRID_SIZE));

        // 현재 해상도에 맞게 스케일링
        float scaleX = Screen.width / GridSettings.REFERENCE_WIDTH;
        float scaleY = Screen.height / GridSettings.REFERENCE_HEIGHT;

        posX *= scaleX;
        posY *= scaleY;

        return new Vector2Int((int)posX, (int)posY);
    }


    /// <summary>
    /// 그리드 인덱스를 받아서 해당 그리드의 월드 좌표를 반환
    /// </summary>
    /// <param name="gridX">그리드 X 인덱스 (0 ~ 31)</param>
    /// <param name="gridY">그리드 Y 인덱스 (0 ~ 23)</param>
    /// <returns>그리드 중심점의 월드 좌표</returns>
    public static Vector3 GetGridWorldPosition(int gridX, int gridY)
    {
        // 인덱스 범위 체크
        if (gridX < 0 || gridX >= GridSettings.GRID_WIDTH ||
            gridY < 0 || gridY >= GridSettings.GRID_HEIGHT)
        {
            Debug.LogWarning($"그리드 인덱스 범위 초과: [{gridX}][{gridY}]");
            return Vector3.zero;
        }

        // 그리드 중심점 계산 (스크린 좌표)
        float screenX = (gridX * GridSettings.GRID_SIZE) + (GridSettings.GRID_SIZE * 0.5f);
        float screenY = (gridY * GridSettings.GRID_SIZE) + (GridSettings.GRID_SIZE * 0.5f);

        // 현재 해상도에 맞게 스케일링
        float scaleX = Screen.width / GridSettings.REFERENCE_WIDTH;
        float scaleY = Screen.height / GridSettings.REFERENCE_HEIGHT;

        screenX *= scaleX;
        screenY *= scaleY;

        // Unity 스크린 좌표계는 왼쪽 아래가 (0,0)이므로 Y축 보정
        screenY = Screen.height - screenY;

        // 스크린 좌표를 월드 좌표로 변환
        Vector3 screenPos = new Vector3(screenX, screenY, 0);
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(screenPos);
        worldPos.z = 0;

        return worldPos;
    }

    /// <summary>
    /// 그리드 인덱스를 받아서 해당 그리드의 스크린 좌표를 반환
    /// </summary>
    /// <param name="gridX">그리드 X 인덱스 (0 ~ 31)</param>
    /// <param name="gridY">그리드 Y 인덱스 (0 ~ 23)</param>
    /// <returns>그리드 중심점의 스크린 좌표</returns>
    public static Vector2 GetGridScreenPosition(int gridX, int gridY)
    {
        // 인덱스 범위 체크
        if (gridX < 0 || gridX >= GridSettings.GRID_WIDTH ||
            gridY < 0 || gridY >= GridSettings.GRID_HEIGHT)
        {
            Debug.LogWarning($"그리드 인덱스 범위 초과: [{gridX}][{gridY}]");
            return Vector2.zero;
        }

        // 그리드 중심점 계산 (스크린 좌표)
        float screenX = (gridX * GridSettings.GRID_SIZE) + (GridSettings.GRID_SIZE * 0.5f);
        float screenY = (gridY * GridSettings.GRID_SIZE) + (GridSettings.GRID_SIZE * 0.5f);

        // 현재 해상도에 맞게 스케일링
        float scaleX = Screen.width / GridSettings.REFERENCE_WIDTH;
        float scaleY = Screen.height / GridSettings.REFERENCE_HEIGHT;

        screenX *= scaleX;
        screenY *= scaleY;

        // Unity 스크린 좌표계는 왼쪽 아래가 (0,0)이므로 Y축 보정
        screenY = Screen.height - screenY;

        return new Vector2(screenX, screenY);
    }

    /// <summary>
    /// 월드 좌표를 그리드 인덱스로 변환
    /// </summary>
    /// <param name="worldPos">월드 좌표</param>
    /// <returns>그리드 인덱스 (x, y)</returns>
    public static Vector2Int WorldToGridIndex(Vector3 worldPos)
    {
        // 월드 좌표를 스크린 좌표로 변환
        Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPos);

        // Unity 스크린 좌표계 보정
        screenPos.y = Screen.height - screenPos.y;

        // 현재 해상도에 맞게 역스케일링
        float scaleX = Screen.width / GridSettings.REFERENCE_WIDTH;
        float scaleY = Screen.height / GridSettings.REFERENCE_HEIGHT;

        screenPos.x /= scaleX;
        screenPos.y /= scaleY;

        // 그리드 인덱스 계산
        int gridX = Mathf.FloorToInt(screenPos.x / GridSettings.GRID_SIZE);
        int gridY = Mathf.FloorToInt(screenPos.y / GridSettings.GRID_SIZE);

        // 범위 클램핑
        gridX = Mathf.Clamp(gridX, 0, GridSettings.GRID_WIDTH - 1);
        gridY = Mathf.Clamp(gridY, 0, GridSettings.GRID_HEIGHT - 1);

        return new Vector2Int(gridX, gridY);
    }

    /// <summary>
    /// 그리드 영역 내의 유효한 인덱스인지 확인
    /// </summary>
    /// <param name="gridX">그리드 X 인덱스</param>
    /// <param name="gridY">그리드 Y 인덱스</param>
    /// <returns>유효한 인덱스인지 여부</returns>
    public static bool IsValidGridIndex(int gridX, int gridY)
    {
        return gridX >= 0 && gridX < GridSettings.GRID_WIDTH &&
               gridY >= 0 && gridY < GridSettings.GRID_HEIGHT;
    }

    /// <summary>
    /// 그리드 시스템 디버그 정보 출력
    /// </summary>
    public static void DebugGridInfo()
    {
        Debug.Log($"그리드 시스템 정보:");
        Debug.Log($"- 그리드 크기: {GridSettings.GRID_SIZE}px");
        Debug.Log($"- 그리드 개수: {GridSettings.GRID_WIDTH} x {GridSettings.GRID_HEIGHT}");
        Debug.Log($"- 기준 해상도: {GridSettings.REFERENCE_WIDTH} x {GridSettings.REFERENCE_HEIGHT}");
        Debug.Log($"- 현재 해상도: {Screen.width} x {Screen.height}");
        Debug.Log($"- 스케일 비율: {Screen.width / GridSettings.REFERENCE_WIDTH:F2} x {Screen.height / GridSettings.REFERENCE_HEIGHT:F2}");
    }

}