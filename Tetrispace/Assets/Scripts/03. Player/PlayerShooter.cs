using System;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerStat))]
public class PlayerShooter : MonoBehaviour
{
    #region 컴포넌트 및 액션 참조
    private PlayerStat stat;
    private InputAction shootAction;
    private InputAction CCWAction;
    private InputAction CWAction;

    // 액션 이름 상수 정의
    private const string ShootActionName = "Attack";
    private const string CCWActionName = "RotationAttackCCW";
    private const string CWActionName = "RotationAttackCW";
    #endregion

    #region 프리팹 참조
    [Header("=== 총알 프리팹 ===")]
    [SerializeField] private Bullet bulletPrefab;
    
    [Header("=== 발사 설정 ===")]
    [SerializeField] private Transform firePoint;
    [SerializeField] private float attackCooldown = 0.3f;
    [SerializeField] private float rotationAttackCooldown = 0.5f;  // 회전 공격은 좀 더 느리게
    [SerializeField] private BulletData bulletData;
    [SerializeField] private BulletData CCWBulletData;        // 회전 총알용 데이터
    [SerializeField] private BulletData CWBulletData;        // 회전 총알용 데이터
    #endregion

    #region 공격 타이머 및 상태
    private float attackTimer = 0f;
    private float CCWAttackTimer = 0f;  // CCW 공격 전용 타이머
    private float CWAttackTimer = 0f;   // CW 공격 전용 타이머

    private bool isAttackHeld = false;
    private bool isCCWAttackHeld = false;
    private bool isCWAttackHeld = false;
    #endregion

    #region 초기화

    public AudioClip Qsound;
    public AudioClip Esound;

    private void Awake()
    {
        // 컴포넌트 참조 획득
        stat = GetComponent<PlayerStat>();

        // Input Action 찾기
        shootAction = InputSystem.actions.FindAction(ShootActionName);
        CCWAction = InputSystem.actions.FindAction(CCWActionName);
        CWAction = InputSystem.actions.FindAction(CWActionName);

        // 오브젝트 풀 생성 (게임잼용 빠른 구현)
        CreateBulletPools();
    }

    /// <summary>
    /// 총알 오브젝트 풀들을 생성합니다
    /// </summary>
    private void CreateBulletPools()
    {
        if (bulletPrefab != null)
            PoolManager.Instance.CreatePool(bulletPrefab, 20);
    }
    #endregion

    #region Enable/Disable 이벤트 등록
    private void OnEnable()
    {
        // 일반 공격 액션 등록
        RegisterShootAction();

        // CCW 회전 공격 액션 등록  
        RegisterCCWAction();

        // CW 회전 공격 액션 등록
        RegisterCWAction();

        // 타이머 초기화
        ResetAllTimers();
    }

    private void OnDisable()
    {
        // 모든 액션 해제
        UnregisterShootAction();
        UnregisterCCWAction();
        UnregisterCWAction();
    }

    /// <summary>
    /// 일반 공격 액션 등록
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
    /// CCW 회전 공격 액션 등록
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
    /// CW 회전 공격 액션 등록
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
    /// 일반 공격 액션 해제
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
    /// CCW 회전 공격 액션 해제
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
    /// CW 회전 공격 액션 해제
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

    #region 업데이트 루프
    private void Update()
    {
        // 각 공격 타입별로 연사 처리
        ProcessContinuousAttacks();
    }

