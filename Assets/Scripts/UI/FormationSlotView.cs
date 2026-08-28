using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class FormationSlotView : MonoBehaviour
{
    public Button Button { get; private set; }
    public Image Portrait { get; private set; }
    public Text Label { get; private set; }

    public static FormationSlotView Create(Transform parent, UIManager uiManager, Sprite frameSprite, int slot, UnityAction onClick)
    {
        GameObject slotObject = new GameObject("Slot_" + slot.ToString("00"), typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button), typeof(FormationSlotView));
        slotObject.transform.SetParent(parent, false);

        Image frame = slotObject.GetComponent<Image>();
        frame.sprite = frameSprite;
        frame.type = Image.Type.Sliced;
        frame.color = Color.white;

        Image commonFrame = UIFrame.Build(slotObject.transform, new Vector2(140f, 140f), Vector2.zero);
        commonFrame.color = Color.clear;

        FormationSlotView view = slotObject.GetComponent<FormationSlotView>();
        view.Button = slotObject.GetComponent<Button>();
        view.Button.onClick.AddListener(onClick);

        GameObject portraitObject = new GameObject("Portrait", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        portraitObject.transform.SetParent(slotObject.transform, false);
        RectTransform portraitRect = portraitObject.GetComponent<RectTransform>();
        portraitRect.anchorMin = new Vector2(0.5f, 0.5f);
        portraitRect.anchorMax = new Vector2(0.5f, 0.5f);
        portraitRect.pivot = new Vector2(0.5f, 0.5f);
        portraitRect.anchoredPosition = Vector2.zero;
        portraitRect.sizeDelta = new Vector2(140f, 140f);
        view.Portrait = portraitObject.GetComponent<Image>();
        view.Portrait.preserveAspect = true;
        view.Portrait.raycastTarget = false;

        GameObject labelObject = new GameObject("Label", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
        labelObject.transform.SetParent(slotObject.transform, false);
        RectTransform labelRect = labelObject.GetComponent<RectTransform>();
        labelRect.anchorMin = new Vector2(0f, 0f);
        labelRect.anchorMax = new Vector2(1f, 0f);
        labelRect.pivot = new Vector2(0.5f, 0f);
        labelRect.offsetMin = new Vector2(0f, 6f);
        labelRect.offsetMax = new Vector2(0f, 38f);

        view.Label = labelObject.GetComponent<Text>();
        view.Label.text = "슬롯 " + slot;
        view.Label.font = uiManager.UIFont != null ? uiManager.UIFont : Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        view.Label.fontSize = 18;
        view.Label.alignment = TextAnchor.MiddleCenter;
        view.Label.color = Color.white;
        view.Label.raycastTarget = false;

        Outline outline = labelObject.AddComponent<Outline>();
        outline.effectColor = new Color(0f, 0f, 0f, 0.9f);
        outline.effectDistance = new Vector2(2f, -2f);
        return view;
    }
}
