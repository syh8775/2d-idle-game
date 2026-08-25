using System;

public enum BattleUnitSide
{
    Ally,
    Enemy
}

public enum BattleUnitState
{
    Idle,
    Moving,
    Attacking,
    Hit,
    Dead
}

[Serializable]
public class BattleUnit
{
    public string Id { get; private set; }
    public string DisplayName { get; private set; }
    public string BattleAssetPath { get; private set; }
    public string MotionAssetFolder { get; private set; }
    public BattleUnitSide Side { get; private set; }
    public int FormationSlot { get; private set; }
    public int Row { get; private set; }
    public int Column { get; private set; }
    public int MaxHitPoints { get; private set; }
    public int CurrentHitPoints { get; private set; }
    public int Attack { get; private set; }
    public int Defense { get; private set; }
    public int Speed { get; private set; }
    public BattleUnitState State { get; private set; }
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
    public BattleUnit(CharacterDefinition definition, PartyMember member)
    {
        if (definition == null || member == null)
        {
            throw new Exception("아군 전투 유닛 데이터가 비어 있습니다.");
        }

        Id = definition.Id;
        DisplayName = definition.DisplayName;
        BattleAssetPath = definition.BattleAssetPath;
        MotionAssetFolder = definition.MotionAssetFolder;
        Side = BattleUnitSide.Ally;
        FormationSlot = member.FormationSlot;

        Row = (member.FormationSlot - 1) / 3 + 1;
        Column = (member.FormationSlot - 1) % 3 + 1;

        MaxHitPoints = definition.HitPoints;
        CurrentHitPoints = definition.HitPoints;
        Attack = definition.Attack;
        Defense = definition.Defense;
        Speed = definition.Speed;
        State = BattleUnitState.Idle;
        attackTimer = 0f;
    }

    public BattleUnit(EnemyDefinition definition, EnemyFormationSlotDefinition slot)
    {
        if (definition == null || slot == null)
        {
            throw new Exception("적 전투 유닛 데이터가 비어 있습니다.");
        }

        Id = definition.Id;
        DisplayName = definition.DisplayName;
        BattleAssetPath = definition.BattleAssetPath;
        MotionAssetFolder = definition.MotionAssetFolder;
        Side = BattleUnitSide.Enemy;
        FormationSlot = slot.FormationSlot;
        Row = slot.Row;
        Column = slot.Column;
        MaxHitPoints = definition.HitPoints;
        CurrentHitPoints = definition.HitPoints;
        Attack = definition.Attack;
        Defense = definition.Defense;
        Speed = definition.Speed;
        State = BattleUnitState.Idle;
        attackTimer = 0f;
    }

    public void TickAttackCooldown(float deltaSeconds)
    {
        if (deltaSeconds <= 0f || attackTimer <= 0f)
        {
            return;
        }

        attackTimer -= deltaSeconds;
    }

    public void ResetAttackCooldown()
    {
        attackTimer = GetAttackInterval();
    }

    private float GetAttackInterval()
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

        int actualDamage = damage - Defense;
        if (actualDamage < 1)
        {
            actualDamage = 1;
        }

        CurrentHitPoints -= actualDamage;

        if (CurrentHitPoints <= 0)
        {
            CurrentHitPoints = 0;
            ChangeState(BattleUnitState.Dead);
        }
        else
        {
            ChangeState(BattleUnitState.Hit);
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

    public void ChangeState(BattleUnitState nextState)
    {
        if (State == BattleUnitState.Dead && nextState != BattleUnitState.Dead)
        {
            return;
        }

        State = nextState;
    }
}
