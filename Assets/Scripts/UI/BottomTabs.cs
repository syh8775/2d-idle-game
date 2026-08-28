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
        transform.Find("Tab_던전").GetComponent<Button>().onClick.AddListener(OpenDungeon);
        transform.Find("Tab_뽑기").GetComponent<Button>().onClick.AddListener(OpenDraw);
    }

private void Start()
    {
        Transform growthButton = uiManager.transform.Find("GrowthButton");
        if (growthButton != null)
        {
            growthButton.gameObject.SetActive(false);
        }

        SetActive(UIType.Battle);
    }

public void SetActive(UIType type)
    {
        SetColor("Tab_홈", type == UIType.Battle);
        SetColor("Tab_캐릭터", type == UIType.Growth);
        SetColor("Tab_편성", type == UIType.Formation);
        SetColor("Tab_던전", type == UIType.Dungeon);
        SetColor("Tab_뽑기", type == UIType.Draw);
    }

    private void SetColor(string tabName, bool selected)
    {
        Transform tab = transform.Find(tabName);
        if (tab == null)
        {
            return;
        }

        Image image = tab.GetComponent<Image>();
        if (image != null)
        {
            image.color = selected
                ? new Color(0.12f, 0.50f, 0.64f, 1f)
                : new Color(0.06f, 0.18f, 0.32f, 1f);
        }
    }


    private void OpenBattle()
    {
        uiManager.Switch(UIType.Battle);
    }

private void OpenGrowth()
    {
        uiManager.Switch(UIType.Growth);
    }

    private void OpenFormation()
    {
        Button button = uiManager.transform.Find("FormationUI/OpenButton").GetComponent<Button>();
        button.onClick.Invoke();
    }

private void OpenDungeon()
    {
        uiManager.Switch(UIType.Dungeon);
    }

    private void OpenDraw()
    {
        uiManager.Switch(UIType.Draw);
    }

}
