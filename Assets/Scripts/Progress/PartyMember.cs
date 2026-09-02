using System;
using System.Collections.Generic;

[Serializable]
public class PartyMember
{
    public string CharacterId;
    public int FormationSlot;

}

[Serializable]
public class PartyFormation
{
    private const int MinimumSlot = 1;
    private const int MaximumSlot = 9;

    public List<PartyMember> Members = new List<PartyMember>();

    public void Load(IEnumerable<PartySlotDefinition> slots)
    {
        Members.Clear();

        foreach (PartySlotDefinition slot in slots)
        {
            if (slot.Side != "Ally")
            {
                continue;
            }

            PartyMember member = new PartyMember();
            member.CharacterId = slot.DefaultCharacterId;
            member.FormationSlot = slot.FormationSlot;

            Members.Add(member);
        }
    }

    public bool TryRemove(string characterId)
    {
        foreach (PartyMember member in Members)
        {
            if (member.CharacterId != characterId || member.FormationSlot == 0)
            {
                continue;
            }

            member.FormationSlot = 0;

            return true;
        }

        return false;
    }

    public bool TryMove(string characterId, int targetSlot)
    {
        if (targetSlot < MinimumSlot || targetSlot > MaximumSlot)
        {
            return false;
        }

        PartyMember movingMember = null;
        PartyMember emptyMember = null;

        foreach (PartyMember member in Members)
        {
            if (member.FormationSlot == targetSlot)
            {
                return false;
            }

            if (member.CharacterId == characterId)
            {
                movingMember = member;
            }
            else if (member.FormationSlot == 0 && emptyMember == null)
            {
                emptyMember = member;
            }
        }

        if (movingMember == null)
        {
            if (emptyMember == null)
            {
                return false;
            }

            emptyMember.CharacterId = characterId;
            movingMember = emptyMember;
        }

        movingMember.FormationSlot = targetSlot;

        return true;
    }
}
