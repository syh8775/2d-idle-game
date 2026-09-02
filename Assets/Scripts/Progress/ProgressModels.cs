using System;
using System.Collections.Generic;

[Serializable]
public class PlayerProgressModel
{
    public int Gold;
    public string LastClearedStageId = string.Empty;
    public string CurrentStageId = string.Empty;
    public List<PartyMember> PartyMembers = new List<PartyMember>();
    public long LastActiveUtcTicks;
    public int PendingOfflineGold;

    public int TotalEnemyKills;
    public int TotalGoldEarned;
    public int HighestDamage;
public int PendingOfflineSeconds;
    public List<CharacterProgressModel> Characters = new List<CharacterProgressModel>();

    public CharacterProgressModel GetCharacter(string characterId)
    {
        foreach (CharacterProgressModel character in Characters)
        {
            if (character.CharacterId == characterId)
            {
                return character;
            }
        }

        return null;
    }

    public CharacterProgressModel AddCharacter(string characterId)
    {
        CharacterProgressModel character = GetCharacter(characterId);

        if (character != null)
        {
            return character;
        }

        character = new CharacterProgressModel();
        character.CharacterId = characterId;
        Characters.Add(character);
        return character;
    }
}

[Serializable]
public class CharacterProgressModel
{
    public string CharacterId;
    public int Level = 1;
}
