using System;
using UnityEngine;

/// <summary>
///  �̺�Ʈ �Ŵ���: ���� �� ��� �̺�Ʈ�� �߾ӿ��� �����ϴ� �ý���
/// 
/// ����:
/// 1. �̺�Ʈ ����: EventManager.Instance.onPlayerHpChanged += �޼����;
/// 2. �̺�Ʈ �߻�: EventManager.Instance.PlayerHpChanged(��);
/// 3. ���� ����: EventManager.Instance.onPlayerHpChanged -= �޼����; (OnDestroy���� �ʼ�!)
/// 
/// ����: UI, ����, ����Ʈ ���� ���� ���������� ���� ����
/// ����: �ݵ�� OnDestroy���� ���� �����ϱ�! (�޸� ���� ����)
/// </summary>
public class EventManager : Singleton<EventManager>
{
    #region === �÷��̾� ���� �̺�Ʈ ===

    /// <summary>
    /// �÷��̾� HP�� ����� �� �߻��ϴ� �̺�Ʈ
    /// 
    /// ��� ����:
    /// - HP UI ������Ʈ
    /// - ü�� ���� �� ȭ�� ������
    /// - ü�� ȸ�� ����Ʈ
    /// 
    /// ���� ���:
    /// EventManager.Instance.onPlayerHpChanged += UpdateHPUI;
    /// 
    /// �߻� ���:
    /// EventManager.Instance.PlayerHpChanged(newHpValue);
    /// </summary>
    public event Action<int> onPlayerHpChanged;
    public event Action<Transform> onPlayerMove;

    /// <summary>
    /// �÷��̾� HP ���� �̺�Ʈ�� �߻���Ű�� �޼���
    /// </summary>
    /// <param name="newHp">���ο� HP ��</param>
    public void PlayerHpChanged(int newHp)
    {
        onPlayerHpChanged?.Invoke(newHp);
    }

    public void PlayerMove(Transform trans)
    {
        onPlayerMove?.Invoke(trans);
    }
    #endregion


    #region ===  ��Ʈ���� �̴ϰ��� �̺�Ʈ ===

    /// <summary>
    /// ��Ʈ���̳밡 ������ �� �߻��ϴ� �̺�Ʈ
    /// 
    /// ��� ����:
    /// - ���� ȿ���� ���
    /// - ���� ��� UI ������Ʈ
    /// - ��ƼŬ ����Ʈ
    /// 
    /// ���� ���:
    /// EventManager.Instance.onTetrominoSpawned += OnBlockSpawned;
    /// 
    /// �߻� ���:
    /// EventManager.Instance.TetrominoSpawned(tetrominoType);
    /// </summary>
    public event Action<Tetrimino.TetrominoType> onTetrominoSpawned;

    /// <summary>
    /// ��Ʈ���̳� ���� �̺�Ʈ �߻�
    /// </summary>
    /// <param name="tetrominoType">������ ��Ʈ���̳� Ÿ��</param>
    public void TetrominoSpawned(Tetrimino.TetrominoType tetrominoType)
    {
        //onTetrominoSpawned?.Invoke(tetrominoType);
        //Debug.Log($"[Event] ��Ʈ���̳� ����: {tetrominoType}");
    }

    /// <summary>
    /// ��Ʈ���̳밡 ����(����)�� �� �߻��ϴ� �̺�Ʈ
    /// 
    /// ��� ����:
    /// - ���� ȿ���� ���
    /// - ȭ�� ����
    /// - ���� ��� �غ�
    /// 
    /// ���� ���:
    /// EventManager.Instance.onTetrominoLocked += OnBlockLocked;
    /// 
    /// �߻� ���:
    /// EventManager.Instance.TetrominoLocked(tetrominoType, position);
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

    #region === ������� ���� �̺�Ʈ ===

    /// <summary>
    /// ���� ���� �̺�Ʈ - ���� UI ������Ʈ��
    /// 
    /// ��� ����:
    /// - ���� UI ������Ʈ
    /// - ���� ���� �ִϸ��̼�
    /// - �ְ� ���� üũ
    /// </summary>
    public event Action<int> onScoreChanged;

    /// <summary>
    /// ���� ���� �̺�Ʈ �߻�
    /// </summary>
    /// <param name="newScore">���ο� ����</param>
    public void ScoreChanged(int newScore)
    {
        onScoreChanged?.Invoke(newScore);
        Debug.Log($"[Event] ���� ����: {newScore}");
    }

    /// <summary>
    /// ���� ���� �̺�Ʈ - BGM ���, UI �ʱ�ȭ��
    /// 
    /// ��� ����:
    /// - ���� BGM ���
    /// - UI �ʱ�ȭ
    /// - Ÿ�̸� ����
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
    /// ���� ���� �̺�Ʈ - ��� ȭ��, ���� �����
    /// 
    /// ��� ����:
    /// - ���� ���� ���� ���
    /// - ��� ȭ�� ǥ��
    /// - ���� ����
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

    /// <summary>
    /// ���� �Ͻ����� �̺�Ʈ
    /// 
    /// ��� ����:
    /// - �Ͻ����� UI ǥ��
    /// - BGM �Ͻ�����
    /// - Ÿ�̸� ����
    /// </summary>
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
    /// ������ ���� �̺�Ʈ - �κ��丮, ȿ������
    /// 
    /// ��� ����:
    /// - ������ ���� ȿ����
    /// - �κ��丮 UI ������Ʈ
    /// - ������ ȿ�� ����
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