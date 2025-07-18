using UnityEngine;
using System.Collections.Generic;
using System.Collections;

/// <summary>
/// ?? ��Ʈ���̳� �Ŵ��� (�������)
/// 
/// ���:
/// - ��Ʈ���̳� ���� �� ����
/// - ���� ��� �ý���
/// - ���� ���� üũ
/// - ���� �� �ӵ� ����
/// </summary>
/// 
public class TetriminoManager : Singleton<TetriminoManager>
{
    [Header("=== ��Ʈ���̳� ���� ===")]
    [SerializeField] private GameObject tetrominoPrefab;     // ��Ʈ���̳� ������
    [SerializeField] private Transform spawnPoint;          // ���� ��ġ
    [SerializeField] private float initialFallSpeed = 1f;   // �ʱ� �ϰ� �ӵ�
    [SerializeField] private float speedIncreaseRate = 0.1f; // �ӵ� ������

    [Header("=== ���� ===")]
    [SerializeField] private PlayerGrid playerGrid;        // �÷��̾� �׸���
    [SerializeField] private Transform nextBlockPreview;   // ���� ��� �̸����� ��ġ

    [Header("=== ���� ���� ===")]
    [SerializeField] private bool autoSpawn = false;        // �ڵ� ���� ����
    [SerializeField] private float spawnDelay = 0.5f;      // ���� ���� �ð�
    [SerializeField] private int maxLevel = 20;            // �ִ� ����

    // === ���� ���� ===
    private Tetrimino currentTetrimino;                    // ���� ��Ʈ���̳�
    private Tetrimino.TetrominoType nextTetrominoType;     // ���� ��� Ÿ��
    private Queue<Tetrimino.TetrominoType> tetrominoQueue; // ��� ��⿭
    private float currentFallSpeed;                        // ���� �ϰ� �ӵ�
    private int currentLevel = 1;                          // ���� ����
    private int linesCleared = 0;                          // Ŭ����� ���� ��
    private bool isGameOver = false;                       // ���� ���� ����

    // === ��Ʈ���̳� Ÿ�� �迭 (6����) ===
    private readonly Tetrimino.TetrominoType[] allTetrominoTypes = {
        Tetrimino.TetrominoType.O,
        Tetrimino.TetrominoType.I,
        Tetrimino.TetrominoType.T,
        Tetrimino.TetrominoType.L,
        Tetrimino.TetrominoType.J,
        Tetrimino.TetrominoType.S
    };

    // === �̺�Ʈ �ý��� ===
    public static System.Action<int> OnLevelUp;
    public static System.Action OnGameOver;

    #region === �ʱ�ȭ ===

    protected override void Awake()
    {
        base.Awake();

        // �ʱ�ȭ
        tetrominoQueue = new Queue<Tetrimino.TetrominoType>();
        currentFallSpeed = initialFallSpeed;

        // ť �ʱ�ȭ
        FillTetrominoQueue();
    }

    void Start()
    {
        InitializeManager();
    }

    /// <summary>
    /// �Ŵ��� �ʱ�ȭ
    /// </summary>
    void InitializeManager()
    {
        // PlayerGrid �ڵ� ã��
        if (playerGrid == null)
            playerGrid = FindFirstObjectByType<PlayerGrid>();
        //
        if (playerGrid == null)
        {
            Debug.LogError("PlayerGrid�� ã�� �� �����ϴ�!");
            return;
        }

        // ���� ��ġ ����
        if (spawnPoint == null)
        {
            GameObject spawnPointObj = new GameObject("TetrominoSpawnPoint");
            spawnPoint = spawnPointObj.transform;
            spawnPoint.SetParent(transform);
        }

        // ���� ����
        if (autoSpawn)
        {
            StartCoroutine(StartGameSequence());
        }

        Debug.Log("TetrominoManager �ʱ�ȭ �Ϸ�");
    }

    /// <summary>
    /// ���� ���� ������
    /// </summary>
    IEnumerator StartGameSequence()
    {
        yield return new WaitForSeconds(0.1f);

        // ù ��° ��Ʈ���̳� ����
        SpawnNextTetrimino();

        // ���� ��� �̸����� ������Ʈ
        UpdateNextBlockPreview();
    }

    #endregion

    #region === ��Ʈ���̳� ���� ===

    /// <summary>
    /// ���� ��Ʈ���̳� ����
    /// </summary>
    public void SpawnNextTetrimino()
    {
        if (isGameOver) return;

        // ���� ���� üũ
        if (CheckGameOver())
        {
            TriggerGameOver();
            return;
        }

        // ���� ��� Ÿ�� ��������
        Tetrimino.TetrominoType typeToSpawn = GetNextTetrominoType();

        // ��Ʈ���̳� ����
        GameObject tetrominoObj = Instantiate(tetrominoPrefab, spawnPoint.position, Quaternion.identity);
        currentTetrimino = tetrominoObj.GetComponent<Tetrimino>();

        if (currentTetrimino == null)
        {
            Debug.LogError("������ ������Ʈ�� Tetrimino ������Ʈ�� �����ϴ�!");
            return;
        }

        // ��Ʈ���̳� ����
        currentTetrimino.SetTetrominoType(typeToSpawn);
        currentTetrimino.SetFallSpeed(currentFallSpeed);

        // �̺�Ʈ �߻�
        EventManager.Instance.TetrominoSpawned(currentTetrimino.GetTetrominoType());

        // ���� ��� �̸����� ������Ʈ
        UpdateNextBlockPreview();

        Debug.Log($"��Ʈ���̳� ����: {typeToSpawn}, �ӵ�: {currentFallSpeed}");
    }

