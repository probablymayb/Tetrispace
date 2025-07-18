using UnityEngine;

/// <summary>
/// 완벽 정렬 테트리스 블록
/// 
/// PlayerController와 정확히 같은 그리드 시스템 사용
/// - PlayerController: GetGridPos() 사용 (그리드 모서리)
/// - TetrisBlock: GetGridMiddlePos() 사용 (그리드 중심)
/// </summary>
public class TetrisBlock : MonoBehaviour
{
    [Header("=== 블록 설정 ===")]
    [SerializeField] private float blockPixelSize = 28f;  // 블록 크기

    [Header("=== 그리드 위치 ===")]
    [SerializeField] private bool useStaticPosition = true;  // 정적 위치 사용 여부
    [SerializeField] private int customGridX = 0;           // 커스텀 그리드 X
    [SerializeField] private int customGridY = 0;           // 커스텀 그리드 Y

    void Start()
    {
        // 블록 크기 설정
        SetBlockSize();

        // 그리드 정보 출력 (한 번만)
        if (useStaticPosition)
        {
            GridSystem.DebugGridInfo();
            GridSystem.TestGridCalculation();
        }
    }

    void Update()
    {
        //  PlayerController와 동일한 방식으로 위치 계산
        Vector2Int gridPos;

        if (useStaticPosition)
        {
            // 정적 GridMiddlePos 사용 (PlayerController의 GridPos와 연동)
            gridPos = GridSystem.GetGridMiddlePos(GridSystem.GridMiddlePos.x, GridSystem.GridMiddlePos.y);
        }
        else
        {
            // 커스텀 위치 사용
            gridPos = GridSystem.GetGridMiddlePos(customGridX, customGridY);
        }

        //  PlayerController와 동일한 스크린-월드 변환
        Vector3 screenPos = new Vector3(gridPos.x, gridPos.y, 10f);
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(screenPos);
        worldPos.z = 0;

        transform.position = worldPos;
    }

    /// <summary>
    /// 블록 크기 설정
    /// </summary>
    void SetBlockSize()
    {
        // 그리드 크기와 블록 크기 비율 계산
        float scaleRatio = blockPixelSize / GridSystem.GridSettings.GRID_SIZE;
        transform.localScale = Vector3.one * scaleRatio;

        Debug.Log($"블록 크기 설정: {GridSystem.GridSettings.GRID_SIZE}px → {blockPixelSize}px (비율: {scaleRatio:F2})");
    }

    /// <summary>
    /// 특정 그리드 위치로 이동
    /// </summary>
    public void SetGridPosition(int gridX, int gridY)
    {
        if (GridSystem.IsValidGridIndex(gridX, gridY))
        {
            customGridX = gridX;
            customGridY = gridY;
            useStaticPosition = false;

            Debug.Log($"블록 위치 설정: [{gridX}][{gridY}]");
        }
        else
        {
            Debug.LogWarning($"유효하지 않은 그리드 위치: [{gridX}][{gridY}]");
        }
    }

    /// <summary>
    /// 정적 위치 사용으로 변경
    /// </summary>
    public void UseStaticPosition()
    {
        useStaticPosition = true;
        Debug.Log("정적 위치 사용 모드로 변경");
    }

    /// <summary>
    /// 현재 위치 정보 출력
    /// </summary>
    [ContextMenu("현재 위치 출력")]
    public void PrintCurrentPosition()
    {
        Vector3 worldPos = transform.position;
        Vector2Int gridIndex = GridSystem.WorldToGridIndex(worldPos);

        Debug.Log($"=== TetrisBlock 위치 정보 ===");
        Debug.Log($"월드 좌표: {worldPos}");
        Debug.Log($"그리드 인덱스: [{gridIndex.x}][{gridIndex.y}]");
        Debug.Log($"사용 중인 그리드 위치: [{(useStaticPosition ? GridSystem.GridMiddlePos.x : customGridX)}][{(useStaticPosition ? GridSystem.GridMiddlePos.y : customGridY)}]");
        Debug.Log($"모드: {(useStaticPosition ? "정적 위치" : "커스텀 위치")}");
    }

    /// <summary>
    /// 그리드 정렬 테스트
    /// </summary>
    [ContextMenu("정렬 테스트")]
    public void TestAlignment()
    {
        Debug.Log("=== 정렬 테스트 ===");

        // 여러 위치에서 테스트
        int[] testPositions = { 0, 1, 17, 35 };

        foreach (int pos in testPositions)
        {
            if (GridSystem.IsValidGridIndex(pos, 0))
            {
                Vector2Int playerPos = GridSystem.GetGridPos(pos, 0);
                Vector2Int blockPos = GridSystem.GetGridMiddlePos(pos, 0);

                Debug.Log($"위치 [{pos}][0] - Player: {playerPos}, Block: {blockPos}, 차이: {blockPos.x - playerPos.x}px");
            }
        }
    }
}