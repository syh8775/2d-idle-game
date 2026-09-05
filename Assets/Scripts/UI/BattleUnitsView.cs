using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleUnitsView : MonoBehaviour
{
    private BattleManager battleManager;
    private BattleSession boundSession;
    private BattleUnitFeedbackView feedbackView;
    private BattleResultView resultView;
    private Text stageText;

    private Image battleStatusIcon;
    private Sprite statusRunning;
    private Sprite statusRetry;
private Text battleTimer;

    private Transform[] allySlots = new Transform[10];
    private Transform[] enemySlots = new Transform[10];
    private Sprite[] allySprites = new Sprite[10];
    private readonly Dictionary<Transform, Image> specialSkillIcons = new Dictionary<Transform, Image>();
    private readonly Dictionary<Transform, Text> damageTexts = new Dictionary<Transform, Text>();
    private readonly Dictionary<Text, int> damageTextVersions = new Dictionary<Text, int>();
    private readonly Dictionary<Image, float> specialSkillIconExpiresAt = new Dictionary<Image, float>();

    private const float SpecialSkillIconDuration = 0.8f;
    private const float SpecialSkillIconPopDuration = 0.18f;
    private static readonly Vector2 SpecialSkillIconSize = new Vector2(52f, 52f);

    private void Start()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogError("전투 화면 연결 실패: 게임 매니저를 찾을 수 없습니다.");
            return;
        }

        battleManager = GameManager.Instance.Battle;

        if (battleManager == null)
        {
            Debug.LogError("전투 화면 연결 실패: 전투 매니저가 연결되지 않았습니다.");
            return;
        }

        feedbackView = new BattleUnitFeedbackView(this);
        resultView = new BattleResultView(this);
        resultView.Initialize(transform);
        resultView.RetryRequested += OnRetryRequest;


        SetupStatus();
CacheSlots();
        stageText = FindText("StageText");
        battleTimer = FindText("BattleTimer");

        battleManager.SessionStarted += OnSessionStart;
        battleManager.SessionCompleted += OnSessionDone;

        if (battleManager.CurrentSession != null)
        {
            BindUnits(battleManager.CurrentSession);
        }

        ShowFinalResult();
    }

private void OnEnable()
    {
        if (battleManager != null && battleManager.CurrentSession != null)
        {
            BindUnits(battleManager.CurrentSession);
        }

        ShowFinalResult();
    }


