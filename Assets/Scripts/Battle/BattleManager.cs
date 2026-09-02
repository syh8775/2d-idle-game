using System;
using UnityEngine;

public class BattleManager : MonoBehaviour
{
    private DataManager dataManager;
    private PlayerProgressModel progress;

    public bool IsInitialized { get; private set; }
    public BattleSession CurrentSession { get; private set; }

    private PartyFormation formation;

    public event Action<BattleSession> SessionStarted;
    public event Action<BattleSession> SessionCompleted;

    public void Initialize(DataManager source, PartyFormation party, PlayerProgressModel playerProgress)
    {
        if (source == null || party == null || playerProgress == null)
        {
            throw new Exception("전투 매니저에 데이터 매니저와 편성이 필요합니다.");
        }

        dataManager = source;
        formation = party;
        progress = playerProgress;
        IsInitialized = true;
    }

    public bool StartStage(string stageId)
    {
        if (!IsInitialized || !dataManager.IsLoaded ||
            string.IsNullOrWhiteSpace(stageId))
        {
            return false;
        }

        if (CurrentSession != null && !CurrentSession.IsFinished)
        {
            return false;
        }

        StageDefinition stage;
        if (!dataManager.TryGetStage(stageId, out stage))
        {
            Debug.LogError("전투 시작 실패: 요청한 스테이지를 찾을 수 없습니다.");
            return false;
        }

        try
        {
            if (CurrentSession != null)
            {
                CurrentSession.StateChanged -= OnSessionChange;
            }

            CurrentSession = new BattleSession(stage, dataManager, formation, progress);
        }
        catch (Exception exception)
        {
            Debug.LogError("전투 유닛 생성 실패: " + exception.Message);
            return false;
        }

        CurrentSession.StateChanged += OnSessionChange;
        CurrentSession.Start();

        if (SessionStarted != null)
        {
            SessionStarted(CurrentSession);
        }

        Debug.Log(
            "전투 시작 - 아군 " +
            CurrentSession.GetUnitCount(BattleUnitSide.Ally) +
            "명, 적군 " +
            CurrentSession.GetUnitCount(BattleUnitSide.Enemy) +
            "명");
        return true;
    }

    public bool RestartStage()
    {
        if (CurrentSession == null)
        {
            return false;
        }

        string stageId = CurrentSession.Stage.Id;

        if (!CurrentSession.IsFinished)
        {
            CurrentSession.Cancel();
        }

        return StartStage(stageId);
    }

    public void Tick(float deltaSeconds)
    {
        if (CurrentSession != null)
        {
            CurrentSession.Tick(deltaSeconds);
        }
    }

    private void OnSessionChange(BattleSession session)
    {
        if (!session.IsFinished)
        {
            return;
        }

        session.StateChanged -= OnSessionChange;

        if (SessionCompleted != null)
        {
            SessionCompleted(session);
        }
    }

    private void OnDestroy()
    {
        if (CurrentSession != null)
        {
            CurrentSession.StateChanged -= OnSessionChange;
        }
    }
}
