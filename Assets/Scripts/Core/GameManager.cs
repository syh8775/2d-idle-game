using UnityEngine;


public class GameManager : MonoBehaviour
{
    private const string StartingStageId = "STAGE_001";

    public static GameManager Instance { get; private set; }

    [SerializeField] private DataManager dataManager;
    [SerializeField] private BattleManager battleManager;
    [SerializeField] private bool persistAcrossScenes = true;

    public bool IsInitialized { get; private set; }

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

        battleManager.Initialize(dataManager, Formation);
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
        if (IsInitialized && battleManager.CurrentSession == null)
        {
            StartStage(StartingStageId);
        }
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
        if (Instance == this)
        {
            Instance = null;
        }
    }
}
