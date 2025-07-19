using UnityEngine;

/// <summary>
/// ���� GridSystem ������ ��Ȯ�� �ݿ��� �׸��� �ð�ȭ
/// </summary>
public class GridVisualizer : MonoBehaviour
{
    [Header("=== �׸��� ǥ�� ���� ===")]
    [SerializeField] private bool showGrid = true;
    [SerializeField] private bool showPlayerGrid = true;    // GetGridPos ǥ��
    [SerializeField] private bool showBlockGrid = true;     // GetGridMiddlePos ǥ��
    [SerializeField] private bool showCurrentPos = true;    // ���� ��ġ ǥ��

    [Header("=== ���� ���� ===")]
    [SerializeField] private Color gridLineColor = Color.white;
    [SerializeField] private Color playerPosColor = Color.red;
    [SerializeField] private Color blockPosColor = Color.blue;
    [SerializeField] private Color currentPlayerColor = Color.yellow;
    [SerializeField] private Color currentBlockColor = Color.green;

    [Header("=== ǥ�� ���� ===")]
    [SerializeField] private int gridStep = 2;              // �� ĭ���� ǥ������
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
    /// ���� GridSystem�� ����� �׸��� ���� �׸���
    /// </summary>
    private void DrawGridLines()
    {
        Gizmos.color = gridLineColor;

        // GridSystem ���� ������ ���� �׸���
        int maxX = GridSystem.GridSettings.ActualGridWidth;
        int maxY = GridSystem.GridSettings.ActualGridHeight;

        // ���� ���� (X��)
        for (int x = 0; x <= maxX; x += gridStep)
        {
            // ���� ��
            Vector2Int topScreen = GridSystem.GetGridPos(x, maxY - 1);
            Vector3 topWorld = ScreenToWorld(topScreen);

            // �Ʒ��� ��  
            Vector2Int bottomScreen = GridSystem.GetGridPos(x, 0);
            Vector3 bottomWorld = ScreenToWorld(bottomScreen);

            Gizmos.DrawLine(topWorld, bottomWorld);
        }

        // ���� ���� (Y��)
        for (int y = 0; y <= maxY; y += gridStep)
        {
            // ���� ��
            Vector2Int leftScreen = GridSystem.GetGridPos(0, y);
            Vector3 leftWorld = ScreenToWorld(leftScreen);

            // ������ ��
            Vector2Int rightScreen = GridSystem.GetGridPos(maxX - 1, y);
            Vector3 rightWorld = ScreenToWorld(rightScreen);

            Gizmos.DrawLine(leftWorld, rightWorld);
        }
    }

    /// <summary>
    /// PlayerController ��ġ ǥ�� (GetGridPos ���)
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
    /// Block ��ġ ǥ�� (GetGridMiddlePos ���)
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
    /// ���� �÷��̾�/��� ��ġ ǥ��
    /// </summary>
    private void DrawCurrentPositions()
    {
        // ���� �÷��̾� ��ġ
        Gizmos.color = currentPlayerColor;
        Vector2Int currentPlayerScreen = GridSystem.GetGridPos(GridSystem.GridPos.x, GridSystem.GridPos.y);
        Vector3 currentPlayerWorld = ScreenToWorld(currentPlayerScreen);
        Gizmos.DrawWireCube(currentPlayerWorld, Vector3.one * (playerMarkerSize * 3f));

        // ���� ��� ��ġ
        Gizmos.color = currentBlockColor;
        Vector2Int currentBlockScreen = GridSystem.GetGridMiddlePos(GridSystem.GridMiddlePos.x, GridSystem.GridMiddlePos.y);
        Vector3 currentBlockWorld = ScreenToWorld(currentBlockScreen);
        Gizmos.DrawSphere(currentBlockWorld, blockMarkerSize * 3f);
    }

    /// <summary>
    /// ��ũ�� ��ǥ�� ���� ��ǥ�� ��ȯ (GridSystem�� ������ ����)
    /// </summary>
    private Vector3 ScreenToWorld(Vector2Int screenPos)
    {
        // Unity ��ũ�� ��ǥ�� ��ȯ (���� �Ʒ��� (0,0))
        Vector3 unityScreenPos = new Vector3(screenPos.x, Screen.height - screenPos.y, 10f);

        Vector3 worldPos = Camera.main.ScreenToWorldPoint(unityScreenPos);
        worldPos.z = 0;

        return worldPos;
    }

