using System;
using System.Collections;
using UnityEngine;

public partial class Enemy : MonoBehaviour, IEntity
{
    private static readonly int ColorSwitch = Shader.PropertyToID("_Color_Switch");
    private static readonly int Dissolve = Shader.PropertyToID("_Dissolve");
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
    
    private Material instancedMat;


    public AudioClip boomSound;
    private void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        sprite = GetComponent<SpriteRenderer>();
        capsule = GetComponent<CapsuleCollider2D>();
        instancedMat = Instantiate(sprite.material);  // 인스턴스화
        sprite.material = instancedMat;        
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
        instancedMat.SetFloat(Dissolve, 0f);
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
        sprite.sprite = data.enemySprite;
        sprite.flipX = enemyType == EnemyType.Z;
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
        instancedMat.SetFloat(ColorSwitch, 1f);
        StartCoroutine(HitCoroutine());
    }

    private IEnumerator HitCoroutine()
    {
        yield return new WaitForSeconds(0.1f);
        instancedMat.SetFloat(ColorSwitch, 0f);
    }

    private void OnHpZero()
    {
        isInited = false;
        PoolManager.Instance.CreatePool(dieEffectPrefab);
        dieEffect = PoolManager.Instance.Get(dieEffectPrefab);
        dieEffect.SetParent(transform);
        dieEffect.localPosition = Vector3.zero;

        capsule.enabled = false;
        StartCoroutine(DissolveThenDie());
    }
    
    private IEnumerator DissolveThenDie()
    {
        float elapsed = 0f;
        float duration = 1.5f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration); // 0 ~ 1

            instancedMat.SetFloat("_Dissolve", t);

            yield return null;
        }

        instancedMat.SetFloat(Dissolve, 1f); // 확실히 1로 마무리
        Die();
    }

    private void Die()
    {
        GameManager.Instance.AddScore(100);
        SFXManager.Instance.PlaySFX(boomSound);

        OnDeath?.Invoke();
        dieEffect.SetParent(null);

        if (canBlock)
        {

            Vector3 spawnPos = GridSystem.GetGridBlockWorldPosition(transform.position);
            //spawnPos.y = transform.position.y;
            Instantiate(data.BlockPrefab, transform.position, Quaternion.identity);
        
        }
        PoolManager.Instance.Return(dieEffect);
        PoolManager.Instance.Return(this);
    }
}