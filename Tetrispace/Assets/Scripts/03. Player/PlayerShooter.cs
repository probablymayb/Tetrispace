using System;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerStat))]
public class PlayerShooter : MonoBehaviour
{
    #region ������Ʈ �� �׼� ����
    private PlayerStat stat;
    private InputAction shootAction;
    private InputAction CCWAction;
    private InputAction CWAction;

    // �׼� �̸� ��� ����
    private const string ShootActionName = "Attack";
    private const string CCWActionName = "RotationAttackCCW";
    private const string CWActionName = "RotationAttackCW";
    #endregion

    #region ������ ����
    [Header("=== �Ѿ� ������ ===")]
    [SerializeField] private Bullet bulletPrefab;
    [SerializeField] private Bullet CCWBulletPrefab;    // �ݽð���� ȸ�� �Ѿ�
    [SerializeField] private Bullet CWBulletPrefab;     // �ð���� ȸ�� �Ѿ�

    [Header("=== �߻� ���� ===")]
    [SerializeField] private Transform firePoint;
    [SerializeField] private float attackCooldown = 0.3f;
    [SerializeField] private float rotationAttackCooldown = 0.5f;  // ȸ�� ������ �� �� ������
    [SerializeField] private BulletData bulletData;
    [SerializeField] private BulletData CCWBulletData;        // ȸ�� �Ѿ˿� ������
    [SerializeField] private BulletData CWBulletData;        // ȸ�� �Ѿ˿� ������
    #endregion

    #region ���� Ÿ�̸� �� ����
    private float attackTimer = 0f;
    private float CCWAttackTimer = 0f;  // CCW ���� ���� Ÿ�̸�
    private float CWAttackTimer = 0f;   // CW ���� ���� Ÿ�̸�

    private bool isAttackHeld = false;
    private bool isCCWAttackHeld = false;
    private bool isCWAttackHeld = false;
    #endregion

    #region �ʱ�ȭ
    private void Awake()
    {
        // ������Ʈ ���� ȹ��
        stat = GetComponent<PlayerStat>();

        // Input Action ã��
        shootAction = InputSystem.actions.FindAction(ShootActionName);
        CCWAction = InputSystem.actions.FindAction(CCWActionName);
        CWAction = InputSystem.actions.FindAction(CWActionName);

        // ������Ʈ Ǯ ���� (������� ���� ����)
        CreateBulletPools();
    }

    /// <summary>
    /// �Ѿ� ������Ʈ Ǯ���� �����մϴ�
    /// </summary>
    private void CreateBulletPools()
    {
        if (bulletPrefab != null)
            PoolManager.Instance.CreatePool(bulletPrefab, 20);

        if (CCWBulletPrefab != null)
            PoolManager.Instance.CreatePool(CCWBulletPrefab, 10);

        if (CWBulletPrefab != null)
            PoolManager.Instance.CreatePool(CWBulletPrefab, 10);
    }
    #endregion

    #region Enable/Disable �̺�Ʈ ���
    private void OnEnable()
    {
        // �Ϲ� ���� �׼� ���
        RegisterShootAction();

        // CCW ȸ�� ���� �׼� ���  
        RegisterCCWAction();

        // CW ȸ�� ���� �׼� ���
        RegisterCWAction();

        // Ÿ�̸� �ʱ�ȭ
        ResetAllTimers();
    }

    private void OnDisable()
    {
        // ��� �׼� ����
        UnregisterShootAction();
        UnregisterCCWAction();
        UnregisterCWAction();
    }

    /// <summary>
    /// �Ϲ� ���� �׼� ���
    /// </summary>
    private void RegisterShootAction()
    {
        if (shootAction != null)
        {
            shootAction.Enable();
            shootAction.started += OnAttack;
            shootAction.performed += OnAttack;
            shootAction.canceled += OnAttack;
        }
    }

    /// <summary>
    /// CCW ȸ�� ���� �׼� ���
    /// </summary>
    private void RegisterCCWAction()
    {
        if (CCWAction != null)
        {
            CCWAction.Enable();
            CCWAction.started += OnCCWRotationAttack;
            CCWAction.performed += OnCCWRotationAttack;
            CCWAction.canceled += OnCCWRotationAttack;
        }
    }