    /// <summary>
    /// ��Ʈ���̳� ť���� ���� Ÿ�� ��������
    /// </summary>
    /// <returns>���� ��� Ÿ��</returns>
    Tetrimino.TetrominoType GetNextTetrominoType()
    {
        // ť�� ��������� ���� ä���
        if (tetrominoQueue.Count == 0)
        {
            FillTetrominoQueue();
        }

        return tetrominoQueue.Dequeue();
    }

    /// <summary>
    /// ��Ʈ���̳� ť ä��� (7-bag �ý���)
    /// </summary>
    void FillTetrominoQueue()
    {
        // ��� Ÿ���� ����Ʈ�� �߰�
        List<Tetrimino.TetrominoType> tempList = new List<Tetrimino.TetrominoType>(allTetrominoTypes);

        // ����
        for (int i = 0; i < tempList.Count; i++)
        {
            int randomIndex = Random.Range(i, tempList.Count);
            var temp = tempList[i];
            tempList[i] = tempList[randomIndex];
            tempList[randomIndex] = temp;
        }

        // ť�� �߰�
        foreach (var type in tempList)
        {
            tetrominoQueue.Enqueue(type);
        }

        Debug.Log($"��Ʈ���̳� ť ä��: {tetrominoQueue.Count}��");
    }

    #endregion

    #region === ��Ʈ���̳� �ݹ� ===

    /// <summary>
    /// ��Ʈ���̳밡 �����Ǿ��� �� ȣ��
    /// </summary>
    /// <param name="tetrimino">������ ��Ʈ���̳�</param>
    public void OnTetrominoLocked(Tetrimino tetrimino)
    {
        if (tetrimino == currentTetrimino)
        {
            currentTetrimino = null;
        }

        // �̺�Ʈ �߻�
        EventManager.Instance.TetrominoLocked(tetrimino.GetTetrominoType(), tetrimino.GetGridPosition());
        //OnTetrominoLocked?.Invoke(tetrimino);

        // ���� Ŭ���� üũ�� PlayerGrid���� ó��

        // ���� ��Ʈ���̳� ���� (����)
        if (autoSpawn && !isGameOver)
        {
            StartCoroutine(SpawnNextTetrominoDelayed());
        }
    }

    /// <summary>
    /// ������ ��Ʈ���̳� ����
    /// </summary>
    IEnumerator SpawnNextTetrominoDelayed()
    {
        yield return new WaitForSeconds(spawnDelay);
        SpawnNextTetrimino();
    }

    /// <summary>
    /// ���� Ŭ���� �Ϸ� �ݹ�
    /// </summary>
    /// <param name="clearedLines">Ŭ����� ���� ��</param>
    public void OnLinesCleared(int clearedLines)
    {
        linesCleared += clearedLines;

        // ������ üũ (10���θ���)
        int newLevel = (linesCleared / 10) + 1;
        if (newLevel > currentLevel && newLevel <= maxLevel)
        {
            currentLevel = newLevel;
            UpdateFallSpeed();
            OnLevelUp?.Invoke(currentLevel);

            Debug.Log($"������! ���� ����: {currentLevel}");
        }

        // ���� �߰�
        if (GameManager.Instance != null)
        {
            int score = clearedLines * 100 * currentLevel;
            GameManager.Instance.AddScore(score);
        }
    }

    #endregion

    #region === ���� ���� ���� ===

