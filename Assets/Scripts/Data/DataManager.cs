using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

public class DataManager : MonoBehaviour
{
    [Header("CSV Sources")]
    [SerializeField] private TextAsset charactersCsv;
    [SerializeField] private TextAsset skillsCsv;
    [SerializeField] private TextAsset enemiesCsv;
    [SerializeField] private TextAsset enemyFormationsCsv;
    [SerializeField] private TextAsset stagesCsv;
    [SerializeField] private TextAsset rewardsCsv;
    [SerializeField] private TextAsset partySlotsCsv;

    private Dictionary<string, CharacterDefinition> characters =
        new Dictionary<string, CharacterDefinition>(StringComparer.OrdinalIgnoreCase);
    private Dictionary<string, SkillDefinition> skills =
        new Dictionary<string, SkillDefinition>(StringComparer.OrdinalIgnoreCase);
    private Dictionary<string, EnemyDefinition> enemies =
        new Dictionary<string, EnemyDefinition>(StringComparer.OrdinalIgnoreCase);
    private Dictionary<string, StageDefinition> stages =
        new Dictionary<string, StageDefinition>(StringComparer.OrdinalIgnoreCase);
    private Dictionary<string, RewardDefinition> rewards =
        new Dictionary<string, RewardDefinition>(StringComparer.OrdinalIgnoreCase);
    private Dictionary<string, PartySlotDefinition> partySlots =
        new Dictionary<string, PartySlotDefinition>(StringComparer.OrdinalIgnoreCase);
    private List<EnemyFormationSlotDefinition> enemyFormationSlots =
        new List<EnemyFormationSlotDefinition>();

    public bool IsLoaded { get; private set; }

    public Dictionary<string, CharacterDefinition> Characters
    {
        get { return characters; }
    }

    public Dictionary<string, SkillDefinition> Skills
    {
        get { return skills; }
    }

    public Dictionary<string, EnemyDefinition> Enemies
    {
        get { return enemies; }
    }

    public Dictionary<string, StageDefinition> Stages
    {
        get { return stages; }
    }

    public Dictionary<string, RewardDefinition> Rewards
    {
        get { return rewards; }
    }

    public Dictionary<string, PartySlotDefinition> PartySlots
    {
        get { return partySlots; }
    }

    public List<EnemyFormationSlotDefinition> EnemyFormationSlots
    {
        get { return enemyFormationSlots; }
    }

    public event Action<DataManager> Loaded;

    public bool LoadAll()
    {
        ClearData();

        try
        {
            LoadCharacters();
            LoadSkills();
            LoadEnemies();
            LoadStages();
            LoadRewards();
            LoadPartySlots();
            LoadEnemyFormations();

            DataValidator.Validate(this);

            IsLoaded = true;
            Debug.Log(
                "데이터 로드 완료 - 캐릭터 " + characters.Count + "명, 스킬 " +
                skills.Count + "개, 적 " + enemies.Count + "명, 스테이지 " +
                stages.Count + "개");

            if (Loaded != null)
            {
                Loaded(this);
            }

            return true;
        }
        catch (Exception exception)
        {
            IsLoaded = false;
            Debug.LogError("데이터 로드 실패: " + exception.Message);
            return false;
        }
    }

    public bool TryGetCharacter(string id, out CharacterDefinition definition)
    {
        return characters.TryGetValue(id, out definition);
    }

    public bool TryGetSkill(string id, out SkillDefinition definition)
    {
        return skills.TryGetValue(id, out definition);
    }

    public bool TryGetEnemy(string id, out EnemyDefinition definition)
    {
        return enemies.TryGetValue(id, out definition);
    }

    public bool TryGetStage(string id, out StageDefinition definition)
    {
        return stages.TryGetValue(id, out definition);
    }

    public bool TryGetReward(string id, out RewardDefinition definition)
    {
        return rewards.TryGetValue(id, out definition);
    }

    private void ClearData()
    {
        IsLoaded = false;
        characters.Clear();
        skills.Clear();
        enemies.Clear();
        stages.Clear();
        rewards.Clear();
        partySlots.Clear();
        enemyFormationSlots.Clear();
    }

    private void LoadCharacters()
    {
        List<Dictionary<string, string>> rows = CsvParser.ReadRows(charactersCsv);

        foreach (Dictionary<string, string> row in rows)
        {
            CharacterDefinition definition = new CharacterDefinition
            {
                Id = Required(row, "id"),
                DisplayName = Required(row, "displayName"),
                Rarity = Required(row, "rarity"),
                Role = Required(row, "role"),
                BattleAssetPath = Optional(row, "battleAssetPath"),
                MotionAssetFolder = Optional(row, "motionAssetFolder"),
                HitPoints = Integer(row, "hp"),
                Attack = Integer(row, "attack"),
                Defense = Integer(row, "defense"),
                Speed = Integer(row, "speed"),
                NormalSkillId = Required(row, "normalSkillId"),
                SpecialSkillId = Required(row, "specialSkillId"),
                UltimateSkillId = Required(row, "ultimateSkillId"),
                Status = Required(row, "status")
            };

            if (characters.ContainsKey(definition.Id))
            {
                throw new FormatException("캐릭터 데이터에 중복된 ID가 있습니다: " + definition.Id);
            }

            characters.Add(definition.Id, definition);
        }
    }

