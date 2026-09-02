public class LowestHpRateDamageEffect : ISkillEffect
{
    public bool Apply(BattleSession session, BattleUnit caster, SkillDefinition skill)
    {
        if (session == null || caster == null || skill == null || !caster.IsAlive)
        {
            return false;
        }

        BattleUnit target = null;
        float lowestHitPointRate = float.MaxValue;

        foreach (BattleUnit unit in session.Units)
        {
            if (!unit.IsAlive || unit.Side == caster.Side)
            {
                continue;
            }

            float hitPointRate = (float)unit.CurrentHitPoints / unit.MaxHitPoints;
            if (target == null || hitPointRate < lowestHitPointRate ||
                hitPointRate == lowestHitPointRate && unit.FormationSlot < target.FormationSlot)
            {
                target = unit;
                lowestHitPointRate = hitPointRate;
            }
        }

        if (target == null)
        {
            return false;
        }

        int damage = (int)(caster.Attack * skill.EffectMultiplier);
        return session.ApplyDamage(target, damage) > 0;
    }
}
