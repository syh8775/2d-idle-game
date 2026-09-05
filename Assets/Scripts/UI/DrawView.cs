using UnityEngine;
using UnityEngine.UI;

public class DrawView : UIBase
{
    [SerializeField] private UIManager uiManager;

    public void Initialize(UIManager manager)
    {
        uiManager = manager;
        bool isWide = Screen.width > Screen.height;
        RectTransform rect = GetComponent<RectTransform>();
        rect.anchoredPosition = new Vector2(0f, isWide ? 120f : 60f);
        rect.localScale = Vector3.one * (isWide ? 0.5f : 1f);
        transform.Find("CloseButton").GetComponent<UIButton>().Bind(Close);
        foreach (Button button in GetComponentsInChildren<Button>(true))
        {
            if (button.GetComponent<UIButton>() == null) button.onClick.AddListener(ShowReady);
        }
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