    /// <summary>
    /// CW ȸ�� ���� �׼� ���
    /// </summary>
    private void RegisterCWAction()
    {
        if (CWAction != null)
        {
            CWAction.Enable();
            CWAction.started += OnCWRotationAttack;
            CWAction.performed += OnCWRotationAttack;
            CWAction.canceled += OnCWRotationAttack;
        }
    }

    /// <summary>
    /// �Ϲ� ���� �׼� ����
    /// </summary>
    private void UnregisterShootAction()
    {
        if (shootAction != null)
        {
            shootAction.started -= OnAttack;
            shootAction.performed -= OnAttack;
            shootAction.canceled -= OnAttack;
            shootAction.Disable();
        }
    }

    /// <summary>
    /// CCW ȸ�� ���� �׼� ����
    /// </summary>
    private void UnregisterCCWAction()
    {
        if (CCWAction != null)
        {
            CCWAction.started -= OnCCWRotationAttack;
            CCWAction.performed -= OnCCWRotationAttack;
            CCWAction.canceled -= OnCCWRotationAttack;
            CCWAction.Disable();
        }
    }

    /// <summary>
    /// CW ȸ�� ���� �׼� ����
    /// </summary>
    private void UnregisterCWAction()
    {
        if (CWAction != null)
        {
            CWAction.started -= OnCWRotationAttack;
            CWAction.performed -= OnCWRotationAttack;
            CWAction.canceled -= OnCWRotationAttack;
            CWAction.Disable();
        }
    }
    #endregion

    #region ������Ʈ ����
    private void Update()
    {
        // �� ���� Ÿ�Ժ��� ���� ó��
        ProcessContinuousAttacks();
    }

    /// <summary>
    /// ���� ���� ó�� (�� ���� Ÿ�Ժ���)
    /// </summary>
    private void ProcessContinuousAttacks()
    {
        // �Ϲ� ���� ���� ó��
        if (isAttackHeld)
        {
            attackTimer -= Time.deltaTime;
            if (attackTimer <= 0f)
            {
                Fire();
                attackTimer = attackCooldown;
            }
        }

        // CCW ȸ�� ���� ���� ó��
        if (isCCWAttackHeld)
        {
            CCWAttackTimer -= Time.deltaTime;
            if (CCWAttackTimer <= 0f)
            {
                FireCCWBullet();
                CCWAttackTimer = rotationAttackCooldown;
            }
        }

        // CW ȸ�� ���� ���� ó��
        if (isCWAttackHeld)
        {
            CWAttackTimer -= Time.deltaTime;
            if (CWAttackTimer <= 0f)
            {
                FireCWBullet();
                CWAttackTimer = rotationAttackCooldown;
            }
        }
    }
    #endregion

    #region �Է� �̺�Ʈ �ڵ鷯
    /// <summary>
    /// �Ϲ� ���� �Է� ó��
    /// </summary>
    private void OnAttack(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            // ù �߻� ��� ����
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

    /// <summary>
    /// CCW(�ݽð����) ȸ�� ���� �Է� ó��
    /// </summary>
    private void OnCCWRotationAttack(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            // ù �߻� ��� ����
            if (CCWAttackTimer <= 0f)
            {
                FireCCWBullet();
                CCWAttackTimer = rotationAttackCooldown;
            }
            isCCWAttackHeld = true;
        }
        else if (context.canceled)
        {
            isCCWAttackHeld = false;
        }
    }

