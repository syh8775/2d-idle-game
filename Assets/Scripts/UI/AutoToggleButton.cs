using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button), typeof(Image))]
public class AutoToggleButton : MonoBehaviour
{
    [SerializeField] private bool isAutoEnabled;
    [SerializeField] private Color enabledColor = new Color(0.2f, 0.82f, 0.96f, 1f);
    [SerializeField] private Color disabledColor = new Color(0.16f, 0.34f, 0.38f, 1f);

    private Button button;
    private Image buttonImage;
    private Text label;
    private BattleManager battleManager;

    public bool IsAutoEnabled
    {
        get { return isAutoEnabled; }
    }

    private void Awake()
    {
        button = GetComponent<Button>();
        buttonImage = GetComponent<Image>();
        label = GetComponentInChildren<Text>();

        button.onClick.AddListener(ToggleAuto);
        ApplyState();
    }

    private void Start()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogError("AUTO 버튼 연결 실패: 게임 매니저를 찾을 수 없습니다.");
            return;
        }

        battleManager = GameManager.Instance.Battle;

        if (battleManager == null)
        {
            Debug.LogError("AUTO 버튼 연결 실패: 전투 매니저가 연결되지 않았습니다.");
            return;
        }

        battleManager.SessionStarted += HandleSessionStarted;
        ApplyAutoState(battleManager.CurrentSession);
    }

    private void OnDestroy()
    {
        if (button != null)
        {
            button.onClick.RemoveListener(ToggleAuto);
        }

        if (battleManager != null)
        {
            battleManager.SessionStarted -= HandleSessionStarted;
        }
    }

    public void ToggleAuto()
    {
        isAutoEnabled = !isAutoEnabled;
        ApplyState();
        ApplyAutoState(battleManager == null ? null : battleManager.CurrentSession);
    }

    private void HandleSessionStarted(BattleSession session)
    {
        ApplyAutoState(session);
    }

    private void ApplyAutoState(BattleSession session)
    {
        if (session != null)
        {
            session.SetAutoEnabled(isAutoEnabled);
        }
    }

    private void ApplyState()
    {
        buttonImage.color = isAutoEnabled ? enabledColor : disabledColor;

        if (label == null)
        {
            return;
        }

        label.text = isAutoEnabled ? "AUTO ON" : "AUTO OFF";
        label.color = isAutoEnabled ? Color.white : new Color(0.55f, 0.65f, 0.67f, 1f);
    }
}
