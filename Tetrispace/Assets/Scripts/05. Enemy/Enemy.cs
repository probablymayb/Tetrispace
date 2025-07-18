using UnityEngine;

public partial class Enemy : MonoBehaviour
{
    [SerializeField] private Bullet bulletPrefab;
    [SerializeField] private EnemyData data;
    [SerializeField] private EnemyType type;
    private IEnemyMovePattern movePattern;
    private bool isInited = false;
    private bool isStarted = false;
    private Vector2 startPosition;
    
    private Rigidbody2D rigid;

    private void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
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

    private void OnDisable()
    {
        isInited = false;
        isStarted = false;
        StopAllCoroutines();
    }

    public void Init(Vector2 startPos, EnemyType enemyType)
    {
        isInited = true;
        isStarted = false;
        startPosition = startPos;
        type = enemyType;
        data = DataManager.Instance.GetEnemyData(enemyType);
        movePattern = EnemyMovePatternFactory.Create(enemyType);
        movePattern.Init(startPos, data.speed, rigid);
        if (data.isEnforced) { ChangeEnforcedSprite(); }
    }

    private void ChangeEnforcedSprite()
    {
        // 강화 적 스프라이트 바꾸는 로직
    }
}