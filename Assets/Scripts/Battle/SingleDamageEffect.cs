public class SingleDamageEffect : ISkillEffect
{
    public bool Apply(BattleSession session, BattleUnit caster, SkillDefinition skill)
    {
        if (session == null || caster == null || skill == null || !caster.IsAlive)
        {
            return false;
        }

        BattleUnit target = session.FindTarget(caster);
        if (target == null)
        {
            return false;
        }

        int damage = (int)(caster.Attack * skill.EffectMultiplier);
        return session.ApplyDamage(target, damage) > 0;
    }
}
