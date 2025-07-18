using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    private PlayerInputActions inputActions;
    
    [SerializeField] private float moveSpeed = 3f;        // 연속 이동 속도
    [SerializeField] private float tileSize = 1f;         // 한 칸 크기 (스냅 기준)
    private Vector2 moveInput = Vector2.zero;
    private bool isInputHeld = false;

    private void Awake()
    {
        inputActions = new PlayerInputActions();
    }
    
    private void OnEnable()
    {
        inputActions.PlayerActions.Enable();
        inputActions.PlayerActions.Move.performed += OnMove;
        inputActions.PlayerActions.Move.started += OnMove;
        inputActions.PlayerActions.Move.canceled += OnMove;
    }

    private void OnDisable()
    {
        inputActions.PlayerActions.Move.performed -= OnMove;
        inputActions.PlayerActions.Move.started -= OnMove;
        inputActions.PlayerActions.Move.canceled -= OnMove;
        inputActions.PlayerActions.Disable();
    }

    private void Update()
    {
        //if (isInputHeld)
        //{
        //    // 좌우 연속 이동
        //    Vector3 move = new Vector3(moveInput.x, 0f, 0f) * (moveSpeed * Time.deltaTime);
        //    transform.position += move;
        //}

        //for test
        Transform trans = GetComponent<Transform>();
        Vector2 gridPos = GridSystem.GetGridWorldPosition(GridSystem.GridPos.x, GridSystem.GridPos.y);
        trans.position = new Vector3(gridPos.x, gridPos.y, 0);
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
        Vector3 pos = transform.position;
        float snappedX = Mathf.Round(pos.x / tileSize) * tileSize;
        transform.position = new Vector3(snappedX, pos.y, pos.z);
    }
}