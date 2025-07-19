using UnityEngine;

/// <summary>
/// 실제 GridSystem 로직을 정확히 반영한 그리드 시각화
/// </summary>
public class GridVisualizer : MonoBehaviour
{
    [Header("=== 그리드 표시 설정 ===")]
    [SerializeField] private bool showGrid = true;
    [SerializeField] private bool showPlayerGrid = true;    // GetGridPos 표시
    [SerializeField] private bool showBlockGrid = true;     // GetGridMiddlePos 표시
    [SerializeField] private bool showCurrentPos = true;    // 현재 위치 표시

    [Header("=== 색상 설정 ===")]
    [SerializeField] private Color gridLineColor = Color.white;
    [SerializeField] private Color playerPosColor = Color.red;
    [SerializeField] private Color blockPosColor = Color.blue;
    [SerializeField] private Color currentPlayerColor = Color.yellow;
    [SerializeField] private Color currentBlockColor = Color.green;

    [Header("=== 표시 설정 ===")]
    [SerializeField] private int gridStep = 2;              // 몇 칸마다 표시할지
    [SerializeField] private float playerMarkerSize = 0.1f;
    [SerializeField] private float blockMarkerSize = 0.05f;

    private void OnDrawGizmos()
    {
        if (!showGrid) return;

        DrawGridLines();

        if (showPlayerGrid)
            DrawPlayerPositions();

        if (showBlockGrid)
            DrawBlockPositions();

        if (showCurrentPos)
            DrawCurrentPositions();
    }

    /// <summary>
    /// 실제 GridSystem을 사용한 그리드 라인 그리기
    /// </summary>
    private void DrawGridLines()
    {
        Gizmos.color = gridLineColor;

        // GridSystem 범위 내에서 라인 그리기
        int maxX = GridSystem.GridSettings.ActualGridWidth;
        int maxY = GridSystem.GridSettings.ActualGridHeight;

        // 세로 라인 (X축)
        for (int x = 0; x <= maxX; x += gridStep)
        {
            // 위쪽 점
            Vector2Int topScreen = GridSystem.GetGridPos(x, maxY - 1);
            Vector3 topWorld = ScreenToWorld(topScreen);

            // 아래쪽 점  
            Vector2Int bottomScreen = GridSystem.GetGridPos(x, 0);
            Vector3 bottomWorld = ScreenToWorld(bottomScreen);

            Gizmos.DrawLine(topWorld, bottomWorld);
        }

        // 가로 라인 (Y축)
        for (int y = 0; y <= maxY; y += gridStep)
        {
            // 왼쪽 점
            Vector2Int leftScreen = GridSystem.GetGridPos(0, y);
            Vector3 leftWorld = ScreenToWorld(leftScreen);

            // 오른쪽 점
            Vector2Int rightScreen = GridSystem.GetGridPos(maxX - 1, y);
            Vector3 rightWorld = ScreenToWorld(rightScreen);

            Gizmos.DrawLine(leftWorld, rightWorld);
        }
    }

    /// <summary>
    /// PlayerController 위치 표시 (GetGridPos 사용)
    /// </summary>
    private void DrawPlayerPositions()
    {
        Gizmos.color = playerPosColor;

        for (int x = 0; x < GridSystem.GridSettings.ActualGridWidth; x += gridStep * 2)
        {
            for (int y = 0; y < GridSystem.GridSettings.ActualGridHeight; y += gridStep * 2)
            {
                Vector2Int screenPos = GridSystem.GetGridPos(x, y);
                Vector3 worldPos = ScreenToWorld(screenPos);

                Gizmos.DrawWireCube(worldPos, Vector3.one * playerMarkerSize);
            }
        }
    }

    /// <summary>
    /// Block 위치 표시 (GetGridMiddlePos 사용)
    /// </summary>
    private void DrawBlockPositions()
    {
        Gizmos.color = blockPosColor;

        for (int x = 0; x < GridSystem.GridSettings.ActualGridWidth; x += gridStep * 2)
        {
            for (int y = 0; y < GridSystem.GridSettings.ActualGridHeight; y += gridStep * 2)
            {
                Vector2Int screenPos = GridSystem.GetGridMiddlePos(x, y);
                Vector3 worldPos = ScreenToWorld(screenPos);

                Gizmos.DrawSphere(worldPos, blockMarkerSize);
            }
        }
    }

    /// <summary>
    /// 현재 플레이어/블록 위치 표시
    /// </summary>
    private void DrawCurrentPositions()
    {
        // 현재 플레이어 위치
        Gizmos.color = currentPlayerColor;
        Vector2Int currentPlayerScreen = GridSystem.GetGridPos(GridSystem.GridPos.x, GridSystem.GridPos.y);
        Vector3 currentPlayerWorld = ScreenToWorld(currentPlayerScreen);
        Gizmos.DrawWireCube(currentPlayerWorld, Vector3.one * (playerMarkerSize * 3f));

        // 현재 블록 위치
        Gizmos.color = currentBlockColor;
        Vector2Int currentBlockScreen = GridSystem.GetGridMiddlePos(GridSystem.GridMiddlePos.x, GridSystem.GridMiddlePos.y);
        Vector3 currentBlockWorld = ScreenToWorld(currentBlockScreen);
        Gizmos.DrawSphere(currentBlockWorld, blockMarkerSize * 3f);
    }

