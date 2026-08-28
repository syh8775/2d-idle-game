using UnityEngine;


public class GameManager : MonoBehaviour
{
    private const string StartingStageId = "STAGE_001";

    public static GameManager Instance { get; private set; }

    [SerializeField] private DataManager dataManager;
    [SerializeField] private BattleManager battleManager;
    [SerializeField] private bool persistAcrossScenes = true;

    public bool IsInitialized { get; private set; }
    public PlayerProgressModel Progress { get; private set; }
    public event System.Action ProgressChanged;

    private SaveManager saveManager;

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

        foreach (CharacterDefinition character in dataManager.Characters.Values)
        {
            Progress.AddCharacter(character.Id);
        }

        battleManager.Initialize(dataManager, Formation, Progress);
        battleManager.SessionCompleted += HandleBattleEnd;
        saveManager.Save(Progress);
        IsInitialized = true;
    }

    public bool StartStage(string stageId)
    {
        if (!IsInitialized)
        {
            return false;
        }

        return battleManager.StartStage(stageId);
    }

private void Start()
    {
        CreateMainUI();

        if (IsInitialized && battleManager.CurrentSession == null)
        {
            StartStage(StartingStageId);
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

    private void HandleBattleEnd(BattleSession session)
    {
        if (session == null || session.Outcome != BattleOutcome.Victory)
        {
            return;
        }

        RewardDefinition reward;

        if (!dataManager.TryGetReward(session.Stage.RewardId, out reward))
        {
            Debug.LogError("전투 보상을 찾을 수 없습니다: " + session.Stage.RewardId);
            return;
        }

        session.SetReward(reward.Gold);
        Progress.Gold += reward.Gold;
        Progress.LastClearedStageId = session.Stage.Id;
        SaveProgress();
    }

    private void SaveProgress()
    {
        saveManager.Save(Progress);

        if (ProgressChanged != null)
        {
            ProgressChanged();
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
    }

    private void Update()
    {
        if (IsInitialized)
        {
            battleManager.Tick(Time.deltaTime);
        }
    }

    private void OnDestroy()
    {
        if (battleManager != null)
        {
            battleManager.SessionCompleted -= HandleBattleEnd;
        }

        if (Instance == this)
        {
            Instance = null;
        }
    }
}
