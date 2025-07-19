using System;
using UnityEngine;

/// <summary>

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
    public event Action<Transform> onPlayerMove;
    public event Action<float> onPlayerBlockHit;

    /// <summary>
    /// 플레이어 HP 변경 이벤트를 발생시키는 메서드
    /// </summary>
    /// <param name="newHp">새로운 HP 값</param>
    public void PlayerHpChanged(int newHp)
    {
        onPlayerHpChanged?.Invoke(newHp);
    }
    
    public event Action<PlayerEnforcement> onPlayerEnforcementLevelUp;

    /// <summary>
    /// 플레이어 강화 레벨업 메서드
    /// </summary>
    /// <param name="enforcement">강화시킬 카테고리</param>
    public void PlayerEnforcementLevelUp(PlayerEnforcement enforcement)
    {
        onPlayerEnforcementLevelUp?.Invoke(enforcement);
    }

    public void PlayerMove(Transform trans)
    {
        onPlayerMove?.Invoke(trans);
    }

    public void PlayerBlockHit(float time)
    {
        onPlayerBlockHit?.Invoke(time);
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
    public event Action<Tetrimino.TetrominoType> onTetrominoSpawned;
    /// <summary>
    /// ��Ʈ���̳� ���� �̺�Ʈ �߻�
    /// </summary>
    /// <param name="tetrominoType">������ ��Ʈ���̳� Ÿ��</param>
    public void TetrominoSpawned(Tetrimino.TetrominoType tetrominoType)

    /// 보스 번개 QTE 이벤트를 발생시키는 메서드
    /// </summary>
    /// <param name="duration">QTE 제한 시간 (초)</param>
    public void BossLightningQTE(float duration)
    {
        //onTetrominoSpawned?.Invoke(tetrominoType);
        //Debug.Log($"[Event] ��Ʈ���̳� ����: {tetrominoType}");
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
    public event Action<Tetrimino.TetrominoType, Vector2Int> onTetrominoLocked;

    /// <summary>
    /// ��Ʈ���̳� ���� �̺�Ʈ �߻�
    /// </summary>
    /// <param name="tetrominoType">������ ��Ʈ���̳� Ÿ��</param>
    /// <param name="position">���� ��ġ</param>
    public void TetrominoLocked(Tetrimino.TetrominoType tetrominoType, Vector2Int position)

    {
        //onTetrominoLocked?.Invoke(tetrominoType, position);
        //Debug.Log($"[Event] ��Ʈ���̳� ����: {tetrominoType} at {position}");
    }

    /// <summary>
    /// ���� Ŭ���� �̺�Ʈ (Ŭ����� ���� ���� ��ġ ����)
    /// 
    /// ��� ����:
    /// - ���� Ŭ���� ȿ���� ���
    /// - ���� ���� �ִϸ��̼�
    /// - ��ƼŬ ����Ʈ
    /// - �޺� �ý���
    /// 
    /// ���� ���:
    /// EventManager.Instance.onLinesCleared += OnLinesCleared;
    /// 
    /// �߻� ���:
    /// EventManager.Instance.LinesCleared(2, new int[]{0, 1});
    /// </summary>
    public event Action<int, int[]> onLinesCleared;

    /// <summary>
    /// ���� Ŭ���� �̺�Ʈ �߻�
    /// </summary>
    /// <param name="lineCount">Ŭ����� ���� ��</param>
    /// <param name="clearedLines">Ŭ����� ���� ��ȣ �迭</param>
    public void LinesCleared(int lineCount, int[] clearedLines)
    {
        onLinesCleared?.Invoke(lineCount, clearedLines);
        Debug.Log($"[Event] ���� Ŭ����: {lineCount}�� ���� ({string.Join(", ", clearedLines)})");
    }

    /// <summary>
    /// ��Ʈ���� ������ �̺�Ʈ
    /// 
    /// ��� ����:
    /// - ������ ȿ���� ���
    /// - ���� UI ������Ʈ
    /// - �ӵ� ���� �˸�
    /// - ���� ����Ʈ
    /// 
    /// ���� ���:
    /// EventManager.Instance.onTetrisLevelUp += OnLevelUp;
    /// 
    /// �߻� ���:
    /// EventManager.Instance.TetrisLevelUp(3, 1.5f);
    /// </summary>
    public event Action<int, float> onTetrisLevelUp;

    /// <summary>
    /// ��Ʈ���� ������ �̺�Ʈ �߻�
    /// </summary>
    /// <param name="newLevel">���ο� ����</param>
    /// <param name="newSpeed">���ο� �ӵ�</param>
    public void TetrisLevelUp(int newLevel, float newSpeed)
    {
        onTetrisLevelUp?.Invoke(newLevel, newSpeed);
        Debug.Log($"[Event] ��Ʈ���� ������: Level {newLevel}, Speed {newSpeed}");
    }

    /// <summary>
    /// ��Ʈ���� ���� ���� �̺�Ʈ
    /// 
    /// ��� ����:
    /// - ���� ���� ���� ���
    /// - ��� ȭ�� ǥ��
    /// - �ְ� ���� Ȯ��
    /// - ����� ��ư Ȱ��ȭ
    /// 
    /// ���� ���:
    /// EventManager.Instance.onTetrisGameOver += OnGameOver;
    /// 
    /// �߻� ���:
    /// EventManager.Instance.TetrisGameOver(1500, 7, 25);
    /// </summary>
    public event Action<int, int, int> onTetrisGameOver;

    /// <summary>
    /// ��Ʈ���� ���� ���� �̺�Ʈ �߻�
    /// </summary>
    /// <param name="finalScore">���� ����</param>
    /// <param name="finalLevel">���� ����</param>
    /// <param name="linesCleared">�� Ŭ������ ���� ��</param>
    public void TetrisGameOver(int finalScore, int finalLevel, int linesCleared)
    {
        onTetrisGameOver?.Invoke(finalScore, finalLevel, linesCleared);
        Debug.Log($"[Event] ��Ʈ���� ���� ����: Score {finalScore}, Level {finalLevel}, Lines {linesCleared}");
    }

    /// <summary>
    /// ��Ʈ���� ���� ���� �̺�Ʈ
    /// 
    /// ��� ����:
    /// - ���� BGM ���
    /// - UI �ʱ�ȭ
    /// - Ÿ�̸� ����
    /// - ī�޶� ����
    /// 
    /// ���� ���:
    /// EventManager.Instance.onTetrisGameStart += OnGameStart;
    /// 
    /// �߻� ���:
    /// EventManager.Instance.TetrisGameStart();
    /// </summary>
    public event Action onTetrisGameStart;

    /// <summary>
    /// ��Ʈ���� ���� ���� �̺�Ʈ �߻�
    /// </summary>
    public void TetrisGameStart()
    {
        onTetrisGameStart?.Invoke();
        Debug.Log("[Event] ��Ʈ���� ���� ����");
    }

    /// <summary>
    /// ��Ʈ���� �޺� �̺�Ʈ (���� ���� Ŭ����)
    /// 
    /// ��� ����:
    /// - �޺� ȿ���� ���
    /// - �޺� UI ǥ��
    /// - �߰� ���� �ο�
    /// - ȭ�� ����Ʈ
    /// 
    /// ���� ���:
    /// EventManager.Instance.onTetrisCombo += OnCombo;
    /// 
    /// �߻� ���:
    /// EventManager.Instance.TetrisCombo(3, 500);
    /// </summary>
    public event Action<int, int> onTetrisCombo;

    /// <summary>
    /// ��Ʈ���� �޺� �̺�Ʈ �߻�
    /// </summary>
    /// <param name="comboCount">�޺� ��</param>
    /// <param name="bonusScore">���ʽ� ����</param>
    public void TetrisCombo(int comboCount, int bonusScore)
    {
        onTetrisCombo?.Invoke(comboCount, bonusScore);
        Debug.Log($"[Event] ��Ʈ���� �޺�: {comboCount}�޺�, ���ʽ� {bonusScore}��");
    }

    #endregion


    #region 게임잼용 추가 이벤트 (필요시 주석 해제하고 사용)

    /// <summary>

    /// 점수 변경 이벤트 - 점수 UI 업데이트용
    /// </summary>
    public event Action<int> onScoreChanged;

    /// <summary>
    /// 적 처치 이벤트 - 킬 카운트, 콤보 시스템용
    /// </summary>
    /// <param name="newScore">���ο� ����</param>
    public void ScoreChanged(int newScore)
    {
        onScoreChanged?.Invoke(newScore);
        Debug.Log($"[Event] ���� ����: {newScore}");
    }

    /// <summary>

    /// 게임 시작 이벤트 - BGM 재생, UI 초기화용
    /// </summary>
    public event Action onGameStart;

    /// <summary>
    /// ���� ���� �̺�Ʈ �߻�
    /// </summary>
    public void GameStart()
    {
        onGameStart?.Invoke();
        Debug.Log("[Event] ���� ����");
    }

    /// <summary>

    /// 게임 오버 이벤트 - 결과 화면, 점수 저장용
    /// </summary>
    public event Action onGameOver;

    /// <summary>
    /// ���� ���� �̺�Ʈ �߻�
    /// </summary>
    public void GameOver()
    {
        onGameOver?.Invoke();
        Debug.Log("[Event] ���� ����");
    }

    public event Action<bool> onGamePaused;

    /// <summary>
    /// ���� �Ͻ����� �̺�Ʈ �߻�
    /// </summary>
    /// <param name="isPaused">�Ͻ����� ����</param>
    public void GamePaused(bool isPaused)
    {
        onGamePaused?.Invoke(isPaused);
        Debug.Log($"[Event] ���� �Ͻ�����: {isPaused}");
    }

    /// <summary>
    /// 아이템 수집 이벤트 - 인벤토리, 효과음용

    /// </summary>
    public event Action<string, int> onItemCollected;

    /// <summary>
    /// ������ ���� �̺�Ʈ �߻�
    /// </summary>
    /// <param name="itemName">������ �̸�</param>
    /// <param name="amount">����</param>
    public void ItemCollected(string itemName, int amount = 1)
    {
        onItemCollected?.Invoke(itemName, amount);
        Debug.Log($"[Event] ������ ����: {itemName} x{amount}");
    }

    #endregion

    #region === ����� �̺�Ʈ ===

    /// <summary>
    /// ȿ���� ��� �̺�Ʈ
    /// 
    /// ��� ����:
    /// - ��ư Ŭ����
    /// - �׼� ȿ����
    /// - �˸���
    /// </summary>
    public event Action<string> onPlaySFX;

    /// <summary>
    /// ȿ���� ��� �̺�Ʈ �߻�
    /// </summary>
    /// <param name="sfxName">ȿ���� �̸�</param>
    public void PlaySFX(string sfxName)
    {
        onPlaySFX?.Invoke(sfxName);
        Debug.Log($"[Event] ȿ���� ���: {sfxName}");
    }

    /// <summary>
    /// BGM ���� �̺�Ʈ
    /// 
    /// ��� ����:
    /// - ���� BGM ���
    /// - ���� BGM ���
    /// - ��� ȭ�� BGM ���
    /// </summary>
    public event Action<string> onChangeBGM;

    /// <summary>
    /// BGM ���� �̺�Ʈ �߻�
    /// </summary>
    /// <param name="bgmName">BGM �̸�</param>
    public void ChangeBGM(string bgmName)
    {
        onChangeBGM?.Invoke(bgmName);
        Debug.Log($"[Event] BGM ����: {bgmName}");
    }

    #endregion

    #region ===  UI �̺�Ʈ ===

    /// <summary>
    /// UI ������Ʈ �̺�Ʈ
    /// 
    /// ��� ����:
    /// - ü�¹� ������Ʈ
    /// - ���� UI ������Ʈ
    /// - ���� UI ������Ʈ
    /// </summary>
    public event Action<string, object> onUIUpdate;

    /// <summary>
    /// UI ������Ʈ �̺�Ʈ �߻�
    /// </summary>
    /// <param name="uiElement">UI ��� �̸�</param>
    /// <param name="value">���ο� ��</param>
    public void UIUpdate(string uiElement, object value)
    {
        onUIUpdate?.Invoke(uiElement, value);
        Debug.Log($"[Event] UI ������Ʈ: {uiElement} = {value}");
    }

    /// <summary>
    /// ȭ�� ȿ�� �̺�Ʈ
    /// 
    /// ��� ����:
    /// - ȭ�� ����ŷ
    /// - ȭ�� �÷���
    /// - ���̵� ��/�ƿ�
    /// </summary>
    public event Action<string, float> onScreenEffect;

    /// <summary>
    /// ȭ�� ȿ�� �̺�Ʈ �߻�
    /// </summary>
    /// <param name="effectName">ȿ�� �̸�</param>
    /// <param name="intensity">����</param>
    public void ScreenEffect(string effectName, float intensity)
    {
        onScreenEffect?.Invoke(effectName, intensity);
        Debug.Log($"[Event] ȭ�� ȿ��: {effectName} (����: {intensity})");
    }

    #endregion

    #region === ����� ��� ===

    /// <summary>
    /// ��� �̺�Ʈ ������ �� ��� (����׿�)
    /// </summary>
    [ContextMenu("�̺�Ʈ ������ �� ���")]
    public void PrintEventSubscribers()
    {
        Debug.Log("=== EventManager ������ �� ===");
        Debug.Log($"��Ʈ���̳� ����: {onTetrominoSpawned?.GetInvocationList().Length ?? 0}");
        Debug.Log($"��Ʈ���̳� ����: {onTetrominoLocked?.GetInvocationList().Length ?? 0}");
        Debug.Log($"���� Ŭ����: {onLinesCleared?.GetInvocationList().Length ?? 0}");
        Debug.Log($"������: {onTetrisLevelUp?.GetInvocationList().Length ?? 0}");
        Debug.Log($"���� ����: {onTetrisGameOver?.GetInvocationList().Length ?? 0}");
        Debug.Log($"���� ����: {onScoreChanged?.GetInvocationList().Length ?? 0}");
    }

    /// <summary>
    /// �׽�Ʈ �̺�Ʈ �߻� (����׿�)
    /// </summary>
    [ContextMenu("�׽�Ʈ �̺�Ʈ �߻�")]
    public void FireTestEvents()
    {
        Debug.Log("=== �׽�Ʈ �̺�Ʈ �߻� ===");
        TetrominoSpawned(Tetrimino.TetrominoType.T);
        ScoreChanged(1000);
        PlaySFX("TestSound");
        Debug.Log("�׽�Ʈ �̺�Ʈ �߻� �Ϸ�");
    }

    #endregion
}