using System;

public enum BattleUnitSide
{
    Ally,
    Enemy
}

[Serializable]
public class BattleUnit
{
    public string Id { get; private set; }
    public string Role { get; private set; }
    public BattleUnitSide Side { get; private set; }
    public int FormationSlot { get; private set; }
    public int Row { get; private set; }
    public int Column { get; private set; }
    public int MaxHitPoints { get; private set; }
    public int CurrentHitPoints { get; private set; }
    public int Attack { get; private set; }
    public int Defense { get; private set; }
    public int Speed { get; private set; }
    private float attackTimer;
    public event Action<BattleUnit> HitPointsChanged;
    public event Action<BattleUnit> Died;

    public bool IsAlive
    {
        get { return CurrentHitPoints > 0; }
    }

    public bool CanAttack
    {
        get { return IsAlive && attackTimer <= 0f; }
    }
    public BattleUnit(CharacterDefinition definition, PartyMember member, int level)
    {
        if (definition == null || member == null)
        {
            throw new Exception("아군 전투 유닛 데이터가 비어 있습니다.");
        }

        Id = definition.Id;
        Role = definition.Role;
        Side = BattleUnitSide.Ally;
        FormationSlot = member.FormationSlot;

        // 화면의 7·4·1 / 8·5·2 / 9·6·3 배치: 오른쪽 1·2·3이 전열입니다.
        Row = (member.FormationSlot - 1) % 3 + 1;
        Column = (member.FormationSlot - 1) / 3 + 1;

        MaxHitPoints = GameUtil.GetLevelStat(definition.HitPoints, level);
        CurrentHitPoints = MaxHitPoints;
        Attack = GameUtil.GetLevelStat(definition.Attack, level);
        Defense = GameUtil.GetLevelStat(definition.Defense, level);
        Speed = definition.Speed;
        attackTimer = 0f;
    }

    public BattleUnit(EnemyDefinition definition, EnemyFormationSlotDefinition slot)
    {
        if (definition == null || slot == null)
        {
            throw new Exception("적 전투 유닛 데이터가 비어 있습니다.");
        }

        Id = definition.Id;
        Role = string.Empty;
        Side = BattleUnitSide.Enemy;
        FormationSlot = slot.FormationSlot;
        Row = slot.Row;
        Column = slot.Column;

        int stageIncrease = GetStageBoost(slot.StageId);
        bool isBoss = definition.Id == "ENEMY_BOSS";

        MaxHitPoints = isBoss ? definition.HitPoints : GetStageStat(definition.HitPoints, stageIncrease, 10);
        CurrentHitPoints = MaxHitPoints;
        Attack = isBoss ? definition.Attack : GetStageStat(definition.Attack, stageIncrease, 6);
        Defense = definition.Defense;
        Speed = definition.Speed;
        attackTimer = 0f;
    }

    private static int GetStageBoost(string stageId)
    {
        const string prefix = "STAGE_";

        int stageNumber;
        if (string.IsNullOrEmpty(stageId) ||
            !stageId.StartsWith(prefix) ||
            !int.TryParse(stageId.Substring(prefix.Length), out stageNumber))
        {
            return 0;
        }

        return Math.Max(0, stageNumber - 1);
    }

    private static int GetStageStat(int baseStat, int stageIncrease, int growthPercent)
    {
        // 1스테이지 기본값을 기준으로 매 스테이지 같은 비율만큼 고정 가산합니다.
        return (baseStat * (100 + stageIncrease * growthPercent) + 50) / 100;
    }

    public void TickAttackCd(float deltaSeconds)
    {
        if (deltaSeconds <= 0f || attackTimer <= 0f)
        {
            return;
        }

        attackTimer -= deltaSeconds;
    }

    public void ResetAttackCd()
    {
        attackTimer = GetAttackDelay();
    }

    private float GetAttackDelay()
    {
        if (Speed <= 0)
        {
            return 1f;
        }

        return 100f / Speed;
    }
    public int ApplyDamage(int damage)
    {
        if (!IsAlive || damage <= 0)
        {
            return 0;
        }

        // 방어력이 기준값 100과 같으면 피해가 절반이 되며,
        // 방어력이 공격력보다 높아도 피해가 갑자기 1로 고정되지 않습니다.
        const int DefenseBase = 100;
        int actualDamage = (int)Math.Round(
            damage * DefenseBase / (double)(DefenseBase + Defense));

        if (actualDamage < 1)
        {
            actualDamage = 1;
        }

        CurrentHitPoints -= actualDamage;

        if (CurrentHitPoints <= 0)
        {
            CurrentHitPoints = 0;
        }

        if (HitPointsChanged != null)
        {
            HitPointsChanged.Invoke(this);
        }

        if (!IsAlive && Died != null)
        {
            Died(this);
        }

        return actualDamage;
    }

    public int ApplyHealing(int healing)
    {
        if (!IsAlive || healing <= 0 || CurrentHitPoints >= MaxHitPoints)
        {
            return 0;
        }

        int previousHitPoints = CurrentHitPoints;

        CurrentHitPoints += healing;
        if (CurrentHitPoints > MaxHitPoints)
        {
            CurrentHitPoints = MaxHitPoints;
        }

        int actualHealing = CurrentHitPoints - previousHitPoints;

        if (HitPointsChanged != null)
        {
            HitPointsChanged(this);
        }
        return actualHealing;
    }


}