    /// <summary>
    /// 스크린 좌표를 월드 좌표로 변환 (GridSystem과 동일한 로직)
    /// </summary>
    private Vector3 ScreenToWorld(Vector2Int screenPos)
    {
        // Unity 스크린 좌표계 변환 (왼쪽 아래가 (0,0))
        Vector3 unityScreenPos = new Vector3(screenPos.x, Screen.height - screenPos.y, 10f);

        Vector3 worldPos = Camera.main.ScreenToWorldPoint(unityScreenPos);
        worldPos.z = 0;

        return worldPos;
    }

    /// <summary>
    /// 런타임 GUI 정보 표시
    /// </summary>
    private void OnGUI()
    {
        if (!showGrid || !Application.isPlaying) return;

        GUILayout.BeginArea(new Rect(10, 10, 500, 300));

        GUILayout.Label("=== 실제 GridSystem 정보 ===", new GUIStyle(GUI.skin.label) { fontSize = 14, fontStyle = FontStyle.Bold });

        GUILayout.Space(5);
        GUILayout.Label($"플레이어 그리드 인덱스: {GridSystem.GridPos}");
        GUILayout.Label($"블록 그리드 인덱스: {GridSystem.GridMiddlePos}");

        GUILayout.Space(5);
        Vector2Int playerScreen = GridSystem.GetGridPos(GridSystem.GridPos.x, GridSystem.GridPos.y);
        Vector2Int blockScreen = GridSystem.GetGridMiddlePos(GridSystem.GridMiddlePos.x, GridSystem.GridMiddlePos.y);

        GUILayout.Label($"플레이어 스크린 좌표: {playerScreen}");
        GUILayout.Label($"블록 스크린 좌표: {blockScreen}");

        GUILayout.Space(5);
        Vector3 playerWorld = ScreenToWorld(playerScreen);
        Vector3 blockWorld = ScreenToWorld(blockScreen);

        GUILayout.Label($"플레이어 월드 좌표: ({playerWorld.x:F2}, {playerWorld.y:F2})");
        GUILayout.Label($"블록 월드 좌표: ({blockWorld.x:F2}, {blockWorld.y:F2})");

        GUILayout.Space(10);

        if (GUILayout.Button("GridSystem 정보 출력"))
        {
            GridSystem.DebugGridInfo();
        }

        if (GUILayout.Button("GridSystem 계산 테스트"))
        {
            GridSystem.TestGridCalculation();
        }

        if (GUILayout.Button("현재 위치 테스트"))
        {
            TestCurrentPositions();
        }

        GUILayout.Space(10);
        GUILayout.Label("단축키: G(그리드 토글), H(플레이어), J(블록), K(현재위치)");

        GUILayout.EndArea();
    }

    /// <summary>
    /// 현재 위치 테스트
    /// </summary>
    private void TestCurrentPositions()
    {
        Debug.Log("=== 현재 위치 테스트 ===");
        Debug.Log($"GridPos: {GridSystem.GridPos}");
        Debug.Log($"GridMiddlePos: {GridSystem.GridMiddlePos}");

        Vector2Int playerScreen = GridSystem.GetGridPos(GridSystem.GridPos.x, GridSystem.GridPos.y);
        Vector2Int blockScreen = GridSystem.GetGridMiddlePos(GridSystem.GridMiddlePos.x, GridSystem.GridMiddlePos.y);

        Debug.Log($"플레이어 스크린: {playerScreen}");
        Debug.Log($"블록 스크린: {blockScreen}");

        Vector3 playerWorld = ScreenToWorld(playerScreen);
        Vector3 blockWorld = ScreenToWorld(blockScreen);

        Debug.Log($"플레이어 월드: {playerWorld}");
        Debug.Log($"블록 월드: {blockWorld}");
    }

    /// <summary>
    /// 키보드 단축키 처리
    /// </summary>
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.G))
        {
            showGrid = !showGrid;
            Debug.Log($"그리드 표시: {showGrid}");
        }

        if (Input.GetKeyDown(KeyCode.H))
        {
            showPlayerGrid = !showPlayerGrid;
            Debug.Log($"플레이어 그리드 표시: {showPlayerGrid}");
        }

        if (Input.GetKeyDown(KeyCode.J))
        {
            showBlockGrid = !showBlockGrid;
            Debug.Log($"블록 그리드 표시: {showBlockGrid}");
        }

        if (Input.GetKeyDown(KeyCode.K))
        {
            showCurrentPos = !showCurrentPos;
            Debug.Log($"현재 위치 표시: {showCurrentPos}");
        }
    }
}