    /// <summary>
    /// CW(�ð����) ȸ�� ���� �Է� ó��
    /// </summary>
    private void OnCWRotationAttack(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            // ù �߻� ��� ����
            if (CWAttackTimer <= 0f)
            {
                FireCWBullet();
                CWAttackTimer = rotationAttackCooldown;
            }
            isCWAttackHeld = true;
        }
        else if (context.canceled)
        {
            isCWAttackHeld = false;
        }
    }
    #endregion

    #region �߻� �޼����
    /// <summary>
    /// �Ϲ� �Ѿ� �߻�
    /// </summary>
    private void Fire()
    {
        if (bulletPrefab == null) return;

        // Ǯ���� �Ѿ� �������� (���׸� ���)
        Bullet pooledBullet = PoolManager.Instance.Get(bulletPrefab);
        if (pooledBullet == null) return;

        // �Ѿ� ���� �� �߻�
        SetupAndFireBullet(pooledBullet, bulletData, Vector2.up);
    }

    /// <summary>
    /// CCW(�ݽð����) ȸ�� �Ѿ� �߻�
    /// </summary>
    private void FireCCWBullet()
    {

        // Ǯ���� CCW �Ѿ� �������� (���׸� ���)
        Bullet pooledBullet = PoolManager.Instance.Get(bulletPrefab);
        if (pooledBullet == null) return;

        // CCW ȸ�� �������� �߻� 
        Vector2 direction = Vector2.up;
        SetupAndFireRotationBullet(pooledBullet, CCWBulletData, direction);
    }

    /// <summary>
    /// CW(�ð����) ȸ�� �Ѿ� �߻�
    /// </summary>
    private void FireCWBullet()
    {
        // Ǯ���� CW �Ѿ� �������� (���׸� ���)
        Bullet pooledBullet = PoolManager.Instance.Get(bulletPrefab);
        if (pooledBullet == null) return;

        // CW ȸ�� �������� �߻�
        Vector2 direction = Vector2.up;
        SetupAndFireRotationBullet(pooledBullet, CWBulletData, direction);
    }


    /// <summary>
    /// �Ѿ� �⺻ ���� �� �߻� (���� ����)
    /// </summary>
    private void SetupAndFireBullet(Bullet bullet, BulletData data, Vector2 direction)
    {
        if (bullet == null) return;

        // ��ġ �� ȸ�� ����
        bullet.transform.position = firePoint.position;
        bullet.transform.rotation = Quaternion.identity;

        // �Ѿ� ������ ����
        BulletData dataToUse = data != null ? data : bulletData;
        float damage = stat.GetStat(PlayerEnforcement.AutoDamage);

        bullet.Setup(dataToUse, damage);
        bullet.Fire(direction, "Enemy");
    }

    /// <summary>
    /// ȸ�� �Ѿ� �⺻ ���� �� �߻� (CCW/CW ����)
    /// </summary>
    private void SetupAndFireRotationBullet<T>(T bullet, BulletData data, Vector2 direction) where T : Component
    {
        if (bullet == null) return;

        // ��ġ �� ȸ�� ����
        bullet.transform.position = firePoint.position;
        bullet.transform.rotation = Quaternion.identity;

        // �Ѿ� ������ ����
        BulletData dataToUse = data != null ? data : bulletData;
        float damage = stat.GetStat(PlayerEnforcement.AutoDamage);

        // Ÿ�Կ� ���� Setup ȣ��
        if (bullet is Bullet ccwBullet)
        {
            ccwBullet.Setup(dataToUse, damage);
            ccwBullet.Fire(direction, "TetriminoBlock");
        }
        else if (bullet is Bullet cwBullet)
        {
            cwBullet.Setup(dataToUse, damage);
            cwBullet.Fire(direction, "TetriminoBlock");
        }
    }
    #endregion

    #region ��ƿ��Ƽ �޼���
    /// <summary>
    /// ��� ���� Ÿ�̸� �ʱ�ȭ
    /// </summary>
    private void ResetAllTimers()
    {
        attackTimer = 0f;
        CCWAttackTimer = 0f;
        CWAttackTimer = 0f;
    }

    /// <summary>
    /// ����� ���� ǥ�� (�������)
    /// </summary>
    private void OnGUI()
    {
        if (!Application.isEditor) return;

        GUILayout.BeginArea(new Rect(10, 200, 250, 150));
        GUILayout.Label("=== PlayerShooter Debug ===");
        GUILayout.Label($"�Ϲ� ����: {(isAttackHeld ? "ON" : "OFF")} ({attackTimer:F1}s)");
        GUILayout.Label($"CCW ����: {(isCCWAttackHeld ? "ON" : "OFF")} ({CCWAttackTimer:F1}s)");
        GUILayout.Label($"CW ����: {(isCWAttackHeld ? "ON" : "OFF")} ({CWAttackTimer:F1}s)");
        GUILayout.EndArea();
    }
    #endregion
}