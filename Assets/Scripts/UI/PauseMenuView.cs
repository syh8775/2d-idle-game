using UnityEngine;
using UnityEngine.UI;

public class PauseMenuView : MonoBehaviour
{
    private GameObject popup;
    private GameObject pauseContent;
    private Canvas popupCanvas;
    private float previousTimeScale = 1f;

    public static void Create(UIManager uiManager)
    {
        GameObject viewObject = new GameObject("PauseMenuView", typeof(PauseMenuView));
        viewObject.transform.SetParent(uiManager.transform, false);
        viewObject.GetComponent<PauseMenuView>().Build(uiManager);
    }

    private void Build(UIManager uiManager)
    {
        Image sourcePanel = FindPanel(uiManager.transform);
        if (sourcePanel == null)
        {
            Debug.LogError("ESC 메뉴에 사용할 기존 결과 팝업을 찾을 수 없습니다.");
            return;
        }

        popup = sourcePanel.gameObject;
        popupCanvas = popup.GetComponent<Canvas>();
        if (popupCanvas == null) popupCanvas = popup.AddComponent<Canvas>();
        popupCanvas.overrideSorting = true;
        popupCanvas.sortingOrder = 1000;
        pauseContent = new GameObject("PauseMenuContent", typeof(RectTransform));
        pauseContent.transform.SetParent(popup.transform, false);
        RectTransform contentRect = pauseContent.GetComponent<RectTransform>();
        contentRect.anchorMin = Vector2.zero;
        contentRect.anchorMax = Vector2.one;
        contentRect.offsetMin = Vector2.zero;
        contentRect.offsetMax = Vector2.zero;

        MakeText(pauseContent.transform, uiManager.UIFont, "일시정지", new Vector2(0f, 170f), new Vector2(500f, 100f), 52);
        MakeButton(pauseContent.transform, uiManager, "계속하기", new Vector2(0f, 30f), ContinueGame);
        MakeButton(pauseContent.transform, uiManager, "게임 종료", new Vector2(0f, -90f), QuitGame);
        pauseContent.SetActive(false);
    }

    private static Image FindPanel(Transform root)
    {
        foreach (Image image in root.GetComponentsInChildren<Image>(true))
        {
            if (image.name == "BattleResultPanel_AutoOff_5s") return image;
        }

        return null;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (pauseContent != null && pauseContent.activeSelf) ContinueGame();
            else if (popup != null && !popup.activeSelf) Open();
        }
    }

    private void Open()
    {
        previousTimeScale = Time.timeScale;
        Time.timeScale = 0f;
        pauseContent.SetActive(true);
        popup.SetActive(true);
        popupCanvas.overrideSorting = true;
        popupCanvas.sortingOrder = 1000;
        popup.transform.SetAsLastSibling();
    }

    private void ContinueGame()
    {
        pauseContent.SetActive(false);
        popup.SetActive(false);
        Time.timeScale = previousTimeScale;
    }

    private void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private static void MakeText(Transform parent, Font font, string value, Vector2 position, Vector2 size, int fontSize)
    {
        GameObject textObject = new GameObject("Text", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
        textObject.transform.SetParent(parent, false);
        RectTransform rect = textObject.GetComponent<RectTransform>();
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        Text text = textObject.GetComponent<Text>();
        text.text = value;
        text.font = font != null ? font : Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = fontSize;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = Color.white;
    }

    private static void MakeButton(Transform parent, UIManager uiManager, string label, Vector2 position, UnityEngine.Events.UnityAction action)
    {
        GameObject buttonObject = new GameObject(label, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Outline), typeof(Button));
        buttonObject.transform.SetParent(parent, false);
        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        rect.anchoredPosition = position;
        rect.sizeDelta = new Vector2(360f, 86f);
        Image image = buttonObject.GetComponent<Image>();
        image.sprite = null;
        image.color = new Color(0.025f, 0.09f, 0.14f, 1f);
        Outline outline = buttonObject.GetComponent<Outline>();
        outline.effectColor = new Color(0.95f, 0.68f, 0.18f, 1f);
        outline.effectDistance = new Vector2(2f, -2f);
        UIFrame.Build(buttonObject.transform, rect.sizeDelta, Vector2.zero);
        buttonObject.GetComponent<Button>().onClick.AddListener(action);
        MakeText(buttonObject.transform, uiManager.UIFont, label, Vector2.zero, rect.sizeDelta, 30);
    }

    private void OnDestroy()
    {
        if (pauseContent != null && pauseContent.activeSelf) Time.timeScale = previousTimeScale;
    }
}