    private void LoadSkills()
    {
        List<Dictionary<string, string>> rows = CsvParser.ReadRows(skillsCsv);

        foreach (Dictionary<string, string> row in rows)
        {
            SkillDefinition definition = new SkillDefinition
            {
                Id = Required(row, "id"),
                CharacterId = Required(row, "characterId"),
                SkillType = Required(row, "skillType"),
                EffectType = Required(row, "effectType"),
                DisplayName = Required(row, "displayName"),
                ManaCost = Integer(row, "manaCost"),
                CooldownSeconds = Decimal(row, "cooldownSeconds"),
                EffectMultiplier = Decimal(row, "effectMultiplier"),
                IconAssetPath = Optional(row, "iconAssetPath"),
                Status = Required(row, "status")
            };

            if (skills.ContainsKey(definition.Id))
            {
                throw new FormatException("스킬 데이터에 중복된 ID가 있습니다: " + definition.Id);
            }

            skills.Add(definition.Id, definition);
        }
    }

    private void LoadEnemies()
    {
        List<Dictionary<string, string>> rows = CsvParser.ReadRows(enemiesCsv);

        foreach (Dictionary<string, string> row in rows)
        {
            EnemyDefinition definition = new EnemyDefinition
            {
                Id = Required(row, "id"),
                DisplayName = Required(row, "displayName"),
                EnemyType = Required(row, "enemyType"),
                BattleAssetPath = Optional(row, "battleAssetPath"),
                MotionAssetFolder = Optional(row, "motionAssetFolder"),
                HitPoints = Integer(row, "hp"),
                Attack = Integer(row, "attack"),
                Defense = Integer(row, "defense"),
                Speed = Integer(row, "speed"),
                Status = Required(row, "status")
            };

            if (enemies.ContainsKey(definition.Id))
            {
                throw new FormatException("적 데이터에 중복된 ID가 있습니다: " + definition.Id);
            }

            enemies.Add(definition.Id, definition);
        }
    }

    private void LoadStages()
    {
        List<Dictionary<string, string>> rows = CsvParser.ReadRows(stagesCsv);

        foreach (Dictionary<string, string> row in rows)
        {
            StageDefinition definition = new StageDefinition
            {
                Id = Required(row, "id"),
                DisplayName = Required(row, "displayName"),
                ChapterId = Required(row, "chapterId"),
                FormationId = Required(row, "formationId"),
                WaveCount = Integer(row, "waveCount"),
                TimeLimitSeconds = Decimal(row, "timeLimitSeconds"),
                RewardId = Required(row, "rewardId"),
                PartySize = Integer(row, "partySize"),
                Status = Required(row, "status")
            };

            if (stages.ContainsKey(definition.Id))
            {
                throw new FormatException("스테이지 데이터에 중복된 ID가 있습니다: " + definition.Id);
            }

            stages.Add(definition.Id, definition);
        }
    }

    private void LoadRewards()
    {
        List<Dictionary<string, string>> rows = CsvParser.ReadRows(rewardsCsv);

        foreach (Dictionary<string, string> row in rows)
        {
            RewardDefinition definition = new RewardDefinition
            {
                Id = Required(row, "id"),
                StageId = Required(row, "stageId"),
                Gold = Integer(row, "gold"),
                Status = Required(row, "status")
            };

            if (rewards.ContainsKey(definition.Id))
            {
                throw new FormatException("보상 데이터에 중복된 ID가 있습니다: " + definition.Id);
            }

            rewards.Add(definition.Id, definition);
        }
    }

    private void LoadPartySlots()
    {
        List<Dictionary<string, string>> rows = CsvParser.ReadRows(partySlotsCsv);

        foreach (Dictionary<string, string> row in rows)
        {
            PartySlotDefinition definition = new PartySlotDefinition
            {
                Id = Required(row, "id"),
                SlotIndex = Integer(row, "slotIndex"),
                Side = Required(row, "side"),
                DefaultCharacterId = Required(row, "defaultCharacterId"),
                FormationSlot = Integer(row, "formationSlot"),
                Row = Integer(row, "row"),
                Column = Integer(row, "column"),
                Status = Required(row, "status")
            };

            if (partySlots.ContainsKey(definition.Id))
            {
                throw new FormatException("편성 칸 데이터에 중복된 ID가 있습니다: " + definition.Id);
            }

            partySlots.Add(definition.Id, definition);
        }
    }

    private void LoadEnemyFormations()
    {
        List<Dictionary<string, string>> rows = CsvParser.ReadRows(enemyFormationsCsv);

        foreach (Dictionary<string, string> row in rows)
        {
            EnemyFormationSlotDefinition definition = new EnemyFormationSlotDefinition
            {
                FormationId = Required(row, "formationId"),
                StageId = Required(row, "stageId"),
                FormationSlot = Integer(row, "formationSlot"),
                EnemyId = Required(row, "enemyId"),
                Row = Integer(row, "row"),
                Column = Integer(row, "column"),
                SpawnDelaySeconds = Decimal(row, "spawnDelaySeconds"),
                Status = Required(row, "status")
            };

            enemyFormationSlots.Add(definition);
        }
    }

    private static string Required(Dictionary<string, string> row, string key)
    {
        if (!row.ContainsKey(key) || string.IsNullOrWhiteSpace(row[key]))
        {
            throw new FormatException("필수 항목이 비어 있습니다: " + key);
        }

        return row[key];
    }

    private static string Optional(Dictionary<string, string> row, string key)
    {
        if (!row.ContainsKey(key))
        {
            return string.Empty;
        }

        return row[key];
    }

    private static int Integer(Dictionary<string, string> row, string key)
    {
        int value;
        string text = Required(row, key);

        if (!int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out value))
        {
            throw new FormatException("숫자로 바꿀 수 없는 정수 항목입니다: " + key);
        }

        return value;
    }

    private static float Decimal(Dictionary<string, string> row, string key)
    {
        float value;
        string text = Required(row, key);

        if (!float.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out value))
        {
            throw new FormatException("숫자로 바꿀 수 없는 소수 항목입니다: " + key);
        }

        return value;
    }
}
