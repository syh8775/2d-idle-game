using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OwnedCharacterListView : MonoBehaviour
{
    private UIManager uiManager;
    private GameManager gameManager;
    private Action<string> selectAction;
    private bool selectable;
    private string selectedId = string.Empty;
    private readonly List<string> characterIds = new List<string>();
    private readonly List<Button> memberButtons = new List<Button>();
    private readonly List<Image> memberImages = new List<Image>();
    private readonly List<Image> memberFrames = new List<Image>();

    public string FirstId
    {
        get { return characterIds.Count > 0 ? characterIds[0] : string.Empty; }
    }

    public static OwnedCharacterListView Create(Transform parent, UIManager manager, GameManager owner, bool canSelect, Action<string> onSelect)
    {
        GameObject listObject = new GameObject("OwnedCharacterList", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(RectMask2D));
        listObject.transform.SetParent(parent, false);

        RectTransform rect = listObject.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(820f, 560f);

        Image background = listObject.GetComponent<Image>();
        background.color = new Color(0.035f, 0.105f, 0.16f, 0.97f);

        OwnedCharacterListView view = listObject.AddComponent<OwnedCharacterListView>();
        view.Initialize(manager, owner, canSelect, onSelect);
        return view;
    }

    private void Initialize(UIManager manager, GameManager owner, bool canSelect, Action<string> onSelect)
    {
        uiManager = manager;
        gameManager = owner;
        selectable = canSelect;
        selectAction = onSelect;

        Vector2 titlePosition = new Vector2(-270f, 235f);
        MakeText(transform, "보유 캐릭터", titlePosition, new Vector2(250f, 42f), 24, new Color(1f, 0.84f, 0.38f), TextAnchor.MiddleLeft);

        float[] faceYs = { -83f, -83f, -79f, -75f, -87f, -87f, -65f, -87f, -93f };
        float[] faceXs = { 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, -12f };
        int index = 0;

        foreach (CharacterDefinition character in gameManager.Data.Characters.Values)
        {
            string id = character.Id;
            int row = index / 5;
            int column = index % 5;
            int rowCount = Mathf.Min(5, gameManager.Data.Characters.Count - row * 5);
            float gapX = 150f;
            float startY = 100f;
            float gapY = 240f;
            float x = (column - (rowCount - 1) * 0.5f) * gapX;
            float y = startY - row * gapY;

            Button button = MakeButton(transform, new Vector2(x, y), new Vector2(145f, 220f));
            button.name = "Member_" + (index + 1).ToString("00");
            button.transform.localScale = Vector3.one;

            Image buttonImage = button.GetComponent<Image>();
            buttonImage.sprite = null;
            buttonImage.color = Color.clear;

            Image rosterFrame = UIFrame.Build(button.transform, new Vector2(135f, 135f), new Vector2(0f, 28f));
            rosterFrame.color = Color.clear;
            rosterFrame.transform.SetAsLastSibling();

            GameObject maskObject = new GameObject("PortraitMask", typeof(RectTransform), typeof(RectMask2D));
            maskObject.transform.SetParent(button.transform, false);
            RectTransform maskRect = maskObject.GetComponent<RectTransform>();
            maskRect.anchoredPosition = new Vector2(0f, 28f);
            maskRect.sizeDelta = new Vector2(115f, 115f);

            float faceY = index < faceYs.Length ? faceYs[index] : -87f;
            float faceX = index < faceXs.Length ? faceXs[index] : 0f;
            Vector2 portraitSize = index == 8 ? new Vector2(300f, 400f) : new Vector2(250f, 333f);
            Image portrait = MakeImage(maskObject.transform, "Portrait", new Vector2(faceX, faceY), portraitSize, uiManager.GetPortrait(id));
            portrait.preserveAspect = true;

            MakeText(button.transform, "Lv.", new Vector2(0f, -60f), new Vector2(120f, 30f), 17, Color.white, TextAnchor.MiddleCenter);
            MakeText(button.transform, GetStars(character.Rarity), new Vector2(0f, -87f), new Vector2(105f, 26f), 16, new Color(1f, 0.72f, 0.18f), TextAnchor.MiddleCenter);

            if (selectable)
            {
                button.onClick.AddListener(delegate { Select(id); });
            }
            else
            {
                button.interactable = false;
            }

            characterIds.Add(id);
            memberButtons.Add(button);
            memberImages.Add(portrait);
            memberFrames.Add(buttonImage);
            index++;
        }

        if (selectable && characterIds.Count > 0)
        {
            selectedId = characterIds[0];
        }

        gameManager.ProgressChanged += Refresh;
        Refresh();
    }

    public void SetSelected(string id)
    {
        selectedId = id;
        Refresh();
    }

    public void Refresh()
    {
        for (int i = 0; i < characterIds.Count; i++)
        {
            CharacterProgressModel progress = gameManager.Progress.GetCharacter(characterIds[i]);
            bool selected = selectable && characterIds[i] == selectedId;
            memberFrames[i].color = selected
                ? new Color(0.08f, 0.22f, 0.25f, 1f)
                : new Color(0.02f, 0.055f, 0.095f, 0.98f);

            Text[] labels = memberButtons[i].GetComponentsInChildren<Text>();
            if (labels.Length > 1 && progress != null)
            {
                labels[1].text = "Lv." + progress.Level;
            }

            memberImages[i].sprite = uiManager.GetPortrait(characterIds[i]);
        }
    }

    private void Select(string id)
    {
        selectedId = id;
        Refresh();

        if (selectAction != null)
        {
            selectAction(id);
        }
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

    private Button MakeButton(Transform parent, Vector2 position, Vector2 size)
    {
        GameObject buttonObject = new GameObject("MemberButton", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        buttonObject.transform.SetParent(parent, false);
        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        rect.anchoredPosition = position;
        rect.sizeDelta = size;

        Image image = buttonObject.GetComponent<Image>();
        image.sprite = uiManager.HeaderPanel;
        image.type = Image.Type.Sliced;
        image.color = new Color(0.04f, 0.18f, 0.25f, 1f);
        return buttonObject.GetComponent<Button>();
    }

    private string GetStars(string rarity)
    {
        if (rarity == "SSR")
        {
            return "★★★";
        }

        if (rarity == "SR")
        {
            return "★★";
        }

        return "★";
    }

    private void OnDestroy()
    {
        if (gameManager != null)
        {
            gameManager.ProgressChanged -= Refresh;
        }
    }
}
