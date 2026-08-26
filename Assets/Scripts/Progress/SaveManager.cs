using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class SaveManager
{
    private readonly string savePath;

    public SaveManager()
    {
        savePath = Path.Combine(Application.persistentDataPath, "player-progress.json");
    }

    public PlayerProgressModel Load()
    {
        if (!File.Exists(savePath))
        {
            return new PlayerProgressModel();
        }

        try
        {
            string json = File.ReadAllText(savePath);
            PlayerProgressModel progress = JsonUtility.FromJson<PlayerProgressModel>(json);

            if (progress == null)
            {
                throw new Exception("저장 데이터가 비어 있습니다.");
            }

            if (progress.Characters == null)
            {
                progress.Characters = new List<CharacterProgressModel>();
            }

            if (progress.Gold < 0)
            {
                progress.Gold = 0;
            }

            foreach (CharacterProgressModel character in progress.Characters)
            {
                if (character.Level < 1)
                {
                    character.Level = 1;
                }
            }

            return progress;
        }
        catch (Exception exception)
        {
            Debug.LogWarning("저장 데이터 복구 실패. 기본값으로 시작합니다: " + exception.Message);
            return new PlayerProgressModel();
        }
    }

    public bool Save(PlayerProgressModel progress)
    {
        if (progress == null)
        {
            return false;
        }

        try
        {
            string json = JsonUtility.ToJson(progress, true);
            File.WriteAllText(savePath, json);
            return true;
        }
        catch (Exception exception)
        {
            Debug.LogError("진행 데이터 저장 실패: " + exception.Message);
            return false;
        }
    }
}
