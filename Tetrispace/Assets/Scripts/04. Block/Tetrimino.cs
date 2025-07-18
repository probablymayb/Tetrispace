using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// ��Ʈ���̳� Ŭ���� (�������)
/// 
/// ���:
/// - 6���� ��� ��� (O, I, T, L, J, S)
/// - ȸ�� �� �̵�
/// - �׸��� ��ȿ�� �˻�
/// - �ڵ� �ϰ�
/// </summary>
public class Tetrimino : MonoBehaviour
{
    [Header("=== ��Ʈ���̳� ���� ===")]
    [SerializeField] private TetrominoType blockType;
    [SerializeField] private float fallSpeed = 1f;           // �ϰ� �ӵ� (��/ĭ)
    [SerializeField] private float moveSpeed = 0.15f;        // �¿� �̵� �ӵ�
    [SerializeField] private float lockDelay = 0.5f;         // ���� �� ���� �����ð�

    [Header("=== ���� ===")]
    [SerializeField] private GameObject blockPrefab;         // ��� ������
    [SerializeField] private PlayerGrid playerGrid;         // �÷��̾� �׸��� ����

    // === ��Ʈ���̳� Ÿ�� ���� ===
    public enum TetrominoType
    {
        O,  // ���簢��
        I,  // ������
        T,  // T����
        L,  // L����
        J,  // J���� (��L)
        S   // S����
    }

    // === ���� ���� ===
    private List<Transform> blocks = new List<Transform>();  // ���� ��ϵ�
    private Vector2Int gridPosition = new Vector2Int(1, 4);  // �׸���� ��ġ (�߾� ���)
    private int currentRotation = 0;                         // ���� ȸ�� ���� (0~3)
    private float fallTimer = 0f;                           // �ϰ� Ÿ�̸�
    private float lockTimer = 0f;                           // ���� Ÿ�̸�
    private bool isLocked = false;                          // ���� ����
    private bool canMove = true;                            // �̵� ���� ����

    // === �� ��Ʈ���̳뺰 ��� ��ġ ���� ===
    private Dictionary<TetrominoType, Vector2Int[,]> blockShapes = new Dictionary<TetrominoType, Vector2Int[,]>
    {
        // O ��� - 2x2 ���簢�� (ȸ�� �Ұ�)
        [TetrominoType.O] = new Vector2Int[,] {
            { new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(0, 1), new Vector2Int(1, 1) },
            { new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(0, 1), new Vector2Int(1, 1) },
            { new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(0, 1), new Vector2Int(1, 1) },
            { new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(0, 1), new Vector2Int(1, 1) }
        },

        // I ��� - ������
        [TetrominoType.I] = new Vector2Int[,] {
            { new Vector2Int(0, 1), new Vector2Int(1, 1), new Vector2Int(2, 1), new Vector2Int(3, 1) },
            { new Vector2Int(1, 0), new Vector2Int(1, 1), new Vector2Int(1, 2), new Vector2Int(1, 3) },
            { new Vector2Int(0, 1), new Vector2Int(1, 1), new Vector2Int(2, 1), new Vector2Int(3, 1) },
            { new Vector2Int(1, 0), new Vector2Int(1, 1), new Vector2Int(1, 2), new Vector2Int(1, 3) }
        },

        // T ��� - T����
        [TetrominoType.T] = new Vector2Int[,] {
            { new Vector2Int(1, 0), new Vector2Int(0, 1), new Vector2Int(1, 1), new Vector2Int(2, 1) },
            { new Vector2Int(1, 0), new Vector2Int(1, 1), new Vector2Int(2, 1), new Vector2Int(1, 2) },
            { new Vector2Int(0, 1), new Vector2Int(1, 1), new Vector2Int(2, 1), new Vector2Int(1, 2) },
            { new Vector2Int(1, 0), new Vector2Int(0, 1), new Vector2Int(1, 1), new Vector2Int(1, 2) }
        },

        // L ��� - L����
        [TetrominoType.L] = new Vector2Int[,] {
            { new Vector2Int(2, 0), new Vector2Int(0, 1), new Vector2Int(1, 1), new Vector2Int(2, 1) },
            { new Vector2Int(1, 0), new Vector2Int(1, 1), new Vector2Int(1, 2), new Vector2Int(2, 2) },
            { new Vector2Int(0, 1), new Vector2Int(1, 1), new Vector2Int(2, 1), new Vector2Int(0, 2) },
            { new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(1, 1), new Vector2Int(1, 2) }
        },

        // J ��� - J���� (��L)
        [TetrominoType.J] = new Vector2Int[,] {
            { new Vector2Int(0, 0), new Vector2Int(0, 1), new Vector2Int(1, 1), new Vector2Int(2, 1) },
            { new Vector2Int(1, 0), new Vector2Int(2, 0), new Vector2Int(1, 1), new Vector2Int(1, 2) },
            { new Vector2Int(0, 1), new Vector2Int(1, 1), new Vector2Int(2, 1), new Vector2Int(2, 2) },
            { new Vector2Int(1, 0), new Vector2Int(1, 1), new Vector2Int(0, 2), new Vector2Int(1, 2) }
        },

        // S ��� - S����
        [TetrominoType.S] = new Vector2Int[,] {
            { new Vector2Int(1, 0), new Vector2Int(2, 0), new Vector2Int(0, 1), new Vector2Int(1, 1) },
            { new Vector2Int(1, 0), new Vector2Int(1, 1), new Vector2Int(2, 1), new Vector2Int(2, 2) },
            { new Vector2Int(1, 0), new Vector2Int(2, 0), new Vector2Int(0, 1), new Vector2Int(1, 1) },
            { new Vector2Int(1, 0), new Vector2Int(1, 1), new Vector2Int(2, 1), new Vector2Int(2, 2) }
        }
    };

