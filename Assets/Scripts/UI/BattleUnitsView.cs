using UnityEngine;
using UnityEngine.UI;

public class BattleUnitsView : MonoBehaviour
{
    // 기존 씬에 연결된 CHAR_001 공격 프레임입니다.
    [SerializeField] private Sprite[] allyAttackFrames = new Sprite[0];
    // 다음 캐릭터를 같은 재생 코드로 시험하기 위한 CHAR_002 공격 프레임입니다.
    [SerializeField] private Sprite[] char002AttackFrames = new Sprite[0];
    [SerializeField] private float char001AttackFrameDuration = 0.02f;
    [SerializeField] private float char002AttackFrameDuration = 0.02f;
    [SerializeField] private int char001AttackHitFrame = 17;
    [SerializeField] private int char002AttackHitFrame = 17;

    private BattleManager battleManager;
    private BattleSession boundSession;
    private BattleUnitFeedbackView feedbackView;
    private BattleResultView resultView;

    private Transform[] allySlots = new Transform[10];
    private Transform[] enemySlots = new Transform[10];

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

        feedbackView = new BattleUnitFeedbackView(
            this,
            allyAttackFrames,
            char002AttackFrames,
            char001AttackFrameDuration,
            char002AttackFrameDuration,
            char001AttackHitFrame,
            char002AttackHitFrame);
        resultView = new BattleResultView(this);
        resultView.Initialize(transform);
        resultView.RetryRequested += HandleRetryRequested;

        CacheSlots();

        InvokeRepeating("RefreshCurrentStatuses", 0f, 0.05f);

        battleManager.SessionStarted += HandleSessionStarted;
        battleManager.SessionCompleted += HandleSessionCompleted;

        if (battleManager.CurrentSession != null)
        {
            BindUnits(battleManager.CurrentSession);
        }
    }

    private void RefreshCurrentStatuses()
    {
        if (battleManager == null || battleManager.CurrentSession == null)
        {
            return;
        }

        RefreshStatuses(battleManager.CurrentSession);
    }

    private void OnDestroy()
    {
        if (battleManager != null)
        {
            battleManager.SessionStarted -= HandleSessionStarted;
            battleManager.SessionCompleted -= HandleSessionCompleted;
        }

        if (boundSession != null)
        {
            boundSession.AttackResolved -= HandleAttackResolved;
        }

        if (resultView != null)
        {
            resultView.RetryRequested -= HandleRetryRequested;
            resultView.Dispose();
        }
    }

    private void HandleSessionStarted(BattleSession session)
    {
        if (resultView != null)
        {
            resultView.Hide();
        }

        BindUnits(session);
    }

    private void HandleSessionCompleted(BattleSession session)
    {
        if (resultView != null)
        {
            resultView.Show(session);
        }
    }

    private void HandleRetryRequested()
    {
        if (battleManager == null || !battleManager.RestartCurrentStage())
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
        feedbackView.ResetSession();

        if (boundSession != null)
        {
            boundSession.AttackResolved -= HandleAttackResolved;
        }

        boundSession = session;
        boundSession.AttackResolved += HandleAttackResolved;

        HideAllBattleSlots();

        int allyCount = 0;
        int enemyCount = 0;

        foreach (BattleUnit unit in session.Units)
        {
            if (!BindUnit(unit))
            {
                continue;
            }

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

    private void CacheSlots()
    {
        for (int i = 1; i < allySlots.Length; i++)
        {
            allySlots[i] = transform.Find("AllyUnit_" + i.ToString("00"));
            enemySlots[i] = transform.Find("EnemyUnit_" + i.ToString("00"));
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

        Transform slot = GetCachedSlot(
            unit.Side,
            unit.FormationSlot);

        if (slot == null)
        {
            return;
        }

        Transform character = slot.Find("Character");

        if (character != null)
        {
            if (unit.IsAlive)
            {
                character.gameObject.SetActive(true);
            }
            else
            {
                Image characterImage = character.GetComponent<Image>();

                if (characterImage != null && feedbackView != null)
                {
                    feedbackView.StartDeathFade(unit, characterImage);
                }
            }
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

        float hpRatio = 0f;

        if (unit.MaxHitPoints > 0)
        {
            hpRatio = (float)unit.CurrentHitPoints / unit.MaxHitPoints;
        }

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
        Transform slot = transform.Find(slotName);

        if (slot == null)
        {
            Debug.LogError("전투 화면 슬롯을 찾을 수 없습니다: " + slotName);
            return false;
        }

        Transform character = slot.Find("Character");

        if (character == null)
        {
            Debug.LogError(
                "전투 화면 슬롯에 Character 오브젝트가 없습니다: " +
                slotName);
            return false;
        }

        Image image = character.GetComponent<Image>();

        if (image == null)
        {
            Debug.LogError(
                "전투 화면 슬롯의 Character에 Image가 없습니다: " +
                slotName);
            return false;
        }

        feedbackView.RegisterCharacter(image);
        slot.gameObject.SetActive(true);
        character.gameObject.SetActive(true);
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

    private void HandleAttackResolved(
        BattleUnit attacker,
        BattleUnit target,
        int damage)
    {
        if (damage <= 0 || feedbackView == null)
        {
            return;
        }

        Image attackerImage = GetCharacterImage(attacker);
        Image targetImage = GetCharacterImage(target);
        StartCoroutine(
            feedbackView.PlayAttackFeedback(
                attacker,
                target,
                attackerImage,
                targetImage));
    }

    private Image GetCharacterImage(BattleUnit unit)
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

    private void HideAllBattleSlots()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform slot = transform.GetChild(i);

            if (!IsBattleSlot(slot.name))
            {
                continue;
            }

            slot.gameObject.SetActive(false);
        }
    }

    private bool IsBattleSlot(string slotName)
    {
        return slotName.StartsWith("AllyUnit_") ||
               slotName.StartsWith("EnemyUnit_");
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
}
