using System;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerStat))]
public class PlayerController : MonoBehaviour
{
    private PlayerStat stat;
    private InputAction moveAction;
    private const string MoveActionName = "Move";

    [SerializeField] private float moveSpeed = 3f;        // 연속 이동 속도
    [SerializeField] private float tileSize = 1f;         // 한 칸 크기 (스냅 기준)
    private Vector2 moveInput = Vector2.zero;
    private bool isInputHeld = false;

    private void Awake()
    {
        stat = GetComponent<PlayerStat>();
        moveAction = InputSystem.actions.FindAction(MoveActionName);
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



        //for test
        Vector2Int gridPos = GridSystem.GetGridPos(GridSystem.GridPos.x, GridSystem.GridPos.y);
        Vector3 screenPos = new Vector3(gridPos.x, gridPos.y, 10f); // Z는 카메라와의 거리
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(screenPos);
        worldPos.z = 0; // 2D 게임용

        transform.position = worldPos;
        EventManager.Instance.PlayerMove(this.transform);



        //if (isInputHeld)
        //{
        //    // 좌우 연속 이동
        //    Vector3 move = new Vector3(moveInput.x, 0f, 0f) * (stat.GetStat(PlayerEnforcement.Speed) * Time.deltaTime);
        //    transform.position += move;
        //}

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

        //이동
        EventManager.Instance.PlayerMove(this.transform);
    }

    private void SnapToNearestTile()
    {
        Vector3 pos = transform.position;
        float snappedX = Mathf.Round(pos.x / tileSize) * tileSize;
        transform.position = new Vector3(snappedX, pos.y, pos.z);
    }
}