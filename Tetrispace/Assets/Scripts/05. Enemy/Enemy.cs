using System;
using System.Collections;
using UnityEngine;

public partial class Enemy : MonoBehaviour, IEntity
{
    public event Action OnDeath;
    [SerializeField] private Bullet bulletPrefab;
    [SerializeField] private EnemyData data;
    [SerializeField] private EnemyType type;
    [SerializeField] private Transform dieEffectPrefab;
    private Transform dieEffect;
    private float hp;
    private IEnemyMovePattern movePattern;
    private bool isInited;
    private bool isStarted;
    private Vector2 startPosition;
    private bool canBlock;
    
    private Rigidbody2D rigid;
    private SpriteRenderer sprite;
    private CapsuleCollider2D capsule;
    [SerializeField] private float deathBlinkDuration = 1f;
    [SerializeField] private float deathBlinkInterval = 0.1f;


    private void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        sprite = GetComponent<SpriteRenderer>();
        capsule = GetComponent<CapsuleCollider2D>();
    }

    private void FixedUpdate()
    {
        if (!isInited) return;

        if (!isStarted)
        {
            MoveToStartPos();
        }
        else
        {
            movePattern?.Tick(Time.fixedDeltaTime);
        }
    }

    private void MoveToStartPos()
    {
        Vector2 currentPos = rigid.position;
        Vector2 targetPos = startPosition;

        Vector2 dir = (targetPos - currentPos).normalized;
        float dist = Vector2.Distance(currentPos, targetPos);
        float step = data.speed * Time.fixedDeltaTime;

        if (dist < step)
        {
            isStarted = true;
            rigid.MovePosition(targetPos);
            StartCoroutine(AutoFire());
        }
        else
        {
            rigid.MovePosition(currentPos + dir * step);
        }
    }

    private void OnEnable()
    {
        capsule.enabled = true;
    }

    private void OnDisable()
    {
        isInited = false;
        isStarted = false;
        StopAllCoroutines();
    }

    public void Init(Vector2 startPos, EnemyType enemyType, bool canTetris)
    {
        isInited = true;
        isStarted = false;
        startPosition = startPos;
        type = enemyType;
        canBlock = canTetris;
        data = DataManager.Instance.GetEnemyData(enemyType);
        hp = data.hp;
        movePattern = EnemyMovePatternFactory.Create(enemyType);
        movePattern.Init(startPos, data.speed, rigid);
        if (data.isEnforced) { ChangeEnforcedSprite(); }
    }

    private void ChangeEnforcedSprite()
    {
        // 강화 적 스프라이트 바꾸는 로직
    }

    public void OnHit(float damage, Vector2 hitPosition)
    {
        if (!isInited) return;
        
        hp -= damage;
        if (hp <= 0)
        {
            OnHpZero();
        }
    }

    private void OnHpZero()
    {
        isInited = false;
        PoolManager.Instance.CreatePool(dieEffectPrefab);
        dieEffect = PoolManager.Instance.Get(dieEffectPrefab);
        dieEffect.SetParent(transform);
        dieEffect.localPosition = Vector3.zero;

        capsule.enabled = false;
        StartCoroutine(BlinkThenDie());
    }
    
    private IEnumerator BlinkThenDie()
    {
        float elapsed = 0f;
        bool visible = true;

        while (elapsed < deathBlinkDuration)
        {
            visible = !visible;
            sprite.enabled = visible;

            yield return new WaitForSeconds(deathBlinkInterval);
            elapsed += deathBlinkInterval;
        }
        sprite.enabled = true; // 마지막엔 켜진 상태로
        Die();
    }

    private void Die()
    {
        OnDeath?.Invoke();
        dieEffect.SetParent(null);
        if (canBlock)
        {
            Vector2Int adjGrid = GridSystem.WorldToGridIndex(transform.position);
            Vector3 spawnPos = GridSystem.GetGridMiddleWorldPosition(adjGrid.x, adjGrid.y);
            spawnPos.y = transform.position.y;
            Instantiate(data.BlockPrefab, spawnPos, Quaternion.identity);
        }
        PoolManager.Instance.Return(dieEffect);
        PoolManager.Instance.Return(this);
    }
}