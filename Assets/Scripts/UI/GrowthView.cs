using UnityEngine;
using UnityEngine.UI;

public class GrowthView : UIBase
{
    private GameManager gameManager;
    private UIManager uiManager;
    private Text goldText;

    private Text rarityText;
    private Text nameText;
    private Text roleText;
    private Text levelText;
    private Text hpText;
    private Text atkText;
    private Text defText;
    private Text spdText;
    private Text costText;
    private Image bodyImage;
    private Image sdImage;
    private Image normalSkillIcon;
    private Image specialSkillIcon;
    private Image levelFill;
    private Button growButton;
    private string selectedId = string.Empty;
    private OwnedCharacterListView rosterList;

    public static void Create(UIManager uiManager, GameManager gameManager)
    {
        GameObject viewObject = new GameObject("GrowthUI", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        viewObject.transform.SetParent(uiManager.transform, false);

        RectTransform viewRect = viewObject.GetComponent<RectTransform>();
        viewRect.anchorMin = new Vector2(0.5f, 0.5f);
        viewRect.anchorMax = new Vector2(0.5f, 0.5f);
        viewRect.anchoredPosition = new Vector2(0f, 60f);
        viewRect.sizeDelta = new Vector2(900f, 1500f);

        Image background = viewObject.GetComponent<Image>();
        background.sprite = null;
        background.color = Color.clear;
        UIFrame.Build(viewObject.transform, viewRect.sizeDelta, Vector2.zero);

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

        UIFrame.MakeHeader(transform, uiManager.UIFont, "캐릭터", 645f);
        UIFrame.MakeClose(transform, uiManager.UIFont, new Vector2(390f, 645f), Close);
        goldText = MakeText(transform, string.Empty, new Vector2(270f, 570f), new Vector2(220f, 44f), 23, new Color(1f, 0.82f, 0.33f), TextAnchor.MiddleRight);

        Image infoPanel = MakePanel(transform, "InfoPanel", new Vector2(-245f, 345f), new Vector2(330f, 560f), null);
        infoPanel.color = Color.clear;
        UIFrame.Build(infoPanel.transform, new Vector2(330f, 560f), Vector2.zero);
        rarityText = MakeText(infoPanel.transform, string.Empty, new Vector2(-55f, 215f), new Vector2(110f, 54f), 27, new Color(1f, 0.76f, 0.26f), TextAnchor.MiddleLeft);
        nameText = MakeText(infoPanel.transform, string.Empty, new Vector2(30f, 142f), new Vector2(280f, 60f), 31, Color.white, TextAnchor.MiddleLeft);
        nameText.resizeTextForBestFit = true;
        nameText.resizeTextMinSize = 20;
        nameText.resizeTextMaxSize = 31;
        roleText = MakeText(infoPanel.transform, string.Empty, new Vector2(15f, 92f), new Vector2(250f, 42f), 21, new Color(0.58f, 0.86f, 0.94f), TextAnchor.MiddleLeft);
        levelText = MakeText(infoPanel.transform, string.Empty, new Vector2(15f, 35f), new Vector2(250f, 48f), 27, new Color(1f, 0.76f, 0.3f), TextAnchor.MiddleLeft);

        Image barBack = MakeImage(infoPanel.transform, "LevelBar", new Vector2(0f, -4f), new Vector2(220f, 16f), null);
        barBack.color = new Color(0.02f, 0.03f, 0.04f, 1f);
        levelFill = MakeImage(barBack.transform, "Fill", Vector2.zero, new Vector2(220f, 10f), null);
        RectTransform fillRect = levelFill.rectTransform;
        fillRect.anchorMin = new Vector2(0f, 0.5f);
        fillRect.anchorMax = new Vector2(0f, 0.5f);
        fillRect.pivot = new Vector2(0f, 0.5f);
        levelFill.color = new Color(0.08f, 0.78f, 0.82f, 1f);

        hpText = MakeStat(infoPanel.transform, "♡  HP", -50f);
        atkText = MakeStat(infoPanel.transform, "⚔  공격력", -108f);
        defText = MakeStat(infoPanel.transform, "▣  방어력", -166f);
        spdText = MakeStat(infoPanel.transform, "♟  속도", -224f);

        bodyImage = MakeImage(transform, "FullBody", new Vector2(180f, 240f), new Vector2(500f, 700f), null);
        bodyImage.preserveAspect = true;
        bodyImage.raycastTarget = false;

        Image sdFrame = MakePanel(transform, "SDFrame", new Vector2(310f, 15f), new Vector2(185f, 245f), uiManager.SlotFrame);
        sdFrame.color = new Color(0.88f, 0.66f, 0.18f, 1f);
        sdImage = MakeImage(sdFrame.transform, "SDCharacter", Vector2.zero, new Vector2(165f, 220f), null);
        sdImage.preserveAspect = true;
        sdImage.raycastTarget = false;

        Image normalSkill = MakePanel(transform, "NormalSkill", new Vector2(-325f, -80f), new Vector2(150f, 150f), null);
        normalSkill.color = Color.clear;
        UIFrame.Build(normalSkill.transform, new Vector2(150f, 150f), Vector2.zero);
        normalSkillIcon = MakeImage(normalSkill.transform, "SkillIcon", Vector2.zero, new Vector2(150f, 150f), null);
        normalSkillIcon.preserveAspect = true;
        normalSkillIcon.raycastTarget = false;
        MakeText(normalSkill.transform, "기본기", new Vector2(0f, 38f), new Vector2(120f, 32f), 20, new Color(0.25f, 0.9f, 0.96f), TextAnchor.MiddleCenter);
        Image specialSkill = MakePanel(transform, "SpecialSkill", new Vector2(-165f, -80f), new Vector2(150f, 150f), null);
        specialSkill.color = Color.clear;
        UIFrame.Build(specialSkill.transform, new Vector2(150f, 150f), Vector2.zero);
        specialSkillIcon = MakeImage(specialSkill.transform, "SkillIcon", Vector2.zero, new Vector2(150f, 150f), null);
        specialSkillIcon.preserveAspect = true;
        specialSkillIcon.raycastTarget = false;
        MakeText(specialSkill.transform, "궁극기", new Vector2(0f, 38f), new Vector2(120f, 32f), 20, new Color(1f, 0.72f, 0.2f), TextAnchor.MiddleCenter);


        growButton = MakeButton(transform, "성장", new Vector2(-245f, 40f), new Vector2(300f, 100f));
        Image growPanel = growButton.GetComponent<Image>();
        growPanel.sprite = null;
        growPanel.color = Color.clear;
        Image growFrame = UIFrame.Build(growButton.transform, new Vector2(300f, 100f), Vector2.zero);
        growFrame.color = Color.clear;
        Text growLabel = growButton.GetComponentInChildren<Text>();
        growLabel.rectTransform.anchoredPosition = new Vector2(-72f, 0f);
        growLabel.rectTransform.sizeDelta = new Vector2(100f, 60f);
        growLabel.fontSize = 27;
        costText = MakeText(growButton.transform, string.Empty, new Vector2(52f, 0f), new Vector2(150f, 60f), 20, new Color(1f, 0.82f, 0.34f), TextAnchor.MiddleCenter);
        growButton.onClick.AddListener(LevelUp);


        rosterList = OwnedCharacterListView.Create(transform, uiManager, gameManager, true, Select);
        rosterList.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, -440f);
        selectedId = rosterList.FirstId;

        // 하단 캐릭터 탭이 Growth 화면을 엽니다.

        goldText.transform.SetAsLastSibling();
        gameManager.ProgressChanged += Refresh;
        Refresh();
    }

