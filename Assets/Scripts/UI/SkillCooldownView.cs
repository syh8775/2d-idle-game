using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class SkillCooldownView : MonoBehaviour
{
    [SerializeField] private Image cooldownOverlay;
    [SerializeField] private int skillIndex;
    [SerializeField] private Color readyColor = new Color(1f, 0.86f, 0.35f, 1f);
    [SerializeField] private Color coolingColor = new Color(0.62f, 0.72f, 0.86f, 1f);

    private Button button;
    private Image frameImage;
    private BattleManager battleManager;
    private BattleSession boundSession;
    private BattleSkill boundSkill;

    public bool IsCoolingDown
    {
        get { return boundSkill != null && !boundSkill.IsReady; }
    }

    private void Awake()
    {
        button = GetComponent<Button>();
        frameImage = GetComponent<Image>();

        if (cooldownOverlay == null)
        {
            cooldownOverlay = transform.Find("PortraitMask/CooldownOverlay")?.GetComponent<Image>();
        }

        Transform gauge = transform.Find("Gauge");

        if (gauge != null)
        {
            gauge.gameObject.SetActive(false);
        }

        if (frameImage != null)
        {
            frameImage.raycastTarget = true;
        }

        button.onClick.AddListener(HandleClick);
        ApplyProgress();
    }

    private void Start()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogError("스킬 UI 연결 실패: 게임 매니저를 찾을 수 없습니다.");
            return;
        }

        battleManager = GameManager.Instance.Battle;

        if (battleManager == null)
        {
            Debug.LogError("스킬 UI 연결 실패: 전투 매니저가 연결되지 않았습니다.");
            return;
        }

        battleManager.SessionStarted += BindSession;

        if (battleManager.CurrentSession != null)
        {
            BindSession(battleManager.CurrentSession);
        }
    }

    private void Update()
    {
        ApplyProgress();
    }

    private void OnDestroy()
    {
        if (button != null)
        {
            button.onClick.RemoveListener(HandleClick);
        }

        if (battleManager != null)
        {
            battleManager.SessionStarted -= BindSession;
        }

        if (boundSession != null)
        {
            boundSession.SkillUsed -= HandleSkillUsed;
        }
    }

    private void BindSession(BattleSession session)
    {
        if (boundSession != null)
        {
            boundSession.SkillUsed -= HandleSkillUsed;
        }

        boundSession = session;
        boundSkill = null;

        if (session == null ||
            session.Skills == null ||
            skillIndex < 0 ||
            skillIndex >= session.Skills.Count)
        {
            ApplyProgress();
            Debug.LogError("스킬 UI 연결 실패: 스킬 슬롯 번호가 올바르지 않습니다.");
            return;
        }

        boundSkill = session.Skills[skillIndex];
        boundSession.SkillUsed += HandleSkillUsed;
        ApplyProgress();
    }

    private void HandleClick()
    {
        if (boundSession != null)
        {
            boundSession.TryUseSkill(skillIndex);
        }
    }

    private void HandleSkillUsed(BattleSkill usedSkill)
    {
        if (usedSkill == boundSkill)
        {
            ApplyProgress();
        }
    }

    private void ApplyProgress()
    {
        float remainingCooldown = 0f;
        float cooldownDuration = 0f;

        if (boundSkill != null)
        {
            remainingCooldown = boundSkill.RemainingCooldown;
            cooldownDuration = boundSkill.Definition.CooldownSeconds;
        }

        if (cooldownOverlay != null)
        {
            cooldownOverlay.fillAmount =
                cooldownDuration > 0f
                    ? remainingCooldown / cooldownDuration
                    : 0f;
            cooldownOverlay.enabled = remainingCooldown > 0f;
        }

        bool isReady =
            boundSession != null &&
            boundSession.State == BattleSessionState.Running &&
            boundSkill != null &&
            boundSkill.Caster.IsAlive &&
            boundSkill.IsReady;

        if (frameImage != null)
        {
            frameImage.color = isReady ? readyColor : coolingColor;
        }

        if (button != null)
        {
            button.interactable = isReady;
        }
    }
}
