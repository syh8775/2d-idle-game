using System.Collections;
using UnityEngine;


public class GameManager : MonoBehaviour
{
    private const string StartingStageId = "STAGE_001";
    private const int OfflineRewardMaximumSeconds = 12 * 60 * 60;
    private const int OfflineRewardMinimumSeconds = 60;
    private const float ActiveTimeSaveIntervalSeconds = 60f;

    public static GameManager Instance { get; private set; }

    [SerializeField] private DataManager dataManager;
    [SerializeField] private BattleManager battleManager;
    [SerializeField] private bool persistAcrossScenes = true;

    public bool IsInitialized { get; private set; }
    public PlayerProgressModel Progress { get; private set; }
    public event System.Action ProgressChanged;

    private SaveManager saveManager;

    private string retryNoticeStageId;
    private BattleSession retryNoticeSession;
private Coroutine retryCoroutine;
    private float activeTimeSaveTimer;

    public DataManager Data
    {
        get { return dataManager; }
    }

    public BattleManager Battle
    {
        get { return battleManager; }
    }

    public PartyFormation Formation { get; private set; }

private void Awake()
    {
        Time.timeScale = 1f;

        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (persistAcrossScenes)
        {
            DontDestroyOnLoad(gameObject);
        }

        Initialize();
    }

    public void Initialize()
    {
        if (IsInitialized)
        {
            return;
        }

        if (dataManager == null)
        {
            dataManager = GetComponent<DataManager>();
        }

        if (battleManager == null)
        {
            battleManager = GetComponent<BattleManager>();
        }

        if (dataManager == null || battleManager == null)
        {
            Debug.LogError(
                "게임 매니저에 데이터 매니저와 전투 매니저가 연결되지 않았습니다.");
            return;
        }

        if (!dataManager.LoadAll())
        {
            return;
        }

        Formation = new PartyFormation();
        Formation.Load(dataManager.PartySlots.Values);

        saveManager = new SaveManager();
        Progress = saveManager.Load();
        LoadFormation();

        foreach (CharacterDefinition character in dataManager.Characters.Values)
        {
            Progress.AddCharacter(character.Id);
        }

        battleManager.Initialize(dataManager, Formation, Progress);

        battleManager.SessionStarted += TrackSession;
battleManager.SessionCompleted += OnEnd;
        AddOfflineGold(System.DateTime.UtcNow.Ticks);
        SyncFormation();
        saveManager.Save(Progress);
        IsInitialized = true;
    }

    public bool StartStage(string stageId)
    {
        if (!IsInitialized)
        {
            return false;
        }

        bool started = battleManager.StartStage(stageId);

        if (started)
        {
            Progress.CurrentStageId = stageId;
            SyncFormation();
            saveManager.Save(Progress);
        }

        return started;
    }

private void TrackSession(BattleSession session)
    {
        if (session == null)
        {
            return;
        }

        session.DamageResolved += OnDamageDone;

        foreach (BattleUnit unit in session.Units)
        {
            if (unit.Side == BattleUnitSide.Enemy)
            {
                unit.Died += OnEnemyDied;
            }
        }
    }

    private void UntrackSession(BattleSession session)
    {
        if (session == null)
        {
            return;
        }

        session.DamageResolved -= OnDamageDone;

        foreach (BattleUnit unit in session.Units)
        {
            if (unit.Side == BattleUnitSide.Enemy)
            {
                unit.Died -= OnEnemyDied;
            }
        }
    }

    private void OnEnemyDied(BattleUnit unit)
    {
        if (Progress != null && unit != null && unit.Side == BattleUnitSide.Enemy)
        {
            Progress.TotalEnemyKills++;
        }
    }

    private void OnDamageDone(BattleUnit target, int damage, bool isSkill)
    {
        if (Progress != null && target != null && target.Side == BattleUnitSide.Enemy && damage > Progress.HighestDamage)
        {
            Progress.HighestDamage = damage;
        }
    }


private void Start()
    {
        CreateMainUI();

        if (IsInitialized && battleManager.CurrentSession == null)
        {
            StartStage(GetResumeStage());
        }
    }

