using System;

public static class DataValidator
{
    public static void Validate(DataManager dataManager)
    {
        CheckCharacters(dataManager);
        ValidateSkills(dataManager);
        ValidateStages(dataManager);
        ValidateRewards(dataManager);
        CheckPartySlots(dataManager);
        CheckEnemyForms(dataManager);
    }

    private static void CheckCharacters(DataManager dataManager)
    {
        foreach (CharacterDefinition character in dataManager.Characters.Values)
        {
            if (character.HitPoints <= 0 || character.Attack < 0 || character.Defense < 0 || character.Speed <= 0)
            {
                throw new FormatException("캐릭터 능력치 범위가 올바르지 않습니다: " + character.Id);
            }

            CheckCharSkill(dataManager, character, character.SpecialSkillId);
        }
    }

    private static void CheckCharSkill(DataManager dataManager, CharacterDefinition character, string skillId)
    {
        SkillDefinition skill;

        if (!dataManager.TryGetSkill(skillId, out skill))
        {
            throw new FormatException("캐릭터가 참조하는 스킬을 찾을 수 없습니다" + character.Id + " -> " + skillId);
        }

        if (!string.Equals(skill.CharacterId, character.Id, StringComparison.OrdinalIgnoreCase))
        {
            throw new FormatException("스킬의 캐릭터 ID가 일치하지 않습니다" + skill.Id + " -> " + skill.CharacterId);
        }
    }

    private static void ValidateSkills(DataManager dataManager)
    {
        foreach (SkillDefinition skill in dataManager.Skills.Values)
        {
            if (skill.CooldownSeconds < 0f || skill.EffectMultiplier < 0f)
            {
                throw new FormatException("스킬 수치 범위가 올바르지 않습니다: " + skill.Id);
            }

            CharacterDefinition character;

            if (!dataManager.TryGetCharacter(skill.CharacterId, out character))
            {
                throw new FormatException("스킬이 참조하는 캐릭터를 찾을 수 없습니다" + skill.Id + " -> " + skill.CharacterId);
            }
        }
    }

    private static void ValidateStages(DataManager dataManager)
    {
        foreach(StageDefinition stage in dataManager.Stages.Values)
        {
            if (stage.TimeLimitSeconds <= 0f || stage.PartySize < 1 || stage.PartySize > 9)
            {
                throw new FormatException("스테이지 수치 범위가 올바르지 않습니다: " + stage.Id);
            }

            RewardDefinition reward;

            if (!dataManager.TryGetReward(stage.RewardId, out reward))
            {
                throw new FormatException("스테이지가 참조하는 보상을 찾을 수 없습니다" + stage.Id + " -> " + stage.RewardId);
            }

            if (!string.Equals(reward.StageId, stage.Id, StringComparison.OrdinalIgnoreCase))
            {
                throw new FormatException("보상의 스테이지 ID가 일치하지 않습니다" + reward.Id + " -> " + reward.StageId);
            }

            CheckStageForm(dataManager, stage);
        }
    }

    private static void ValidateRewards(DataManager dataManager)
    {
        foreach (RewardDefinition reward in dataManager.Rewards.Values)
        {
            if (reward.Gold < 0)
            {
                throw new FormatException("보상 금액이 음수입니다: " + reward.Id);
            }

            StageDefinition stage;

            if (!dataManager.TryGetStage(reward.StageId, out stage))
            {
                throw new FormatException("보상이 참조하는 스테이지를 찾을 수 없습니다" + reward.Id + " -> " + reward.StageId);
            }
        }
    }

    private static void CheckStageForm(DataManager dataManager, StageDefinition stage)
    {
        bool formationFound = false;

        foreach (EnemyFormationSlotDefinition formation in dataManager.EnemyFormationSlots)
        {
            bool sameStage = string.Equals(formation.StageId, stage.Id, StringComparison.OrdinalIgnoreCase);
            bool sameFormation = string.Equals(formation.FormationId, stage.FormationId, StringComparison.OrdinalIgnoreCase);

            if (sameStage && sameFormation)
            {
                formationFound = true;
                break;
            }
        }

        if (!formationFound)
        {
            throw new FormatException("스테이지가 참조하는 적 편성 슬롯을 찾을 수 없습니다" + stage.Id + " -> " + stage.FormationId);
        }
    }

    private static void CheckPartySlots(DataManager dataManager)
    {
        foreach (PartySlotDefinition partySlot in dataManager.PartySlots.Values)
        {
            if (partySlot.FormationSlot < 1 || partySlot.FormationSlot > 9)
            {
                throw new FormatException("파티 슬롯 범위가 올바르지 않습니다: " + partySlot.Id);
            }

            CharacterDefinition character;

            if (!dataManager.TryGetCharacter(partySlot.DefaultCharacterId, out character))
            {
                throw new FormatException("파티 슬롯이 참조하는 캐릭터를 찾을 수 없습니다" + partySlot.Id + " -> " + partySlot.DefaultCharacterId);
            }
        }
    }

    private static void CheckEnemyForms(DataManager dataManager)
    {
        foreach (EnemyFormationSlotDefinition formation in dataManager.EnemyFormationSlots)
        {
            if (formation.FormationSlot < 1 || formation.FormationSlot > 9)
            {
                throw new FormatException("적 편성 수치 범위가 올바르지 않습니다: " + formation.FormationId);
            }

            StageDefinition stage;
            EnemyDefinition enemy;

            if (!dataManager.TryGetStage(formation.StageId, out stage))
            {
                throw new FormatException("적 편성 슬롯이 참조하는 스테이지를 찾을 수 없습니다" + formation.FormationId + " -> " + formation.StageId);
            }

            if (!dataManager.TryGetEnemy(formation.EnemyId, out enemy))
            {
                throw new FormatException("적 편성 슬롯이 참조하는 적을 찾을 수 없습니다" + formation.FormationId + " -> " + formation.EnemyId);
            }

            if (!string.Equals(stage.FormationId, formation.FormationId, StringComparison.OrdinalIgnoreCase))
            {
                throw new FormatException("스테이지와 적 편성 ID가 일치하지 않습니다" + stage.Id + " -> " + formation.FormationId);
            }
        }
    }
}
