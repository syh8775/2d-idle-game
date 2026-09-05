using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class BattleResultView
{
    private readonly MonoBehaviour coroutineHost;
    private GameObject resultPanel;
    private Text resultText;
    private Text rewardText;
    private Text countdownText;
    private Button retryButton;
    private Coroutine timer;
    private bool isRestarting;

    public event Action RetryRequested;

    public BattleResultView(MonoBehaviour coroutineHost)
    {
        if (coroutineHost == null)
        {
            throw new Exception("전투 결과 UI를 실행할 MonoBehaviour가 필요합니다.");
        }

        this.coroutineHost = coroutineHost;
    }

    public void Initialize(Transform owner)
    {
        Transform panel = FindInHierarchy(owner.root, "BattleResultPanel_AutoOff_5s");
        if (panel == null)
        {
            Debug.LogError("전투 결과 패널을 찾을 수 없습니다.");
            return;
        }

        resultPanel = panel.gameObject;
        resultText = panel.Find("ResultText").GetComponent<Text>();
        rewardText = panel.Find("RewardText").GetComponent<Text>();
        countdownText = panel.Find("CountdownText").GetComponent<Text>();
        retryButton = panel.Find("RetryButton").GetComponent<Button>();
        retryButton.onClick.AddListener(OnRetryClick);
        Hide();
    }
    public void Show(BattleSession session)
    {
        if (session == null || session.Outcome == BattleOutcome.Cancelled || resultPanel == null || resultText == null || rewardText == null || countdownText == null)
        {
            return;
        }

        StopTick();
        isRestarting = false;
        resultText.gameObject.SetActive(true);
        rewardText.gameObject.SetActive(false);
        countdownText.gameObject.SetActive(false);
        retryButton.gameObject.SetActive(false);
        resultPanel.SetActive(true);

        Canvas resultCanvas = resultPanel.GetComponent<Canvas>();

        if (resultCanvas != null)
        {
            resultCanvas.overrideSorting = true;
            resultCanvas.sortingOrder = 1000;
        }

        if (session.Outcome == BattleOutcome.Victory)
        {
            resultText.text = "Victory";
            rewardText.text = "Gold +" + session.RewardGold;
            rewardText.gameObject.SetActive(true);
            SetupTextArea(countdownText, new Vector2(0f, -160f), new Vector2(600f, 64f), 36, new Color(0.6f, 1f, 1f));
            countdownText.gameObject.SetActive(true);
            if (IsFinalStage(session))
            {
                resultText.text = "게임 완료";
                countdownText.text = "모든 스테이지를 클리어했습니다\nF12키를 눌러 초기화";
                SetupTextArea(countdownText, new Vector2(0f, -180f), new Vector2(600f, 120f), 30, new Color(0.6f, 1f, 1f));
            }
            else
            {
                GameManager.Instance.ResultShown(session);
                timer = coroutineHost.StartCoroutine(Tick(session, "Next Stage "));
            }
        }
        else
        {
            bool autoRetry = GameManager.Instance != null && GameManager.Instance.ShouldAutoRetry(session);
            if (autoRetry && !GameManager.Instance.ShowRetryNotice(session))
            {
                Hide();
                return;
            }
            resultText.text = autoRetry && session.Outcome == BattleOutcome.Timeout ? "Time Limit" : "Defeat";

            if (autoRetry)
            {
                SetupTextArea(countdownText, new Vector2(0f, -160f), new Vector2(600f, 64f), 36, new Color(0.6f, 1f, 1f));
                countdownText.gameObject.SetActive(true);
                GameManager.Instance.ResultShown(session);
                timer = coroutineHost.StartCoroutine(Tick(session, "Retrying "));
            }
            else
            {
                if (session.Outcome == BattleOutcome.Timeout)
                {
                    countdownText.text = "Time Limit";
                    SetupTextArea(countdownText, new Vector2(0f, 30f), new Vector2(600f, 64f), 36, new Color(0.6f, 1f, 1f));
                    countdownText.gameObject.SetActive(true);
                }

                retryButton.gameObject.SetActive(true);
                retryButton.interactable = true;
            }
        }
    }

    public void Hide()
    {
        StopTick();

        if (resultText != null)
        {
            resultText.text = string.Empty;
        }

        if (rewardText != null)
        {
            rewardText.text = string.Empty;
            rewardText.gameObject.SetActive(false);
        }

        if (countdownText != null)
        {
            countdownText.text = string.Empty;
            countdownText.gameObject.SetActive(false);
        }

        if (resultPanel != null)
        {
            resultPanel.SetActive(false);
        }

        if (retryButton != null)
        {
            retryButton.gameObject.SetActive(false);
        }
    }

    public void RetryFailed()
    {
        isRestarting = false;

        if (retryButton != null)
        {
            retryButton.gameObject.SetActive(true);
            retryButton.interactable = true;
        }
    }

public void Dispose()
    {
        StopTick();

        if (retryButton != null)
        {
            retryButton.onClick.RemoveListener(OnRetryClick);
        }
    }

    private void OnRetryClick()
    {
        if (isRestarting)
        {
            return;
        }

        isRestarting = true;

        if (retryButton != null)
        {
            retryButton.interactable = false;
        }

        if (RetryRequested != null)
        {
            RetryRequested();
        }
    }

    private void SetupTextArea(Text text, Vector2 position, Vector2 size, int fontSize, Color color)
    {
        if (text == null)
        {
            return;
        }

        RectTransform rect = text.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        text.fontSize = fontSize;
        text.color = color;
    }


    private static Transform FindInHierarchy(
        Transform root,
        string objectName)
    {
        Transform[] children = root.GetComponentsInChildren<Transform>(true);

        foreach (Transform child in children)
        {
            if (child.name == objectName)
            {
                return child;
            }
        }

        return null;
    }


    private IEnumerator Tick(BattleSession session, string label)
    {
        while (GameManager.Instance != null && GameManager.Instance.ResultSeconds(session) > 0)
        {
            countdownText.text = label + GameManager.Instance.ResultSeconds(session);
            yield return null;
        }

        timer = null;
    }

    private void StopTick()
    {
        if (timer == null)
        {
            return;
        }

        coroutineHost.StopCoroutine(timer);
        timer = null;
    }


private bool IsFinalStage(BattleSession session)
    {
        if (session == null ||
            session.Stage == null ||
            string.IsNullOrEmpty(session.Stage.Id) ||
            !session.Stage.Id.StartsWith("STAGE_") ||
            GameManager.Instance == null ||
            GameManager.Instance.Data == null)
        {
            return false;
        }

        int stageNumber;
        if (!int.TryParse(session.Stage.Id.Substring(6), out stageNumber))
        {
            return false;
        }

        StageDefinition nextStage;
        string nextStageId = "STAGE_" + (stageNumber + 1).ToString("000");
        return !GameManager.Instance.Data.TryGetStage(nextStageId, out nextStage);
    }
}