    // === �ʱ�ȭ ===
    void Start()
    {
        InitializeTetrimino();
    }

    void Update()
    {
        if (isLocked) return;

        HandleInput();      // �Է� ó��
        HandleFalling();    // �ڵ� �ϰ�
        HandleLocking();    // ���� ó��
    }

    #region === �ʱ�ȭ ===

    /// <summary>
    /// ��Ʈ���̳� �ʱ�ȭ
    /// </summary>
    void InitializeTetrimino()
    {
        // PlayerGrid �ڵ� ã��
        if (playerGrid == null)
            playerGrid = FindFirstObjectByType<PlayerGrid>();

        if (playerGrid == null)
        {
            Debug.LogError("PlayerGrid�� ã�� �� �����ϴ�!");
            return;
        }

        // ��� ����
        CreateBlocks();

        // �ʱ� ��ġ ����
        UpdateBlockPositions();

        Debug.Log($"��Ʈ���̳� ����: {blockType}");
    }

    /// <summary>
    /// ��ϵ� ����
    /// </summary>
    void CreateBlocks()
    {
        // ���� ��� ����
        foreach (Transform block in blocks)
        {
            if (block != null)
                Destroy(block.gameObject);
        }
        blocks.Clear();

        // �� ��� ����
        for (int i = 0; i < 4; i++)
        {
            GameObject block = Instantiate(blockPrefab, transform);
            blocks.Add(block.transform);
        }
    }

    #endregion

    #region === �Է� ó�� ===

    /// <summary>
    /// �÷��̾� �Է� ó��
    /// </summary>
    void HandleInput()
    {
        if (!canMove) return;

        // �¿� �̵�
        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
        {
            TryMove(Vector2Int.left);
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
        {
            TryMove(Vector2Int.right);
        }

        // ȸ��
        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
        {
            TryRotate();
        }

        // ���� �ϰ�
        if (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S))
        {
            fallTimer += Time.deltaTime * 5f; // 5��� �ϰ�
        }

        // ��� �ϰ� (�ϵ� ���)
        if (Input.GetKeyDown(KeyCode.Space))
        {
            HardDrop();
        }
    }

    #endregion

    #region === �̵� �� ȸ�� ===

    /// <summary>
    /// �̵� �õ�
    /// </summary>
    /// <param name="direction">�̵� ����</param>
    /// <returns>�̵� ���� ����</returns>
    bool TryMove(Vector2Int direction)
    {
        Vector2Int newPosition = gridPosition + direction;

        if (IsValidPosition(newPosition, currentRotation))
        {
            gridPosition = newPosition;
            UpdateBlockPositions();

            // ���� Ÿ�̸� ����
            if (direction == Vector2Int.down)
            {
                lockTimer = 0f;
            }

            return true;
        }

        return false;
    }

    /// <summary>
    /// ȸ�� �õ�
    /// </summary>
    /// <returns>ȸ�� ���� ����</returns>
    bool TryRotate()
    {
        int newRotation = (currentRotation + 1) % 4;

        if (IsValidPosition(gridPosition, newRotation))
        {
            currentRotation = newRotation;
            UpdateBlockPositions();
            lockTimer = 0f; // ���� Ÿ�̸� ����
            return true;
        }

        return false;
    }

    /// <summary>
    /// ��� �ϰ� (�ϵ� ���)
    /// </summary>
    void HardDrop()
    {
        while (TryMove(Vector2Int.down))
        {
            // �� �̻� ������ �� ���� ������ ��� �̵�
        }

        // ��� ����
        lockTimer = lockDelay;
    }

    #endregion

    #region === ��ȿ�� �˻� ===

