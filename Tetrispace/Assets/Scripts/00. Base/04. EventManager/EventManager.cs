using System;
using UnityEngine;

/// <summary>
///  이벤트 매니저: 게임 내 모든 이벤트를 중앙에서 관리하는 시스템
/// 
/// 사용법:
/// 1. 이벤트 구독: EventManager.Instance.onPlayerHpChanged += 메서드명;
/// 2. 이벤트 발생: EventManager.Instance.PlayerHpChanged(값);
/// 3. 구독 해제: EventManager.Instance.onPlayerHpChanged -= 메서드명; (OnDestroy에서 필수!)
/// 
/// 장점: UI, 사운드, 이펙트 등이 서로 독립적으로 반응 가능
/// 주의: 반드시 OnDestroy에서 구독 해제하기! (메모리 누수 방지)
/// </summary>
public class EventManager : Singleton<EventManager>
{
    #region === 플레이어 관련 이벤트 ===

    /// <summary>
    /// 플레이어 HP가 변경될 때 발생하는 이벤트
    /// 
    /// 사용 예시:
    /// - HP UI 업데이트
    /// - 체력 낮을 때 화면 빨갛게
    /// - 체력 회복 이펙트
    /// 
    /// 구독 방법:
    /// EventManager.Instance.onPlayerHpChanged += UpdateHPUI;
    /// 
    /// 발생 방법:
    /// EventManager.Instance.PlayerHpChanged(newHpValue);
    /// </summary>
    public event Action<int> onPlayerHpChanged;
    public event Action<Transform> onPlayerMove;

    /// <summary>
    /// 플레이어 HP 변경 이벤트를 발생시키는 메서드
    /// </summary>
    /// <param name="newHp">새로운 HP 값</param>
    public void PlayerHpChanged(int newHp)
    {
        onPlayerHpChanged?.Invoke(newHp);
    }

    public void PlayerMove(Transform trans)
    {
        onPlayerMove?.Invoke(trans);
    }
    #endregion


    #region ===  테트리스 미니게임 이벤트 ===

    /// <summary>
    /// 테트리미노가 생성될 때 발생하는 이벤트
    /// 
    /// 사용 예시:
    /// - 생성 효과음 재생
    /// - 다음 블록 UI 업데이트
    /// - 파티클 이펙트
    /// 
    /// 구독 방법:
    /// EventManager.Instance.onTetrominoSpawned += OnBlockSpawned;
    /// 
    /// 발생 방법:
    /// EventManager.Instance.TetrominoSpawned(tetrominoType);
    /// </summary>
    public event Action<Tetrimino.TetrominoType> onTetrominoSpawned;

    /// <summary>
    /// 테트리미노 생성 이벤트 발생
    /// </summary>
    /// <param name="tetrominoType">생성된 테트리미노 타입</param>
    public void TetrominoSpawned(Tetrimino.TetrominoType tetrominoType)
    {
        //onTetrominoSpawned?.Invoke(tetrominoType);
        //Debug.Log($"[Event] 테트리미노 생성: {tetrominoType}");
    }

    /// <summary>
    /// 테트리미노가 착지(고정)될 때 발생하는 이벤트
    /// 
    /// 사용 예시:
    /// - 착지 효과음 재생
    /// - 화면 진동
    /// - 다음 블록 준비
    /// 
    /// 구독 방법:
    /// EventManager.Instance.onTetrominoLocked += OnBlockLocked;
    /// 
    /// 발생 방법:
    /// EventManager.Instance.TetrominoLocked(tetrominoType, position);
    /// </summary>
    public event Action<Tetrimino.TetrominoType, Vector2Int> onTetrominoLocked;

    /// <summary>
    /// 테트리미노 착지 이벤트 발생
    /// </summary>
    /// <param name="tetrominoType">착지된 테트리미노 타입</param>
    /// <param name="position">착지 위치</param>
    public void TetrominoLocked(Tetrimino.TetrominoType tetrominoType, Vector2Int position)
    {
        //onTetrominoLocked?.Invoke(tetrominoType, position);
        //Debug.Log($"[Event] 테트리미노 착지: {tetrominoType} at {position}");
    }

