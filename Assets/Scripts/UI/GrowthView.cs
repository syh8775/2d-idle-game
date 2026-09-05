using UnityEngine;
using UnityEngine.UI;

public class GrowthView : UIBase
{
    private GameManager gameManager;
    [SerializeField] private UIManager uiManager;
    [SerializeField] private Text goldText;

    [SerializeField] private Text rarityText;
    [SerializeField] private Text nameText;
    [SerializeField] private Text roleText;
    [SerializeField] private Text levelText;
    [SerializeField] private Text hpText;
    [SerializeField] private Text atkText;
    [SerializeField] private Text defText;
    [SerializeField] private Text spdText;
    [SerializeField] private Text costText;
    [SerializeField] private Image bodyImage;
    [SerializeField] private Image sdImage;
    [SerializeField] private Image normalSkillIcon;
    [SerializeField] private Image specialSkillIcon;
    [SerializeField] private Image levelFill;
    [SerializeField] private Button growButton;
    private string selectedId = string.Empty;
    [SerializeField] private OwnedCharacterListView rosterList;

    public void Initialize(UIManager manager, GameManager owner)
    {
        uiManager = manager;
        gameManager = owner;
        transform.Find("CloseButton").GetComponent<UIButton>().Bind(Close);
        growButton.onClick.AddListener(LevelUp);
        rosterList.Initialize(manager, owner, true, Select);
        selectedId = rosterList.FirstId;
        gameManager.ProgressChanged += Refresh;
        Refresh();
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
        levelText.text = "Lv. " + progress.Level + " / " + GameUtil.MaxLevel;
        levelFill.rectTransform.sizeDelta = new Vector2(220f * Mathf.Clamp01((float)progress.Level / GameUtil.MaxLevel), 10f);
        hpText.text = hp.ToString();
        atkText.text = attack.ToString();
        defText.text = defense.ToString();
        spdText.text = speed.ToString();
        costText.text = progress.Level >= GameUtil.MaxLevel ? "최대 레벨" : "●  " + cost + " GOLD";
        growButton.interactable = progress.Level < GameUtil.MaxLevel && gameManager.Progress.Gold >= cost;

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



    private void OnDestroy()
    {
        if (gameManager != null)
        {
            gameManager.ProgressChanged -= Refresh;
        }
    }


}
