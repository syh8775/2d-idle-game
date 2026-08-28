using UnityEngine;

public class UIBase : MonoBehaviour
{
    [SerializeField]
    private UIType type;

    public UIType Type { get { return type; } }

    public void SetType(UIType value)
    {
        type = value;
    }

public virtual void Show()
    {
        gameObject.SetActive(true);
        transform.SetAsLastSibling();

        Transform bottomTabs = transform.parent.Find("PersistentBottomTabs");
        if (bottomTabs != null)
        {
            bottomTabs.SetAsLastSibling();
        }
    }

    public virtual void Hide()
    {
        gameObject.SetActive(false);
    }
}
