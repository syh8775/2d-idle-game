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
        // 인접 칸을 포함한 모든 프레임보다 캐릭터를 앞에 그립니다.
        Canvas parentCanvas = parent.GetComponentInParent<Canvas>();
        Canvas portraitCanvas = portraitObject.AddComponent<Canvas>();
        portraitCanvas.overrideSorting = true;
        portraitCanvas.sortingLayerID = parentCanvas != null ? parentCanvas.sortingLayerID : 0;
        portraitCanvas.sortingOrder = (parentCanvas != null ? parentCanvas.sortingOrder : 0) + 1 + (slot - 1) % 3 * 2;

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
        Canvas labelCanvas = labelObject.AddComponent<Canvas>();
        labelCanvas.overrideSorting = true;
        labelCanvas.sortingLayerID = portraitCanvas.sortingLayerID;
        labelCanvas.sortingOrder = portraitCanvas.sortingOrder + 1;

        Outline outline = labelObject.AddComponent<Outline>();
        outline.effectColor = new Color(0f, 0f, 0f, 0.9f);
        outline.effectDistance = new Vector2(2f, -2f);
        return view;
    }
}