private void OnDestroy()
    {
        if (battleManager != null)
        {
            battleManager.SessionStarted -= OnSessionStart;
            battleManager.SessionCompleted -= OnSessionDone;
        }

        if (boundSession != null)
        {
            boundSession.AttackResolved -= OnAttackDone;
            boundSession.DamageResolved -= OnDamageDone;
            boundSession.SkillUsed -= HandleSkillUsed;
            UnbindEvents(boundSession);
        }

        HideSkillIcons();

        if (resultView != null)
        {
            resultView.RetryRequested -= OnRetryRequest;
            resultView.Dispose();
        }
    }

    private void OnDisable()
    {
        HideDamageTexts();
        HideSkillIcons();
    }

        private void OnSessionStart(BattleSession session)
    {
        if (resultView != null)
        {
            resultView.Hide();
        }

        bool isRetry = GameManager.Instance != null &&
                       GameManager.Instance.IsRetryStage(session);
        SetStatus(isRetry);
        BindUnits(session);
    }

    private void OnSessionDone(BattleSession session)
    {
        bool autoRetry = session != null &&
                         session.Outcome != BattleOutcome.Victory &&
                         GameManager.Instance != null &&
                         GameManager.Instance.ShouldAutoRetry(session);
        SetStatus(autoRetry);

        if (resultView == null || !isActiveAndEnabled)
        {
            return;
        }

        if (session != null && session.Outcome == BattleOutcome.Victory)
        {
            StartCoroutine(ShowVictoryFade(session));
        }
        else
        {
            resultView.Show(session);
        }
    }

    private void ShowFinalResult()
    {
        if (!isActiveAndEnabled || battleManager == null || resultView == null)
        {
            return;
        }

        BattleSession session = battleManager.CurrentSession;

        if (session != null && session.IsFinished)
        {
            resultView.Show(session);
        }
    }


    private IEnumerator ShowVictoryFade(BattleSession session)
    {
        yield return new WaitForSeconds(BattleUnitFeedbackView.DeathFadeDuration);
        resultView.Show(session);
    }

    private void OnRetryRequest()
    {
        SetStatus(false);

        if (battleManager == null || !battleManager.RestartStage())
        {
            if (resultView != null)
            {
                resultView.RetryFailed();
            }

            Debug.LogError("전투 재시작에 실패했습니다.");
        }
    }

    private void BindUnits(BattleSession session)
    {
        if (session == null || session.Units == null)
        {
            Debug.LogError("전투 화면 연결 실패: 전투 세션이 비어 있습니다.");
            return;
        }

        StopAllCoroutines();
        HideDamageTexts();
        HideSkillIcons();
        feedbackView.ResetSession();

        if (boundSession != null)
        {
            boundSession.AttackResolved -= OnAttackDone;
            boundSession.DamageResolved -= OnDamageDone;
            boundSession.SkillUsed -= HandleSkillUsed;
            UnbindEvents(boundSession);
        }

        boundSession = session;
        SetStage(session);
        boundSession.AttackResolved += OnAttackDone;
        boundSession.DamageResolved += OnDamageDone;
        boundSession.SkillUsed += HandleSkillUsed;

        HideBattleSlots();

        int allyCount = 0;
        int enemyCount = 0;

        foreach (BattleUnit unit in session.Units)
        {
            if (!BindUnit(unit))
            {
                continue;
            }
unit.HitPointsChanged += OnHpChanged;
            unit.Died += HandleUnitDied;

            if (unit.Side == BattleUnitSide.Ally)
            {
                allyCount++;
            }
            else
            {
                enemyCount++;
            }
        }

        RefreshStatuses(session);

        Debug.Log(
            "전투 화면 연결 완료 - 아군 " +
            allyCount +
            "명, 적군 " +
            enemyCount +
            "명");
    }

        private void SetupStatus()
    {
        Transform bar = transform.parent == null ? null : transform.parent.Find("BattleSkillBar");
        if (bar == null)
        {
            return;
        }

        statusRunning = Resources.Load<Sprite>("UI/BattleStatus_Running");
        statusRetry = Resources.Load<Sprite>("UI/BattleStatus_Retry");

        Transform existing = bar.Find("BattleStatusIcon");
        GameObject iconObject = existing == null
            ? new GameObject("BattleStatusIcon", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image))
            : existing.gameObject;
        iconObject.transform.SetParent(bar, false);

        battleStatusIcon = iconObject.GetComponent<Image>();
        RectTransform rect = iconObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(72f, 72f);
        rect.anchoredPosition = new Vector2(264f, -40f);

        battleStatusIcon.type = Image.Type.Simple;
        battleStatusIcon.preserveAspect = true;
        battleStatusIcon.raycastTarget = false;
        SetStatus(false);
    }

    private void SetStatus(bool retry)
    {
        if (battleStatusIcon == null)
        {
            return;
        }

        battleStatusIcon.sprite = retry ? statusRetry : statusRunning;
        battleStatusIcon.enabled = battleStatusIcon.sprite != null;
    }

private void CacheSlots()
    {
        for (int i = 1; i < allySlots.Length; i++)
        {
            allySlots[i] = transform.Find("AllyUnit_" + i.ToString("00"));
            enemySlots[i] = transform.Find("EnemyUnit_" + i.ToString("00"));

            if (allySlots[i] == null)
            {
                continue;
            }

            RectTransform slotRect = allySlots[i].GetComponent<RectTransform>();
            int horizontalGroup = (i - 1) / 3;
            int verticalIndex = (i - 1) % 3;
            slotRect.anchoredPosition = new Vector2(-160f - horizontalGroup * 140f, 255f - verticalIndex * 150f);
            Transform character = allySlots[i].Find("Character");
            Image image = character == null ? null : character.GetComponent<Image>();

            if (image == null || image.sprite == null)
            {
                continue;
            }

            string spriteName = image.sprite.name;
            int characterNumber;

            if (spriteName.StartsWith("character-") &&
                spriteName.Length >= 12 &&
                int.TryParse(spriteName.Substring(10, 2), out characterNumber) &&
                characterNumber > 0 &&
                characterNumber < allySprites.Length)
            {
                allySprites[characterNumber] = image.sprite;
            }
        }
    }

    private Transform GetCachedSlot(
        BattleUnitSide side,
        int formationSlot)
    {
        Transform[] slots = allySlots;

        if (side == BattleUnitSide.Enemy)
        {
            slots = enemySlots;
        }

        if (formationSlot < 0 || formationSlot >= slots.Length)
        {
            return null;
        }

        return slots[formationSlot];
    }

    private void RefreshStatuses(BattleSession session)
    {
        if (session == null || session.Units == null)
        {
            return;
        }

        foreach (BattleUnit unit in session.Units)
        {
            RefreshStatus(unit);
        }
    }

