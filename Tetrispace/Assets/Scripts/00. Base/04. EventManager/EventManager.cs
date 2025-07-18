using System;

/// <summary>
/// 이벤트 매니저: 게임 내 모든 이벤트를 중앙에서 관리하는 시스템
/// 
/// 사용법:
/// 1. 이벤트 구독: EventManager.Instance.onPlayerHpChanged += 메서드명;
/// 2. 이벤트 발생: EventManager.Instance.PlayerHpChanged(값);
/// 3. 구독 해제: EventManager.Instance.onPlayerHpChanged -= 메서드명; (OnDestroy에서 필수!)
/// 
/// 장점: UI, 사운드, 이펙트 등이 서로 독립적으로 반응 가능
///  주의: 반드시 OnDestroy에서 구독 해제하기! (메모리 누수 방지)
/// </summary>
public class EventManager : Singleton<EventManager>
{
    #region 플레이어 관련 이벤트

    /// <summary>
    /// 플레이어 HP가 변경될 때 발생하는 이벤트
    /// 
    ///사용 예시:
    /// - HP UI 업데이트
    /// - 체력 낮을 때 화면 빨갛게
    /// - 체력 회복 이펙트
    /// 
    ///  구독 방법:
    /// EventManager.Instance.onPlayerHpChanged += UpdateHPUI;
    /// 
    ///  발생 방법:
    /// EventManager.Instance.PlayerHpChanged(newHpValue);
    /// </summary>
    public event Action<int> onPlayerHpChanged;

    /// <summary>
    /// 플레이어 HP 변경 이벤트를 발생시키는 메서드
    /// </summary>
    /// <param name="newHp">새로운 HP 값</param>
    public void PlayerHpChanged(int newHp)
    {
        onPlayerHpChanged?.Invoke(newHp);
    }

    #endregion

    #region 보스 관련 이벤트

    /// <summary>
    /// 보스의 번개 QTE 이벤트가 시작될 때 발생
    /// 
    /// 사용 예시:
    /// - QTE UI 표시
    /// - 카메라 셰이킹
    /// - 긴박한 음악 재생
    /// 
    /// 구독 방법:
    /// EventManager.Instance.onBossLightningQTE += StartQTE;
    /// 
    /// 발생 방법:
    /// EventManager.Instance.BossLightningQTE(2.0f); // 2초 제한
    /// </summary>
    public event Action<float> onBossLightningQTE;

    /// <summary>
    /// 보스 번개 QTE 이벤트를 발생시키는 메서드
    /// </summary>
    /// <param name="duration">QTE 제한 시간 (초)</param>
    public void BossLightningQTE(float duration)
    {
        onBossLightningQTE?.Invoke(duration);
    }

    /// <summary>
    /// QTE 완료 시 발생하는 이벤트 (성공/실패 결과 포함)
    /// 
    /// 사용 예시:
    /// - 성공시: 보스 스턴, 추가 점수
    /// - 실패시: 플레이어 데미지, 보스 강화
    /// 
    /// 구독 방법:
    /// EventManager.Instance.onQTECompleted += OnQTEResult;
    /// 
    /// 발생 방법:
    /// EventManager.Instance.QTECompleted(true);  // 성공
    /// EventManager.Instance.QTECompleted(false); // 실패
    /// </summary>
    public event Action<bool> onQTECompleted;

    /// <summary>
    /// QTE 완료 이벤트를 발생시키는 메서드
    /// </summary>
    /// <param name="isSuccess">QTE 성공 여부 (true: 성공, false: 실패)</param>
    public void QTECompleted(bool isSuccess)
    {
        onQTECompleted?.Invoke(isSuccess);
    }

    #endregion

    #region 게임잼용 추가 이벤트 (필요시 주석 해제하고 사용)

    /*
    /// <summary>
    /// 점수 변경 이벤트 - 점수 UI 업데이트용
    /// </summary>
    public event Action<int> onScoreChanged;
    public void ScoreChanged(int newScore) { onScoreChanged?.Invoke(newScore); }
    
    /// <summary>
    /// 적 처치 이벤트 - 킬 카운트, 콤보 시스템용
    /// </summary>
    public event Action<int> onEnemyKilled;
    public void EnemyKilled(int killCount) { onEnemyKilled?.Invoke(killCount); }
    
    /// <summary>
    /// 게임 시작 이벤트 - BGM 재생, UI 초기화용
    /// </summary>
    public event Action onGameStart;
    public void GameStart() { onGameStart?.Invoke(); }
    
    /// <summary>
    /// 게임 오버 이벤트 - 결과 화면, 점수 저장용
    /// </summary>
    public event Action onGameOver;
    public void GameOver() { onGameOver?.Invoke(); }
    
    /// <summary>
    /// 아이템 수집 이벤트 - 인벤토리, 효과음용
    /// </summary>
    public event Action<string> onItemCollected;
    public void ItemCollected(string itemName) { onItemCollected?.Invoke(itemName); }
    */

    #endregion
}