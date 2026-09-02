public class BackRowDamageEffect : ISkillEffect
{
    public bool Apply(BattleSession session, BattleUnit caster, SkillDefinition skill)
    {
        if (session == null || caster == null || skill == null || !caster.IsAlive)
        {
            return false;
        }

        BattleUnit target = null;
        foreach (BattleUnit unit in session.Units)
        {
            if (!unit.IsAlive || unit.Side == caster.Side || unit.Column != 3)
            {
                continue;
            }

            if (target == null || unit.FormationSlot < target.FormationSlot)
            {
                target = unit;
            }
        }

        if (target == null)
        {
            target = session.FindTarget(caster);
        }

        if (target == null)
        {
            return false;
        }

        int damage = (int)(caster.Attack * skill.EffectMultiplier);
        return session.ApplyDamage(target, damage) > 0;
    }
}