    private Text MakeStat(Transform parent, string label, float y)
    {
        Image row = MakeImage(parent, "Stat", new Vector2(0f, y), new Vector2(250f, 44f), null);
        row.color = new Color(0.035f, 0.09f, 0.14f, 0.96f);
        MakeText(row.transform, label, new Vector2(-40f, 0f), new Vector2(140f, 38f), 20, Color.white, TextAnchor.MiddleLeft);
        return MakeText(row.transform, string.Empty, new Vector2(75f, 0f), new Vector2(50f, 38f), 20, Color.white, TextAnchor.MiddleRight);
    }

    private void Close()
    {
        uiManager.Switch(UIType.Battle);
    }

    private void Select(string id)
    {
        selectedId = id;
        Refresh();
    }

    private void LevelUp()
    {
        if (!string.IsNullOrEmpty(selectedId))
        {
            gameManager.TryLevelUp(selectedId);
            Refresh();
        }
    }

private void Refresh()
    {
        CharacterDefinition definition;
        CharacterProgressModel progress = gameManager.Progress.GetCharacter(selectedId);

        if (!gameManager.Data.TryGetCharacter(selectedId, out definition) || progress == null)
        {
            return;
        }

        int cost = GameUtil.GetLevelCost(progress.Level);
        int hp = GameUtil.GetLevelStat(definition.HitPoints, progress.Level);
        int attack = GameUtil.GetLevelStat(definition.Attack, progress.Level);
        int defense = GameUtil.GetLevelStat(definition.Defense, progress.Level);
        int speed = definition.Speed;

        goldText.text = "GOLD  " + gameManager.Progress.Gold;
        bodyImage.sprite = uiManager.GetBody(selectedId);
        sdImage.sprite = uiManager.GetPortrait(selectedId);
        ApplySkillIcon(normalSkillIcon, "normal");
        ApplySkillIcon(specialSkillIcon, "special");

        nameText.text = definition.DisplayName;
        rarityText.text = GameUtil.GetStars(definition.Rarity);
        roleText.text = definition.Role;
        levelText.text = "Lv. " + progress.Level + " / 50";
        levelFill.rectTransform.sizeDelta = new Vector2(220f * Mathf.Clamp01(progress.Level / 50f), 10f);
        hpText.text = hp.ToString();
        atkText.text = attack.ToString();
        defText.text = defense.ToString();
        spdText.text = speed.ToString();
        costText.text = "●  " + cost + " GOLD";
        growButton.interactable = gameManager.Progress.Gold >= cost;

        if (rosterList != null)
        {
            rosterList.SetSelected(selectedId);
        }
    }

private Sprite GetSkillIcon(string skillType)
    {
        if (string.IsNullOrEmpty(selectedId) ||
            !selectedId.StartsWith("CHAR_") ||
            selectedId.Length != 8 ||
            (skillType != "normal" && skillType != "special"))
        {
            return null;
        }

        int characterNumber;
        if (!int.TryParse(selectedId.Substring(5), out characterNumber) ||
            characterNumber < 1 ||
            characterNumber > 9)
        {
            return null;
        }

        return Resources.Load<Sprite>(
            "UI/SkillIcons/skill-char-" +
            characterNumber.ToString("000") +
            "-" +
            skillType);
    }

private void ApplySkillIcon(Image target, string skillType)
    {
        if (target == null)
        {
            return;
        }

        Sprite icon = GetSkillIcon(skillType);
        target.sprite = icon;
        target.color = icon == null
            ? new Color(0.02f, 0.055f, 0.095f, 0.98f)
            : Color.white;
    }



