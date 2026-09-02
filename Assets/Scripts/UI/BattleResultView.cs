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
        UIManager uiManager = UnityEngine.Object.FindFirstObjectByType<UIManager>();
        Font uiFont = uiManager != null && uiManager.UIFont != null
            ? uiManager.UIFont
            : Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        Transform panel = FindInHierarchy(owner.root, "BattleResultPanel_AutoOff_5s");

        if (panel == null)
        {
            Debug.LogError("전투 결과 패널을 찾을 수 없습니다.");
            return;
        }

        RectTransform panelRect = panel.GetComponent<RectTransform>();
        if (panelRect != null)
        {
            panelRect.anchorMin = new Vector2(0.5f, 0.5f);
            panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.pivot = new Vector2(0.5f, 0.5f);
            panelRect.anchoredPosition = Vector2.zero;
        }

        panel.localScale = new Vector3(0.936f, 0.936f, panel.localScale.z);
        resultPanel = panel.gameObject;
        Image panelImage = panel.GetComponent<Image>();
        if (panelImage != null)
        {
            panelImage.color = Color.white;
        }

        Canvas parentCanvas = panel.GetComponentInParent<Canvas>();
        Canvas resultCanvas = panel.GetComponent<Canvas>();

        if (resultCanvas == null)
        {
            resultCanvas = panel.gameObject.AddComponent<Canvas>();
        }

        resultCanvas.overrideSorting = true;
        resultCanvas.sortingOrder = 1000;

        if (parentCanvas != null)
        {
            resultCanvas.sortingLayerID = parentCanvas.sortingLayerID;

            if (parentCanvas.GetComponent<GraphicRaycaster>() != null && panel.GetComponent<GraphicRaycaster>() == null)
            {
                panel.gameObject.AddComponent<GraphicRaycaster>();
            }
        }

        resultText = EnsureText(panel, "ResultText", uiFont);
        rewardText = EnsureText(panel, "RewardText", uiFont);
        countdownText = EnsureText(panel, "CountdownText", uiFont);

        SetupTextArea(resultText, new Vector2(0f, 165f), new Vector2(600f, 120f), 72, new Color(1f, 0.9f, 0.6f));
        SetupTextArea(rewardText, new Vector2(0f, 30f), new Vector2(600f, 72f), 48, new Color(1f, 0.9f, 0.6f));
        SetupTextArea(countdownText, new Vector2(0f, -160f), new Vector2(600f, 64f), 36, new Color(0.6f, 1f, 1f));

        retryButton = owner.Find("RetryButton")?.GetComponent<Button>();

        if (retryButton == null)
        {
            GameObject buttonObject = new GameObject(
                "RetryButton",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image),
                typeof(Button));
            buttonObject.transform.SetParent(owner, false);

            Image buttonImage = buttonObject.GetComponent<Image>();
            buttonImage.color = new Color(1f, 0.86f, 0.5f, 1f);

            GameObject textObject = new GameObject(
                "Text",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Text));
            textObject.transform.SetParent(buttonObject.transform, false);

            Text buttonText = textObject.GetComponent<Text>();
            buttonText.text = "Retry";
            buttonText.font = uiFont;
            buttonText.fontSize = 28;
            buttonText.alignment = TextAnchor.MiddleCenter;
            buttonText.color = new Color(0.08f, 0.13f, 0.22f);

            RectTransform textRect = textObject.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            retryButton = buttonObject.GetComponent<Button>();
        }

        Text retryText = retryButton.GetComponentInChildren<Text>(true);
        if (retryText != null)
        {
            retryText.font = uiFont;
            retryText.color = new Color(0.08f, 0.13f, 0.22f);
        }

        retryButton.GetComponent<Image>().color = new Color(1f, 0.86f, 0.5f, 1f);

        SetupRetryBtn();
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
                timer = coroutineHost.StartCoroutine(Tick("Next Stage "));
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
                timer = coroutineHost.StartCoroutine(Tick("Retrying "));
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

    private void SetupRetryBtn()
    {
        if (retryButton == null || resultPanel == null)
        {
            return;
        }

        RectTransform buttonRect = retryButton.GetComponent<RectTransform>();
        if (buttonRect == null)
        {
            return;
        }

        buttonRect.SetParent(resultPanel.transform, false);
        buttonRect.anchorMin = new Vector2(0.5f, 0.5f);
        buttonRect.anchorMax = new Vector2(0.5f, 0.5f);
        buttonRect.pivot = new Vector2(0.5f, 0.5f);
        buttonRect.anchoredPosition = new Vector2(0f, -160f);
        buttonRect.sizeDelta = new Vector2(220f, 70f);
    }

    private Text EnsureText(Transform parent, string objectName, Font font)
    {
        Text text = parent.Find(objectName)?.GetComponent<Text>();

        if (text == null)
        {
            GameObject textObject = new GameObject(
                objectName,
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Text));
            textObject.transform.SetParent(parent, false);
            text = textObject.GetComponent<Text>();
        }

        text.font = font;
        text.alignment = TextAnchor.MiddleCenter;
        text.raycastTarget = false;
        return text;
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


    private IEnumerator Tick(string label)
    {
        for (int seconds = 5; seconds > 0; seconds--)
        {
            countdownText.text = label + seconds;
            yield return new WaitForSeconds(1f);
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
