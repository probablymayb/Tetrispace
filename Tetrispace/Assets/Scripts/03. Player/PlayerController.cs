using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

[RequireComponent(typeof(PlayerStat))]
public class PlayerController : MonoBehaviour, IEntity
{
    private static readonly int ColorSwitch = Shader.PropertyToID("_Color_Switch");
    private PlayerStat stat;
    private InputAction moveAction;
    private InputAction magnetAction;  // MagnetAction 추가
    private const string MoveActionName = "Move";
    private const string MagnetActionName = "MagnetAttack";  // MagnetAction 이름

    [SerializeField] private float hitInvincibleTime = 1.5f;
    private float hitInvincibleTimer = 0f;

    [SerializeField] private UI_Enforcement enforcementUI;
    [SerializeField] private float moveSpeed = 3f; // 연속 이동 속도
    [SerializeField] private float tileSize = 1f; // 한 칸 크기 (스냅 기준)
    private Vector2 moveInput = Vector2.zero;
    private bool isInputHeld = false;
    [SerializeField] private float gridWorldUnitPerStep = 0.28f;
    private float gridMoveBuffer = 0f;
    private Vector3 targetWorldPos;

    [Header("=== Magnet Lazer 설정 ===")]
    [SerializeField] private GameObject lazerObject;        // Lazer 하위 객체 참조
    [SerializeField] private float lazerActiveDuration = 3f; // Lazer 활성화 시간
    [SerializeField] private float magnetCooldown = 5f;      // Magnet 쿨다운

    // Magnet 관련 내부 변수들
    private bool isMagnetOnCooldown = false;
    private Coroutine lazerCoroutine = null;
    private float magnetCooldownTimer = 0f;

    private Material instancedMat;

    public AudioClip Wsound;
    public AudioClip hitsound;

    private void Awake()
    {
        var sr = GetComponentInChildren<SpriteRenderer>();
        instancedMat = Instantiate(sr.material);  // 인스턴스화
        sr.material = instancedMat;

        stat = GetComponent<PlayerStat>();
        moveAction = InputSystem.actions.FindAction(MoveActionName);
        magnetAction = InputSystem.actions.FindAction(MagnetActionName);  // MagnetAction 찾기

        Vector2Int initGrid = GridSystem.WorldToGridIndex(transform.position);
        GridSystem.GridPos = initGrid;
        SetTargetWorldPosByGrid();

        // Lazer 오브젝트 설정
        SetupLazerObject();
    }
    
    /// <summary>
    /// Lazer 오브젝트 초기 설정
    /// </summary>
    private void SetupLazerObject()
    {
        // 자동으로 Lazer 하위 객체 찾기
        if (lazerObject == null)
        {
            Transform lazerTransform = transform.Find("Lazer");
            if (lazerTransform != null)
            {
                lazerObject = lazerTransform.gameObject;
                Debug.Log("Lazer 오브젝트 자동 발견: " + lazerObject.name);
            }
            else
            {
                Debug.LogWarning("Lazer 하위 객체를 찾을 수 없습니다!");
            }
        }

        // 초기에는 비활성화
        if (lazerObject != null)
        {
            lazerObject.SetActive(false);
            Debug.Log("Lazer 초기 비활성화 완료");
        }
    }

    private void Start()
    {
        GameManager.OnGameStart -= OnGameStart;
        GameManager.OnGameStart += OnGameStart;
    }

    private void OnGameStart()
    {
        TetriminoManager.Instance.OnLineClear -= TurnOnEnhancement;
        TetriminoManager.Instance.OnLineClear += TurnOnEnhancement;
        enforcementUI.gameObject.SetActive(true);
    }

    private void TurnOnEnhancement()
    {
        enforcementUI.gameObject.SetActive(true);
    }

    private void SetTargetWorldPosByGrid()
    {
        Vector2Int gridPos = GridSystem.GetGridPos(GridSystem.GridPos.x, GridSystem.GridPos.y);
        print("Grid: " + gridPos);
        Vector3 screenPos = new Vector3(gridPos.x, gridPos.y, 10f);
        targetWorldPos = Camera.main.ScreenToWorldPoint(screenPos);
        targetWorldPos.z = 0f;
    }