    /// <summary>
    /// ��Ÿ�� GUI ���� ǥ��
    /// </summary>
    private void OnGUI()
    {
        if (!showGrid || !Application.isPlaying) return;

        GUILayout.BeginArea(new Rect(10, 10, 500, 300));

        GUILayout.Label("=== ���� GridSystem ���� ===", new GUIStyle(GUI.skin.label) { fontSize = 14, fontStyle = FontStyle.Bold });

        GUILayout.Space(5);
        GUILayout.Label($"�÷��̾� �׸��� �ε���: {GridSystem.GridPos}");
        GUILayout.Label($"��� �׸��� �ε���: {GridSystem.GridMiddlePos}");

        GUILayout.Space(5);
        Vector2Int playerScreen = GridSystem.GetGridPos(GridSystem.GridPos.x, GridSystem.GridPos.y);
        Vector2Int blockScreen = GridSystem.GetGridMiddlePos(GridSystem.GridMiddlePos.x, GridSystem.GridMiddlePos.y);

        GUILayout.Label($"�÷��̾� ��ũ�� ��ǥ: {playerScreen}");
        GUILayout.Label($"��� ��ũ�� ��ǥ: {blockScreen}");

        GUILayout.Space(5);
        Vector3 playerWorld = ScreenToWorld(playerScreen);
        Vector3 blockWorld = ScreenToWorld(blockScreen);

        GUILayout.Label($"�÷��̾� ���� ��ǥ: ({playerWorld.x:F2}, {playerWorld.y:F2})");
        GUILayout.Label($"��� ���� ��ǥ: ({blockWorld.x:F2}, {blockWorld.y:F2})");

        GUILayout.Space(10);

        if (GUILayout.Button("GridSystem ���� ���"))
        {
            GridSystem.DebugGridInfo();
        }

        if (GUILayout.Button("GridSystem ��� �׽�Ʈ"))
        {
            GridSystem.TestGridCalculation();
        }

        if (GUILayout.Button("���� ��ġ �׽�Ʈ"))
        {
            TestCurrentPositions();
        }

        GUILayout.Space(10);
        GUILayout.Label("����Ű: G(�׸��� ���), H(�÷��̾�), J(���), K(������ġ)");

        GUILayout.EndArea();
    }

    /// <summary>
    /// ���� ��ġ �׽�Ʈ
    /// </summary>
    private void TestCurrentPositions()
    {
        Debug.Log("=== ���� ��ġ �׽�Ʈ ===");
        Debug.Log($"GridPos: {GridSystem.GridPos}");
        Debug.Log($"GridMiddlePos: {GridSystem.GridMiddlePos}");

        Vector2Int playerScreen = GridSystem.GetGridPos(GridSystem.GridPos.x, GridSystem.GridPos.y);
        Vector2Int blockScreen = GridSystem.GetGridMiddlePos(GridSystem.GridMiddlePos.x, GridSystem.GridMiddlePos.y);

        Debug.Log($"�÷��̾� ��ũ��: {playerScreen}");
        Debug.Log($"��� ��ũ��: {blockScreen}");

        Vector3 playerWorld = ScreenToWorld(playerScreen);
        Vector3 blockWorld = ScreenToWorld(blockScreen);

        Debug.Log($"�÷��̾� ����: {playerWorld}");
        Debug.Log($"��� ����: {blockWorld}");
    }

    /// <summary>
    /// Ű���� ����Ű ó��
    /// </summary>
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.G))
        {
            showGrid = !showGrid;
            Debug.Log($"�׸��� ǥ��: {showGrid}");
        }

        if (Input.GetKeyDown(KeyCode.H))
        {
            showPlayerGrid = !showPlayerGrid;
            Debug.Log($"�÷��̾� �׸��� ǥ��: {showPlayerGrid}");
        }

        if (Input.GetKeyDown(KeyCode.J))
        {
            showBlockGrid = !showBlockGrid;
            Debug.Log($"��� �׸��� ǥ��: {showBlockGrid}");
        }

        if (Input.GetKeyDown(KeyCode.K))
        {
            showCurrentPos = !showCurrentPos;
            Debug.Log($"���� ��ġ ǥ��: {showCurrentPos}");
        }
    }
}