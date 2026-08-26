using System;
using System.Collections.Generic;

public class BattleSession
{
    public StageDefinition Stage { get; private set; }
    public BattleSessionState State { get; private set; }
    public BattleOutcome Outcome { get; private set; }
    public float ElapsedSeconds { get; private set; }
    public List<BattleUnit> Units { get; private set; }
    public List<BattleSkill> Skills { get; private set; }
    public bool IsAutoEnabled { get; private set; }
    public int RewardGold { get; private set; }

    private int nextAutoSkillIndex;

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
    public event Action<BattleSkill> SkillUsed;

    public BattleSession(StageDefinition stage, DataManager dataManager, PartyFormation formation, PlayerProgressModel progress)
    {
        if (stage == null)
        {
            throw new Exception("전투를 시작하려면 스테이지 데이터가 필요합니다.");
        }

        if (dataManager == null)
        {
            throw new Exception("전투를 시작하려면 데이터 매니저가 필요합니다.");
        }

        if (formation == null)
        {
            throw new Exception("전투를 시작하려면 파티 편성이 필요합니다.");
        }

        if (progress == null)
        {
            throw new Exception("전투를 시작하려면 진행 데이터가 필요합니다.");
        }

        Stage = stage;
        State = BattleSessionState.Ready;
        Outcome = BattleOutcome.None;
        ElapsedSeconds = 0f;
        Units = new List<BattleUnit>();
        Skills = new List<BattleSkill>();
        IsAutoEnabled = false;
        nextAutoSkillIndex = 0;

        CreateUnits(dataManager, formation, progress);
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

    public void SetAutoEnabled(bool isEnabled)
    {
        IsAutoEnabled = isEnabled;
    }

    public bool TryUseSkill(int skillIndex)
    {
        if (State != BattleSessionState.Running || skillIndex < 0 || skillIndex >= Skills.Count)
        {
            return false;
        }

        BattleSkill skill = Skills[skillIndex];

        if (!skill.TryUse(this))
        {
            return false;
        }

        if (SkillUsed != null)
        {
            SkillUsed(skill);
        }

        return true;
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

        if (State == BattleSessionState.Running)
        {
            ProcessSkills(deltaSeconds);
        }
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

    public void SetReward(int gold)
    {
        if (RewardGold == 0 && gold > 0)
        {
            RewardGold = gold;
        }
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

    public int ApplyHealing(BattleUnit target, int healing)
    {
        if (State != BattleSessionState.Running || target == null || !Units.Contains(target))
        {
            return 0;
        }

        return target.ApplyHealing(healing);
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

    private void ProcessSkills(float deltaSeconds)
    {
        foreach (BattleSkill skill in Skills)
        {
            skill.Tick(deltaSeconds);
        }

        if (!IsAutoEnabled || Skills.Count == 0)
        {
            return;
        }

        for (int checkedCount = 0; checkedCount < Skills.Count; checkedCount++)
        {
            int skillIndex = (nextAutoSkillIndex + checkedCount) % Skills.Count;

            if (!TryUseSkill(skillIndex))
            {
                continue;
            }

            nextAutoSkillIndex = (skillIndex + 1) % Skills.Count;
            return;
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

    public BattleUnit FindTarget(BattleUnit attacker)
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
    private void CreateUnits(DataManager dataManager, PartyFormation formation, PlayerProgressModel progress)
    {
        int allyCount = 0;
        List<PartyMember> partySlots = new List<PartyMember>(formation.Members);
        partySlots.Sort(CompareMembers);

        foreach (PartyMember member in partySlots)
        {
            if (allyCount >= Stage.PartySize)
            {
                break;
            }

            CharacterDefinition character;

            if (!dataManager.TryGetCharacter(member.CharacterId, out character))
            {
                throw new Exception("아군 캐릭터 데이터를 찾을 수 없습니다: " + member.CharacterId);
            }

            CharacterProgressModel characterProgress = progress.GetCharacter(member.CharacterId);
            int level = 1;

            if (characterProgress != null)
            {
                level = characterProgress.Level;
            }

            BattleUnit ally = new BattleUnit(character, member, level);

            Units.Add(ally);
            CreateBattleSkill(ally, character, dataManager);
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

    private void CreateBattleSkill(BattleUnit caster, CharacterDefinition character, DataManager dataManager)
    {
        SkillDefinition skill;

        if (!dataManager.TryGetSkill(character.SpecialSkillId, out skill))
        {
            throw new Exception(
                "아군 특수 스킬 데이터를 찾을 수 없습니다: " + character.SpecialSkillId);
        }

        ISkillEffect effect = CreateSkillEffect(skill.EffectType);
        Skills.Add(new BattleSkill(skill, caster, effect));
    }

    private static ISkillEffect CreateSkillEffect(string effectType)
    {
        switch (effectType)
        {
            case "SingleDamage": return new SingleDamageEffect();
            case "AreaDamage": return new AreaDamageEffect();
            case "Heal": return new HealEffect();
            default: throw new Exception("지원하지 않는 스킬 효과입니다." + effectType);
        }
    }

    private static int CompareMembers(PartyMember left, PartyMember right)
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
