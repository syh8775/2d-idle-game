using UnityEngine;
using UnityEngine.UI;

public class DrawView : UIBase
{
    private UIManager uiManager;

    public static void Create(UIManager manager)
    {
        GameObject root = new GameObject("DrawUI", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        root.transform.SetParent(manager.transform, false);

        RectTransform rect = root.GetComponent<RectTransform>();
        bool isWide = Screen.width > Screen.height;
        rect.anchoredPosition = new Vector2(0f, isWide ? 120f : 60f);
        rect.localScale = Vector3.one * (isWide ? 0.5f : 1f);
        rect.sizeDelta = new Vector2(900f, 1500f);

        Image background = root.GetComponent<Image>();
        background.color = Color.clear;
        background.raycastTarget = true;
        UIFrame.Build(root.transform, rect.sizeDelta, Vector2.zero);

        DrawView view = root.AddComponent<DrawView>();
        view.SetType(UIType.Draw);
        view.Initialize(manager);
        manager.Register(view);
        view.Hide();
    }

private void Initialize(UIManager manager)
    {
        uiManager = manager;
        UIFrame.MakeHeader(transform, uiManager.UIFont, "뽑기", 645f);
        UIFrame.MakeClose(transform, uiManager.UIFont, new Vector2(390f, 645f), Close);

        AddCard("Character", "UI/Draw/CharacterDraw", new Vector2(0f, 290f));
        AddCard("Equipment", "UI/Draw/EquipmentDraw", new Vector2(0f, -290f));
    }



private GameObject AddCard(string name, string path, Vector2 position)
    {
        GameObject card = new GameObject(name + "Card", typeof(RectTransform));
        card.transform.SetParent(transform, false);
        RectTransform cardRect = card.GetComponent<RectTransform>();
        cardRect.anchoredPosition = position;
        cardRect.sizeDelta = new Vector2(820f, 547f);

        GameObject artObject = new GameObject("Art", typeof(RectTransform), typeof(CanvasRenderer), typeof(RawImage));
        artObject.transform.SetParent(card.transform, false);
        RectTransform artRect = artObject.GetComponent<RectTransform>();
        artRect.sizeDelta = cardRect.sizeDelta;
        RawImage art = artObject.GetComponent<RawImage>();
        art.texture = Resources.Load<Texture2D>(path);
        art.raycastTarget = false;

        GameObject hitObject = new GameObject("DrawHit", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        hitObject.transform.SetParent(card.transform, false);
        RectTransform hitRect = hitObject.GetComponent<RectTransform>();
        hitRect.anchoredPosition = new Vector2(0f, -210f);
        hitRect.sizeDelta = new Vector2(310f, 72f);
        hitObject.GetComponent<Image>().color = Color.clear;
        hitObject.GetComponent<Button>().onClick.AddListener(ShowReady);
        return card;
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