    /// <summary>
    /// 라인 클리어 이벤트 (클리어된 라인 수와 위치 정보)
    /// 
    /// 사용 예시:
    /// - 라인 클리어 효과음 재생
    /// - 점수 증가 애니메이션
    /// - 파티클 이펙트
    /// - 콤보 시스템
    /// 
    /// 구독 방법:
    /// EventManager.Instance.onLinesCleared += OnLinesCleared;
    /// 
    /// 발생 방법:
    /// EventManager.Instance.LinesCleared(2, new int[]{0, 1});
    /// </summary>
    public event Action<int, int[]> onLinesCleared;

    /// <summary>
    /// 라인 클리어 이벤트 발생
    /// </summary>
    /// <param name="lineCount">클리어된 라인 수</param>
    /// <param name="clearedLines">클리어된 라인 번호 배열</param>
    public void LinesCleared(int lineCount, int[] clearedLines)
    {
        onLinesCleared?.Invoke(lineCount, clearedLines);
        Debug.Log($"[Event] 라인 클리어: {lineCount}개 라인 ({string.Join(", ", clearedLines)})");
    }

    /// <summary>
    /// 테트리스 레벨업 이벤트
    /// 
    /// 사용 예시:
    /// - 레벨업 효과음 재생
    /// - 레벨 UI 업데이트
    /// - 속도 증가 알림
    /// - 축하 이펙트
    /// 
    /// 구독 방법:
    /// EventManager.Instance.onTetrisLevelUp += OnLevelUp;
    /// 
    /// 발생 방법:
    /// EventManager.Instance.TetrisLevelUp(3, 1.5f);
    /// </summary>
    public event Action<int, float> onTetrisLevelUp;

    /// <summary>
    /// 테트리스 레벨업 이벤트 발생
    /// </summary>
    /// <param name="newLevel">새로운 레벨</param>
    /// <param name="newSpeed">새로운 속도</param>
    public void TetrisLevelUp(int newLevel, float newSpeed)
    {
        onTetrisLevelUp?.Invoke(newLevel, newSpeed);
        Debug.Log($"[Event] 테트리스 레벨업: Level {newLevel}, Speed {newSpeed}");
    }

    /// <summary>
    /// 테트리스 게임 오버 이벤트
    /// 
    /// 사용 예시:
    /// - 게임 오버 사운드 재생
    /// - 결과 화면 표시
    /// - 최고 점수 확인
    /// - 재시작 버튼 활성화
    /// 
    /// 구독 방법:
    /// EventManager.Instance.onTetrisGameOver += OnGameOver;
    /// 
    /// 발생 방법:
    /// EventManager.Instance.TetrisGameOver(1500, 7, 25);
    /// </summary>
    public event Action<int, int, int> onTetrisGameOver;

    /// <summary>
    /// 테트리스 게임 오버 이벤트 발생
    /// </summary>
    /// <param name="finalScore">최종 점수</param>
    /// <param name="finalLevel">최종 레벨</param>
    /// <param name="linesCleared">총 클리어한 라인 수</param>
    public void TetrisGameOver(int finalScore, int finalLevel, int linesCleared)
    {
        onTetrisGameOver?.Invoke(finalScore, finalLevel, linesCleared);
        Debug.Log($"[Event] 테트리스 게임 오버: Score {finalScore}, Level {finalLevel}, Lines {linesCleared}");
    }

    /// <summary>
    /// 테트리스 게임 시작 이벤트
    /// 
    /// 사용 예시:
    /// - 게임 BGM 재생
    /// - UI 초기화
    /// - 타이머 시작
    /// - 카메라 설정
    /// 
    /// 구독 방법:
    /// EventManager.Instance.onTetrisGameStart += OnGameStart;
    /// 
    /// 발생 방법:
    /// EventManager.Instance.TetrisGameStart();
    /// </summary>
    public event Action onTetrisGameStart;

    /// <summary>
    /// 테트리스 게임 시작 이벤트 발생
    /// </summary>
    public void TetrisGameStart()
    {
        onTetrisGameStart?.Invoke();
        Debug.Log("[Event] 테트리스 게임 시작");
    }

