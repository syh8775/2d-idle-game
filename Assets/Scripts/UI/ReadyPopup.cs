using UnityEngine;

public class ReadyPopup : MonoBehaviour
{
    private static ReadyPopup instance;

    public static void Open(UIManager manager)
    {
        if (instance == null)
        {
            instance = manager.transform.Find("ReadyPopup").GetComponent<ReadyPopup>();
            instance.transform.Find("Panel/CloseButton").GetComponent<UIButton>().Bind(Hide);
        }
        instance.gameObject.SetActive(true);
        instance.transform.SetAsLastSibling();
    }

    private static void Hide()
    {
        if (instance != null) instance.gameObject.SetActive(false);
    }
}
