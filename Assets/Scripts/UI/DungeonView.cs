using UnityEngine;
using UnityEngine.UI;

public class DungeonView : UIBase
{
    private UIManager uiManager;

    public static void Create(UIManager manager)
    {
        GameObject root = new GameObject("DungeonUI", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        root.transform.SetParent(manager.transform, false);

        RectTransform rect = root.GetComponent<RectTransform>();
        rect.anchoredPosition = new Vector2(0f, 60f);
        rect.sizeDelta = new Vector2(900f, 1500f);

        Image background = root.GetComponent<Image>();
        background.color = Color.clear;
        background.raycastTarget = true;
        UIFrame.Build(root.transform, rect.sizeDelta, Vector2.zero);

        DungeonView view = root.AddComponent<DungeonView>();
        view.SetType(UIType.Dungeon);
        view.Initialize(manager);
        manager.Register(view);
        view.Hide();
    }

private void Initialize(UIManager manager)
    {
        uiManager = manager;
        UIFrame.MakeHeader(transform, uiManager.UIFont, "던전", 645f);
        UIFrame.MakeClose(transform, uiManager.UIFont, new Vector2(390f, 645f), Close);

        AddCard("Equipment", "UI/Dungeon/EquipmentDungeon", new Vector2(0f, 360f));
        AddCard("Currency", "UI/Dungeon/CurrencyDungeon", new Vector2(0f, -70f));
        AddCard("Growth", "UI/Dungeon/GrowthDungeon", new Vector2(0f, -500f));
    }

private void AddCard(string name, string path, Vector2 position)
    {
        GameObject card = new GameObject(name + "Card", typeof(RectTransform));
        card.transform.SetParent(transform, false);
        RectTransform cardRect = card.GetComponent<RectTransform>();
        cardRect.anchoredPosition = position;
        cardRect.sizeDelta = new Vector2(630f, 420f);

        GameObject artObject = new GameObject("Art", typeof(RectTransform), typeof(CanvasRenderer), typeof(RawImage));
        artObject.transform.SetParent(card.transform, false);
        RectTransform artRect = artObject.GetComponent<RectTransform>();
        artRect.sizeDelta = cardRect.sizeDelta;
        RawImage art = artObject.GetComponent<RawImage>();
        art.texture = Resources.Load<Texture2D>(path);
        art.raycastTarget = false;

        GameObject hitObject = new GameObject("EnterHit", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        hitObject.transform.SetParent(card.transform, false);
        RectTransform hitRect = hitObject.GetComponent<RectTransform>();
        hitRect.anchoredPosition = new Vector2(0f, -150f);
        hitRect.sizeDelta = new Vector2(300f, 60f);
        hitObject.GetComponent<Image>().color = Color.clear;
        hitObject.GetComponent<Button>().onClick.AddListener(ShowReady);
    }

    private void ShowReady()
    {
        ReadyPopup.Open(uiManager);
    }

    private void Close()
    {
        uiManager.Switch(UIType.Battle);
    }
}
