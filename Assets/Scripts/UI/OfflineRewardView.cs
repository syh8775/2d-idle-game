using UnityEngine;
using UnityEngine.UI;

public class OfflineRewardView : MonoBehaviour
{
    private GameManager gameManager;
    private Font font;
    private GameObject popup;
    private Text timeText;
    private Text goldText;
    private Button receiveButton;
    private GameObject recordPopup;
    private Text stageRecordText;
    private Text killRecordText;
    private Text goldRecordText;
    private Text damageRecordText;
    private float previousTimeScale = 1f;

    public static OfflineRewardView Create(UIManager uiManager, GameManager owner)
    {
        if (uiManager == null || owner == null)
        {
            return null;
        }

        GameObject viewObject = new GameObject("OfflineRewardView", typeof(RectTransform), typeof(OfflineRewardView));
        viewObject.transform.SetParent(uiManager.transform, false);
        OfflineRewardView view = viewObject.GetComponent<OfflineRewardView>();
        view.Initialize(uiManager, owner);
        return view;
    }

private void Initialize(UIManager uiManager, GameManager owner)
    {
        gameManager = owner;
        font = uiManager.UIFont != null ? uiManager.UIFont : Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        Transform skillBar = FindInHierarchy(uiManager.transform, "BattleSkillBar");
        if (skillBar == null)
        {
            Debug.LogError("누적 보상 UI를 배치할 스킬창을 찾을 수 없습니다.");
            return;
        }

        Transform canvasRoot = uiManager.GetComponentInParent<Canvas>().transform;
        CreateWidget(skillBar);
        CreateRecord(skillBar);
        CreatePopup(canvasRoot);
        CreateRecordPop(canvasRoot);
        gameManager.ProgressChanged += Refresh;
        Refresh();

        if (gameManager.Progress.PendingOfflineGold > 0)
        {
            Show();
        }
    }