    private void OnEnable()
    {
        // Move Action 등록
        if (moveAction != null)
        {
            moveAction.Enable();
            moveAction.performed += OnMove;
            moveAction.started += OnMove;
            moveAction.canceled += OnMove;
        }

        // Magnet Action 등록
        if (magnetAction != null)
        {
            magnetAction.Enable();
            magnetAction.started += OnMagnetAction;
            magnetAction.performed += OnMagnetAction;
            magnetAction.canceled += OnMagnetAction;
            Debug.Log("MagnetAction 등록 완료");
        }
        else
        {
            Debug.LogWarning("MagnetAction을 찾을 수 없습니다!");
        }
    }

    private void OnDisable()
    {
        // Move Action 해제
        if (moveAction != null)
        {
            moveAction.performed -= OnMove;
            moveAction.started -= OnMove;
            moveAction.canceled -= OnMove;
            moveAction.Disable();
        }

        // Magnet Action 해제
        if (magnetAction != null)
        {
            magnetAction.started -= OnMagnetAction;
            magnetAction.performed -= OnMagnetAction;
            magnetAction.canceled -= OnMagnetAction;
            magnetAction.Disable();
        }

        // 활성화된 코루틴 정리
        if (lazerCoroutine != null)
        {
            StopCoroutine(lazerCoroutine);
            lazerCoroutine = null;
        }
    }

    private void Update()
    {
        hitInvincibleTimer += Time.deltaTime;

        // Magnet 쿨다운 처리
        if (isMagnetOnCooldown)
        {
            magnetCooldownTimer -= Time.deltaTime;
            if (magnetCooldownTimer <= 0f)
            {
                isMagnetOnCooldown = false;
                Debug.Log("Magnet 쿨다운 완료!");
            }
        }

        // enforcementUI 테스트
        if (Input.GetKeyDown(KeyCode.B))
        {
            enforcementUI.gameObject.SetActive(true);
        }

        // 수동 Magnet 테스트 (개발용)
        if (Input.GetKeyDown(KeyCode.L))
        {
            Debug.Log("수동 Magnet 테스트!");
            ActivateMagnetLazer();
        }

        float speed = stat.GetStat(PlayerEnforcement.Speed)[0]; // 초당 이동속도
        bool hasArrived = Vector3.Distance(transform.position, targetWorldPos) < 0.001f;

        if (isInputHeld && hasArrived)
        {
            int dir = (int)Mathf.Sign(moveInput.x);
            float gridPerSecond = speed / gridWorldUnitPerStep;
            gridMoveBuffer += gridPerSecond * Time.deltaTime * dir;

            while (Mathf.Abs(gridMoveBuffer) >= 1f)
            {
                GridSystem.GridPos.x = Mathf.Clamp(GridSystem.GridPos.x + (int)Mathf.Sign(gridMoveBuffer), 0, GridSystem.GridSettings.ActualGridWidth);
                gridMoveBuffer -= Mathf.Sign(gridMoveBuffer);
                SetTargetWorldPosByGrid();
            }
        }

        // 러프 이동
        transform.position = Vector3.MoveTowards(transform.position, targetWorldPos, speed * Time.deltaTime);
        EventManager.Instance.PlayerMove(this.transform);
    }

    #region === Move Action (기존) ===

    public void OnMove(InputAction.CallbackContext context)
    {
        if (context.started || context.performed)
        {
            Vector2 newInput = context.ReadValue<Vector2>();
            int newDir = (int)Mathf.Sign(newInput.x);
            int oldDir = (int)Mathf.Sign(moveInput.x);

            moveInput = newInput;
            isInputHeld = true;

            if (newDir != 0 && newDir != oldDir)
            {
                gridMoveBuffer = 0f;
            }
        }
        else if (context.canceled)
        {
            isInputHeld = false;
            moveInput = Vector2.zero;
            SnapToNearestTile();
        }
    }

    private void SnapToNearestTile()
    {
        transform.position = targetWorldPos;
        gridMoveBuffer = 0;
    }

    #endregion

    #region === Magnet Action (새로 추가) ===

    /// <summary>
    /// MagnetAction 입력 처리
    /// </summary>
    public void OnMagnetAction(InputAction.CallbackContext context)
    {
        SFXManager.Instance.PlaySFX(Wsound);
        if (context.started)
        {
            Debug.Log("MagnetAction 입력 감지!");
            ActivateMagnetLazer();
        }
        // performed와 canceled는 필요에 따라 추가 구현
    }

