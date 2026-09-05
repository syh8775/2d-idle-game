using System;

public enum BattleSessionState
{
    None,
    Ready,
    Running,
    Completed,
    Cancelled
}

public enum BattleOutcome
{
    None,
    Victory,
    Defeat,
    Timeout,
    Cancelled
}

[Serializable]
public class SkillDefinition
{
    public string Id;
    public string CharacterId;
    public string SkillType;
    public string EffectType;
    public string DisplayName;
    public float CooldownSeconds;
    public float EffectMultiplier;
}

[Serializable]
public class CharacterDefinition
{
    public string Id;
    public string DisplayName;
    public string Rarity;
    public string Role;
    public int HitPoints;
    public int Attack;
    public int Defense;
    public int Speed;
    public string SpecialSkillId;
}

[Serializable]
public class EnemyDefinition
{
    public string Id;
    public string DisplayName;
    public int HitPoints;
    public int Attack;
    public int Defense;
    public int Speed;
}

[Serializable]
public class StageDefinition
{
    public string Id;
    public string DisplayName;
    public string FormationId;
    public float TimeLimitSeconds;
    public string RewardId;
    public int PartySize;
}

[Serializable]
public class EnemyFormationSlotDefinition
{
    public string FormationId;
    public string StageId;
    public int FormationSlot;
    public string EnemyId;
    public int Row;
    public int Column;
}

[Serializable]
public class PartySlotDefinition
{
    public string Id;
    public string Side;
    public string DefaultCharacterId;
    public int FormationSlot;
}

[Serializable]
public class RewardDefinition
{
    public string Id;
    public string StageId;
    public int Gold;
}