    /// <summary>
    /// ���� ���� üũ
    /// </summary>
    /// <returns>���� ���� ����</returns>
    bool CheckGameOver()
    {
        // ��� 2�ٿ� ����� �ִ��� Ȯ��
        for (int x = 0; x < playerGrid.gridWidth; x++)
        {
            if (playerGrid.IsGridOccupied(x, playerGrid.gridHeight - 1) ||
                playerGrid.IsGridOccupied(x, playerGrid.gridHeight - 2))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// ���� ���� ó��
    /// </summary>
    void TriggerGameOver()
    {
        isGameOver = true;

        // ���� ��Ʈ���̳� ����
        if (currentTetrimino != null)
        {
            currentTetrimino.SetCanMove(false);
        }

        // �̺�Ʈ �߻�
        OnGameOver?.Invoke();

        // ���� �Ŵ����� �˸�
        if (GameManager.Instance != null)
        {
            GameManager.Instance.GameOver();
        }

        Debug.Log("���� ����!");
    }

    /// <summary>
    /// �ϰ� �ӵ� ������Ʈ
    /// </summary>
    void UpdateFallSpeed()
    {
        currentFallSpeed = Mathf.Max(0.1f, initialFallSpeed - (currentLevel - 1) * speedIncreaseRate);

        // ���� ��Ʈ���̳� �ӵ��� ������Ʈ
        if (currentTetrimino != null)
        {
            currentTetrimino.SetFallSpeed(currentFallSpeed);
        }
    }

    #endregion

    #region === �̸����� �ý��� ===

    /// <summary>
    /// ���� ��� �̸����� ������Ʈ
    /// </summary>
    void UpdateNextBlockPreview()
    {
        if (nextBlockPreview == null) return;

        // ���� �̸����� ����
        foreach (Transform child in nextBlockPreview)
        {
            Destroy(child.gameObject);
        }

        // ���� ��� Ÿ�� ��������
        if (tetrominoQueue.Count > 0)
        {
            nextTetrominoType = tetrominoQueue.Peek();
            // ���⼭ �̸����� ��� ���� ������ ������ �� ����
        }
    }

    #endregion

    #region === ���� �޼��� ===

    /// <summary>
    /// ���� ����
    /// </summary>
    public void StartGame()
    {
        isGameOver = false;
        currentLevel = 1;
        linesCleared = 0;
        currentFallSpeed = initialFallSpeed;

        // ť �ʱ�ȭ
        tetrominoQueue.Clear();
        FillTetrominoQueue();

        // ù ��° ��Ʈ���̳� ����
        SpawnNextTetrimino();

        Debug.Log("���� ����!");
    }

    /// <summary>
    /// ���� �����
    /// </summary>
    public void RestartGame()
    {
        // ���� ��Ʈ���̳� ����
        if (currentTetrimino != null)
        {
            Destroy(currentTetrimino.gameObject);
            currentTetrimino = null;
        }

        // �׸��� �ʱ�ȭ
        if (playerGrid != null)
        {
            // PlayerGrid�� �ʱ�ȭ �޼��� �ʿ�
        }

        // ���� �����
        StartGame();
    }

    /// <summary>
    /// ���� �Ͻ�����
    /// </summary>
    public void PauseGame()
    {
        if (currentTetrimino != null)
        {
            currentTetrimino.SetCanMove(false);
        }
    }

    /// <summary>
    /// ���� �簳
    /// </summary>
    public void ResumeGame()
    {
        if (currentTetrimino != null && !isGameOver)
        {
            currentTetrimino.SetCanMove(true);
        }
    }

    /// <summary>
    /// ���� ��Ʈ���̳� ����
    /// </summary>
    /// <param name="type">������ ��� Ÿ��</param>
    public void SpawnTetrimino(Tetrimino.TetrominoType type)
    {
        if (isGameOver) return;

        // ���� ��Ʈ���̳밡 ������ ����
        if (currentTetrimino != null)
        {
            Destroy(currentTetrimino.gameObject);
        }

        // ������ Ÿ������ ����
        GameObject tetrominoObj = Instantiate(tetrominoPrefab, spawnPoint.position, Quaternion.identity);
        currentTetrimino = tetrominoObj.GetComponent<Tetrimino>();
        currentTetrimino.SetTetrominoType(type);
        currentTetrimino.SetFallSpeed(currentFallSpeed);

        EventManager.Instance.TetrominoSpawned(currentTetrimino.GetTetrominoType());
    }

    #endregion

    #region === �Ӽ� ===

    public int CurrentLevel => currentLevel;
    public int LinesCleared => linesCleared;
    public float CurrentFallSpeed => currentFallSpeed;
    public bool IsGameOver => isGameOver;
    public Tetrimino CurrentTetrimino => currentTetrimino;
    public Tetrimino.TetrominoType NextTetrominoType => nextTetrominoType;

    #endregion

    #region === ����� ===

    /// <summary>
    /// �Ŵ��� ���� ���
    /// </summary>
    [ContextMenu("�Ŵ��� ����")]
    void PrintManagerInfo()
    {
        Debug.Log($"=== TetrominoManager ���� ===");
        Debug.Log($"���� ����: {currentLevel}");
        Debug.Log($"Ŭ����� ����: {linesCleared}");
        Debug.Log($"�ϰ� �ӵ�: {currentFallSpeed}");
        Debug.Log($"���� ����: {isGameOver}");
        Debug.Log($"ť ũ��: {tetrominoQueue.Count}");
        Debug.Log($"���� ��Ʈ���̳�: {(currentTetrimino != null ? currentTetrimino.GetTetrominoType().ToString() : "����")}");
    }

    /// <summary>
    /// �׽�Ʈ�� ������
    /// </summary>
    [ContextMenu("�׽�Ʈ ������")]
    void TestLevelUp()
    {
        OnLinesCleared(10);
    }

    #endregion
}