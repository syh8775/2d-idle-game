using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class SkillCooldownView : MonoBehaviour
{
    [SerializeField] private Image cooldownOverlay;
    [SerializeField] private int skillIndex;
    [SerializeField] private Color readyColor = new Color(1f, 0.86f, 0.35f, 1f);

    [SerializeField] private Color deadFrameColor = new Color(0.16f, 0.18f, 0.22f, 1f);
    [SerializeField] private Color deadPortraitColor = new Color(0.25f, 0.25f, 0.25f, 0.65f);
    [SerializeField] private Color coolingColor = new Color(0.62f, 0.72f, 0.86f, 1f);

    private Button button;

    private Image portraitImage;
    private Color defaultPortraitColor = Color.white;
    private Image frameImage;
    private BattleManager battleManager;
    private BattleSession boundSession;
    private BattleSkill boundSkill;

private void Awake()
    {
        button = GetComponent<Button>();

        portraitImage = transform.Find("PortraitMask/Character")?.GetComponent<Image>();

        if (portraitImage != null)
        {
            defaultPortraitColor = portraitImage.color;
        }
        frameImage = GetComponent<Image>();

        if (cooldownOverlay == null)
        {
            cooldownOverlay = transform.Find("PortraitMask/CooldownOverlay")?.GetComponent<Image>();
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

    private void OnEnable()
    {
        if (battleManager == null && GameManager.Instance != null)
        {
            battleManager = GameManager.Instance.Battle;
        }

        if (battleManager != null && battleManager.CurrentSession != null)
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
            skillIndex >= session.Stage.PartySize)
        {
            ApplyProgress();
            Debug.LogError("스킬 UI 연결 실패: 스킬 슬롯 번호가 올바르지 않습니다.");
            return;
        }

        Image portrait = transform.Find("PortraitMask/Character")?.GetComponent<Image>();
        if (skillIndex >= session.Skills.Count)
        {
            // 편성에서 빠진 인원만큼 남는 스킬 버튼은 빈칸으로 표시합니다.
            if (portrait != null)
            {
                portrait.enabled = false;
            }
            ApplyProgress();
            return;
        }

        boundSkill = session.Skills[skillIndex];
        UIManager uiManager = GetComponentInParent<UIManager>();

        if (portrait != null)
        {
            portrait.enabled = true;
            portrait.sprite = Resources.Load<Sprite>("UI/SkillPortraits/" + boundSkill.Caster.Id + "-face");

            if (portrait.sprite == null && uiManager != null)
            {
                portrait.sprite = uiManager.GetPortrait(boundSkill.Caster.Id);
            }

            SetFace(portrait, boundSkill.Caster.Id);
        }
        boundSession.SkillUsed += HandleSkillUsed;
        ApplyProgress();
    }

private void SetFace(Image portrait, string characterId)
    {
        if (portrait == null || string.IsNullOrEmpty(characterId) || characterId.Length <= 5)
        {
            return;
        }

        int characterNumber;
        if (!int.TryParse(characterId.Substring(5), out characterNumber))
        {
            return;
        }

        Vector2 size;
        Vector2 offset;

        switch (characterNumber)
        {
            case 1:
                size = new Vector2(120f, 120f);
                offset = new Vector2(-12f, 15f);
                break;
            case 2:
                size = new Vector2(92f, 92f);
                offset = new Vector2(2f, -9f);
                break;
            case 3:
                size = new Vector2(144f, 144f);
                offset = new Vector2(-33f, 27f);
                break;
            case 4:
                size = new Vector2(92f, 92f);
                offset = new Vector2(-4f, 5f);
                break;
            case 5:
                size = new Vector2(120f, 120f);
                offset = new Vector2(-22f, 16f);
                break;
            case 6:
                size = new Vector2(120f, 120f);
                offset = new Vector2(-10f, 28f);
                break;
            case 7:
                size = new Vector2(92f, 92f);
                offset = new Vector2(4f, -3f);
                break;
            case 8:
                size = new Vector2(108f, 108f);
                offset = new Vector2(-10f, 12f);
                break;
            case 9:
                size = new Vector2(240f, 240f);
                offset = new Vector2(-34f, 30f);
                break;
            default:
                return;
        }

        RectTransform rect = portrait.rectTransform;
        rect.sizeDelta = size;
        rect.anchoredPosition = offset;
        portrait.preserveAspect = true;
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

        bool isDead = boundSkill != null && !boundSkill.Caster.IsAlive;

        if (cooldownOverlay != null)
        {
            if (isDead)
            {
                cooldownOverlay.fillAmount = 1f;
                cooldownOverlay.enabled = true;
            }
            else
            {
                cooldownOverlay.fillAmount =
                    cooldownDuration > 0f
                        ? remainingCooldown / cooldownDuration
                        : 0f;
                cooldownOverlay.enabled = remainingCooldown > 0f;
            }
        }

        if (portraitImage != null)
        {
            portraitImage.color = isDead ? deadPortraitColor : defaultPortraitColor;
        }

        bool isReady =
            boundSession != null &&
            boundSession.State == BattleSessionState.Running &&
            boundSkill != null &&
            boundSkill.Caster.IsAlive &&
            boundSkill.IsReady;

        if (frameImage != null)
        {
            frameImage.color = isDead
                ? deadFrameColor
                : isReady ? readyColor : coolingColor;
        }

        if (button != null)
        {
            button.interactable = isReady;
        }
    }
}
