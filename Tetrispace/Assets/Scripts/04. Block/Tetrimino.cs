using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 테트리미노 클래스 (게임잼용)
/// 
/// 기능:
/// - 6가지 블록 모양 (O, I, T, L, J, S)
/// - 회전 및 이동
/// - 그리드 유효성 검사
/// - 자동 하강
/// </summary>
public class Tetrimino : MonoBehaviour
{
    [Header("=== 테트리미노 설정 ===")]
    [SerializeField] private TetrominoType blockType;
    [SerializeField] private float fallSpeed = 1f;           // 하강 속도 (초/칸)
    [SerializeField] private float moveSpeed = 0.15f;        // 좌우 이동 속도
    [SerializeField] private float lockDelay = 0.5f;         // 착지 후 고정 지연시간

    [Header("=== 참조 ===")]
    [SerializeField] private GameObject blockPrefab;         // 블록 프리팹
    [SerializeField] private PlayerGrid playerGrid;         // 플레이어 그리드 참조

    // === 테트리미노 타입 정의 ===
    public enum TetrominoType
    {
        O,  // 정사각형
        I,  // 일자형
        T,  // T자형
        L,  // L자형
        J,  // J자형 (역L)
        S   // S자형
    }

    // === 상태 변수 ===
    private List<Transform> blocks = new List<Transform>();  // 구성 블록들
    private Vector2Int gridPosition = new Vector2Int(1, 4);  // 그리드상 위치 (중앙 상단)
    private int currentRotation = 0;                         // 현재 회전 상태 (0~3)
    private float fallTimer = 0f;                           // 하강 타이머
    private float lockTimer = 0f;                           // 착지 타이머
    private bool isLocked = false;                          // 고정 여부
    private bool canMove = true;                            // 이동 가능 여부

    // === 각 테트리미노별 블록 배치 정의 ===
    private Dictionary<TetrominoType, Vector2Int[,]> blockShapes = new Dictionary<TetrominoType, Vector2Int[,]>
    {
        // O 블록 - 2x2 정사각형 (회전 불가)
        [TetrominoType.O] = new Vector2Int[,] {
            { new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(0, 1), new Vector2Int(1, 1) },
            { new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(0, 1), new Vector2Int(1, 1) },
            { new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(0, 1), new Vector2Int(1, 1) },
            { new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(0, 1), new Vector2Int(1, 1) }
        },

        // I 블록 - 일자형
        [TetrominoType.I] = new Vector2Int[,] {
            { new Vector2Int(0, 1), new Vector2Int(1, 1), new Vector2Int(2, 1), new Vector2Int(3, 1) },
            { new Vector2Int(1, 0), new Vector2Int(1, 1), new Vector2Int(1, 2), new Vector2Int(1, 3) },
            { new Vector2Int(0, 1), new Vector2Int(1, 1), new Vector2Int(2, 1), new Vector2Int(3, 1) },
            { new Vector2Int(1, 0), new Vector2Int(1, 1), new Vector2Int(1, 2), new Vector2Int(1, 3) }
        },

        // T 블록 - T자형
        [TetrominoType.T] = new Vector2Int[,] {
            { new Vector2Int(1, 0), new Vector2Int(0, 1), new Vector2Int(1, 1), new Vector2Int(2, 1) },
            { new Vector2Int(1, 0), new Vector2Int(1, 1), new Vector2Int(2, 1), new Vector2Int(1, 2) },
            { new Vector2Int(0, 1), new Vector2Int(1, 1), new Vector2Int(2, 1), new Vector2Int(1, 2) },
            { new Vector2Int(1, 0), new Vector2Int(0, 1), new Vector2Int(1, 1), new Vector2Int(1, 2) }
        },

        // L 블록 - L자형
        [TetrominoType.L] = new Vector2Int[,] {
            { new Vector2Int(2, 0), new Vector2Int(0, 1), new Vector2Int(1, 1), new Vector2Int(2, 1) },
            { new Vector2Int(1, 0), new Vector2Int(1, 1), new Vector2Int(1, 2), new Vector2Int(2, 2) },
            { new Vector2Int(0, 1), new Vector2Int(1, 1), new Vector2Int(2, 1), new Vector2Int(0, 2) },
            { new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(1, 1), new Vector2Int(1, 2) }
        },

        // J 블록 - J자형 (역L)
        [TetrominoType.J] = new Vector2Int[,] {
            { new Vector2Int(0, 0), new Vector2Int(0, 1), new Vector2Int(1, 1), new Vector2Int(2, 1) },
            { new Vector2Int(1, 0), new Vector2Int(2, 0), new Vector2Int(1, 1), new Vector2Int(1, 2) },
            { new Vector2Int(0, 1), new Vector2Int(1, 1), new Vector2Int(2, 1), new Vector2Int(2, 2) },
            { new Vector2Int(1, 0), new Vector2Int(1, 1), new Vector2Int(0, 2), new Vector2Int(1, 2) }
        },

        // S 블록 - S자형
        [TetrominoType.S] = new Vector2Int[,] {
            { new Vector2Int(1, 0), new Vector2Int(2, 0), new Vector2Int(0, 1), new Vector2Int(1, 1) },
            { new Vector2Int(1, 0), new Vector2Int(1, 1), new Vector2Int(2, 1), new Vector2Int(2, 2) },
            { new Vector2Int(1, 0), new Vector2Int(2, 0), new Vector2Int(0, 1), new Vector2Int(1, 1) },
            { new Vector2Int(1, 0), new Vector2Int(1, 1), new Vector2Int(2, 1), new Vector2Int(2, 2) }
        }
    };

