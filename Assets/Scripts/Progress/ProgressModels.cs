using System;
using System.Collections.Generic;

[Serializable]
public class PlayerProgressModel
{
    public int Gold;
    public string LastClearedStageId = string.Empty;
    public List<CharacterProgressModel> Characters = new List<CharacterProgressModel>();
}

[Serializable]
public class CharacterProgressModel
{
    public string CharacterId;
    public int Level = 1;
}