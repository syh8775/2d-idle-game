using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class UIButton : MonoBehaviour
{
    private Button button;
    private UnityAction clickAction;

    private void Awake()
    {
        LoadButton();
    }

    public void Bind(UnityAction action)
    {
        LoadButton();

        if (clickAction != null)
        {
            button.onClick.RemoveListener(clickAction);
        }

        clickAction = action;

        if (clickAction != null)
        {
            button.onClick.AddListener(clickAction);
        }
    }

    public void Lock(bool value)
    {
        LoadButton();
        button.interactable = !value;
    }

    private void LoadButton()
    {
        if (button == null)
        {
            button = GetComponent<Button>();
        }
    }

    private void OnDestroy()
    {
        if (button != null && clickAction != null)
        {
            button.onClick.RemoveListener(clickAction);
        }
    }
}