    /// <summary>
    /// Magnet Lazer 활성화
    /// </summary>
    public void ActivateMagnetLazer()
    {
        // 쿨다운 체크
        if (isMagnetOnCooldown)
        {
            Debug.Log($"Magnet 쿨다운 중! 남은 시간: {magnetCooldownTimer:F1}초");
            return;
        }

        // Lazer 오브젝트 체크
        if (lazerObject == null)
        {
            Debug.LogWarning("Lazer 오브젝트가 설정되지 않았습니다!");
            return;
        }

        Debug.Log("=== Magnet Lazer 활성화 ===");

        // 기존 코루틴이 있다면 정지
        if (lazerCoroutine != null)
        {
            StopCoroutine(lazerCoroutine);
        }

        // 새 코루틴 시작
        lazerCoroutine = StartCoroutine(LazerActivationCoroutine());

        // 쿨다운 시작
        StartMagnetCooldown();
    }

    /// <summary>
    /// Lazer 활성화 코루틴
    /// </summary>
    private IEnumerator LazerActivationCoroutine()
    {
        Debug.Log($"Lazer 활성화 시작 - {lazerActiveDuration}초간");

        // Lazer 활성화
        lazerObject.SetActive(true);

        // 지정된 시간만큼 대기
        yield return new WaitForSeconds(lazerActiveDuration);

        // Lazer 비활성화
        lazerObject.SetActive(false);
        Debug.Log("Lazer 비활성화 완료");

        // 코루틴 참조 정리
        lazerCoroutine = null;
    }

    /// <summary>
    /// Magnet 쿨다운 시작
    /// </summary>
    private void StartMagnetCooldown()
    {
        isMagnetOnCooldown = true;
        magnetCooldownTimer = magnetCooldown;
        Debug.Log($"Magnet 쿨다운 시작: {magnetCooldown}초");
    }

    /// <summary>
    /// Magnet 쿨다운 상태 확인
    /// </summary>
    public bool IsMagnetOnCooldown()
    {
        return isMagnetOnCooldown;
    }

    /// <summary>
    /// 남은 쿨다운 시간 반환
    /// </summary>
    public float GetMagnetCooldownRemaining()
    {
        return isMagnetOnCooldown ? magnetCooldownTimer : 0f;
    }

    /// <summary>
    /// 강제로 Lazer 비활성화
    /// </summary>
    public void DeactivateLazer()
    {
        if (lazerCoroutine != null)
        {
            StopCoroutine(lazerCoroutine);
            lazerCoroutine = null;
        }

        if (lazerObject != null)
        {
            lazerObject.SetActive(false);
        }

        Debug.Log("Lazer 강제 비활성화");
    }

    #endregion

    #region === Hit System (기존) ===

    public void OnHit(float Damage, Vector2 hitPosition)
    {
        SFXManager.Instance.PlaySFX(hitsound);
        if (hitInvincibleTimer < hitInvincibleTime) return;

        hitInvincibleTimer = 0f;
        if(GameManager.Instance.IsGamePlaying()) TetriminoManager.Instance.SpawnUselessBlock();
        instancedMat.SetFloat(ColorSwitch, 1f);
        StartCoroutine(HitCoroutine());
    }

    private IEnumerator HitCoroutine()
    {
        yield return new WaitForSeconds(0.1f);
        instancedMat.SetFloat(ColorSwitch, 0f);
    }

    #endregion

    #region === 디버그 및 유틸리티 ===

    /// <summary>
    /// 디버그 정보 표시
    /// </summary>
    private void OnGUI()
    {
        if (!Application.isEditor) return;

        GUILayout.BeginArea(new Rect(10, 400, 300, 150));
        GUILayout.Label("=== PlayerController Debug ===");
        GUILayout.Label($"Magnet 쿨다운: {(isMagnetOnCooldown ? $"{magnetCooldownTimer:F1}s" : "준비됨")}");
        GUILayout.Label($"Lazer 상태: {(lazerObject != null && lazerObject.activeSelf ? "활성" : "비활성")}");

        GUILayout.Space(10);

        if (GUILayout.Button("Test Magnet Lazer"))
        {
            ActivateMagnetLazer();
        }

        if (GUILayout.Button("Force Deactivate Lazer"))
        {
            DeactivateLazer();
        }

        GUILayout.EndArea();
    }

    /// <summary>
    /// Context Menu 테스트 메서드들
    /// </summary>
    [ContextMenu("Test Magnet Activation")]
    public void TestMagnetActivation()
    {
        Debug.Log("Context Menu에서 Magnet 테스트!");
        ActivateMagnetLazer();
    }

    [ContextMenu("Reset Magnet Cooldown")]
    public void ResetMagnetCooldown()
    {
        isMagnetOnCooldown = false;
        magnetCooldownTimer = 0f;
        Debug.Log("Magnet 쿨다운 리셋!");
    }

    #endregion
}