    /// <summary>
    /// Ư�� ��ġ�� ȸ������ ��ȿ���� �˻�
    /// </summary>
    /// <param name="position">�˻��� ��ġ</param>
    /// <param name="rotation">�˻��� ȸ��</param>
    /// <returns>��ȿ�� ��ġ���� ����</returns>
    bool IsValidPosition(Vector2Int position, int rotation)
    {
        Vector2Int[] shape = GetCurrentShape(rotation);

        foreach (Vector2Int blockOffset in shape)
        {
            Vector2Int blockPos = position + blockOffset;

            // �׸��� ��� üũ
            if (!playerGrid.IsValidGridPosition(blockPos.x, blockPos.y))
            {
                return false;
            }

            // �ٸ� ��ϰ� �浹 üũ
            if (playerGrid.IsGridOccupied(blockPos.x, blockPos.y))
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// ���� ȸ�� ������ ��� ��� ��������
    /// </summary>
    /// <param name="rotation">ȸ�� ����</param>
    /// <returns>��� ������ �迭</returns>
    Vector2Int[] GetCurrentShape(int rotation)
    {
        Vector2Int[] shape = new Vector2Int[4];

        for (int i = 0; i < 4; i++)
        {
            shape[i] = blockShapes[blockType][rotation, i];
        }

        return shape;
    }

    #endregion

    #region === ��ġ ������Ʈ ===

    /// <summary>
    /// ��ϵ��� ���� ��ġ ������Ʈ
    /// </summary>
    void UpdateBlockPositions()
    {
        Vector2Int[] shape = GetCurrentShape(currentRotation);

        for (int i = 0; i < blocks.Count && i < shape.Length; i++)
        {
            Vector2Int blockGridPos = gridPosition + shape[i];
            Vector3 worldPos = playerGrid.GridToWorldPosition(blockGridPos.x, blockGridPos.y);
            blocks[i].position = worldPos;
        }
    }

    #endregion

    #region === �ڵ� �ϰ� ===

    /// <summary>
    /// �ڵ� �ϰ� ó��
    /// </summary>
    void HandleFalling()
    {
        fallTimer += Time.deltaTime;

        if (fallTimer >= fallSpeed)
        {
            if (!TryMove(Vector2Int.down))
            {
                // �� �̻� ������ �� ���� - ���� ����
                StartLanding();
            }

            fallTimer = 0f;
        }
    }

    /// <summary>
    /// ���� ó�� ����
    /// </summary>
    void StartLanding()
    {
        lockTimer += Time.deltaTime;
    }

    /// <summary>
    /// ���� ó��
    /// </summary>
    void HandleLocking()
    {
        if (lockTimer > 0)
        {
            lockTimer += Time.deltaTime;

            if (lockTimer >= lockDelay)
            {
                LockTetrimino();
            }
        }
    }

    /// <summary>
    /// ��Ʈ���̳� ����
    /// </summary>
    void LockTetrimino()
    {
        isLocked = true;

        // PlayerGrid�� ��ϵ� ���
        Vector2Int[] shape = GetCurrentShape(currentRotation);

        for (int i = 0; i < blocks.Count && i < shape.Length; i++)
        {
            Vector2Int blockGridPos = gridPosition + shape[i];
            playerGrid.PlaceBlock(blockGridPos.x, blockGridPos.y, blocks[i]);
        }

        // �θ� ���� ����
        foreach (Transform block in blocks)
        {
            block.SetParent(playerGrid.transform);
        }

        Debug.Log($"��Ʈ���̳� ����: {blockType} at {gridPosition}");

        // ��Ʈ���̳� �Ŵ����� �Ϸ� �˸�
        //TetrominoManager.Instance?.OnTetrominoLocked(this);
        EventManager.Instance.TetrominoLocked(blockType, gridPosition);

        // �ڱ� �ڽ� �ı�
        Destroy(gameObject);
    }

    #endregion

    #region === ���� �޼��� ===

    /// <summary>
    /// ��Ʈ���̳� Ÿ�� ����
    /// </summary>
    /// <param name="type">��� Ÿ��</param>
    public void SetTetrominoType(TetrominoType type)
    {
        blockType = type;
        if (Application.isPlaying)
        {
            CreateBlocks();
            UpdateBlockPositions();
        }
    }

    /// <summary>
    /// �ϰ� �ӵ� ����
    /// </summary>
    /// <param name="speed">�ϰ� �ӵ� (��/ĭ)</param>
    public void SetFallSpeed(float speed)
    {
        fallSpeed = speed;
    }

    /// <summary>
    /// �̵� ���� ���� ����
    /// </summary>
    /// <param name="canMove">�̵� ���� ����</param>
    public void SetCanMove(bool canMove)
    {
        this.canMove = canMove;
    }

    /// <summary>
    /// ���� ��Ʈ���̳� Ÿ�� ��������
    /// </summary>
    /// <returns>���� ��� Ÿ��</returns>
    public TetrominoType GetTetrominoType()
    {
        return blockType;
    }

    /// <summary>
    /// ���� �׸��� ��ġ ��������
    /// </summary>
    /// <returns>�׸��� ��ġ</returns>
    public Vector2Int GetGridPosition()
    {
        return gridPosition;
    }

    #endregion

    #region === ����� ===

    /// <summary>
    /// ��Ʈ���̳� ���� ���
    /// </summary>
    [ContextMenu("��Ʈ���̳� ����")]
    void PrintTetrominoInfo()
    {
        Debug.Log($"=== ��Ʈ���̳� ���� ===");
        Debug.Log($"Ÿ��: {blockType}");
        Debug.Log($"��ġ: {gridPosition}");
        Debug.Log($"ȸ��: {currentRotation}");
        Debug.Log($"������: {isLocked}");
        Debug.Log($"��� ����: {blocks.Count}");
    }

    #endregion
}