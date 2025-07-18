using UnityEngine;

public class TetrisBlock : MonoBehaviour
{
    [Header("블록 크기 설정")]
    public float blockPixelSize = 28f;  // 원하는 블록 크기 (픽셀)

    void Start()
    {
        //SetBlockSize();
    }

    void Update()
    {
        // 스크린 좌표를 직접 사용 (카메라 독립적)
        Vector2Int gridPos = GridSystem.GetGridMiddlePos(GridSystem.GridMiddlePos.x, GridSystem.GridMiddlePos.y);

        // 스크린 좌표를 월드 좌표로 변환
        Vector3 screenPos = new Vector3(gridPos.x, gridPos.y, 10f); // Z는 카메라와의 거리
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(screenPos);
        worldPos.z = 0; // 2D 게임용

        transform.position = worldPos;
    }

    /// <summary>
    /// 블록 크기를 28x28 픽셀로 설정
    /// </summary>
    //void SetBlockSize()
    //{
    //    // 현재 그리드 크기 (32픽셀)와 원하는 크기 (28픽셀)의 비율 계산
    //    float scaleRatio = blockPixelSize / GridSystem.GridSettings.GRID_SIZE;

    //    // 블록 스케일 적용
    //    transform.localScale = Vector3.one * scaleRatio;

    //    Debug.Log($"블록 크기 조정: {GridSystem.GridSettings.GRID_SIZE}px → {blockPixelSize}px (비율: {scaleRatio:F2})");
    //}
}