    /// <summary>
    /// 연속 공격 처리 (각 공격 타입별로)
    /// </summary>
    private void ProcessContinuousAttacks()
    {
        // 일반 공격 연사 처리
        if (isAttackHeld)
        {
            attackTimer -= Time.deltaTime;
            if (attackTimer <= 0f)
            {
                Fire();
                attackTimer = attackCooldown;
            }
        }

        // CCW 회전 공격 연사 처리
        if (isCCWAttackHeld)
        {
            CCWAttackTimer -= Time.deltaTime;
            if (CCWAttackTimer <= 0f)
            {
                FireCCWBullet();
                CCWAttackTimer = rotationAttackCooldown;
            }
        }

        // CW 회전 공격 연사 처리
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

    #region 입력 이벤트 핸들러
    /// <summary>
    /// 일반 공격 입력 처리
    /// </summary>
    private void OnAttack(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            // 첫 발사 즉시 실행
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
    /// CCW(반시계방향) 회전 공격 입력 처리
    /// </summary>
    private void OnCCWRotationAttack(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            // 첫 발사 즉시 실행
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
    /// CW(시계방향) 회전 공격 입력 처리
    /// </summary>
    private void OnCWRotationAttack(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            // 첫 발사 즉시 실행
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

    #region 발사 메서드들
    /// <summary>
    /// 일반 총알 발사
    /// </summary>
    private void Fire()
    {
        if (bulletPrefab == null) return;

        // 풀에서 총알 가져오기 (제네릭 방식)
        Bullet pooledBullet = PoolManager.Instance.Get(bulletPrefab);
        if (pooledBullet == null) return;

        // 총알 설정 및 발사
        SetupAndFireBullet(pooledBullet, bulletData, Vector2.up);
    }

    /// <summary>
    /// CCW(반시계방향) 회전 총알 발사
    /// </summary>
    private void FireCCWBullet()
    {
        SFXManager.Instance.PlaySFX(Qsound);
        // 풀에서 CCW 총알 가져오기 (제네릭 방식)
        Bullet pooledBullet = PoolManager.Instance.Get(bulletPrefab);
        if (pooledBullet == null) return;

        // CCW 회전 방향으로 발사 
        Vector2 direction = Vector2.up;
        SetupAndFireRotationBullet(pooledBullet, CCWBulletData, direction);
    }

    /// <summary>
    /// CW(시계방향) 회전 총알 발사
    /// </summary>
    private void FireCWBullet()
    {
        SFXManager.Instance.PlaySFX(Esound);

        // 풀에서 CW 총알 가져오기 (제네릭 방식)
        Bullet pooledBullet = PoolManager.Instance.Get(bulletPrefab);
        if (pooledBullet == null) return;

        // CW 회전 방향으로 발사
        Vector2 direction = Vector2.up;
        SetupAndFireRotationBullet(pooledBullet, CWBulletData, direction);
    }


    /// <summary>
    /// 총알 기본 설정 및 발사 (공통 로직)
    /// </summary>
    private void SetupAndFireBullet(Bullet bullet, BulletData data, Vector2 direction)
    {
        if (bullet == null) return;

        // 위치 및 회전 설정
        bullet.transform.position = firePoint.position;
        bullet.transform.rotation = Quaternion.identity;

        // 총알 데이터 설정
        BulletData dataToUse = data != null ? data : bulletData;
        float damage = stat.GetStat(PlayerEnforcement.AutoDamage)[0];

        bullet.Setup(dataToUse, damage);
        bullet.Fire(direction, "Enemy");
    }

    /// <summary>
    /// 회전 총알 기본 설정 및 발사 (CCW/CW 전용)
    /// </summary>
    private void SetupAndFireRotationBullet<T>(T bullet, BulletData data, Vector2 direction) where T : Component
    {
        if (bullet == null) return;

        // 위치 및 회전 설정
        bullet.transform.position = firePoint.position;
        bullet.transform.rotation = Quaternion.identity;

        // 총알 데이터 설정
        BulletData dataToUse = data != null ? data : bulletData;
        float damage = stat.GetStat(PlayerEnforcement.AutoDamage)[0];

        // 타입에 따라 Setup 호출
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

    #region 유틸리티 메서드
    /// <summary>
    /// 모든 공격 타이머 초기화
    /// </summary>
    private void ResetAllTimers()
    {
        attackTimer = 0f;
        CCWAttackTimer = 0f;
        CWAttackTimer = 0f;
    }
    #endregion
}