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
        public const float REFERENCE_WIDTH = 1024f;
        public const float REFERENCE_HEIGHT = 768f;
        public const float GRID_SIZE = 28f;

        // === 정확한 계산 ===
        public static int ActualGridWidth => 32;      // 896 / 28 = 32개 (0~31)
        public static int ActualGridHeight => 27;     // 768 / 28 = 27.43 → 27개 (0~26)

        // === 여백 계산 (수정) ===
        public static float HorizontalMargin => 64f;    // (1024 - 896) / 2 = 64px
        public static float VerticalMargin => 12f;      // (768 - 27*28) / 2 = 12px

        // === 게임 영역 ===
        public static float GameAreaWidth => 896f;      // 실제 게임 영역
        public static float GameAreaStartX => 64f;      // 게임 영역 시작점
        public static float GameAreaEndX => 960f;       // 게임 영역 끝점 (64 + 896)
    }

    // === 정적 그리드 위치 변수 ===
    public static Vector2Int GridPos = new Vector2Int(5, 5);
    public static Vector2Int GridMiddlePos = new Vector2Int(0, 0);

    /// <summary>
    /// PlayerController용 그리드 위치 계산 (그리드 왼쪽 모서리)
    /// grid[0] = 64px, grid[1] = 92px, grid[31] = 932px
    /// </summary>
    public static Vector2Int GetGridPos(int gridX, int gridY)
    {
        gridX = Mathf.Clamp(gridX, 0, GridSettings.ActualGridWidth - 1);
        gridY = Mathf.Clamp(gridY, 0, GridSettings.ActualGridHeight - 1);
        // 범위 체크
        if (gridX < 0 || gridX >= GridSettings.ActualGridWidth ||
            gridY < 0 || gridY >= GridSettings.ActualGridHeight)
        {
            Debug.LogWarning($"그리드 인덱스 범위 초과: [{gridX}][{gridY}] (최대: {GridSettings.ActualGridWidth - 1}x{GridSettings.ActualGridHeight - 1})");
            return Vector2Int.zero;
        }

        // 정확한 계산: 왼쪽 여백 + (그리드 인덱스 * 그리드 크기)
        float posX = GridSettings.HorizontalMargin + (gridX * GridSettings.GRID_SIZE);
        float posY = GridSettings.VerticalMargin + (gridY * GridSettings.GRID_SIZE);

        // 범위 체크 (게임 영역 내)
        posX = Mathf.Clamp(posX, GridSettings.GameAreaStartX, GridSettings.GameAreaEndX - GridSettings.GRID_SIZE);
        posY = Mathf.Clamp(posY, GridSettings.VerticalMargin, GridSettings.VerticalMargin + (GridSettings.ActualGridHeight - 1) * GridSettings.GRID_SIZE);

        // 해상도 스케일링
        float scaleX = Screen.width / GridSettings.REFERENCE_WIDTH;
        float scaleY = Screen.height / GridSettings.REFERENCE_HEIGHT;

        posX *= scaleX;
        posY *= scaleY;

        return new Vector2Int(Mathf.RoundToInt(posX), Mathf.RoundToInt(posY));
    }

    /// <summary>
    /// Block용 그리드 중심점 계산 (그리드 중앙)
    /// grid[0] = 78px, grid[1] = 106px, grid[31] = 946px
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

        // 정확한 계산: 왼쪽 여백 + (그리드 인덱스 * 그리드 크기) + (그리드 크기 / 2)
        float posX = GridSettings.HorizontalMargin + (gridX * GridSettings.GRID_SIZE) + (GridSettings.GRID_SIZE * 0.5f);
        float posY = GridSettings.VerticalMargin + (gridY * GridSettings.GRID_SIZE) + (GridSettings.GRID_SIZE * 0.5f);

        // 범위 체크
        posX = Mathf.Clamp(posX, GridSettings.HorizontalMargin + 14f, GridSettings.GameAreaEndX - 14f);
        posY = Mathf.Clamp(posY, GridSettings.VerticalMargin + 14f, GridSettings.VerticalMargin + GridSettings.ActualGridHeight * GridSettings.GRID_SIZE - 14f);

        // 해상도 스케일링
        float scaleX = Screen.width / GridSettings.REFERENCE_WIDTH;
        float scaleY = Screen.height / GridSettings.REFERENCE_HEIGHT;

        posX *= scaleX;
        posY *= scaleY;

        return new Vector2Int(Mathf.RoundToInt(posX), Mathf.RoundToInt(posY));
    }

    /// <summary>
    /// PlayerController용 월드 좌표 계산
    /// </summary>
    public static Vector3 GetGridWorldPosition(int gridX, int gridY)
    {
        Vector2Int screenPos = GetGridPos(gridX, gridY);
        return ScreenToWorld(screenPos);
    }

    /// <summary>
    /// Block용 월드 좌표 중심점 계산
    /// </summary>
    public static Vector3 GetGridMiddleWorldPosition(int gridX, int gridY)
    {
        Vector2Int screenPos = GetGridMiddlePos(gridX, gridY);
        return ScreenToWorld(screenPos);
    }

    /// <summary>
    /// 스크린 좌표를 월드 좌표로 변환
    /// </summary>
    private static Vector3 ScreenToWorld(Vector2Int screenPos)
    {
        Vector3 unityScreenPos = new Vector3(screenPos.x, Screen.height - screenPos.y, 10f);
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(unityScreenPos);
        worldPos.z = 0;
        return worldPos;
    }

    /// <summary>
    /// 스크린 좌표를 그리드 인덱스로 변환
    /// </summary>
    public static Vector2Int ScreenToGridIndex(Vector2 screenPos)
    {
        // 해상도 역스케일링
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
        Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPos);
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
    /// 그리드 시스템 디버그 정보 출력
    /// </summary>
    public static void DebugGridInfo()
    {
        Debug.Log("=== 수정된 그리드 시스템 정보 ===");
        Debug.Log($"해상도: {GridSettings.REFERENCE_WIDTH} x {GridSettings.REFERENCE_HEIGHT}");
        Debug.Log($"게임 영역: {GridSettings.GameAreaWidth}px (시작: {GridSettings.GameAreaStartX}, 끝: {GridSettings.GameAreaEndX})");
        Debug.Log($"그리드 크기: {GridSettings.GRID_SIZE}px");
        Debug.Log($"그리드 개수: {GridSettings.ActualGridWidth} x {GridSettings.ActualGridHeight}");
        Debug.Log($"여백: 좌우 각 {GridSettings.HorizontalMargin}px, 상하 각 {GridSettings.VerticalMargin}px");
        Debug.Log("");
        Debug.Log("=== 계산 검증 ===");
        Debug.Log($"플레이어 grid[0] = {GetGridPos(0, 0)} (예상: 64)");
        Debug.Log($"플레이어 grid[1] = {GetGridPos(1, 0)} (예상: 92)");
        Debug.Log($"플레이어 grid[31] = {GetGridPos(31, 0)} (예상: 932)");
        Debug.Log($"블록 grid[0] = {GetGridMiddlePos(0, 0)} (예상: 78)");
        Debug.Log($"블록 grid[1] = {GetGridMiddlePos(1, 0)} (예상: 106)");
        Debug.Log($"블록 grid[31] = {GetGridMiddlePos(31, 0)} (예상: 946)");
    }

    /// <summary>
    /// 그리드 계산 테스트
    /// </summary>
    public static void TestGridCalculation()
    {
        Debug.Log("=== 그리드 계산 정확성 테스트 ===");

        // 플레이어 위치 테스트
        for (int i = 0; i <= 31; i += 10)
        {
            Vector2Int pos = GetGridPos(i, 0);
            int expected = 64 + i * 28;
            Debug.Log($"플레이어 grid[{i}] = {pos.x} (예상: {expected}) - {(pos.x == expected ? "o" : "x")}");
        }

        // 블록 위치 테스트
        for (int i = 0; i <= 31; i += 10)
        {
            Vector2Int pos = GetGridMiddlePos(i, 0);
            int expected = 64 + i * 28 + 14;
            Debug.Log($"블록 grid[{i}] = {pos.x} (예상: {expected}) - {(pos.x == expected ? "o" : "x")}");
        }
    }
    
    /// <summary>
    /// 주어진 그리드 칸 수에 해당하는 월드 거리 (X축 또는 Y축 방향)
    /// </summary>
    /// <param name="gridCount">그리드 단위 수</param>
    /// <param name="axis">계산할 방향 (Vector2.right 또는 Vector2.up)</param>
    /// <returns>해당 그리드 수만큼의 월드 거리</returns>
    public static float GetWorldDistanceFromGridCount(int gridCount, Vector2 axis)
    {
        if (axis == Vector2.right)
        {
            // X 방향: GridPos.x = 0 → GridPos.x = gridCount 까지의 거리
            Vector3 worldStart = GetGridWorldPosition(0, 0);
            Vector3 worldEnd = GetGridWorldPosition(gridCount, 0);
            return Mathf.Abs(worldEnd.x - worldStart.x);
        }
        else if (axis == Vector2.up)
        {
            // Y 방향
            Vector3 worldStart = GetGridWorldPosition(0, 0);
            Vector3 worldEnd = GetGridWorldPosition(0, gridCount);
            return Mathf.Abs(worldEnd.y - worldStart.y);
        }
        else
        {
            Debug.LogWarning("GetWorldDistanceFromGridCount: axis는 Vector2.right 또는 Vector2.up이어야 합니다.");
            return 0f;
        }
    }
    /// <summary>
    /// Grid X 1단위 당 얼마의 월드 거리인지 반환
    /// </summary>
    public static float WorldUnitsPerGridX => GetWorldDistanceFromGridCount(1, Vector2.right);
    /// <summary>
    /// Grid Y 1단위 당 얼마의 월드 거리인지 반환
    /// </summary>
    public static float WorldUnitsPerGridY => GetWorldDistanceFromGridCount(1, Vector2.up);
}
