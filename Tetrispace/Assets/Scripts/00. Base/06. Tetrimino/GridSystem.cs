using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GridSystem
{
    [System.Serializable]
    public static class GridSettings
    {
        // === 고정 해상도 및 그리드 설정 ===
        public const float REFERENCE_WIDTH = 1024f;   // 고정 해상도 가로
        public const float REFERENCE_HEIGHT = 784f;   // 고정 해상도 세로
        public const float GRID_SIZE = 28f;           // 그리드 한 칸 크기 (픽셀)

        // === 계산된 그리드 정보 ===
        public static int ActualGridWidth => 32;      // 1024 / 28 = 36.57 → 36개 (0~35)
        public static int ActualGridHeight => 27;     // 784 / 28 = 28 → 28개 (0~27)

        // === 여백 계산 ===
        public static float HorizontalMargin => 128f;   // (1024 - 36*28) / 2 = 8px
        public static float VerticalMargin => 12f;     // (784 - 28*28) / 2 = 0px (정확히 나누어떨어짐)
    }

    private static Vector2Int minVector = new Vector2Int(64, 6);
    private static Vector2Int maxVector = new Vector2Int(960, 762);

    // === 정적 그리드 위치 변수 ===
    public static Vector2Int GridPos = new Vector2Int(0, 0);
    public static Vector2Int GridMiddlePos = new Vector2Int(0, 0);

    /// <summary>
    /// PlayerController용 그리드 위치 계산
    /// GridPos[0][0] = 8px, GridPos[35][35] = 988px
    /// </summary>
    public static Vector2Int GetGridPos(int gridX, int gridY)
    {
        // 범위 체크
        if (gridX < 0 || gridX >= GridSettings.ActualGridWidth ||
            gridY < 0 || gridY >= GridSettings.ActualGridHeight)
        {
            Debug.LogWarning($"그리드 인덱스 범위 초과: [{gridX}][{gridY}] (최대: {GridSettings.ActualGridWidth - 1}x{GridSettings.ActualGridHeight - 1})");
            return Vector2Int.zero;
        }

        // PlayerController 위치 계산: 여백 + (인덱스 * 그리드크기)
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

        // 현재 해상도에 맞게 스케일링 (고정 해상도라면 보통 1.0)
        float scaleX = Screen.width / GridSettings.REFERENCE_WIDTH;
        float scaleY = Screen.height / GridSettings.REFERENCE_HEIGHT;

        posX *= scaleX;
        posY *= scaleY;

        return new Vector2Int((int)posX, (int)posY);
    }

    /// <summary>
    /// Block용 그리드 중심점 계산
    /// GridMiddlePos[0][0] = 22px, GridMiddlePos[35][35] = 1002px
    /// </summary>
    public static Vector2Int GetGridMiddlePos(int gridX, int gridY)
    {
        // 범위 체크
        if (gridX < 0 || gridX >= GridSettings.ActualGridWidth ||
            gridY < 0 || gridY >= GridSettings.ActualGridHeight)
        {
            Debug.LogWarning($"그리드 인덱스 범위 초과: [{gridX}][{gridY}] (최대: {GridSettings.ActualGridWidth - 1}x{GridSettings.ActualGridHeight - 1})");
            return Vector2Int.zero;
        }

        // Block 중심점 계산: 여백 + (인덱스 * 그리드크기) + (그리드크기 / 2)
        float posX = GridSettings.HorizontalMargin + (gridX * GridSettings.GRID_SIZE) + (GridSettings.GRID_SIZE * 0.5f);
        float posY = GridSettings.VerticalMargin + (gridY * GridSettings.GRID_SIZE) + (GridSettings.GRID_SIZE * 0.5f);

        // 현재 해상도에 맞게 스케일링
        float scaleX = Screen.width / GridSettings.REFERENCE_WIDTH;
        float scaleY = Screen.height / GridSettings.REFERENCE_HEIGHT;

        posX *= scaleX;
        posY *= scaleY;

        return new Vector2Int((int)posX, (int)posY);
    }

    /// <summary>
    /// PlayerController용 월드 좌표 계산
    /// </summary>
    public static Vector3 GetGridWorldPosition(int gridX, int gridY)
    {
        // 범위 체크
        if (gridX < 0 || gridX >= GridSettings.ActualGridWidth ||
            gridY < 0 || gridY >= GridSettings.ActualGridHeight)
        {
            Debug.LogWarning($"그리드 인덱스 범위 초과: [{gridX}][{gridY}]");
            return Vector3.zero;
        }

        // 스크린 좌표 계산
        float screenX = GridSettings.HorizontalMargin + (gridX * GridSettings.GRID_SIZE);
        float screenY = GridSettings.VerticalMargin + (gridY * GridSettings.GRID_SIZE);

        // 현재 해상도에 맞게 스케일링
        float scaleX = Screen.width / GridSettings.REFERENCE_WIDTH;
        float scaleY = Screen.height / GridSettings.REFERENCE_HEIGHT;

        screenX *= scaleX;
        screenY *= scaleY;

        // Unity 스크린 좌표계 보정 (왼쪽 아래가 (0,0))
        screenY = Screen.height - screenY;

        // 스크린 좌표를 월드 좌표로 변환
        Vector3 screenPos = new Vector3(screenX, screenY, 0);
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(screenPos);
        worldPos.z = 0;

        return worldPos;
    }

    /// <summary>
    /// Block용 월드 좌표 중심점 계산
    /// </summary>
    public static Vector3 GetGridMiddleWorldPosition(int gridX, int gridY)
    {
        // 범위 체크
        if (gridX < 0 || gridX >= GridSettings.ActualGridWidth ||
            gridY < 0 || gridY >= GridSettings.ActualGridHeight)
        {
            Debug.LogWarning($"그리드 인덱스 범위 초과: [{gridX}][{gridY}]");
            return Vector3.zero;
        }

        // 스크린 좌표 계산 (중심점)
        float screenX = GridSettings.HorizontalMargin + (gridX * GridSettings.GRID_SIZE) + (GridSettings.GRID_SIZE * 0.5f);
        float screenY = GridSettings.VerticalMargin + (gridY * GridSettings.GRID_SIZE) + (GridSettings.GRID_SIZE * 0.5f);

        // 현재 해상도에 맞게 스케일링
        float scaleX = Screen.width / GridSettings.REFERENCE_WIDTH;
        float scaleY = Screen.height / GridSettings.REFERENCE_HEIGHT;

        screenX *= scaleX;
        screenY *= scaleY;

        // Unity 스크린 좌표계 보정
        screenY = Screen.height - screenY;

        // 스크린 좌표를 월드 좌표로 변환
        Vector3 screenPos = new Vector3(screenX, screenY, 0);
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(screenPos);
        worldPos.z = 0;

        return worldPos;
    }

    /// <summary>
    /// 스크린 좌표를 그리드 인덱스로 변환
    /// </summary>
    public static Vector2Int ScreenToGridIndex(Vector2 screenPos)
    {
        // 현재 해상도에 맞게 역스케일링
        float scaleX = Screen.width / GridSettings.REFERENCE_WIDTH;
        float scaleY = Screen.height / GridSettings.REFERENCE_HEIGHT;

        screenPos.x /= scaleX;
        screenPos.y /= scaleY;

        // 여백을 빼고 그리드 인덱스 계산
        screenPos.x -= GridSettings.HorizontalMargin;
        screenPos.y -= GridSettings.VerticalMargin;

        int gridX = Mathf.FloorToInt(screenPos.x / GridSettings.GRID_SIZE);
        int gridY = Mathf.FloorToInt(screenPos.y / GridSettings.GRID_SIZE);

        // 범위 클램핑
        gridX = Mathf.Clamp(gridX, 0, GridSettings.ActualGridWidth - 1);
        gridY = Mathf.Clamp(gridY, 0, GridSettings.ActualGridHeight - 1);

        return new Vector2Int(gridX, gridY);
    }

    /// <summary>
    /// 월드 좌표를 그리드 인덱스로 변환
    /// </summary>
    public static Vector2Int WorldToGridIndex(Vector3 worldPos)
    {
        // 월드 좌표를 스크린 좌표로 변환
        Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPos);

        // Unity 스크린 좌표계 보정
        screenPos.y = Screen.height - screenPos.y;

        return ScreenToGridIndex(new Vector2(screenPos.x, screenPos.y));
    }

    /// <summary>
    /// 유효한 그리드 인덱스인지 확인
    /// </summary>
    public static bool IsValidGridIndex(int gridX, int gridY)
    {
        return gridX >= 0 && gridX < GridSettings.ActualGridWidth &&
               gridY >= 0 && gridY < GridSettings.ActualGridHeight;
    }

    /// <summary>
    //그리드 시스템 디버그 정보 출력
    /// </summary>
    public static void DebugGridInfo()
    {
        Debug.Log($"===  최종 그리드 시스템 정보 ===");
        Debug.Log($"해상도: {GridSettings.REFERENCE_WIDTH} x {GridSettings.REFERENCE_HEIGHT}");
        Debug.Log($"그리드 크기: {GridSettings.GRID_SIZE}px");
        Debug.Log($"그리드 개수: {GridSettings.ActualGridWidth} x {GridSettings.ActualGridHeight} (0~{GridSettings.ActualGridWidth - 1}, 0~{GridSettings.ActualGridHeight - 1})");
        Debug.Log($"여백: 가로 {GridSettings.HorizontalMargin}px, 세로 {GridSettings.VerticalMargin}px");
        Debug.Log($"");
        Debug.Log($"=== PlayerController 위치 (GetGridPos) ===");
        Debug.Log($"GridPos[0][0] = {GridSettings.HorizontalMargin}px");
        Debug.Log($"GridPos[35][27] = {GridSettings.HorizontalMargin + 35 * GridSettings.GRID_SIZE}px, {GridSettings.VerticalMargin + 27 * GridSettings.GRID_SIZE}px");
        Debug.Log($"");
        Debug.Log($"=== Block 위치 (GetGridMiddlePos) ===");
        Debug.Log($"GridMiddlePos[0][0] = {GridSettings.HorizontalMargin + GridSettings.GRID_SIZE * 0.5f}px");
        Debug.Log($"GridMiddlePos[35][27] = {GridSettings.HorizontalMargin + 35 * GridSettings.GRID_SIZE + GridSettings.GRID_SIZE * 0.5f}px, {GridSettings.VerticalMargin + 27 * GridSettings.GRID_SIZE + GridSettings.GRID_SIZE * 0.5f}px");
    }

    /// <summary>
    /// 그리드 좌표 검증 (테스트용)
    /// </summary>
    public static void TestGridCalculation()
    {
        Debug.Log("===  그리드 계산 테스트 ===");

        // PlayerController 위치 테스트
        Vector2Int playerPos0 = GetGridPos(0, 0);
        Vector2Int playerPos35 = GetGridPos(35, 27);
        Debug.Log($"Player [0,0]: {playerPos0} (예상: 8, 0)");
        Debug.Log($"Player [35,27]: {playerPos35} (예상: 988, 756)");

        // Block 위치 테스트
        Vector2Int blockPos0 = GetGridMiddlePos(0, 0);
        Vector2Int blockPos35 = GetGridMiddlePos(35, 27);
        Debug.Log($"Block [0,0]: {blockPos0} (예상: 22, 14)");
        Debug.Log($"Block [35,27]: {blockPos35} (예상: 1002, 770)");
    }
}