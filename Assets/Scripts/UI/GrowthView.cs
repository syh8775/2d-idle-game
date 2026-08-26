using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GrowthView : UIBase
{
    private GameManager gameManager;
    private UIManager uiManager;
    private Text goldText;
    private Button openButton;
    private bool returnToPopup;
    private List<string> characterIds = new List<string>();
    private List<Text> characterTexts = new List<Text>();
    private List<Button> levelButtons = new List<Button>();

    public static void Create(UIManager uiManager, GameManager gameManager)
    {
        GameObject viewObject = new GameObject("GrowthUI", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        viewObject.transform.SetParent(uiManager.transform, false);

        RectTransform viewRect = viewObject.GetComponent<RectTransform>();
        viewRect.anchorMin = new Vector2(0.5f, 0.5f);
        viewRect.anchorMax = new Vector2(0.5f, 0.5f);
        viewRect.sizeDelta = new Vector2(900f, 700f);

        Image background = viewObject.GetComponent<Image>();
        background.color = new Color(0.08f, 0.12f, 0.2f, 0.98f);

        GrowthView view = viewObject.AddComponent<GrowthView>();
        view.SetType(UIType.Growth);
        view.Initialize(uiManager, gameManager);
        uiManager.Register(view);
        view.Hide();
    }

    private void Initialize(UIManager manager, GameManager owner)
    {
        uiManager = manager;
        gameManager = owner;

        CreateText(transform, "Character Growth", new Vector2(0f, 285f), new Vector2(700f, 70f), 42);
        goldText = CreateText(transform, string.Empty, new Vector2(0f, 220f), new Vector2(700f, 50f), 30);

        int row = 0;

        foreach (CharacterDefinition character in gameManager.Data.Characters.Values)
        {
            string characterId = character.Id;
            float y = 130f - row * 105f;
            Text infoText = CreateText(transform, string.Empty, new Vector2(-110f, y), new Vector2(520f, 80f), 25);
            Button levelButton = CreateButton(transform, "Level Up", new Vector2(290f, y), new Vector2(190f, 66f));
            levelButton.onClick.AddListener(delegate { LevelUp(characterId); });

            characterIds.Add(characterId);
            characterTexts.Add(infoText);
            levelButtons.Add(levelButton);
            row++;
        }

        Button closeButton = CreateButton(transform, "Back", new Vector2(0f, -290f), new Vector2(220f, 70f));
        closeButton.onClick.AddListener(Close);

        openButton = CreateButton(uiManager.transform, "Growth", new Vector2(420f, 820f), new Vector2(190f, 70f));
        openButton.gameObject.SetActive(false);
        openButton.onClick.AddListener(Open);

        gameManager.ProgressChanged += Refresh;
        Refresh();
    }

    private void Open()
    {
        returnToPopup = gameManager.Battle.CurrentSession != null && gameManager.Battle.CurrentSession.IsFinished;
        uiManager.Hide(UIType.Popup);

        if (uiManager.Switch(UIType.Growth))
        {
            openButton.gameObject.SetActive(false);
            Refresh();
        }
    }

    private void Close()
    {
        if (uiManager.Switch(UIType.Battle))
        {
            openButton.gameObject.SetActive(true);

            if (returnToPopup)
            {
                uiManager.Show(UIType.Popup);
            }
        }
    }

    private void LevelUp(string characterId)
    {
        gameManager.TryLevelUp(characterId);
        Refresh();
    }

    private void Refresh()
    {
        goldText.text = "Gold  " + gameManager.Progress.Gold;

        for (int i = 0; i < characterIds.Count; i++)
        {
            CharacterDefinition definition;
            CharacterProgressModel progress = gameManager.Progress.GetCharacter(characterIds[i]);

            if (!gameManager.Data.TryGetCharacter(characterIds[i], out definition) || progress == null)
            {
                continue;
            }

            int cost = GameUtil.GetLevelCost(progress.Level);
            int hitPoints = GameUtil.GetLevelStat(definition.HitPoints, progress.Level);
            int attack = GameUtil.GetLevelStat(definition.Attack, progress.Level);
            characterTexts[i].text = definition.DisplayName + "  Lv." + progress.Level + "\nHP " + hitPoints + "  ATK " + attack + "  Cost " + cost;
            levelButtons[i].interactable = gameManager.Progress.Gold >= cost;
        }
    }

    private Text CreateText(Transform parent, string value, Vector2 position, Vector2 size, int fontSize)
    {
        GameObject textObject = new GameObject("Text", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
        textObject.transform.SetParent(parent, false);

        RectTransform rect = textObject.GetComponent<RectTransform>();
        rect.anchoredPosition = position;
        rect.sizeDelta = size;

        Text text = textObject.GetComponent<Text>();
        text.text = value;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = fontSize;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = Color.white;
        return text;
    }

    private Button CreateButton(Transform parent, string label, Vector2 position, Vector2 size)
    {
        GameObject buttonObject = new GameObject(label + "Button", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        buttonObject.transform.SetParent(parent, false);

        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        rect.anchoredPosition = position;
        rect.sizeDelta = size;

        Image image = buttonObject.GetComponent<Image>();
        image.color = new Color(0.15f, 0.35f, 0.55f, 1f);

        CreateText(buttonObject.transform, label, Vector2.zero, size, 26);
        return buttonObject.GetComponent<Button>();
    }

    private void OnDestroy()
    {
        if (gameManager != null)
        {
            gameManager.ProgressChanged -= Refresh;
        }
    }
}