    public bool TryLevelUp(string characterId)
    {
        if (!IsInitialized || string.IsNullOrWhiteSpace(characterId))
        {
            return false;
        }

        CharacterProgressModel character = Progress.GetCharacter(characterId);

        if (character == null)
        {
            return false;
        }

        int cost = GameUtil.GetLevelCost(character.Level);

        if (Progress.Gold < cost)
        {
            return false;
        }

        Progress.Gold -= cost;
        character.Level++;
        SaveProgress();
        return true;
    }

private void OnEnd(BattleSession session)
    {
        if (session == null) return;

        UntrackSession(session);
        if (session.Outcome != BattleOutcome.Victory)
        {
            SaveProgress();
            if (ShouldAutoRetry(session))
            {
                bool firstFailure = retryNoticeStageId != session.Stage.Id;
                if (firstFailure)
                {
                    retryNoticeStageId = session.Stage.Id;
                    retryNoticeSession = session;
                }
                else
                {
                    retryNoticeSession = null;
                }

                QueueAutoRetry(session, firstFailure ? 5f : 0f);
            }
            return;
        }

        RewardDefinition reward;
        if (!dataManager.TryGetReward(session.Stage.RewardId, out reward))
        {
            Debug.LogError("전투 보상을 찾을 수 없습니다: " + session.Stage.RewardId);
            SaveProgress();
            return;
        }

        session.SetReward(reward.Gold);
        Progress.Gold += reward.Gold;
        Progress.TotalGoldEarned += reward.Gold;
        Progress.LastClearedStageId = session.Stage.Id;
        SaveProgress();
        StartCoroutine(GoNext(session));
    }

    public bool ShouldAutoRetry(BattleSession session)
    {
        if (session == null ||
            (session.Outcome != BattleOutcome.Defeat && session.Outcome != BattleOutcome.Timeout))
        {
            return false;
        }

        foreach (BattleUnit unit in session.Units)
        {
            if (unit.Side == BattleUnitSide.Enemy && unit.Id == "ENEMY_BOSS")
            {
                return false;
            }
        }

        return true;
    }

public bool ShowRetryNotice(BattleSession session)
    {
        return session != null && retryNoticeSession == session;
    }

    public bool IsRetryStage(BattleSession session)
    {
        return session != null &&
               session.Stage != null &&
               retryNoticeStageId == session.Stage.Id;
    }



private void QueueAutoRetry(BattleSession session, float delay)
    {
        if (retryCoroutine != null)
        {
            StopCoroutine(retryCoroutine);
        }

        retryCoroutine = StartCoroutine(RetryAfterDelay(session, delay));
    }

private IEnumerator RetryAfterDelay(BattleSession session, float delay)
    {
        if (delay > 0f)
        {
            yield return new WaitForSeconds(delay);
        }
        else
        {
            yield return null;
        }

        retryCoroutine = null;
        if (battleManager == null || battleManager.CurrentSession != session) yield break;
        if (!battleManager.RestartStage())
        {
            Debug.LogError("스테이지 재도전에 실패했습니다: " + session.Stage.Id);
        }
    }

    private IEnumerator GoNext(BattleSession session)
    {
        yield return new WaitForSeconds(5f);

        if (session == null || battleManager == null || battleManager.CurrentSession != session)
        {
            yield break;
        }

        string stageId = session.Stage.Id;

        if (string.IsNullOrEmpty(stageId) || !stageId.StartsWith("STAGE_"))
        {
            Debug.LogError("스테이지 ID 형식이 잘못되었습니다: " + stageId);
            yield break;
        }

        int stageNumber;

        if (!int.TryParse(stageId.Substring(6), out stageNumber))
        {
            Debug.LogError("스테이지 번호를 읽을 수 없습니다: " + stageId);
            yield break;
        }

        string nextStageId = "STAGE_" + (stageNumber + 1).ToString("000");
        StageDefinition nextStage;

        if (!dataManager.TryGetStage(nextStageId, out nextStage))
        {
            Debug.Log("마지막 스테이지를 클리어했습니다.");
            yield break;
        }

        if (!StartStage(nextStageId))
        {
            Debug.LogError("다음 스테이지 시작에 실패했습니다: " + nextStageId);
        }
    }

    private void SaveProgress()
    {
        saveManager.Save(Progress);

        if (ProgressChanged != null)
        {
            ProgressChanged();
        }
    }

    public void SaveFormation()
    {
        if (!IsInitialized)
        {
            return;
        }

        SyncFormation();
        SaveProgress();
    }

public int ClaimOffline()
    {
        if (!IsInitialized || Progress.PendingOfflineGold <= 0)
        {
            return 0;
        }

        int rewardGold = Progress.PendingOfflineGold;
        Progress.Gold += rewardGold;
        Progress.TotalGoldEarned += rewardGold;
        Progress.PendingOfflineGold = 0;
        Progress.PendingOfflineSeconds = 0;
        Progress.LastActiveUtcTicks = System.DateTime.UtcNow.Ticks;
        SaveProgress();
        return rewardGold;
    }

