using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

[RequireComponent(typeof(PlayerStat))]
public class PlayerController : MonoBehaviour, IEntity
{
    private static readonly int ColorSwitch = Shader.PropertyToID("_Color_Switch");
    private PlayerStat stat;
    private InputAction moveAction;
    private const string MoveActionName = "Move";

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
    
    private Material instancedMat;

    private void Awake()
    {    
        var sr = GetComponentInChildren<SpriteRenderer>();
        instancedMat = Instantiate(sr.material);  // 인스턴스화
        sr.material = instancedMat;        
            
        stat = GetComponent<PlayerStat>();
        moveAction = InputSystem.actions.FindAction(MoveActionName);
        
        Vector2Int initGrid = GridSystem.WorldToGridIndex(transform.position);
        GridSystem.GridPos = initGrid;
        SetTargetWorldPosByGrid();
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
        moveAction.Enable();
        moveAction.performed += OnMove;
        moveAction.started += OnMove;
        moveAction.canceled += OnMove;
    }

    private void OnDisable()
    {
        moveAction.performed -= OnMove;
        moveAction.started -= OnMove;
        moveAction.canceled -= OnMove;
        moveAction.Disable();
    }
    
    private void Update()
    {
        hitInvincibleTimer += Time.deltaTime;
        // enforcementUI 테스트
        if (Input.GetKeyDown(KeyCode.B))
        {
            enforcementUI.gameObject.SetActive(true);
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
                GridSystem.GridPos.x += (int)Mathf.Sign(gridMoveBuffer);
                gridMoveBuffer -= Mathf.Sign(gridMoveBuffer);
                SetTargetWorldPosByGrid();
            }
        }

        // 러프 이동
        transform.position = Vector3.MoveTowards(transform.position, targetWorldPos, speed * Time.deltaTime);
        EventManager.Instance.PlayerMove(this.transform);
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        // 눌렸을 때
        if (context.started || context.performed)
        {
            moveInput = context.ReadValue<Vector2>();
            isInputHeld = true;
        }
        // 뗐을 때 이동 취소 및 가장 가까운 타일로
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

    public void OnHit(float Damage, Vector2 hitPosition)
    {
        if (hitInvincibleTimer < hitInvincibleTime) return;
        
        hitInvincibleTimer = 0f;
        TetriminoManager.Instance.SpawnUselessBlock();
        instancedMat.SetFloat(ColorSwitch, 1f);
        StartCoroutine(HitCoroutine());
    }

    private IEnumerator HitCoroutine()
    {
        yield return new WaitForSeconds(0.1f);
        instancedMat.SetFloat(ColorSwitch, 0f);
    }
}