    // === 초기화 ===
    void Start()
    {
        InitializeTetrimino();
    }

    void Update()
    {
        if (isLocked) return;

        HandleInput();      // 입력 처리
        HandleFalling();    // 자동 하강
        HandleLocking();    // 착지 처리
    }

    #region === 초기화 ===

    /// <summary>
    /// 테트리미노 초기화
    /// </summary>
    void InitializeTetrimino()
    {
        // PlayerGrid 자동 찾기
        if (playerGrid == null)
            playerGrid = FindFirstObjectByType<PlayerGrid>();

        if (playerGrid == null)
        {
            Debug.LogError("PlayerGrid를 찾을 수 없습니다!");
            return;
        }

        // 블록 생성
        CreateBlocks();

        // 초기 위치 설정
        UpdateBlockPositions();

        Debug.Log($"테트리미노 생성: {blockType}");
    }

    /// <summary>
    /// 블록들 생성
    /// </summary>
    void CreateBlocks()
    {
        // 기존 블록 제거
        foreach (Transform block in blocks)
        {
            if (block != null)
                Destroy(block.gameObject);
        }
        blocks.Clear();

        // 새 블록 생성
        for (int i = 0; i < 4; i++)
        {
            GameObject block = Instantiate(blockPrefab, transform);
            blocks.Add(block.transform);
        }
    }

    #endregion

    #region === 입력 처리 ===

    /// <summary>
    /// 플레이어 입력 처리
    /// </summary>
    void HandleInput()
    {
        if (!canMove) return;

        // 좌우 이동
        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
        {
            TryMove(Vector2Int.left);
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
        {
            TryMove(Vector2Int.right);
        }

        // 회전
        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
        {
            TryRotate();
        }

        // 빠른 하강
        if (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S))
        {
            fallTimer += Time.deltaTime * 5f; // 5배속 하강
        }

        // 즉시 하강 (하드 드롭)
        if (Input.GetKeyDown(KeyCode.Space))
        {
            HardDrop();
        }
    }

    #endregion

    #region === 이동 및 회전 ===

    /// <summary>
    /// 이동 시도
    /// </summary>
    /// <param name="direction">이동 방향</param>
    /// <returns>이동 성공 여부</returns>
    bool TryMove(Vector2Int direction)
    {
        Vector2Int newPosition = gridPosition + direction;

        if (IsValidPosition(newPosition, currentRotation))
        {
            gridPosition = newPosition;
            UpdateBlockPositions();

            // 착지 타이머 리셋
            if (direction == Vector2Int.down)
            {
                lockTimer = 0f;
            }

            return true;
        }

        return false;
    }

    /// <summary>
    /// 회전 시도
    /// </summary>
    /// <returns>회전 성공 여부</returns>
    bool TryRotate()
    {
        int newRotation = (currentRotation + 1) % 4;

        if (IsValidPosition(gridPosition, newRotation))
        {
            currentRotation = newRotation;
            UpdateBlockPositions();
            lockTimer = 0f; // 착지 타이머 리셋
            return true;
        }

        return false;
    }

    /// <summary>
    /// 즉시 하강 (하드 드롭)
    /// </summary>
    void HardDrop()
    {
        while (TryMove(Vector2Int.down))
        {
            // 더 이상 내려갈 수 없을 때까지 계속 이동
        }

        // 즉시 고정
        lockTimer = lockDelay;
    }