    private void AddOfflineGold(long nowUtcTicks)
    {
        if (nowUtcTicks <= 0)
        {
            return;
        }

        long lastActiveTicks = Progress.LastActiveUtcTicks;
        Progress.LastActiveUtcTicks = nowUtcTicks;

        if (lastActiveTicks <= 0 || lastActiveTicks > nowUtcTicks)
        {
            return;
        }

        long elapsedSeconds = (nowUtcTicks - lastActiveTicks) / System.TimeSpan.TicksPerSecond;
        if (elapsedSeconds < OfflineRewardMinimumSeconds)
        {
            return;
        }

        int storedSeconds = Mathf.Clamp(Progress.PendingOfflineSeconds, 0, OfflineRewardMaximumSeconds);
        int availableSeconds = OfflineRewardMaximumSeconds - storedSeconds;
        int rewardSeconds = (int)System.Math.Min(elapsedSeconds, availableSeconds);

        if (rewardSeconds <= 0)
        {
            return;
        }

        int stageRewardGold = GetStageGold();
        int earnedGold = (int)((long)stageRewardGold * rewardSeconds / 240L);
        Progress.PendingOfflineSeconds = storedSeconds + rewardSeconds;
        Progress.PendingOfflineGold += earnedGold;
    }

    private int GetStageGold()
    {
        StageDefinition stage;
        RewardDefinition reward;
        string stageId = GetResumeStage();

        if (dataManager.TryGetStage(stageId, out stage) &&
            dataManager.TryGetReward(stage.RewardId, out reward))
        {
            return reward.Gold;
        }

        return 0;
    }

    private void SaveActiveTime()
    {
        if (!IsInitialized || saveManager == null || Progress == null)
        {
            return;
        }

        Progress.LastActiveUtcTicks = System.DateTime.UtcNow.Ticks;
        saveManager.Save(Progress);
    }

    private string GetResumeStage()
    {
        StageDefinition stage;

        if (!string.IsNullOrEmpty(Progress.CurrentStageId) &&
            dataManager.TryGetStage(Progress.CurrentStageId, out stage))
        {
            if (Progress.CurrentStageId == Progress.LastClearedStageId &&
                Progress.CurrentStageId.StartsWith("STAGE_") &&
                int.TryParse(Progress.CurrentStageId.Substring(6), out int clearedStageNumber))
            {
                string nextStageId = "STAGE_" + (clearedStageNumber + 1).ToString("000");
                StageDefinition nextStage;
                if (dataManager.TryGetStage(nextStageId, out nextStage))
                {
                    return nextStageId;
                }
            }

            return Progress.CurrentStageId;
        }

        if (!string.IsNullOrEmpty(Progress.LastClearedStageId) &&
            dataManager.TryGetStage(Progress.LastClearedStageId, out stage))
        {
            return Progress.LastClearedStageId;
        }

        return StartingStageId;
    }

    private void LoadFormation()
    {
        if (Progress.PartyMembers == null || Progress.PartyMembers.Count != Formation.Members.Count)
        {
            return;
        }

        System.Collections.Generic.HashSet<string> characterIds = new System.Collections.Generic.HashSet<string>();
        System.Collections.Generic.HashSet<int> occupiedSlots = new System.Collections.Generic.HashSet<int>();

        foreach (PartyMember savedMember in Progress.PartyMembers)
        {
            CharacterDefinition character;
            if (savedMember == null || !dataManager.TryGetCharacter(savedMember.CharacterId, out character) ||
                !characterIds.Add(savedMember.CharacterId) || savedMember.FormationSlot < 0 ||
                savedMember.FormationSlot > 9 ||
                (savedMember.FormationSlot > 0 && !occupiedSlots.Add(savedMember.FormationSlot)))
            {
                return;
            }
        }

        Formation.Members.Clear();
        foreach (PartyMember savedMember in Progress.PartyMembers)
        {
            PartyMember member = new PartyMember();
            member.CharacterId = savedMember.CharacterId;
            member.FormationSlot = savedMember.FormationSlot;
            Formation.Members.Add(member);
        }
    }

    private void SyncFormation()
    {
        Progress.PartyMembers.Clear();

        foreach (PartyMember formationMember in Formation.Members)
        {
            PartyMember savedMember = new PartyMember();
            savedMember.CharacterId = formationMember.CharacterId;
            savedMember.FormationSlot = formationMember.FormationSlot;
            Progress.PartyMembers.Add(savedMember);
        }
    }
private void CreateMainUI()
    {
        UIManager uiManager = FindFirstObjectByType<UIManager>();

        if (uiManager == null)
        {
            Debug.LogError("화면을 연결할 UIManager가 없습니다.");
            return;
        }

        GrowthView.Create(uiManager, this);
        DungeonView.Create(uiManager);
        DrawView.Create(uiManager);
        OfflineRewardView.Create(uiManager, this);
        PauseMenuView.Create(uiManager);
    }

