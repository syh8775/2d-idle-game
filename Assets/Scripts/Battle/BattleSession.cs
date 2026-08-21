using System;
using System.Collections.Generic;

public class BattleSession
{
    public StageDefinition Stage { get; private set; }
    public BattleSessionState State { get; private set; }
    public BattleOutcome Outcome { get; private set; }
    public float ElapsedSeconds { get; private set; }
    public List<BattleUnit> Units { get; private set; }

    public bool IsFinished
    {
        get
        {
            return State == BattleSessionState.Completed ||
                   State == BattleSessionState.Cancelled;
        }
    }

    public event Action<BattleSession> StateChanged;
    public event Action<BattleUnit, BattleUnit, int> AttackResolved;

    public BattleSession(StageDefinition stage, DataManager dataManager)
    {
        if (stage == null)
        {
            throw new Exception("전투를 시작하려면 스테이지 데이터가 필요합니다.");
        }

        if (dataManager == null)
        {
            throw new Exception("전투를 시작하려면 데이터 매니저가 필요합니다.");
        }

        Stage = stage;
        State = BattleSessionState.Ready;
        Outcome = BattleOutcome.None;
        ElapsedSeconds = 0f;
        Units = new List<BattleUnit>();

        CreateUnits(dataManager);
    }

    public int GetUnitCount(BattleUnitSide side)
    {
        int count = 0;

        foreach (BattleUnit unit in Units)
        {
            if (unit.Side == side)
            {
                count++;
            }
        }

        return count;
    }

    public void Start()
    {
        if (State != BattleSessionState.Ready)
        {
            return;
        }

        SetState(BattleSessionState.Running);
    }

    public void Tick(float deltaSeconds)
    {
        if (State != BattleSessionState.Running || deltaSeconds <= 0f)
        {
            return;
        }

        ElapsedSeconds += deltaSeconds;

        if (Stage.TimeLimitSeconds > 0f &&
            ElapsedSeconds >= Stage.TimeLimitSeconds)
        {
            Complete(BattleOutcome.Timeout);
            return;
        }

        ProcessAttacks(deltaSeconds);
    }
    public void Complete(BattleOutcome outcome)
    {
        if (IsFinished || outcome == BattleOutcome.None)
        {
            return;
        }

        Outcome = outcome;
        SetState(BattleSessionState.Completed);
    }

    public void Cancel()
    {
        if (IsFinished)
        {
            return;
        }

        Outcome = BattleOutcome.Cancelled;
        SetState(BattleSessionState.Cancelled);
    }

    public int ApplyDamage(BattleUnit target, int damage)
    {
        if (State != BattleSessionState.Running ||
            target == null ||
            !Units.Contains(target))
        {
            return 0;
        }

        int actualDamage = target.ApplyDamage(damage);
        CheckBattleOutcome();
        return actualDamage;
    }

    private void CheckBattleOutcome()
    {
        int aliveAllies = 0;
        int aliveEnemies = 0;

        foreach (BattleUnit unit in Units)
        {
            if (!unit.IsAlive)
            {
                continue;
            }

            if (unit.Side == BattleUnitSide.Ally)
            {
                aliveAllies++;
            }
            else
            {
                aliveEnemies++;
            }
        }

        if (aliveEnemies == 0)
        {
            Complete(BattleOutcome.Victory);
            return;
        }

        if (aliveAllies == 0)
        {
            Complete(BattleOutcome.Defeat);
        }
    }

    private void ProcessAttacks(float deltaSeconds)
    {
        foreach (BattleUnit unit in Units)
        {
            if (!unit.IsAlive)
            {
                continue;
            }

            unit.TickAttackCooldown(deltaSeconds);
            if (!unit.CanAttack)
            {
                continue;
            }

            BattleUnit target = FindTarget(unit);
            if (target == null)
            {
                continue;
            }

            unit.ChangeState(BattleUnitState.Attacking);
            int actualDamage = ApplyDamage(target, unit.Attack);

            if (AttackResolved != null)
            {
                AttackResolved(unit, target, actualDamage);
            }

            unit.ResetAttackCooldown();

            if (State != BattleSessionState.Running)
            {
                return;
            }

            unit.ChangeState(BattleUnitState.Idle);
        }
    }

    private BattleUnit FindTarget(BattleUnit attacker)
    {
        BattleUnit closestTarget = null;
        int closestDistance = int.MaxValue;

        foreach (BattleUnit unit in Units)
        {
            if (!unit.IsAlive || unit.Side == attacker.Side)
            {
                continue;
            }

            int distance =
                Math.Abs(attacker.Row - unit.Row) +
                Math.Abs(attacker.Column - unit.Column);

            if (closestTarget == null ||
                distance < closestDistance ||
                distance == closestDistance &&
                unit.FormationSlot < closestTarget.FormationSlot)
            {
                closestTarget = unit;
                closestDistance = distance;
            }
        }

        return closestTarget;
    }
    private void CreateUnits(DataManager dataManager)
    {
        int allyCount = 0;
        List<PartySlotDefinition> partySlots =
            new List<PartySlotDefinition>(dataManager.PartySlots.Values);
        partySlots.Sort(ComparePartySlots);

        foreach (PartySlotDefinition slot in partySlots)
        {
            if (slot.Side != "Ally" || allyCount >= Stage.PartySize)
            {
                continue;
            }

            CharacterDefinition character;
            if (!dataManager.TryGetCharacter(
                    slot.DefaultCharacterId, out character))
            {
                throw new Exception(
                    "아군 캐릭터 데이터를 찾을 수 없습니다: " +
                    slot.DefaultCharacterId);
            }

            Units.Add(new BattleUnit(character, slot));
            allyCount++;
        }

        if (allyCount == 0)
        {
            throw new Exception("전투에 배치할 아군이 없습니다.");
        }

        int enemyCount = 0;

        foreach (EnemyFormationSlotDefinition slot in
                 dataManager.EnemyFormationSlots)
        {
            if (slot.FormationId != Stage.FormationId ||
                slot.StageId != Stage.Id)
            {
                continue;
            }

            EnemyDefinition enemy;
            if (!dataManager.TryGetEnemy(slot.EnemyId, out enemy))
            {
                throw new Exception(
                    "적 데이터를 찾을 수 없습니다: " + slot.EnemyId);
            }

            Units.Add(new BattleUnit(enemy, slot));
            enemyCount++;
        }

        if (enemyCount == 0)
        {
            throw new Exception("전투에 배치할 적이 없습니다.");
        }
    }

    private static int ComparePartySlots(
        PartySlotDefinition left,
        PartySlotDefinition right)
    {
        return left.SlotIndex.CompareTo(right.SlotIndex);
    }

    private void SetState(BattleSessionState nextState)
    {
        State = nextState;

        if (StateChanged != null)
        {
            StateChanged(this);
        }
    }
}