private void RefreshStatus(BattleUnit unit)
    {
        if (unit == null)
        {
            return;
        }

        Transform slot = GetCachedSlot(unit.Side, unit.FormationSlot);
        if (slot == null)
        {
            return;
        }

        Transform character = slot.Find("Character");
        // 사망한 캐릭터는 사망 페이드가 끝난 뒤 숨깁니다.
        if (character != null && unit.IsAlive)
        {
            character.gameObject.SetActive(true);
        }

        Transform statusOverlay = slot.Find("StatusOverlay");
        Transform hpFill = slot.Find("HP_Fill");
        if (statusOverlay != null)
        {
            hpFill = statusOverlay.Find("HP_Fill");
        }

        if (hpFill == null)
        {
            return;
        }

        Image hpImage = hpFill.GetComponent<Image>();
        if (hpImage == null)
        {
            return;
        }

        float hpRatio = unit.MaxHitPoints > 0 ? (float)unit.CurrentHitPoints / unit.MaxHitPoints : 0f;
        hpImage.fillAmount = Mathf.Clamp01(hpRatio);

        if (statusOverlay != null)
        {
            statusOverlay.gameObject.SetActive(unit.IsAlive);
        }
        else
        {
            Transform hpBack = slot.Find("HP_Back");
            if (hpBack != null)
            {
                hpBack.gameObject.SetActive(unit.IsAlive);
            }

            hpFill.gameObject.SetActive(unit.IsAlive);
        }
    }

    private bool BindUnit(BattleUnit unit)
    {
        string slotName = GetSlotName(unit.Side, unit.FormationSlot);
        Transform slot = GetCachedSlot(unit.Side, unit.FormationSlot);

        if (slot == null)
        {
            Debug.LogError("전투 화면 슬롯을 찾을 수 없습니다: " + slotName);
            return false;
        }

        Transform character = slot.Find("Character");

        if (character == null)
        {
            Debug.LogError("전투 화면 슬롯에 Character 오브젝트가 없습니다: " +slotName);
            return false;
        }

        Image image = character.GetComponent<Image>();

        if (image == null)
        {
            Debug.LogError("전투 화면 슬롯의 Character에 Image가 없습니다: " +slotName);
            return false;
        }

        bool isBoss = IsBoss(unit);
        SetUnit(slot, character, isBoss);
        SetHpBar(slot, isBoss);

        if (unit.Side == BattleUnitSide.Ally)
        {
            int characterNumber;

            if (unit.Id.Length > 5 && int.TryParse(unit.Id.Substring(5), out characterNumber) && characterNumber < allySprites.Length && allySprites[characterNumber] != null)
            {
                image.sprite = allySprites[characterNumber];
            }
        }

        feedbackView.RegisterChar(image);

        if (!isBoss)
        {
            RectTransform characterRect = image.rectTransform;

            if (unit.Id == "CHAR_009" || unit.Id == "ENEMY_009")
            {
                characterRect.sizeDelta = new Vector2(261f, 297f);
                characterRect.pivot = new Vector2(0.5f, 0.4166667f);
            }
            else
            {
                characterRect.sizeDelta = new Vector2(217.5f, 247.5f);
                characterRect.pivot = new Vector2(0.5f, 0.5f);
            }
        }

        slot.gameObject.SetActive(true);
        character.gameObject.SetActive(unit.IsAlive);
        image.enabled = true;

        Transform statusOverlay = slot.Find("StatusOverlay");

        if (statusOverlay != null)
        {
            statusOverlay.gameObject.SetActive(true);
        }
        else
        {
            Transform hpBack = slot.Find("HP_Back");
            Transform hpFill = slot.Find("HP_Fill");

            if (hpBack != null)
            {
                hpBack.gameObject.SetActive(true);
            }

            if (hpFill != null)
            {
                hpFill.gameObject.SetActive(true);
            }
        }

        return true;
    }

    private void OnAttackDone(
        BattleUnit attacker,
        BattleUnit target,
        int damage)
    {
        if (!isActiveAndEnabled || damage <= 0 || feedbackView == null)
        {
            return;
        }

        Image attackerImage = GetCharImage(attacker);
        Image targetImage = GetCharImage(target);
        StartCoroutine(
            feedbackView.PlayAttackFx(
                attacker,
                target,
                attackerImage,
                targetImage));
    }

