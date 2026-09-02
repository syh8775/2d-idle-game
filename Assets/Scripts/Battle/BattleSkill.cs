using System;

public class BattleSkill
{
    public SkillDefinition Definition { get; private set; }
    public BattleUnit Caster { get; private set; }
    public float RemainingCooldown { get; private set; }

    private ISkillEffect effect;

    public bool IsReady
    {
        get { return RemainingCooldown <= 0f; }
    }

    public BattleSkill(SkillDefinition definition, BattleUnit caster, ISkillEffect effect)
    {
        if (definition == null || caster == null || effect == null)
        {
            throw new Exception("전투 스킬을 생성하는 데 필요한 데이터가 비어 있습니다.");
        }

        Definition = definition;
        Caster = caster;
        this.effect = effect;
        RemainingCooldown = 0f;
    }

    public void Tick(float deltaSeconds)
    {
        if (deltaSeconds <= 0f || RemainingCooldown <= 0f)
        {
            return;
        }

        RemainingCooldown -= deltaSeconds;

        if (RemainingCooldown < 0f)
        {
            RemainingCooldown = 0f;
        }
    }

public void Ready()
    {
        RemainingCooldown = 0f;
    }


    public bool TryUse(BattleSession session)
    {
        if (!IsReady || !Caster.IsAlive)
        {
            return false;
        }

        if (!effect.Apply(session, Caster, Definition))
        {
            return false;
        }

        RemainingCooldown = Definition.CooldownSeconds;
        return true;
    }
}
