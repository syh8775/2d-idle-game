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
    public int ManaCost;
    public float CooldownSeconds;
    public float EffectMultiplier;
    public string IconAssetPath;
    public string Status;
}

[Serializable]
public class CharacterDefinition
{
    public string Id;
    public string DisplayName;
    public string Rarity;
    public string Role;
    public string BattleAssetPath;
    public int HitPoints;
    public int Attack;
    public int Defense;
    public int Speed;
    public string NormalSkillId;
    public string SpecialSkillId;
    public string Status;
}

[Serializable]
public class EnemyDefinition
{
    public string Id;
    public string DisplayName;
    public string BattleAssetPath;
    public int HitPoints;
    public int Attack;
    public int Defense;
    public int Speed;
    public string Status;
}

[Serializable]
public class StageDefinition
{
    public string Id;
    public string DisplayName;
    public string ChapterId;
    public string FormationId;
    public int WaveCount;
    public float TimeLimitSeconds;
    public string RewardId;
    public int PartySize;
    public string Status;
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
    public float SpawnDelaySeconds;
    public string Status;
}

[Serializable]
public class PartySlotDefinition
{
    public string Id;
    public string Side;
    public string DefaultCharacterId;
    public int FormationSlot;
    public string Status;
}

[Serializable]
public class RewardDefinition
{
    public string Id;
    public string StageId;
    public int Gold;
    public string Status;
}