private void OnHpChanged(BattleUnit unit)
    {
        RefreshStatus(unit);
    }

private void OnDamageDone(BattleUnit target, int damage, bool isSkill)
    {
        if (!isActiveAndEnabled || target == null || target.Side != BattleUnitSide.Enemy || damage <= 0)
        {
            return;
        }

        Color damageColor = isSkill ? new Color(1f, 0.55f, 0.55f) : Color.white;
        ShowDamage(target, damage, damageColor);
    }


private void ShowDamage(BattleUnit unit, int damage, Color damageColor)
    {
        Transform slot = GetCachedSlot(unit.Side, unit.FormationSlot);
        Image characterImage = GetCharImage(unit);

        if (slot == null || characterImage == null || damage <= 0)
        {
            return;
        }

        Text damageText;

        if (!damageTexts.TryGetValue(slot, out damageText) || damageText == null)
        {
            GameObject textObject = new GameObject("DamageText", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text), typeof(Outline));
            textObject.transform.SetParent(slot, false);
            damageText = textObject.GetComponent<Text>();
            damageText.font = stageText != null ? stageText.font : Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            damageText.fontSize = 30;
            damageText.fontStyle = FontStyle.Bold;
            damageText.alignment = TextAnchor.MiddleCenter;
            damageText.raycastTarget = false;

            Outline outline = textObject.GetComponent<Outline>();
            outline.effectColor = new Color(0f, 0f, 0f, 0.9f);
            outline.effectDistance = new Vector2(2f, -2f);
            damageTexts[slot] = damageText;
        }

        RectTransform characterRect = characterImage.rectTransform;
        RectTransform textRect = damageText.rectTransform;
        float characterTop = characterRect.anchoredPosition.y + characterRect.rect.height * (1f - characterRect.pivot.y) * Mathf.Abs(characterRect.localScale.y);
        textRect.anchorMin = new Vector2(0.5f, 0.5f);
        textRect.anchorMax = new Vector2(0.5f, 0.5f);
        textRect.pivot = new Vector2(0.5f, 0.5f);
        textRect.sizeDelta = new Vector2(140f, 50f);
        textRect.anchoredPosition = new Vector2(characterRect.anchoredPosition.x, characterTop + 20f);
        damageText.text = damage.ToString();
        damageText.color = damageColor;
        damageText.transform.SetAsLastSibling();
        damageText.gameObject.SetActive(true);

        int version = 1;
        if (damageTextVersions.ContainsKey(damageText))
        {
            version = damageTextVersions[damageText] + 1;
        }
        damageTextVersions[damageText] = version;
        StartCoroutine(DamageRise(damageText, version, damageColor));
    }

    private void HideDamageTexts()
    {
        foreach (Text damageText in damageTexts.Values)
        {
            if (damageText != null) damageText.gameObject.SetActive(false);
        }
        damageTextVersions.Clear();
    }