    private void CreateWidget(Transform skillBar)
    {
        GameObject buttonObject = new GameObject("OfflineRewardButton", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        buttonObject.transform.SetParent(skillBar.parent, false);
        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        RectTransform skillBarRect = skillBar.GetComponent<RectTransform>();
        float halfWidth = skillBarRect.sizeDelta.x * 0.5f;
        rect.sizeDelta = new Vector2(halfWidth - 35f, 322f);
        rect.anchoredPosition = new Vector2(
            skillBarRect.anchoredPosition.x - skillBarRect.sizeDelta.x * 0.5f + rect.sizeDelta.x * 0.5f + 12f,
            skillBarRect.anchoredPosition.y - skillBarRect.sizeDelta.y * 0.5f - rect.sizeDelta.y * 0.5f + 10f);

        Image image = buttonObject.GetComponent<Image>();
        image.color = new Color(0.035f, 0.14f, 0.19f, 1f);
        Outline outline = buttonObject.AddComponent<Outline>();
        outline.effectColor = new Color(0.86f, 0.63f, 0.18f, 1f);
        outline.effectDistance = new Vector2(2f, -2f);

        GameObject iconObject = new GameObject("GemChestIcon", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        iconObject.transform.SetParent(buttonObject.transform, false);
        RectTransform iconRect = iconObject.GetComponent<RectTransform>();
        iconRect.anchoredPosition = new Vector2(0f, 20f);
        iconRect.sizeDelta = new Vector2(270f, 270f);
        Texture2D iconTexture = Resources.Load<Texture2D>("UI/offline-reward-gem-chest");
        if (iconTexture != null)
        {
            iconObject.GetComponent<Image>().sprite = Sprite.Create(iconTexture, new Rect(0f, 0f, iconTexture.width, iconTexture.height), new Vector2(0.5f, 0.5f));
            iconObject.GetComponent<Image>().preserveAspect = true;
        }

        MakeText(buttonObject.transform, "Label", "누적 보상", new Vector2(0f, -135f), new Vector2(halfWidth - 24f, 40f), 26, new Color(1f, 0.84f, 0.38f));
        buttonObject.GetComponent<Button>().onClick.AddListener(Show);
    }

private void CreateRecord(Transform skillBar)
    {
        GameObject cardObject = new GameObject("ExpeditionRecordCard", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        cardObject.transform.SetParent(skillBar.parent, false);
        RectTransform rect = cardObject.GetComponent<RectTransform>();
        RectTransform skillBarRect = skillBar.GetComponent<RectTransform>();
        float halfWidth = skillBarRect.sizeDelta.x * 0.5f;
        rect.sizeDelta = new Vector2(halfWidth - 35f, 322f);
        rect.anchoredPosition = new Vector2(
            skillBarRect.anchoredPosition.x + skillBarRect.sizeDelta.x * 0.5f - rect.sizeDelta.x * 0.5f - 13f,
            skillBarRect.anchoredPosition.y - skillBarRect.sizeDelta.y * 0.5f - rect.sizeDelta.y * 0.5f + 10f);

        Image image = cardObject.GetComponent<Image>();
        image.color = new Color(0.035f, 0.14f, 0.19f, 1f);
        Outline outline = cardObject.AddComponent<Outline>();
        outline.effectColor = new Color(0.86f, 0.63f, 0.18f, 1f);
        outline.effectDistance = new Vector2(2f, -2f);

        GameObject iconObject = new GameObject("MedalIcon", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        iconObject.transform.SetParent(cardObject.transform, false);
        RectTransform iconRect = iconObject.GetComponent<RectTransform>();
        iconRect.anchoredPosition = new Vector2(0f, 20f);
        iconRect.sizeDelta = new Vector2(270f, 270f);
        Texture2D iconTexture = Resources.Load<Texture2D>("UI/expedition-record-medal");
        if (iconTexture != null)
        {
            Image iconImage = iconObject.GetComponent<Image>();
            iconImage.sprite = Sprite.Create(iconTexture, new Rect(0f, 0f, iconTexture.width, iconTexture.height), new Vector2(0.5f, 0.5f));
            iconImage.preserveAspect = true;
            iconImage.raycastTarget = false;
        }

        MakeText(cardObject.transform, "Label", "원정 기록", new Vector2(0f, -135f), new Vector2(halfWidth - 24f, 40f), 26, new Color(1f, 0.84f, 0.38f));
        cardObject.GetComponent<Button>().onClick.AddListener(ShowRecord);
    }

private void CreateRecordPop(Transform canvasRoot)
    {
        recordPopup = new GameObject("ExpeditionRecordPopup", typeof(RectTransform), typeof(Canvas), typeof(GraphicRaycaster), typeof(CanvasRenderer), typeof(Image));
        recordPopup.transform.SetParent(canvasRoot, false);
        RectTransform popupRect = recordPopup.GetComponent<RectTransform>();
        popupRect.anchorMin = Vector2.zero;
        popupRect.anchorMax = Vector2.one;
        popupRect.offsetMin = Vector2.zero;
        popupRect.offsetMax = Vector2.zero;
        recordPopup.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.68f);

        Canvas popupCanvas = recordPopup.GetComponent<Canvas>();
        popupCanvas.overrideSorting = true;
        popupCanvas.sortingOrder = 1110;

        GameObject panel = new GameObject("Panel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        panel.transform.SetParent(recordPopup.transform, false);
        RectTransform panelRect = panel.GetComponent<RectTransform>();
        panelRect.sizeDelta = new Vector2(720f, 650f);
        panel.GetComponent<Image>().color = new Color(0.04f, 0.11f, 0.16f, 1f);
        UIFrame.Build(panel.transform, panelRect.sizeDelta, Vector2.zero);

        GameObject iconObject = new GameObject("MedalIcon", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        iconObject.transform.SetParent(panel.transform, false);
        RectTransform iconRect = iconObject.GetComponent<RectTransform>();
        iconRect.anchoredPosition = new Vector2(0f, 238f);
        iconRect.sizeDelta = new Vector2(110f, 110f);
        Texture2D iconTexture = Resources.Load<Texture2D>("UI/expedition-record-medal");
        if (iconTexture != null)
        {
            Image iconImage = iconObject.GetComponent<Image>();
            iconImage.sprite = Sprite.Create(iconTexture, new Rect(0f, 0f, iconTexture.width, iconTexture.height), new Vector2(0.5f, 0.5f));
            iconImage.preserveAspect = true;
            iconImage.raycastTarget = false;
        }

        MakeText(panel.transform, "Title", "원정대 전투 기록", new Vector2(0f, 158f), new Vector2(600f, 80f), 30, new Color(1f, 0.84f, 0.38f));
        stageRecordText = MakeStat(panel.transform, "StageStat", "최고 도달 스테이지", new Vector2(-165f, 15f), new Color(0.6f, 1f, 1f));
        killRecordText = MakeStat(panel.transform, "KillStat", "누적 적 처치", new Vector2(165f, 15f), Color.white);
        goldRecordText = MakeStat(panel.transform, "GoldStat", "누적 획득 골드", new Vector2(-165f, -135f), new Color(1f, 0.88f, 0.55f));
        damageRecordText = MakeStat(panel.transform, "DamageStat", "최고 단일 데미지", new Vector2(165f, -135f), new Color(1f, 0.55f, 0.55f));
        UIFrame.MakeClose(panel.transform, font, new Vector2(315f, 280f), HideRecord);
        recordPopup.SetActive(false);
    }

private Text MakeStat(Transform parent, string name, string label, Vector2 position, Color valueColor)
    {
        GameObject card = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        card.transform.SetParent(parent, false);
        RectTransform rect = card.GetComponent<RectTransform>();
        rect.anchoredPosition = position;
        rect.sizeDelta = new Vector2(300f, 125f);
        card.GetComponent<Image>().color = new Color(0.055f, 0.17f, 0.23f, 1f);
        Outline outline = card.AddComponent<Outline>();
        outline.effectColor = new Color(0.35f, 0.47f, 0.5f, 0.75f);
        outline.effectDistance = new Vector2(1f, -1f);
        MakeText(card.transform, "Label", label, new Vector2(0f, 25f), new Vector2(270f, 38f), 22, new Color(0.72f, 0.82f, 0.85f));
        return MakeText(card.transform, "Value", "0", new Vector2(0f, -22f), new Vector2(270f, 70f), 38, valueColor);
    }

private void ShowRecord()
    {
        if (recordPopup == null || recordPopup.activeSelf) return;
        RefreshRecord();
        previousTimeScale = Time.timeScale;
        Time.timeScale = 0f;
        recordPopup.SetActive(true);
    }

    private void HideRecord()
    {
        if (recordPopup == null || !recordPopup.activeSelf) return;
        recordPopup.SetActive(false);
        Time.timeScale = previousTimeScale;
    }

    private void RefreshRecord()
    {
        if (gameManager == null || gameManager.Progress == null) return;
        stageRecordText.text = GetStageLabel(gameManager.Progress.LastClearedStageId);
        killRecordText.text = gameManager.Progress.TotalEnemyKills.ToString("N0");
        goldRecordText.text = gameManager.Progress.TotalGoldEarned.ToString("N0");
        damageRecordText.text = gameManager.Progress.HighestDamage.ToString("N0");
    }

    private string GetStageLabel(string stageId)
    {
        if (string.IsNullOrEmpty(stageId)) return "-";
        int separator = stageId.LastIndexOf('_');
        if (separator < 0 || !int.TryParse(stageId.Substring(separator + 1), out int stageNumber)) return stageId;
        return "1-" + stageNumber;
    }



    private void CreatePopup(Transform canvasRoot)
    {
        popup = new GameObject("OfflineRewardPopup", typeof(RectTransform), typeof(Canvas), typeof(GraphicRaycaster), typeof(CanvasRenderer), typeof(Image));
        popup.transform.SetParent(canvasRoot, false);
        RectTransform popupRect = popup.GetComponent<RectTransform>();
        popupRect.anchorMin = Vector2.zero;
        popupRect.anchorMax = Vector2.one;
        popupRect.offsetMin = Vector2.zero;
        popupRect.offsetMax = Vector2.zero;
        popup.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.68f);

        Canvas popupCanvas = popup.GetComponent<Canvas>();
        popupCanvas.overrideSorting = true;
        popupCanvas.sortingOrder = 1100;

        GameObject panel = new GameObject("Panel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        panel.transform.SetParent(popup.transform, false);
        RectTransform panelRect = panel.GetComponent<RectTransform>();
        panelRect.sizeDelta = new Vector2(680f, 540f);
        panel.GetComponent<Image>().color = new Color(0.04f, 0.11f, 0.16f, 1f);
        UIFrame.Build(panel.transform, panelRect.sizeDelta, Vector2.zero);

        MakeText(panel.transform, "Title", "오프라인 누적 보상", new Vector2(0f, 170f), new Vector2(580f, 70f), 44, new Color(1f, 0.84f, 0.38f));
        timeText = MakeText(panel.transform, "Time", "누적 시간 0분", new Vector2(0f, 55f), new Vector2(560f, 60f), 32, new Color(0.6f, 1f, 1f));
        goldText = MakeText(panel.transform, "Gold", "Gold +0", new Vector2(0f, -35f), new Vector2(560f, 70f), 46, new Color(1f, 0.9f, 0.6f));
        receiveButton = MakeButton(panel.transform, "받기", new Vector2(0f, -165f), new Vector2(260f, 82f), Receive);
        UIFrame.MakeClose(panel.transform, font, new Vector2(285f, 215f), Hide);
        popup.SetActive(false);
    }

public void Show()
    {
        if (popup == null || popup.activeSelf)
        {
            return;
        }

        Refresh();
        popup.SetActive(true);
    }

public void Hide()
    {
        if (popup == null || !popup.activeSelf)
        {
            return;
        }

        popup.SetActive(false);
    }

    private void Receive()
    {
        if (gameManager.ClaimOffline() > 0)
        {
            Hide();
        }
        else
        {
            Refresh();
        }
    }

    private void Refresh()
    {
        if (gameManager == null || gameManager.Progress == null)
        {
            return;
        }

        int seconds = gameManager.Progress.PendingOfflineSeconds;
        int hours = seconds / 3600;
        int minutes = seconds % 3600 / 60;
        string duration = hours > 0 ? hours + "시간 " + minutes + "분" : minutes + "분";
        int pendingGold = gameManager.Progress.PendingOfflineGold;

        if (timeText != null)
        {
            timeText.text = "누적 시간 " + duration;
        }

        if (goldText != null)
        {
            goldText.text = "Gold +" + pendingGold;
        }

        if (receiveButton != null)
        {
            receiveButton.interactable = pendingGold > 0;
        }
    }

    private Text MakeText(Transform parent, string name, string value, Vector2 position, Vector2 size, int fontSize, Color color)
    {
        GameObject textObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
        textObject.transform.SetParent(parent, false);
        RectTransform rect = textObject.GetComponent<RectTransform>();
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        Text text = textObject.GetComponent<Text>();
        text.text = value;
        text.font = font;
        text.fontSize = fontSize;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = color;
        text.raycastTarget = false;
        return text;
    }

    private Button MakeButton(Transform parent, string label, Vector2 position, Vector2 size, UnityEngine.Events.UnityAction action)
    {
        GameObject buttonObject = new GameObject("ReceiveButton", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        buttonObject.transform.SetParent(parent, false);
        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        buttonObject.GetComponent<Image>().color = new Color(1f, 0.86f, 0.5f, 1f);
        Button button = buttonObject.GetComponent<Button>();
        button.onClick.AddListener(action);
        MakeText(buttonObject.transform, "Text", label, Vector2.zero, size, 32, new Color(0.08f, 0.13f, 0.22f));
        return button;
    }

    private Transform FindInHierarchy(Transform root, string targetName)
    {
        foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
        {
            if (child.name == targetName)
            {
                return child;
            }
        }

        return null;
    }

private void OnDestroy()
    {
        if (recordPopup != null && recordPopup.activeSelf)
        {
            Time.timeScale = previousTimeScale;
        }

        if (gameManager != null)
        {
            gameManager.ProgressChanged -= Refresh;
        }
    }
}
