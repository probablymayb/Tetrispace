using System;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerStat))]
public class PlayerShooter : MonoBehaviour
{
    private PlayerStat stat;
    private InputAction shootAction;
    private const string ShootActionName = "Attack";
    
    [SerializeField] private Bullet bulletPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float attackCooldown = 0.3f;
    [SerializeField] private BulletData bulletData;

    private float attackTimer = 0f;
    private bool isAttackHeld = false;
    
    private void Awake()
    {
        stat = GetComponent<PlayerStat>();
        shootAction = InputSystem.actions.FindAction(ShootActionName);
        PoolManager.Instance.CreatePool(bulletPrefab);
    }

    private void OnEnable()
    {
        shootAction.Enable();
        shootAction.started += OnAttack;
        shootAction.performed += OnAttack;
        shootAction.canceled += OnAttack;

        attackTimer = 0f;
    }

    private void OnDisable()
    {
        shootAction.started -= OnAttack;
        shootAction.performed -= OnAttack;
        shootAction.canceled -= OnAttack;
        shootAction.Disable();
    }

    private void Update()
    {
        if (isAttackHeld)
        {
            attackTimer -= Time.deltaTime;

            if (attackTimer <= 0f)
            {
                Fire();
                attackTimer = attackCooldown;
            }
        }
    }

    private void OnAttack(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            if (attackTimer <= 0f)
            {
                Fire();
                attackTimer = attackCooldown;
            }
            isAttackHeld = true;
        }
        else if (context.canceled)
        {
            isAttackHeld = false;
        }
    }
    
    private void Fire()
    {
        Bullet pooledBullet = PoolManager.Instance.Get(bulletPrefab);
        pooledBullet.transform.position = firePoint.position;
        pooledBullet.transform.rotation = Quaternion.identity;

        pooledBullet.Setup(bulletData, stat.GetStat(PlayerEnforcement.AutoDamage));
        pooledBullet.Fire(Vector2.up, true);
    }
}
