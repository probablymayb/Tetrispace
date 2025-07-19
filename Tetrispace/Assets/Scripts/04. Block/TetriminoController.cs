using UnityEngine;

/// <summary>
/// 완벽 정렬 테트리스 블록
/// 
/// PlayerController와 정확히 같은 그리드 시스템 사용
/// - PlayerController: GetGridPos() 사용 (그리드 모서리)
/// - TetrisBlock: GetGridMiddlePos() 사용 (그리드 중심)
/// </summary>
public class TetriminoController : MonoBehaviour
{
    [Header("=== 블록 설정 ===")]
    [SerializeField] private float blockPixelSize = 28f;  // 블록 크기

    [Header("=== 그리드 위치 ===")]
    [SerializeField] private bool useStaticPosition = true;  // 정적 위치 사용 여부
    [SerializeField] private int customGridX = 0;           // 커스텀 그리드 X
    [SerializeField] private int customGridY = 0;           // 커스텀 그리드 Y
    [SerializeField] private float fSpeed = 0.5f;           // 커스텀 그리드 Y

    void Start()
    {
        // 블록 크기 설정
        //SetBlockSize();

        // 그리드 정보 출력 (한 번만)
        if (useStaticPosition)
        {
            //GridSystem.DebugGridInfo();
            //GridSystem.TestGridCalculation();
        }
    }

    void Update()
    {

        transform.position += Vector3.down * fSpeed * Time.deltaTime;
        // 또는
        Vector3 pos = transform.position;
        pos.y -= fSpeed * Time.deltaTime;
        transform.position = pos;
        //if(Input.GetKeyDown(KeyCode.Q))
        //{
        //    TurnLeft();
        //}

        //if (Input.GetKeyDown(KeyCode.E))
        //{
        //    TurnRight();
        //}

    }

    public void TurnLeft()
    {
        this.transform.Rotate(new Vector3(0f, 0f, 1f), 90);
    }

    public void TurnRight()
    {
        this.transform.Rotate(new Vector3(0f, 0f, 1f), -90);


    }

}