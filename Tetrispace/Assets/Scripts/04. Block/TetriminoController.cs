using UnityEngine;

/// <summary>
/// �Ϻ� ���� ��Ʈ���� ���
/// 
/// PlayerController�� ��Ȯ�� ���� �׸��� �ý��� ���
/// - PlayerController: GetGridPos() ��� (�׸��� �𼭸�)
/// - TetrisBlock: GetGridMiddlePos() ��� (�׸��� �߽�)
/// </summary>
public class TetriminoController : MonoBehaviour
{
    [Header("=== ��� ���� ===")]
    [SerializeField] private float blockPixelSize = 28f;  // ��� ũ��

    [Header("=== �׸��� ��ġ ===")]
    [SerializeField] private bool useStaticPosition = true;  // ���� ��ġ ��� ����
    [SerializeField] private int customGridX = 0;           // Ŀ���� �׸��� X
    [SerializeField] private int customGridY = 0;           // Ŀ���� �׸��� Y

    void Start()
    {
        // ��� ũ�� ����
        //SetBlockSize();

        // �׸��� ���� ��� (�� ����)
        if (useStaticPosition)
        {
            //GridSystem.DebugGridInfo();
            //GridSystem.TestGridCalculation();
        }
    }

    void Update()
    {
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
        this.transform.Rotate(new Vector3(0f, 0f, 1f), -90);

    }

    public void TurnRight()
    {
        this.transform.Rotate(new Vector3(0f, 0f, 1f), 90);

    }

}