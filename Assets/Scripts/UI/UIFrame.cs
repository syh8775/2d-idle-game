using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public static class UIFrame
{
    public static Image Build(Transform parent, Vector2 size, Vector2 position)
    {
        GameObject root = new GameObject("CommonFrame", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        root.transform.SetParent(parent, false);

        RectTransform rect = root.GetComponent<RectTransform>();
        rect.anchoredPosition = position;
        rect.sizeDelta = size;

        Image background = root.GetComponent<Image>();
        background.color = new Color(0.02f, 0.055f, 0.095f, 0.98f);
        background.raycastTarget = false;

        float corner = 52f;
        float halfWidth = (size.x - corner) * 0.5f;
        float halfHeight = (size.y - corner) * 0.5f;

        AddPiece(root.transform, "Top", "FrameTop", new Vector2(0f, halfHeight), new Vector2(size.x - corner * 2f, corner));
        AddPiece(root.transform, "Right", "FrameRight", new Vector2(halfWidth, 0f), new Vector2(corner, size.y - corner * 2f));
        AddPiece(root.transform, "Bottom", "FrameBottom", new Vector2(0f, -halfHeight), new Vector2(size.x - corner * 2f, corner));
        AddPiece(root.transform, "Left", "FrameLeft", new Vector2(-halfWidth, 0f), new Vector2(corner, size.y - corner * 2f));

        AddPiece(root.transform, "CornerTL", "FrameCornerTL", new Vector2(-halfWidth, halfHeight), new Vector2(corner, corner));
        AddPiece(root.transform, "CornerTR", "FrameCornerTR", new Vector2(halfWidth, halfHeight), new Vector2(corner, corner));
        AddPiece(root.transform, "CornerBR", "FrameCornerBR", new Vector2(halfWidth, -halfHeight), new Vector2(corner, corner));
        AddPiece(root.transform, "CornerBL", "FrameCornerBL", new Vector2(-halfWidth, -halfHeight), new Vector2(corner, corner));

        root.transform.SetAsFirstSibling();
        return background;
    }

    private static Image AddPiece(Transform parent, string name, string spriteName, Vector2 position, Vector2 size)
    {
        GameObject piece = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        piece.transform.SetParent(parent, false);

        RectTransform rect = piece.GetComponent<RectTransform>();
        rect.anchoredPosition = position;
        rect.sizeDelta = size;

        Image image = piece.GetComponent<Image>();
        image.sprite = Resources.Load<Sprite>("UI/CommonFrameModules/" + spriteName);
        image.type = Image.Type.Simple;
        image.raycastTarget = false;
        return image;
    }


    public static UIButton MakeClose(Transform parent, Font font, Vector2 position, UnityAction action)
    {
        GameObject buttonObject = new GameObject("CloseButton", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button), typeof(UIButton));
        buttonObject.transform.SetParent(parent, false);
        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        rect.anchoredPosition = position;
        rect.sizeDelta = new Vector2(64f, 64f);
        buttonObject.GetComponent<Image>().color = new Color(0.04f, 0.18f, 0.25f, 1f);

        GameObject textObject = new GameObject("Text", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
        textObject.transform.SetParent(buttonObject.transform, false);
        textObject.GetComponent<RectTransform>().sizeDelta = new Vector2(64f, 64f);
        Text text = textObject.GetComponent<Text>();
        text.text = "X";
        text.font = font != null ? font : Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = 30;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = Color.white;

        UIButton button = buttonObject.GetComponent<UIButton>();
        button.Bind(action);
        return button;
    }


public static Image MakeHeader(Transform parent, Font font, string title, float y)
    {
        GameObject headerObject = new GameObject("Header", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        headerObject.transform.SetParent(parent, false);
        RectTransform headerRect = headerObject.GetComponent<RectTransform>();
        headerRect.anchoredPosition = new Vector2(0f, y);
        headerRect.sizeDelta = new Vector2(820f, 96f);

        Image header = headerObject.GetComponent<Image>();
        header.color = new Color(0.025f, 0.07f, 0.11f, 0.98f);
        AddLine(headerObject.transform, new Vector2(0f, 37f));
        AddLine(headerObject.transform, new Vector2(0f, -37f));

        GameObject textObject = new GameObject("Title", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
        textObject.transform.SetParent(headerObject.transform, false);
        textObject.GetComponent<RectTransform>().sizeDelta = new Vector2(720f, 70f);
        Text text = textObject.GetComponent<Text>();
        text.text = title;
        text.font = font != null ? font : Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = 42;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = new Color(1f, 0.84f, 0.38f);
        return header;
    }

    private static void AddLine(Transform parent, Vector2 position)
    {
        GameObject lineObject = new GameObject("Line", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        lineObject.transform.SetParent(parent, false);
        RectTransform lineRect = lineObject.GetComponent<RectTransform>();
        lineRect.anchoredPosition = position;
        lineRect.sizeDelta = new Vector2(780f, 4f);
        lineObject.GetComponent<Image>().color = new Color(0.86f, 0.63f, 0.18f, 1f);
    }
}