    private void ResetProgress()
    {
        if (!IsInitialized)
        {
            return;
        }

        if (battleManager.CurrentSession != null && !battleManager.CurrentSession.IsFinished)
        {
            battleManager.CurrentSession.Cancel();
        }


        retryNoticeStageId = string.Empty;
        retryNoticeSession = null;
Progress = new PlayerProgressModel();
        Progress.LastActiveUtcTicks = System.DateTime.UtcNow.Ticks;
        Formation.Load(dataManager.PartySlots.Values);

        foreach (CharacterDefinition character in dataManager.Characters.Values)
        {
            Progress.AddCharacter(character.Id);
        }

        SyncFormation();
        battleManager.Initialize(dataManager, Formation, Progress);
        activeTimeSaveTimer = 0f;
        StartStage(StartingStageId);
        SaveProgress();
    }

private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F12))
        {
            ResetProgress();
            return;
        }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        RunCheats();
#endif

        if (IsInitialized)
        {
            battleManager.Tick(Time.deltaTime);
            activeTimeSaveTimer += Time.unscaledDeltaTime;

            if (activeTimeSaveTimer >= ActiveTimeSaveIntervalSeconds)
            {
                activeTimeSaveTimer = 0f;
                SaveActiveTime();
            }
        }
    }

    private void OnApplicationPause(bool paused)
    {
        if (paused)
        {
            SaveActiveTime();
        }
    }

    private void OnApplicationQuit()
    {
        SaveActiveTime();
    }

private void OnDestroy()
    {
        SaveActiveTime();

        if (battleManager != null)
        {
            battleManager.SessionStarted -= TrackSession;
            battleManager.SessionCompleted -= OnEnd;
            UntrackSession(battleManager.CurrentSession);
        }

        if (Instance == this)
        {
            Instance = null;
        }
    }


#if UNITY_EDITOR || DEVELOPMENT_BUILD
    private void RunCheats()
    {
        if (Input.GetKeyDown(KeyCode.F8))
        {
            bool killAll = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
            KillAllies(killAll);
        }
        else if (Input.GetKeyDown(KeyCode.F9))
        {
            KillEnemies();
        }
        else if (Input.GetKeyDown(KeyCode.F10))
        {
            ReadySkills();
        }
        else if (Input.GetKeyDown(KeyCode.F11))
        {
            AddOfflineHour();
        }
    }

    private void KillAllies(bool killAll)
    {
        BattleSession session = battleManager.CurrentSession;
        if (session == null || session.IsFinished) return;

        BattleUnit nextUnit = null;
        foreach (BattleUnit unit in session.Units)
        {
            if (unit.Side != BattleUnitSide.Ally || !unit.IsAlive) continue;
            if (killAll)
            {
                KillUnit(session, unit);
            }
            else if (nextUnit == null || unit.FormationSlot < nextUnit.FormationSlot)
            {
                nextUnit = unit;
            }
        }

        if (!killAll && nextUnit != null) KillUnit(session, nextUnit);
    }

    private void KillEnemies()
    {
        BattleSession session = battleManager.CurrentSession;
        if (session == null || session.IsFinished) return;

        foreach (BattleUnit unit in session.Units)
        {
            if (unit.Side == BattleUnitSide.Enemy && unit.IsAlive)
            {
                KillUnit(session, unit);
            }
        }
    }

    private void KillUnit(BattleSession session, BattleUnit unit)
    {
        long rawDamage = (long)unit.CurrentHitPoints * (100 + Mathf.Max(0, unit.Defense)) / 100 + 2;
        session.ApplyDamage(unit, (int)System.Math.Min(rawDamage, int.MaxValue));
    }

    private void ReadySkills()
    {
        BattleSession session = battleManager.CurrentSession;
        if (session == null || session.IsFinished) return;

        foreach (BattleSkill skill in session.Skills)
        {
            if (skill.Caster.Side == BattleUnitSide.Ally && skill.Caster.IsAlive)
            {
                skill.Ready();
            }
        }
    }

    private void AddOfflineHour()
    {
        if (!IsInitialized) return;

        int storedSeconds = Mathf.Clamp(Progress.PendingOfflineSeconds, 0, OfflineRewardMaximumSeconds);
        int addedSeconds = Mathf.Min(60 * 60, OfflineRewardMaximumSeconds - storedSeconds);
        if (addedSeconds <= 0) return;

        Progress.PendingOfflineSeconds = storedSeconds + addedSeconds;
        Progress.PendingOfflineGold += (int)((long)GetStageGold() * addedSeconds / 240L);
        SaveProgress();
    }
#endif
}
