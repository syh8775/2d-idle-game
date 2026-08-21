public sealed class HealEffect : ISkillEffect
{
    public bool Apply(BattleSession session, BattleUnit caster, SkillDefinition skill)
    {
        if (session == null || caster == null || skill == null || !caster.IsAlive)
        {
            return false;
        }

        BattleUnit target = null;
        float lowestHitPointRate = 1f;

        foreach (BattleUnit unit in session.Units)
        {
            if (!unit.IsAlive || unit.Side != caster.Side)
            {
                continue;
            }

            float hitPointRate = (float)unit.CurrentHitPoints / unit.MaxHitPoints;
            if (hitPointRate < lowestHitPointRate)
            {
                target = unit;
                lowestHitPointRate = hitPointRate;
            }
        }

        if (target == null)
        {
            return false;
        }

        int healing = (int)(caster.Attack * skill.EffectMultiplier);
        return session.ApplyHealing(target, healing) > 0;
    }
}