    private Text MakeText(Transform parent, string value, Vector2 position, Vector2 size, int fontSize, Color color, TextAnchor align)
    {
        GameObject textObject = new GameObject("Text", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
        textObject.transform.SetParent(parent, false);
        RectTransform rect = textObject.GetComponent<RectTransform>();
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        Text text = textObject.GetComponent<Text>();
        text.text = value;
        text.font = uiManager.UIFont != null ? uiManager.UIFont : Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = fontSize;
        text.alignment = align;
        text.color = color;
        return text;
    }

    private Image MakeImage(Transform parent, string name, Vector2 position, Vector2 size, Sprite sprite)
    {
        GameObject imageObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        imageObject.transform.SetParent(parent, false);
        RectTransform rect = imageObject.GetComponent<RectTransform>();
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        Image image = imageObject.GetComponent<Image>();
        image.sprite = sprite;
        return image;
    }

    private Image MakePanel(Transform parent, string name, Vector2 position, Vector2 size, Sprite sprite)
    {
        Image image = MakeImage(parent, name, position, size, sprite);
        image.type = Image.Type.Sliced;
        return image;
    }

    private Button MakeButton(Transform parent, string label, Vector2 position, Vector2 size)
    {
        GameObject buttonObject = new GameObject(label + "Button", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        buttonObject.transform.SetParent(parent, false);
        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        Image image = buttonObject.GetComponent<Image>();
        image.sprite = uiManager == null ? null : uiManager.HeaderPanel;
        image.type = Image.Type.Sliced;
        image.color = new Color(0.04f, 0.18f, 0.25f, 1f);
        MakeText(buttonObject.transform, label, Vector2.zero, size, 27, Color.white, TextAnchor.MiddleCenter);
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
