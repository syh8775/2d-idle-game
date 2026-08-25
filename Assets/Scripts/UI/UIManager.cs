using UnityEngine;
using System.Collections.Generic;

public class UIManager : MonoBehaviour
{
    private Dictionary<UIType, UIBase> views = new Dictionary<UIType, UIBase>();

    private void Awake()
    {
        UIBase[] foundViews = GetComponentsInChildren<UIBase>(true);

        foreach (UIBase view in foundViews)
        {
            if (views.ContainsKey(view.Type))
            {
                Debug.LogError("같은 종류의 UI가 이미 존재합니다: " + view.Type);
                continue;
            }
            views.Add(view.Type, view);
        }
    }

    public bool Switch(UIType type)
    {
        if (type == UIType.Popup || !views.ContainsKey(type))
        {
            return false;
        }

        foreach (KeyValuePair<UIType, UIBase> pair in views)
        {
            if (pair.Key == UIType.Popup)
            {
                continue;
            }

            if (pair.Key == type)
            {
                pair.Value.Show();
            }
            else
            {
                pair.Value.Hide();
            }
        }

        return true;
    }

    public bool Show(UIType type)
    {
        UIBase view;

        if (!views.TryGetValue(type, out view))
        {
            return false;
        }

        view.Show();
        return true;
    }

    public bool Hide(UIType type)
    {
        UIBase view;
        if (!views.TryGetValue(type, out view))
        {
            return false;
        }
        view.Hide();
        return true;
    }
}
