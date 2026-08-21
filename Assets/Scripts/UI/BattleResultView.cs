using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public sealed class BattleResultView
{
    private readonly MonoBehaviour coroutineHost;
    private GameObject resultPanel;
    private Text resultText;
    private Button retryButton;
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
        Transform panel = FindInHierarchy(
            owner.root,
            "BattleResultPanel_AutoOff_5s");

        if (panel == null)
        {
            Debug.LogError("전투 결과 패널을 찾을 수 없습니다.");
            return;
        }

        resultPanel = panel.gameObject;
        resultText = panel.Find("ResultText")?.GetComponent<Text>();

        if (resultText == null)
        {
            GameObject textObject = new GameObject(
                "ResultText",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Text));
            textObject.transform.SetParent(panel, false);
            resultText = textObject.GetComponent<Text>();
            resultText.font = Resources.GetBuiltinResource<Font>(
                "LegacyRuntime.ttf");
            resultText.fontSize = 72;
            resultText.alignment = TextAnchor.MiddleCenter;
            resultText.color = Color.white;

            RectTransform rect = textObject.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

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

            RectTransform buttonRect =
                buttonObject.GetComponent<RectTransform>();
            buttonRect.anchorMin = new Vector2(1f, 1f);
            buttonRect.anchorMax = new Vector2(1f, 1f);
            buttonRect.pivot = new Vector2(1f, 1f);
            buttonRect.anchoredPosition = new Vector2(-24f, -320f);
            buttonRect.sizeDelta = new Vector2(180f, 64f);

            Image buttonImage = buttonObject.GetComponent<Image>();
            buttonImage.color = new Color(0.15f, 0.25f, 0.4f, 0.95f);

            GameObject textObject = new GameObject(
                "Text",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Text));
            textObject.transform.SetParent(buttonObject.transform, false);

            Text buttonText = textObject.GetComponent<Text>();
            buttonText.text = "Retry";
            buttonText.font = Resources.GetBuiltinResource<Font>(
                "LegacyRuntime.ttf");
            buttonText.fontSize = 28;
            buttonText.alignment = TextAnchor.MiddleCenter;
            buttonText.color = Color.white;

            RectTransform textRect = textObject.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            retryButton = buttonObject.GetComponent<Button>();
        }

        ConfigureRetryButton();
        retryButton.onClick.AddListener(HandleRetryClicked);
        Hide();
    }

    public void Show(BattleSession session)
    {
        if (session == null ||
            session.Outcome == BattleOutcome.Cancelled ||
            resultPanel == null ||
            resultText == null)
        {
            return;
        }

        resultText.text = "Defeat";

        if (session.Outcome == BattleOutcome.Victory)
        {
            resultText.text = "Victory";
        }
        else if (session.Outcome == BattleOutcome.Timeout)
        {
            resultText.text = "Defeat\nTime Limit";
        }

        retryButton.gameObject.SetActive(true);
        resultPanel.SetActive(true);
    }

    public void Hide()
    {
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
        if (retryButton != null)
        {
            retryButton.onClick.RemoveListener(HandleRetryClicked);
        }
    }

    private void HandleRetryClicked()
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

        coroutineHost.StartCoroutine(EnableRetryNextFrame());
    }

    private void ConfigureRetryButton()
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
        buttonRect.anchorMin = new Vector2(1f, 1f);
        buttonRect.anchorMax = new Vector2(1f, 1f);
        buttonRect.pivot = new Vector2(1f, 1f);
        buttonRect.anchoredPosition = new Vector2(-24f, -80f);
        buttonRect.sizeDelta = new Vector2(180f, 64f);
    }

    private IEnumerator EnableRetryNextFrame()
    {
        yield return null;
        isRestarting = false;

        if (retryButton != null)
        {
            retryButton.interactable = true;
        }
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
}