    #endregion

    #region === 유효성 검사 ===

    /// <summary>
    /// 특정 위치와 회전에서 유효한지 검사
    /// </summary>
    /// <param name="position">검사할 위치</param>
    /// <param name="rotation">검사할 회전</param>
    /// <returns>유효한 위치인지 여부</returns>
    bool IsValidPosition(Vector2Int position, int rotation)
    {
        Vector2Int[] shape = GetCurrentShape(rotation);

        foreach (Vector2Int blockOffset in shape)
        {
            Vector2Int blockPos = position + blockOffset;

            // 그리드 경계 체크
            if (!playerGrid.IsValidGridPosition(blockPos.x, blockPos.y))
            {
                return false;
            }

            // 다른 블록과 충돌 체크
            if (playerGrid.IsGridOccupied(blockPos.x, blockPos.y))
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// 현재 회전 상태의 블록 모양 가져오기
    /// </summary>
    /// <param name="rotation">회전 상태</param>
    /// <returns>블록 오프셋 배열</returns>
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

    #region === 위치 업데이트 ===

    /// <summary>
    /// 블록들의 실제 위치 업데이트
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

    #region === 자동 하강 ===

    /// <summary>
    /// 자동 하강 처리
    /// </summary>
    void HandleFalling()
    {
        fallTimer += Time.deltaTime;

        if (fallTimer >= fallSpeed)
        {
            if (!TryMove(Vector2Int.down))
            {
                // 더 이상 내려갈 수 없음 - 착지 시작
                StartLanding();
            }

            fallTimer = 0f;
        }
    }

    /// <summary>
    /// 착지 처리 시작
    /// </summary>
    void StartLanding()
    {
        lockTimer += Time.deltaTime;
    }

    /// <summary>
    /// 착지 처리
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
    /// 테트리미노 고정
    /// </summary>
    void LockTetrimino()
    {
        isLocked = true;

        // PlayerGrid에 블록들 등록
        Vector2Int[] shape = GetCurrentShape(currentRotation);

        for (int i = 0; i < blocks.Count && i < shape.Length; i++)
        {
            Vector2Int blockGridPos = gridPosition + shape[i];
            playerGrid.PlaceBlock(blockGridPos.x, blockGridPos.y, blocks[i]);
        }

        // 부모 관계 해제
        foreach (Transform block in blocks)
        {
            block.SetParent(playerGrid.transform);
        }

        Debug.Log($"테트리미노 고정: {blockType} at {gridPosition}");

        // 테트리미노 매니저에 완료 알림
        //TetrominoManager.Instance?.OnTetrominoLocked(this);
        EventManager.Instance.TetrominoLocked(blockType, gridPosition);

        // 자기 자신 파괴
        Destroy(gameObject);
    }

    #endregion

    #region === 공개 메서드 ===

    /// <summary>
    /// 테트리미노 타입 설정
    /// </summary>
    /// <param name="type">블록 타입</param>
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
    /// 하강 속도 설정
    /// </summary>
    /// <param name="speed">하강 속도 (초/칸)</param>
    public void SetFallSpeed(float speed)
    {
        fallSpeed = speed;
    }

    /// <summary>
    /// 이동 가능 여부 설정
    /// </summary>
    /// <param name="canMove">이동 가능 여부</param>
    public void SetCanMove(bool canMove)
    {
        this.canMove = canMove;
    }

    /// <summary>
    /// 현재 테트리미노 타입 가져오기
    /// </summary>
    /// <returns>현재 블록 타입</returns>
    public TetrominoType GetTetrominoType()
    {
        return blockType;
    }

    /// <summary>
    /// 현재 그리드 위치 가져오기
    /// </summary>
    /// <returns>그리드 위치</returns>
    public Vector2Int GetGridPosition()
    {
        return gridPosition;
    }

    #endregion

    #region === 디버그 ===

    /// <summary>
    /// 테트리미노 정보 출력
    /// </summary>
    [ContextMenu("테트리미노 정보")]
    void PrintTetrominoInfo()
    {
        Debug.Log($"=== 테트리미노 정보 ===");
        Debug.Log($"타입: {blockType}");
        Debug.Log($"위치: {gridPosition}");
        Debug.Log($"회전: {currentRotation}");
        Debug.Log($"고정됨: {isLocked}");
        Debug.Log($"블록 개수: {blocks.Count}");
    }

    #endregion
}