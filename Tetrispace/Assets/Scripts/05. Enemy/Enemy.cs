using System;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] private EnemyData data;
    private IEnemyMovePattern movePattern;
    private bool isInited = false;
    private bool isStarted = false;
    private Vector2 startPosition;
    
    private Rigidbody2D rigid;
    // private SpriteRenderer sprite;

    private void Awake()
    {
        // sprite = GetComponent<SpriteRenderer>();
        rigid = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        // 아직 시작 위치에 도착안함
        if (isInited)
        {
            if (!isStarted)
            {
                float dist = Vector2.Distance(transform.position, startPosition);
                const float distThreshold = 0.1f;
                // 시작 위치 도착
                if (dist < distThreshold)
                {
                    isStarted = true;
                    rigid.linearVelocity = Vector2.zero;
                }
            }
            else
            {
                movePattern?.Tick(Time.fixedDeltaTime);
            }
        }
    }

    private void OnDisable()
    {
        isInited = false;
        isStarted = false;
    }

    public void Init(Vector2 startPos, EnemyType type)
    {
        print(Enum.GetName(typeof(EnemyType), type));
        isInited = true;
        isStarted = false;
        
        startPosition = startPos;
        data = DataManager.Instance.GetEnemyData(type);
        movePattern = EnemyMovePatternFactory.Create(type);
        movePattern.Init(startPos, data.speed, rigid);
        if (data.isEnforced) { ChangeEnforcedSprite(); }
        
        Vector2 dir = startPosition - (Vector2)transform.position;
        rigid.linearVelocity = dir * data.speed * Time.fixedDeltaTime;
    }

    private void ChangeEnforcedSprite()
    {
        // 강화 적 스프라이트 컬러 바꾸거나.. 암튼 그런거
    }
}