private IEnumerator DamageRise(Text damageText, int version, Color startColor)
    {
        const float duration = 0.65f;
        RectTransform textRect = damageText.rectTransform;
        Vector2 startPosition = textRect.anchoredPosition;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            if (damageText == null || !damageTextVersions.ContainsKey(damageText) || damageTextVersions[damageText] != version)
            {
                yield break;
            }

            elapsed += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsed / duration);
            textRect.anchoredPosition = startPosition + new Vector2(0f, 35f * progress);
            textRect.localScale = Vector3.one * Mathf.Lerp(0.85f, 1.1f, Mathf.Min(progress * 4f, 1f));
            Color color = startColor;
            color.a = 1f - progress;
            damageText.color = color;
            yield return null;
        }

        if (damageText != null && damageTextVersions.ContainsKey(damageText) && damageTextVersions[damageText] == version)
        {
            damageText.gameObject.SetActive(false);
            damageTextVersions.Remove(damageText);
        }
    }


    private void HandleSkillUsed(BattleSkill skill)
    {
        if (!isActiveAndEnabled ||
            skill == null ||
            skill.Definition == null ||
            skill.Caster == null ||
            skill.Definition.SkillType != "Special")
        {
            return;
        }

        Image casterImage = GetCharImage(skill.Caster);

        if (casterImage == null || !casterImage.gameObject.activeInHierarchy)
        {
            return;
        }

        Sprite iconSprite = GetSkillIcon(skill.Caster.Id);

        if (iconSprite == null)
        {
            return;
        }

        Transform slot = GetCachedSlot(
            skill.Caster.Side,
            skill.Caster.FormationSlot);
        Image icon = EnsureSkillIcon(slot, casterImage);

        if (icon == null)
        {
            return;
        }

        icon.sprite = iconSprite;
        icon.transform.SetAsLastSibling();
        icon.gameObject.SetActive(true);
        icon.rectTransform.localScale = Vector3.zero;
        specialSkillIconExpiresAt[icon] = Time.unscaledTime + SpecialSkillIconDuration;
    }

    private void HandleUnitDied(BattleUnit unit)
    {
        if (!isActiveAndEnabled || feedbackView == null)
        {
            return;
        }

        HideSkillIcon(unit);
        Image image = GetCharImage(unit);
        feedbackView.StartDeathFade(unit, image);
    }

    private Sprite GetSkillIcon(string characterId)
    {
        if (string.IsNullOrEmpty(characterId) || characterId.Length <= 5)
        {
            return null;
        }

        int characterNumber;

        if (!int.TryParse(characterId.Substring(5), out characterNumber))
        {
            return null;
        }

        return Resources.Load<Sprite>(
            "UI/SkillIcons/skill-char-" +
            characterNumber.ToString("000") +
            "-special");
    }

    private Image EnsureSkillIcon(Transform slot, Image casterImage)
    {
        if (slot == null || casterImage == null)
        {
            return null;
        }

        Image icon;

        if (!specialSkillIcons.TryGetValue(slot, out icon) || icon == null)
        {
            Transform existingIcon = slot.Find("SpecialSkillIcon");
            icon = existingIcon == null ? null : existingIcon.GetComponent<Image>();

            if (icon == null)
            {
                GameObject iconObject = new GameObject(
                    "SpecialSkillIcon",
                    typeof(RectTransform),
                    typeof(CanvasRenderer),
                    typeof(Image));
                iconObject.transform.SetParent(slot, false);
                icon = iconObject.GetComponent<Image>();
            }

            specialSkillIcons[slot] = icon;
        }

        RectTransform iconRect = icon.rectTransform;
        iconRect.anchorMin = new Vector2(0.5f, 0.5f);
        iconRect.anchorMax = new Vector2(0.5f, 0.5f);
        iconRect.pivot = new Vector2(0.5f, 0.5f);
        iconRect.sizeDelta = SpecialSkillIconSize;

        RectTransform casterRect = casterImage.rectTransform;
        float characterTop = casterRect.anchoredPosition.y +
                             casterRect.rect.height *
                             (1f - casterRect.pivot.y) *
                             Mathf.Abs(casterRect.localScale.y);
        iconRect.anchoredPosition = new Vector2(
            casterRect.anchoredPosition.x,
            characterTop + SpecialSkillIconSize.y * 0.5f + 8f);
        icon.raycastTarget = false;
        icon.preserveAspect = true;

        return icon;
    }

    private void HideSkillIcon(BattleUnit unit)
    {
        if (unit == null)
        {
            return;
        }

        Transform slot = GetCachedSlot(unit.Side, unit.FormationSlot);
        Image icon;

        if (slot == null ||
            !specialSkillIcons.TryGetValue(slot, out icon) ||
            icon == null)
        {
            return;
        }

        icon.gameObject.SetActive(false);
        specialSkillIconExpiresAt.Remove(icon);
    }

    private void HideSkillIcons()
    {
        foreach (KeyValuePair<Transform, Image> entry in specialSkillIcons)
        {
            if (entry.Value != null)
            {
                entry.Value.gameObject.SetActive(false);
            }
        }

        specialSkillIconExpiresAt.Clear();
    }

    private void UnbindEvents(BattleSession session)
    {
        if (session == null || session.Units == null)
        {
            return;
        }

        foreach (BattleUnit unit in session.Units)
        {
            unit.HitPointsChanged -= OnHpChanged;
            unit.Died -= HandleUnitDied;
        }
    }

    private Image GetCharImage(BattleUnit unit)
    {
        if (unit == null)
        {
            return null;
        }

        Transform slot = GetCachedSlot(unit.Side, unit.FormationSlot);

        if (slot == null)
        {
            return null;
        }

        Transform character = slot.Find("Character");
        return character == null ? null : character.GetComponent<Image>();
    }

