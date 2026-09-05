using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class SaveManager
{
    private readonly string savePath;
    private bool saveBlocked;

    public SaveManager()
    {
        savePath = Path.Combine(Application.persistentDataPath, "player-progress.json");
    }

    public PlayerProgressModel Load()
    {
        if (!File.Exists(savePath))
        {
            // 첫 실행의 저장 파일 없음은 정상 상태이며, 이전 로드 실패 차단은 유지합니다.
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

            if (progress.PartyMembers == null)
            {
                progress.PartyMembers = new List<PartyMember>();
            }

            if (progress.Gold < 0)
            {
                progress.Gold = 0;
            }

            if (progress.PendingOfflineGold < 0)
            {
                progress.PendingOfflineGold = 0;
            }



            if (progress.TotalEnemyKills < 0)
            {
                progress.TotalEnemyKills = 0;
            }

            if (progress.TotalGoldEarned < 0)
            {
                progress.TotalGoldEarned = 0;
            }

            if (progress.HighestDamage < 0)
            {
                progress.HighestDamage = 0;
            }
if (progress.PendingOfflineSeconds < 0)
            {
                progress.PendingOfflineSeconds = 0;
            }

            foreach (CharacterProgressModel character in progress.Characters)
            {
                character.Level = Mathf.Clamp(character.Level, 1, GameUtil.MaxLevel);
            }

            saveBlocked = false;
            return progress;
        }
        catch (Exception exception)
        {
            saveBlocked = true;
            Debug.LogWarning("저장 데이터 복구 실패. 기본값으로 시작하지만 원본 보호를 위해 저장을 비활성화합니다: " + exception.Message);
            return new PlayerProgressModel();
        }
    }

    public bool Save(PlayerProgressModel progress)
    {
        if (saveBlocked || progress == null)
        {
            return false;
        }

        try
        {
            string json = JsonUtility.ToJson(progress, true);
            string temporaryPath = savePath + ".tmp";
            File.WriteAllText(temporaryPath, json);

            if (File.Exists(savePath))
            {
                File.Replace(temporaryPath, savePath, null);
            }
            else
            {
                File.Move(temporaryPath, savePath);
            }

            return true;
        }
        catch (Exception exception)
        {
            Debug.LogError("진행 데이터 저장 실패: " + exception.Message);
            return false;
        }
    }
}
