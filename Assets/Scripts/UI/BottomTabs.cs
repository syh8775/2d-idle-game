using UnityEngine;
using UnityEngine.UI;

public class BottomTabs : MonoBehaviour
{
    private UIManager uiManager;

    private void Awake()
    {
        uiManager = GetComponentInParent<UIManager>();

        transform.Find("Tab_홈").GetComponent<Button>().onClick.AddListener(OpenBattle);
        transform.Find("Tab_캐릭터").GetComponent<Button>().onClick.AddListener(OpenGrowth);
        transform.Find("Tab_편성").GetComponent<Button>().onClick.AddListener(OpenFormation);
    }

    private void Start()
    {
        Transform growthButton = uiManager.transform.Find("GrowthButton");
        if (growthButton != null)
        {
            growthButton.gameObject.SetActive(false);
        }
    }

    private void OpenBattle()
    {
        uiManager.Switch(UIType.Battle);
    }

    private void OpenGrowth()
    {
        Button button = uiManager.transform.Find("GrowthButton").GetComponent<Button>();
        button.onClick.Invoke();
    }

    private void OpenFormation()
    {
        Button button = uiManager.transform.Find("FormationUI/OpenButton").GetComponent<Button>();
        button.onClick.Invoke();
    }
}
