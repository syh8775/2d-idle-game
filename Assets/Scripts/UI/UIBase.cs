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
    }

    public virtual void Hide()
    {
        gameObject.SetActive(false);
    }
}
