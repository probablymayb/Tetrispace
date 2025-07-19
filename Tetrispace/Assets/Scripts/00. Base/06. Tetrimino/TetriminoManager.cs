using UnityEngine;
using System.Collections.Generic;
using System.Collections;

/// <summary>
/// ��Ʈ���� �Ŵ��� ���� ����
/// 
/// �ֿ� ��������:
/// 1. ��� ���� ����ȭ ��ȭ
/// 2. ���� �ı� ���� �ذ�
/// 3. �׸��� �̵� �� ������ ��ȭ
/// </summary>
public class TetriminoManager : Singleton<TetriminoManager>
{
    private int width = 4;
    private int height = 5;

    private GameObject[,] gridArray;
    Vector3 lastPlayerPosition = Vector3.zero;

    [SerializeField] private Transform gridOriginTransform;
    private Vector3 gridOrigin;
    [SerializeField] private float cellSize = 1f;

    // �׸��� �̵� �� Ʈ���� �̺�Ʈ ���ø� ���� �÷���
    public bool IsGridMoving { get; private set; } = false;

    // �ı� ���� ��ϵ��� ���� (���� �ı� ���� �ذ�)
    private HashSet<GameObject> pendingDestroy = new HashSet<GameObject>();

    protected override void Awake()
    {
        base.Awake();
        gridArray = new GameObject[width, height];
        gridOrigin = gridOriginTransform.position;
        EventManager.Instance.onPlayerMove += OnPlayerMove;
    }

    /// <summary>
    /// ����� �׸��忡 ��� (�ߺ� ��� ����)
    /// </summary>
    public void RegisterBlock(Vector2Int gridPos, GameObject block)
    {
        if (IsInsideGrid(gridPos) && !pendingDestroy.Contains(block))
        {
            // ���� ����� �ִٸ� ��� ���
            if (gridArray[gridPos.x, gridPos.y] != null)
            {
                Debug.LogWarning($"Grid position {gridPos} already occupied! Replacing...");
            }

            Debug.Log($"Registering block at {gridPos}: {block.name}");
            gridArray[gridPos.x, gridPos.y] = block;

            // ��� ���� Ȯ���� ����
            block.tag = "LockedBlock";
        }
    }

    /// <summary>
    /// �ش� ��ġ�� ����ִ��� Ȯ�� (�ı� ���� ��� ����)
    /// </summary>
    public bool IsLocked(int x, int y)
    {
        if (x < 0 || x > width - 1 || y < 0 || y > height - 1)
        {
            return true;
        }

        GameObject block = gridArray[x, y];
        return block != null && !pendingDestroy.Contains(block);
    }

    /// <summary>
    /// ���� üũ �� ���� (������ ����)
    /// </summary>
    public void CheckAndClearLines()
    {
        // �׸��� �̵� �߿��� ���� üũ���� ����
        if (IsGridMoving) return;

        List<int> linesToClear = new List<int>();

        // ���� ������ ���ε��� ��� ã��
        for (int y = 0; y < height; y++)
        {
            bool isLineFull = true;
            for (int x = 0; x < width; x++)
            {
                if (!IsLocked(x, y))
                {
                    isLineFull = false;
                    break;
                }
            }

            if (isLineFull)
            {
                linesToClear.Add(y);
            }
        }

        // ���� ���� ����
        if (linesToClear.Count > 0)
        {
            StartCoroutine(ClearLinesCoroutine(linesToClear));
        }
    }

    /// <summary>
    /// ���� ���Ÿ� �ڷ�ƾ���� ó�� (������ �ı�)
    /// </summary>
    private IEnumerator ClearLinesCoroutine(List<int> linesToClear)
    {
        // ������ ��ϵ��� ���� ��Ȱ��ȭ�ϰ� �ı� ���� ��Ͽ� �߰�
        foreach (int y in linesToClear)
        {
            for (int x = 0; x < width; x++)
            {
                GameObject block = gridArray[x, y];
                if (block != null)
                {
                    pendingDestroy.Add(block);
                    block.SetActive(false); // ��� ��Ȱ��ȭ�� �浹 ����
                    gridArray[x, y] = null;
                }
            }
        }

        // �� ������ ��� (�ٸ� �ý��۵��� ���� ��ȭ�� �ν��� �� �ֵ���)
        yield return null;

        // ���� �ı�
        foreach (int y in linesToClear)
        {
            for (int x = 0; x < width; x++)
            {
                // �̹� gridArray������ ���ŵǾ����Ƿ� pendingDestroy������ ã�Ƽ� �ı�
            }
        }

        // �ı� ���� ��ϵ� ����
        foreach (GameObject block in pendingDestroy)
        {
            if (block != null)
            {
                Destroy(block);
            }
        }
        pendingDestroy.Clear();

        // ���� ���ε� ���
        foreach (int clearedY in linesToClear)
        {
            DropLinesAbove(clearedY);
        }
    }

