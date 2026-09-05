using UnityEngine;
using UnityEngine.UI;

public class DungeonView : UIBase
{
    [SerializeField] private UIManager uiManager;

    public void Initialize(UIManager manager)
    {
        uiManager = manager;
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