    /// <summary>
    /// 테트리스 콤보 이벤트 (연속 라인 클리어)
    /// 
    /// 사용 예시:
    /// - 콤보 효과음 재생
    /// - 콤보 UI 표시
    /// - 추가 점수 부여
    /// - 화면 이펙트
    /// 
    /// 구독 방법:
    /// EventManager.Instance.onTetrisCombo += OnCombo;
    /// 
    /// 발생 방법:
    /// EventManager.Instance.TetrisCombo(3, 500);
    /// </summary>
    public event Action<int, int> onTetrisCombo;

    /// <summary>
    /// 테트리스 콤보 이벤트 발생
    /// </summary>
    /// <param name="comboCount">콤보 수</param>
    /// <param name="bonusScore">보너스 점수</param>
    public void TetrisCombo(int comboCount, int bonusScore)
    {
        onTetrisCombo?.Invoke(comboCount, bonusScore);
        Debug.Log($"[Event] 테트리스 콤보: {comboCount}콤보, 보너스 {bonusScore}점");
    }

    #endregion

    #region === 게임잼용 범용 이벤트 ===

    /// <summary>
    /// 점수 변경 이벤트 - 점수 UI 업데이트용
    /// 
    /// 사용 예시:
    /// - 점수 UI 업데이트
    /// - 점수 증가 애니메이션
    /// - 최고 점수 체크
    /// </summary>
    public event Action<int> onScoreChanged;

    /// <summary>
    /// 점수 변경 이벤트 발생
    /// </summary>
    /// <param name="newScore">새로운 점수</param>
    public void ScoreChanged(int newScore)
    {
        onScoreChanged?.Invoke(newScore);
        Debug.Log($"[Event] 점수 변경: {newScore}");
    }

    /// <summary>
    /// 게임 시작 이벤트 - BGM 재생, UI 초기화용
    /// 
    /// 사용 예시:
    /// - 게임 BGM 재생
    /// - UI 초기화
    /// - 타이머 시작
    /// </summary>
    public event Action onGameStart;

    /// <summary>
    /// 게임 시작 이벤트 발생
    /// </summary>
    public void GameStart()
    {
        onGameStart?.Invoke();
        Debug.Log("[Event] 게임 시작");
    }

    /// <summary>
    /// 게임 오버 이벤트 - 결과 화면, 점수 저장용
    /// 
    /// 사용 예시:
    /// - 게임 오버 사운드 재생
    /// - 결과 화면 표시
    /// - 점수 저장
    /// </summary>
    public event Action onGameOver;

    /// <summary>
    /// 게임 오버 이벤트 발생
    /// </summary>
    public void GameOver()
    {
        onGameOver?.Invoke();
        Debug.Log("[Event] 게임 오버");
    }

    /// <summary>
    /// 게임 일시정지 이벤트
    /// 
    /// 사용 예시:
    /// - 일시정지 UI 표시
    /// - BGM 일시정지
    /// - 타이머 정지
    /// </summary>
    public event Action<bool> onGamePaused;

    /// <summary>
    /// 게임 일시정지 이벤트 발생
    /// </summary>
    /// <param name="isPaused">일시정지 상태</param>
    public void GamePaused(bool isPaused)
    {
        onGamePaused?.Invoke(isPaused);
        Debug.Log($"[Event] 게임 일시정지: {isPaused}");
    }

    /// <summary>
    /// 아이템 수집 이벤트 - 인벤토리, 효과음용
    /// 
    /// 사용 예시:
    /// - 아이템 수집 효과음
    /// - 인벤토리 UI 업데이트
    /// - 아이템 효과 적용
    /// </summary>
    public event Action<string, int> onItemCollected;

    /// <summary>
    /// 아이템 수집 이벤트 발생
    /// </summary>
    /// <param name="itemName">아이템 이름</param>
    /// <param name="amount">수량</param>
    public void ItemCollected(string itemName, int amount = 1)
    {
        onItemCollected?.Invoke(itemName, amount);
        Debug.Log($"[Event] 아이템 수집: {itemName} x{amount}");
    }

    #endregion

    #region === 오디오 이벤트 ===

