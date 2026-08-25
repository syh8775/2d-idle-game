public class AreaDamageEffect : ISkillEffect
{
    public bool Apply(BattleSession session, BattleUnit caster, SkillDefinition skill)
    {
        if (session == null || caster == null || skill == null || !caster.IsAlive)
        {
            return false;
        }

        int damage = (int)(caster.Attack * skill.EffectMultiplier);
        int totalDamage = 0;

        foreach (BattleUnit unit in session.Units)
        {
            if (!unit.IsAlive || unit.Side == caster.Side)
            {
                continue;
            }

            totalDamage += session.ApplyDamage(unit, damage);
        }

        return totalDamage > 0;
    }
}
