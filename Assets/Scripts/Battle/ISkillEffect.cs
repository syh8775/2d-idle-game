public interface ISkillEffect
{
    bool Apply(BattleSession session, BattleUnit caster, SkillDefinition skill);
}
