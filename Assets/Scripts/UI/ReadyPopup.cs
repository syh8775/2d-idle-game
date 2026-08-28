using UnityEngine;
using UnityEngine.UI;

public class ReadyPopup : MonoBehaviour
{
    private static ReadyPopup instance;

    public static void Open(UIManager manager)
    {
        if (instance == null)
        {
            Create(manager);
        }

        instance.gameObject.SetActive(true);
        instance.transform.SetAsLastSibling();
    }

    private static void Create(UIManager manager)
    {
        Transform oldPopup = manager.transform.Find("ReadyPopup");
        if (oldPopup != null)
        {
            instance = oldPopup.GetComponent<ReadyPopup>();
            return;
        }

        GameObject root = new GameObject("ReadyPopup", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(ReadyPopup));
        root.transform.SetParent(manager.transform, false);

        RectTransform rootRect = root.GetComponent<RectTransform>();
        rootRect.anchorMin = Vector2.zero;
        rootRect.anchorMax = Vector2.one;
        rootRect.offsetMin = Vector2.zero;
        rootRect.offsetMax = Vector2.zero;

        Image blocker = root.GetComponent<Image>();
        blocker.color = new Color(0f, 0f, 0f, 0.72f);
        blocker.raycastTarget = true;

        GameObject panel = new GameObject("Panel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        panel.transform.SetParent(root.transform, false);
        RectTransform panelRect = panel.GetComponent<RectTransform>();
        panelRect.sizeDelta = new Vector2(560f, 260f);
        panel.GetComponent<Image>().color = Color.clear;
        UIFrame.Build(panel.transform, panelRect.sizeDelta, Vector2.zero);

        MakeText(panel.transform, manager.UIFont, "준비 중입니다", new Vector2(0f, 5f), new Vector2(450f, 100f), 38);
        UIFrame.MakeClose(panel.transform, manager.UIFont, new Vector2(230f, 95f), Hide);

        instance = root.GetComponent<ReadyPopup>();
        root.SetActive(false);
    }

    private static void Hide()
    {
        if (instance != null)
        {
            instance.gameObject.SetActive(false);
        }
    }

    private static Text MakeText(Transform parent, Font font, string value, Vector2 position, Vector2 size, int fontSize)
    {
        GameObject textObject = new GameObject("Message", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
        textObject.transform.SetParent(parent, false);
        RectTransform rect = textObject.GetComponent<RectTransform>();
        rect.anchoredPosition = position;
        rect.sizeDelta = size;

        Text text = textObject.GetComponent<Text>();
        text.text = value;
        text.font = font != null ? font : Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = fontSize;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = new Color(1f, 0.84f, 0.38f);
        return text;
    }
}