    /// <summary>
    /// 효과음 재생 이벤트
    /// 
    /// 사용 예시:
    /// - 버튼 클릭음
    /// - 액션 효과음
    /// - 알림음
    /// </summary>
    public event Action<string> onPlaySFX;

    /// <summary>
    /// 효과음 재생 이벤트 발생
    /// </summary>
    /// <param name="sfxName">효과음 이름</param>
    public void PlaySFX(string sfxName)
    {
        onPlaySFX?.Invoke(sfxName);
        Debug.Log($"[Event] 효과음 재생: {sfxName}");
    }

    /// <summary>
    /// BGM 변경 이벤트
    /// 
    /// 사용 예시:
    /// - 메인 BGM 재생
    /// - 보스 BGM 재생
    /// - 결과 화면 BGM 재생
    /// </summary>
    public event Action<string> onChangeBGM;

    /// <summary>
    /// BGM 변경 이벤트 발생
    /// </summary>
    /// <param name="bgmName">BGM 이름</param>
    public void ChangeBGM(string bgmName)
    {
        onChangeBGM?.Invoke(bgmName);
        Debug.Log($"[Event] BGM 변경: {bgmName}");
    }

    #endregion

    #region ===  UI 이벤트 ===

    /// <summary>
    /// UI 업데이트 이벤트
    /// 
    /// 사용 예시:
    /// - 체력바 업데이트
    /// - 점수 UI 업데이트
    /// - 레벨 UI 업데이트
    /// </summary>
    public event Action<string, object> onUIUpdate;

    /// <summary>
    /// UI 업데이트 이벤트 발생
    /// </summary>
    /// <param name="uiElement">UI 요소 이름</param>
    /// <param name="value">새로운 값</param>
    public void UIUpdate(string uiElement, object value)
    {
        onUIUpdate?.Invoke(uiElement, value);
        Debug.Log($"[Event] UI 업데이트: {uiElement} = {value}");
    }

    /// <summary>
    /// 화면 효과 이벤트
    /// 
    /// 사용 예시:
    /// - 화면 셰이킹
    /// - 화면 플래시
    /// - 페이드 인/아웃
    /// </summary>
    public event Action<string, float> onScreenEffect;

    /// <summary>
    /// 화면 효과 이벤트 발생
    /// </summary>
    /// <param name="effectName">효과 이름</param>
    /// <param name="intensity">강도</param>
    public void ScreenEffect(string effectName, float intensity)
    {
        onScreenEffect?.Invoke(effectName, intensity);
        Debug.Log($"[Event] 화면 효과: {effectName} (강도: {intensity})");
    }

    #endregion

    #region === 디버그 기능 ===

    /// <summary>
    /// 모든 이벤트 구독자 수 출력 (디버그용)
    /// </summary>
    [ContextMenu("이벤트 구독자 수 출력")]
    public void PrintEventSubscribers()
    {
        Debug.Log("=== EventManager 구독자 수 ===");
        Debug.Log($"테트리미노 생성: {onTetrominoSpawned?.GetInvocationList().Length ?? 0}");
        Debug.Log($"테트리미노 착지: {onTetrominoLocked?.GetInvocationList().Length ?? 0}");
        Debug.Log($"라인 클리어: {onLinesCleared?.GetInvocationList().Length ?? 0}");
        Debug.Log($"레벨업: {onTetrisLevelUp?.GetInvocationList().Length ?? 0}");
        Debug.Log($"게임 오버: {onTetrisGameOver?.GetInvocationList().Length ?? 0}");
        Debug.Log($"점수 변경: {onScoreChanged?.GetInvocationList().Length ?? 0}");
    }

    /// <summary>
    /// 테스트 이벤트 발생 (디버그용)
    /// </summary>
    [ContextMenu("테스트 이벤트 발생")]
    public void FireTestEvents()
    {
        Debug.Log("=== 테스트 이벤트 발생 ===");
        TetrominoSpawned(Tetrimino.TetrominoType.T);
        ScoreChanged(1000);
        PlaySFX("TestSound");
        Debug.Log("테스트 이벤트 발생 완료");
    }

    #endregion
}