    /// <summary>
    /// ���� ���ε��� �Ʒ��� ����߸��� (������ ����)
    /// </summary>
    private void DropLinesAbove(int clearedY)
    {
        for (int y = clearedY + 1; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                GameObject block = gridArray[x, y];
                if (block != null && !pendingDestroy.Contains(block))
                {
                    // �׸��� �迭 ������Ʈ
                    gridArray[x, y - 1] = block;
                    gridArray[x, y] = null;

                    // ��ġ �̵�
                    block.transform.position += Vector3.down * cellSize;

                    // ��� ���� ��Ȯ�� �� ������Ʈ
                    UpdateBlockState(block);
                }
            }
        }
    }

    /// <summary>
    /// ��� ���� ������Ʈ (�±�, �ݶ��̴� ��)
    /// </summary>
    private void UpdateBlockState(GameObject block)
    {
        if (block != null)
        {
            // ���� ������� �±� ����
            block.tag = "LockedBlock";

            // �ʿ�� �ݶ��̴� ��Ȱ��ȭ
            Collider2D collider = block.GetComponent<Collider2D>();
            if (collider != null)
            {
                collider.enabled = true;
            }
        }
    }

    /// <summary>
    /// ��ü �׸��� �̵� (������ ��ȭ)
    /// </summary>
    public void MoveEntireGrid(Vector3 moveDelta)
    {
        // �׸��� �̵� �÷��� ����
        IsGridMoving = true;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                GameObject block = gridArray[x, y];
                if (block != null && !pendingDestroy.Contains(block))
                {
                    block.transform.position += moveDelta;
                }
            }
        }

        // �� ������ �� �÷��� ����
        StartCoroutine(ResetGridMovingFlag());
    }

    /// <summary>
    /// �׸��� �̵� �÷��� ����
    /// </summary>
    private IEnumerator ResetGridMovingFlag()
    {
        yield return null;
        IsGridMoving = false;
    }

    public void ClearAll()
    {
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (gridArray[x, y] != null)
                {
                    pendingDestroy.Add(gridArray[x, y]);
                    Destroy(gridArray[x, y]);
                    gridArray[x, y] = null;
                }
            }
        }
        pendingDestroy.Clear();
    }

    private bool IsInsideGrid(Vector2Int pos)
    {
        return pos.x >= 0 && pos.x < width && pos.y >= 0 && pos.y < height;
    }

    public Vector2Int WorldToGrid(Vector3 worldPos)
    {
        Vector3 origin = gridOriginTransform.position;
        Vector3 local = worldPos - origin;

        int x = Mathf.FloorToInt(local.x / cellSize);
        int y = Mathf.FloorToInt(local.y / cellSize);

        // Ŭ���� ��� ��ȿ ���� üũ �� ���
        if (x < 0 || x >= width || y < 0 || y >= height)
        {
            Debug.LogWarning($"WorldToGrid: Position {worldPos} is outside grid bounds");
        }

        x = Mathf.Clamp(x, 0, width - 1);
        y = Mathf.Clamp(y, 0, height - 1);

        return new Vector2Int(x, y);
    }

    private void OnPlayerMove(Transform playerTransform)
    {
        Debug.Log("OnPlayerMove on TetriminoManager");
        if (lastPlayerPosition == Vector3.zero)
        {
            lastPlayerPosition = playerTransform.position;
            return;
        }

        Vector3 delta = playerTransform.position - lastPlayerPosition;
        lastPlayerPosition = playerTransform.position;
        Debug.Log($"�÷��̾� ��ġ ��ȭ: {delta}");
        MoveEntireGrid(delta);
    }

    private void Test()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            int count = 0;
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (IsLocked(x, y))
                    {
                        count++;
                    }
                }
            }
            Debug.Log($"Locked �׸��� ����: {count}");
        }
    }

    public void Update()
    {
        Test();
    }
}