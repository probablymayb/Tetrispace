using UnityEngine;
using System.Collections.Generic;
using System.Collections;

/// <summary>
/// ��Ʈ���̳� �Ŵ��� ���� ���� (UselessBlock �׸��� ���, ���� Ŭ��� ����)
/// 
/// �ֿ� ���:
/// 1. UselessBlock�� �׸��忡 ���� ���
/// 2. ���� Ŭ���� üũ������ UselessBlock ����
/// 3. �÷��̾� �����ӿ� �ڵ����� ����ٴ�
/// 4. 10�� �� �ڵ� ����
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

    // �ı� ���� ��ϵ��� ���� (�ߺ� �ı� ���� �ذ�)
    private HashSet<GameObject> pendingDestroy = new HashSet<GameObject>();
    // UselessBlock ��ġ ���� �迭 �߰�
    private bool[,] uselessBlockPositions = new bool[4, 5];

    // ���� ���ӿ��� ���� ����
    private bool _globalGameOver = false;
    public bool IsGameOver
    {
        get { return _globalGameOver; }
        private set
        {
            if (_globalGameOver != value)
            {
                _globalGameOver = value;
                Debug.Log($"���� ���ӿ��� ���� ����: {value}");

                if (value)
                {
                    OnGameOver();
                }
            }
        }
    }

    [Header("=== UselessBlock �ý��� ===")]
    [SerializeField] private GameObject uselessBlockPrefab; // I���� UselessBlock ������
    [SerializeField] private float uselessBlockLifetime = 10f;
    [SerializeField] private bool showUselessBlockWarning = true;

    // UselessBlock ���� (�����ϰ�)
    private List<UselessBlock> activeUselessBlocks = new List<UselessBlock>();

    protected override void Awake()
    {
        base.Awake();
        gridArray = new GameObject[width, height];
        gridOrigin = gridOriginTransform.position;
        EventManager.Instance.onPlayerMove += OnPlayerMove;
    }

    /// <summary>
    /// ����� �׸��忡 ��� (UselessBlock�� ����)
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

            Debug.Log($"Registering block at {gridPos}: {block.name} (�±�: {block.tag})");
            gridArray[gridPos.x, gridPos.y] = block;

            // ��� ���� ���� (UselessBlock�̵� �Ϲ� ����̵� �����ϰ�)
            if (block.CompareTag("UselessBlock"))
            {
                // UselessBlock�� �±� ����
                Debug.Log($"UselessBlock �׸��� ���: {block.name}");
            }
            else
            {
                block.tag = "LockedBlock";
            }

            // �ݶ��̴� Ȱ��ȭ
            Collider2D collider = block.GetComponent<Collider2D>();
            if (collider != null)
            {
                collider.enabled = true;
            }

            // �� ��ĭ�� ����� ��ϵǸ� ���ӿ��� ���� üũ
            if (gridPos.y == height - 1)
            {
                CheckAndUpdateGameOverState();
            }
        }
    }

    /// <summary>
    /// ���ӿ��� ���� üũ �� ������Ʈ
    /// </summary>
    private void CheckAndUpdateGameOverState()
    {
        bool hasTopRowBlocks = false;

        // �� ��ĭ�� ����� �ִ��� Ȯ�� (UselessBlock�� ����)
        for (int x = 0; x < width; x++)
        {
            if (IsLocked(x, height - 1))
            {
                hasTopRowBlocks = true;
                Debug.Log($"�� ��ĭ Grid({x}, {height - 1})�� ��� �߰�");
                break;
            }
        }

        // ���ӿ��� ���� ������Ʈ
        IsGameOver = hasTopRowBlocks;

        if (hasTopRowBlocks)
        {
            Debug.Log("�� ��ĭ�� ����� ���� - ���ӿ��� ���� Ȱ��ȭ");
        }
        else
        {
            Debug.Log("�� ��ĭ ������� - ���ӿ��� ���� ����");
        }
    }

    /// <summary>
    /// ���ӿ��� �߻� �� ó��
    /// </summary>
    private void OnGameOver()
    {
        Debug.Log("=== ���ӿ��� �߻� ===");
        // �߰� ���ӿ��� ó�� ���� ���⿡ ����
    }

    /// <summary>
    /// �ش� ��ġ�� ����ִ��� Ȯ�� (UselessBlock�� ��� ������ �ν�)
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
    /// ���� Ŭ����� ��� üũ (UselessBlock ����)
    /// </summary>
    private bool IsLockedForLineClear(int x, int y)
    {
        if (x < 0 || x > width - 1 || y < 0 || y > height - 1)
            return true;

        // �ش� ��ġ�� UselessBlock�� ������ ���� Ŭ���� ����
        if (uselessBlockPositions[x, y])
        {
            return false;
        }

        GameObject block = gridArray[x, y];
        return block != null && !pendingDestroy.Contains(block);
    }

    /// <summary>
    /// ���� üũ �� ���� (UselessBlock ����)
    /// </summary>
    public void CheckAndClearLines()
    {
        // �׸��� �̵� �߿��� ���� üũ���� ����
        if (IsGridMoving) return;

        List<int> linesToClear = new List<int>();

        // ��� ������ ���ε��� ���� ã�� (UselessBlock ����)
        for (int y = 0; y < height; y++)
        {
            bool isLineFull = true;
            for (int x = 0; x < width; x++)
            {
                if (!IsLockedForLineClear(x, y))
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
            Debug.Log($"���� Ŭ���� �߻�: {linesToClear.Count}�� (UselessBlock ����)");
            StartCoroutine(ClearLinesCoroutine(linesToClear));
        }
    }

    public void ClearLine(int i)
    {
        // �׸��� �̵� �߿��� ���� üũ���� ����
        if (IsGridMoving) return;

        List<int> linesToClear = new List<int>();
    
        linesToClear.Add(i);
        StartCoroutine(ClearLinesCoroutine(linesToClear));
        
    }

    /// <summary>
    /// ���� ���Ÿ� �ڷ�ƾ���� ó�� (UselessBlock ����)
    /// </summary>
    private IEnumerator ClearLinesCoroutine(List<int> linesToClear)
    {
        Debug.Log("=== ���� Ŭ���� ���� ===");

        // ������ ��ϵ��� ���� ��Ȱ��ȭ (UselessBlock ����)
        foreach (int y in linesToClear)
        {
            for (int x = 0; x < width; x++)
            {
                GameObject block = gridArray[x, y];
                if (block != null && !block.CompareTag("UselessBlock"))
                {
                    pendingDestroy.Add(block);
                    block.SetActive(false);
                    gridArray[x, y] = null;
                    Debug.Log($"���� Ŭ����: Grid({x}, {y}) ��� ����");
                }
                else if (block != null && block.CompareTag("UselessBlock"))
                {
                    Debug.Log($"UselessBlock ����: Grid({x}, {y}) - {block.name}");
                }
            }
        }

        // �� ������ ���
        yield return null;

        // �ı� ���� ��ϵ� ����
        foreach (GameObject block in pendingDestroy)
        {
            if (block != null)
            {
                Destroy(block);
            }
        }
        pendingDestroy.Clear();

        // ���� ���ε� ��� (UselessBlock�� �Բ�)
        foreach (int clearedY in linesToClear)
        {
            DropLinesAbove(clearedY);
        }

        Debug.Log("=== ���� Ŭ���� �Ϸ� (UselessBlock�� ���� ����) ===");

        // ���� Ŭ���� �� ���ӿ��� ���� ��üũ
        CheckAndUpdateGameOverState();
    }

    /// <summary>
    /// ���� ���ε��� �Ʒ��� ����߸��� (UselessBlock�� �Բ�)
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

                    if (block.CompareTag("UselessBlock"))
                    {
                        Debug.Log($"UselessBlock ���: {block.name} to Grid({x}, {y - 1})");
                    }
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
            // UselessBlock�� �ƴ� ��츸 �±� ����
            if (!block.CompareTag("UselessBlock"))
            {
                block.tag = "LockedBlock";
            }

            // �ݶ��̴� ��Ȱ��ȭ
            Collider2D collider = block.GetComponent<Collider2D>();
            if (collider != null)
            {
                collider.enabled = true;
            }
        }
    }

    /// <summary>
    /// ��ü �׸��� �̵� (UselessBlock�� �ڵ����� �Բ� �̵�)
    /// </summary>
    public void MoveEntireGrid(Vector3 moveDelta)
    {
        IsGridMoving = true;

        // �̹� �̵��� ������Ʈ ����
        HashSet<GameObject> movedObjects = new HashSet<GameObject>();

        // 1. �׸��� �迭�� ��ϵ� �̵�
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                GameObject block = gridArray[x, y];
                if (block != null && !pendingDestroy.Contains(block) && !movedObjects.Contains(block))
                {
                    block.transform.position += moveDelta;
                    movedObjects.Add(block);
                }
            }
        }
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

    /// <summary>
    /// UselessBlock ��ȯ (I����, �� �Ʒ���)
    /// </summary>
    public void SpawnUselessBlock()
    {
        if (uselessBlockPrefab == null)
        {
            Debug.LogError("uselessBlockPrefab�� �������� �ʾҽ��ϴ�!");
            return;
        }
        if (IsGridMoving)
        {
            Debug.LogWarning("�׸��� �̵� �߿��� UselessBlock�� ������ �� �����ϴ�.");
            return;
        }

        // �� �Ʒ����� ����ִ��� Ȯ��
        bool canSpawn = true;
        for (int x = 0; x < width; x++)
        {
            if (IsLocked(x, 0))
            {
                canSpawn = false;
                break;
            }
        }

        if (!canSpawn)
        {
            Debug.Log("�� �Ʒ����� ���� ���� ���ε��� ���� �ø��ϴ�!");
            StartCoroutine(PushUpAndSpawnUselessBlockCoroutine());
        }
        else
        {
            Debug.Log("=== UselessBlock ��ȯ ���� ===");
            StartCoroutine(SpawnUselessBlockCoroutine());
        }
    }

    /// <summary>
    /// ���� ���ε��� ���� �ø��� UselessBlock ��ȯ
    /// </summary>
    private IEnumerator PushUpAndSpawnUselessBlockCoroutine()
    {
        Debug.Log("=== ���� ���� �ø��� ���� ===");

        // �� ���� üũ (���ӿ��� ����)
        bool topRowHasBlocks = false;
        for (int x = 0; x < width; x++)
        {
            if (IsLocked(x, height - 1))
            {
                topRowHasBlocks = true;
                break;
            }
        }

        if (topRowHasBlocks)
        {
            Debug.LogWarning("�� ���ٿ� ����� �־ �� �̻� �ø� �� �����ϴ�! ���ӿ���!");
            // ���ӿ��� ó��
            CheckAndUpdateGameOverState();
            yield break;
        }

        // ���������� �Ʒ��� ��� ������ �� ĭ�� ���� �̵�
        for (int y = height - 1; y >= 1; y--) // �� ��(height-1)���� Y=1����
        {
            for (int x = 0; x < width; x++)
            {
                GameObject blockBelow = gridArray[x, y - 1]; // �Ʒ��� ���

                if (blockBelow != null && !pendingDestroy.Contains(blockBelow))
                {
                    // �׸��� �迭 ������Ʈ
                    gridArray[x, y] = blockBelow;
                    gridArray[x, y - 1] = null;

                    // ���� ��ġ �̵�
                    blockBelow.transform.position += Vector3.up * cellSize;

                    // UselessBlock ��ġ ��ŷ�� �Բ� �̵�
                    if (uselessBlockPositions[x, y - 1])
                    {
                        uselessBlockPositions[x, y] = true;
                        uselessBlockPositions[x, y - 1] = false;
                    }

                    Debug.Log($"��� ���� �̵�: {blockBelow.name} from Grid({x}, {y - 1}) to Grid({x}, {y})");
                }
                else
                {
                    // �� ������ �״�� �� ��������
                    gridArray[x, y] = null;
                    uselessBlockPositions[x, y] = false;
                }
            }
        }

        // �� �Ʒ���(Y=0) ������ ����
        for (int x = 0; x < width; x++)
        {
            gridArray[x, 0] = null;
            uselessBlockPositions[x, 0] = false;
        }

        Debug.Log("=== ���� ���� �ø��� �Ϸ� ===");

        // �� ������ ���
        yield return null;

        // ���� �� �Ʒ����� ������Ƿ� UselessBlock ��ȯ
        Debug.Log("=== UselessBlock ��ȯ ���� (���� �ø��� ��) ===");
        yield return StartCoroutine(SpawnUselessBlockCoroutine());
    }

    /// <summary>
    /// UselessBlock ��ȯ �ڷ�ƾ (������ ����)
    /// </summary>
    private IEnumerator SpawnUselessBlockCoroutine()
    {
        if (showUselessBlockWarning)
        {
            Debug.Log("UselessBlock�� ��ȯ�˴ϴ�!");
            yield return new WaitForSeconds(0.5f);
        }

        gridOrigin = gridOriginTransform.position;

        // I���� ��� ��Ȯ�� �߾� ��ġ
        Vector3 spawnPosition = gridOrigin + new Vector3(2f * cellSize, 0.5f * cellSize, 0f);
        Debug.Log("UselessBlock ������ " + spawnPosition);

        // UselessBlock ����
        GameObject uselessBlockObj = Instantiate(uselessBlockPrefab, spawnPosition, Quaternion.identity);
        uselessBlockObj.name = $"UselessBlock_I_{Time.time:F1}";

        // UselessBlock ������Ʈ ��������
        UselessBlock uselessBlockComponent = uselessBlockObj.GetComponent<UselessBlock>();
        if (uselessBlockComponent == null)
        {
            uselessBlockComponent = uselessBlockObj.AddComponent<UselessBlock>();
        }

        // ��� �׸��� ��� �Ϸ�
        for (int x = 0; x < width; x++)
        {
            Vector2Int gridPos = new Vector2Int(x, 0);
            RegisterBlock(gridPos, uselessBlockObj);
            uselessBlockPositions[x, 0] = true;
        }

        // �׸��� ��� �Ϸ� �� ���� ��Ͽ� �߰�
        activeUselessBlocks.Add(uselessBlockComponent);

        // �� ������ ���� Ȯ���� ��� ����
        yield return null;

        Debug.Log($"UselessBlock ���� �� �׸��� ��� �Ϸ�: {uselessBlockObj.name}");
        Debug.Log("=== UselessBlock ��ȯ �Ϸ� ===");

        // ���� �ø��� �� ���ӿ��� ���� üũ
        CheckAndUpdateGameOverState();
    }

    /// <summary>
    /// UselessBlock ��ȯ �ڷ�ƾ
    /// </summary>
    /// <summary>
    /// UselessBlock ��ȯ �ڷ�ƾ (��� �Ϸ� ����)
    /// </summary>

    /// <summary>
    /// UselessBlock�� ������ �� �׸��忡�� ����
    /// </summary>
    public void RemoveUselessBlockFromGrid(UselessBlock uselessBlock)
    {
        if (uselessBlock == null) return;

        GameObject uselessBlockObj = uselessBlock.gameObject;

        // UselessBlock ��ġ ����
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (gridArray[x, y] == uselessBlockObj)
                {
                    gridArray[x, y] = null;
                    uselessBlockPositions[x, y] = false; // ��ġ ��ŷ ����
                }
            }
        }

        // ���� ��Ͽ��� ����
        activeUselessBlocks.Remove(uselessBlock);

        Debug.Log($"UselessBlock ���� ����: {uselessBlock.name}");

        // ���� Ŭ���� üũ (�� ������ ������ �� ����)
        CheckAndClearLines();

        // ���ӿ��� ���� ��üũ
        CheckAndUpdateGameOverState();
    }

    /// <summary>
    /// ��� UselessBlock ��� ����
    /// </summary>
    public void RemoveAllUselessBlocks()
    {
        Debug.Log("=== ��� UselessBlock ��� ���� ===");

        List<UselessBlock> blocksToRemove = new List<UselessBlock>(activeUselessBlocks);
        foreach (UselessBlock uselessBlock in blocksToRemove)
        {
            if (uselessBlock != null)
            {
                uselessBlock.ForceDestroy();
            }
        }

        activeUselessBlocks.Clear();
    }

    /// <summary>
    /// Ȱ�� UselessBlock ���� ��ȯ
    /// </summary>
    public int GetUselessBlockCount()
    {
        return activeUselessBlocks.Count;
    }

    /// <summary>
    /// ��� ��� ����
    /// </summary>
    public void ClearAll()
    {
        // UselessBlock ���� ����
        RemoveAllUselessBlocks();

        // ��� �׸��� ��� ����
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

        // ���ӿ��� ���� ����
        IsGameOver = false;
        Debug.Log("��� ��� ���� - ���� ���� ���� ����");
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

    /// <summary>
    /// ����� �׽�Ʈ �޼����
    /// </summary>
    private void Test()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            int totalCount = 0;
            int uselessCount = 0;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (IsLocked(x, y))
                    {
                        totalCount++;
                        GameObject block = gridArray[x, y];
                        if (block != null && block.CompareTag("UselessBlock"))
                        {
                            uselessCount++;
                        }
                    }
                }
            }
            Debug.Log($"��ü Locked ���: {totalCount}�� (UselessBlock: {uselessCount}��)");
        }

        if (Input.GetKeyDown(KeyCode.G))
        {
            Debug.Log("=== ���� ���ӿ��� ���� üũ ===");
            CheckAndUpdateGameOverState();
        }

        if (Input.GetKeyDown(KeyCode.U))
        {
            Debug.Log("=== ���� UselessBlock ��ȯ ===");
            SpawnUselessBlock();
        }

        if (Input.GetKeyDown(KeyCode.I))
        {
            Debug.Log("=== ���� UselessBlock ���� ===");
            RemoveAllUselessBlocks();
        }

        if (Input.GetKeyDown(KeyCode.O))
        {
            Debug.Log($"=== UselessBlock ���� ===");
            Debug.Log($"Ȱ�� UselessBlock: {GetUselessBlockCount()}��");

            foreach (UselessBlock uselessBlock in activeUselessBlocks)
            {
                if (uselessBlock != null)
                {
                    Vector2Int pos = WorldToGrid(uselessBlock.transform.position);
                    Debug.Log($"  - {uselessBlock.name} at Grid({pos.x}, {pos.y})");
                }
            }
        }
    }

    [ContextMenu("Log Top Row Status")]
    public void LogTopRowStatus()
    {
        Debug.Log("=== �� ��ĭ ���� ===");
        for (int x = 0; x < width; x++)
        {
            bool isLocked = IsLocked(x, height - 1);
            GameObject block = gridArray[x, height - 1];
            string blockType = "";

            if (block != null && block.CompareTag("UselessBlock"))
            {
                blockType = " (UselessBlock)";
            }

            Debug.Log($"Grid({x}, {height - 1}): {(isLocked ? "��" : "��")}{blockType}");
        }
        Debug.Log($"���� ���� ���ӿ��� ����: {IsGameOver}");
    }

    [ContextMenu("Force Check Game Over")]
    public void ForceCheckGameOver()
    {
        CheckAndUpdateGameOverState();
    }

    [ContextMenu("Test Spawn UselessBlock")]
    public void TestSpawnUselessBlock()
    {
        SpawnUselessBlock();
    }

    [ContextMenu("Remove All UselessBlocks")]
    public void TestRemoveAllUselessBlocks()
    {
        RemoveAllUselessBlocks();
    }

    [ContextMenu("Show Grid State")]
    public void ShowGridState()
    {
        Debug.Log("=== ��ü �׸��� ���� ===");
        for (int y = height - 1; y >= 0; y--)
        {
            string row = $"Y={y}: ";
            for (int x = 0; x < width; x++)
            {
                GameObject block = gridArray[x, y];
                if (block == null)
                {
                    row += "�� ";
                }
                else if (block.CompareTag("UselessBlock"))
                {
                    row += "U "; // UselessBlock
                }
                else
                {
                    row += "�� "; // �Ϲ� ���
                }
            }
            Debug.Log(row);
        }

        Debug.Log($"Ȱ�� UselessBlock: {GetUselessBlockCount()}��");
    }

    public void Update()
    {
        Test();
    }

    private void OnDestroy()
    {
        // ��� UselessBlock ����
        RemoveAllUselessBlocks();
    }
}