private void HideBattleSlots()
    {
        foreach (Transform slot in transform)
        {
            slot.gameObject.SetActive(false);
        }
    }

    private string GetSlotName(BattleUnitSide side, int formationSlot)
    {
        string prefix = "AllyUnit_";

        if (side == BattleUnitSide.Enemy)
        {
            prefix = "EnemyUnit_";
        }

        return string.Format("{0}{1:00}", prefix, formationSlot);
    }


    private void Update()
    {
        ExpireSkillIcon();

        if (boundSession == null || battleTimer == null)
        {
            return;
        }

        int totalSeconds = Mathf.CeilToInt(boundSession.RemainTime);
        int minutes = totalSeconds / 60;
        int seconds = totalSeconds % 60;
        battleTimer.text = "⌛ " + minutes.ToString("00") + ":" + seconds.ToString("00");
    }

    private void ExpireSkillIcon()
    {
        float currentTime = Time.unscaledTime;

        foreach (KeyValuePair<Image, float> entry in specialSkillIconExpiresAt)
        {
            if (entry.Key == null || !entry.Key.gameObject.activeSelf)
            {
                continue;
            }

            float remainingTime = entry.Value - currentTime;

            if (remainingTime <= 0f)
            {
                entry.Key.rectTransform.localScale = Vector3.one;
                entry.Key.gameObject.SetActive(false);
                continue;
            }

            float elapsedTime = SpecialSkillIconDuration - remainingTime;
            float halfPopTime = SpecialSkillIconPopDuration * 0.5f;
            float iconScale = 1f;

            if (elapsedTime < halfPopTime)
            {
                iconScale = Mathf.Lerp(0f, 1.2f, elapsedTime / halfPopTime);
            }
            else if (elapsedTime < SpecialSkillIconPopDuration)
            {
                float settleTime = elapsedTime - halfPopTime;
                iconScale = Mathf.Lerp(1.2f, 1f, settleTime / halfPopTime);
            }

            entry.Key.rectTransform.localScale = Vector3.one * iconScale;
        }
    }

    private Text FindText(string objectName)
    {
        Text[] texts = transform.root.GetComponentsInChildren<Text>(true);

        foreach (Text text in texts)
        {
            if (text.name == objectName)
            {
                return text;
            }
        }

        return null;
    }

    private void SetStage(BattleSession session)
    {
        if (session != null && stageText != null)
        {
            stageText.text = session.Stage.DisplayName;
        }
    }

    private bool IsBoss(BattleUnit unit)
    {
        return boundSession != null && boundSession.Stage.Id == "STAGE_010" && unit.Side == BattleUnitSide.Enemy;
    }

    private void SetUnit(Transform slot, Transform character, bool isBoss)
    {
        Image image = character.GetComponent<Image>();

        if (image != null)
        {
            image.preserveAspect = true;
        }

        if (!isBoss)
        {
            return;
        }

        RectTransform slotRect = slot.GetComponent<RectTransform>();
        RectTransform charRect = character.GetComponent<RectTransform>();

        if (slotRect != null)
        {
            slotRect.sizeDelta = new Vector2(500f, 560f);
        }

        if (charRect != null)
        {
            charRect.sizeDelta = new Vector2(480f, 540f);
            charRect.anchoredPosition = new Vector2(0f, -25f);
        }
    }

    private void SetHpBar(Transform slot, bool isBoss)
    {
        if (slot == null)
        {
            return;
        }

        Transform status = slot.Find("StatusOverlay");
        Transform hpBack = status == null ? slot.Find("HP_Back") : status.Find("HP_Back");
        Transform hpFill = status == null ? slot.Find("HP_Fill") : status.Find("HP_Fill");
        Vector2 barSize = isBoss ? new Vector2(420f, 26f) : new Vector2(104f, 15f);
        Vector2 barPosition = isBoss ? new Vector2(0f, 245f) : new Vector2(0f, 90f);
        RectTransform backRect = hpBack == null ? null : hpBack.GetComponent<RectTransform>();
        RectTransform fillRect = hpFill == null ? null : hpFill.GetComponent<RectTransform>();

        SetBar(backRect, barSize, barPosition);
        SetBar(fillRect, barSize, barPosition);
    }

    private void SetBar(RectTransform rect, Vector2 size, Vector2 position)
    {
        if (rect == null)
        {
            return;
        }

        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
    }
}
