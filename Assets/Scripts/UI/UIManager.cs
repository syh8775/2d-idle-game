using UnityEngine;
using System.Collections.Generic;

public class UIManager : MonoBehaviour
{
    [Header("UI Theme")]
    [SerializeField] private Font uiFont;
    [SerializeField] private Sprite headerPanel;
    [SerializeField] private Sprite slotFrame;
    [SerializeField] private string[] portraitIds = new string[0];
    [SerializeField] private Sprite[] portraits = new Sprite[0];
    [SerializeField] private Sprite[] fullBodies = new Sprite[0];

    private Dictionary<UIType, UIBase> views = new Dictionary<UIType, UIBase>();

    public Font UIFont { get { return uiFont; } }
    public Sprite HeaderPanel { get { return headerPanel; } }
    public Sprite SlotFrame { get { return slotFrame; } }

    private void Awake()
    {
        UIBase[] foundViews = GetComponentsInChildren<UIBase>(true);

        foreach (UIBase view in foundViews)
        {
            Register(view);
        }
    }

    public bool Register(UIBase view)
    {
        if (view == null || views.ContainsKey(view.Type))
        {
            return false;
        }

        views.Add(view.Type, view);
        return true;
    }

public bool Switch(UIType type)
    {
        if (!views.ContainsKey(type))
        {
            return false;
        }

        foreach (UIBase view in views.Values)
        {
            view.Hide();
        }

        views[type].Show();

        Transform tabs = transform.Find("PersistentBottomTabs");
        if (tabs != null)
        {
            BottomTabs bottomTabs = tabs.GetComponent<BottomTabs>();
            if (bottomTabs != null)
            {
                bottomTabs.SetActive(type);
            }
        }

        return true;
    }

    public Sprite GetPortrait(string id)
    {
        for (int i = 0; i < portraitIds.Length && i < portraits.Length; i++)
        {
            if (portraitIds[i] == id)
            {
                return portraits[i];
            }
        }

        return Resources.Load<Sprite>("UI/Formation/" + id + "-trimmed");
    }

    public Sprite GetBody(string id)
    {
        for (int i = 0; i < portraitIds.Length && i < fullBodies.Length; i++)
        {
            if (portraitIds[i] == id)
            {
                return fullBodies[i];
            }
        }

        return Resources.Load<Sprite>("UI/Formation/" + id + "-trimmed");
    }
}
