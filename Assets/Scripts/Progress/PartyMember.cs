using System;
using System.Collections.Generic;

[Serializable]
public class PartyMember
{
    public int SlotIndex;
    public string CharacterId;
    public int FormationSlot;

}

[Serializable]
public class PartyFormation
{
    private const int MinimumSlot = 1;
    private const int MaximumSlot = 4;

    public List<PartyMember> Members = new List<PartyMember>();

    public event Action Changed;

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
            member.SlotIndex = slot.SlotIndex;
            member.CharacterId = slot.DefaultCharacterId;
            member.FormationSlot = slot.FormationSlot;

            Members.Add(member);
        }
    }

    public bool TryMove(string characterId, int targetSlot)
    {
        if (targetSlot < MinimumSlot || targetSlot > MaximumSlot)
        {
            return false;
        }

        PartyMember movingMember = null;

        foreach (PartyMember member in Members)
        {
            if (member.CharacterId == characterId)
            {
                movingMember = member;
            }
            else if (member.FormationSlot == targetSlot)
            {
                return false;
            }
        }

        if (movingMember == null)
        {
            return false;
        }

        movingMember.FormationSlot = targetSlot;

        if (Changed != null)
        {
            Changed();
        }

        return true;
    }
}
