using UnityEngine;
using UnityEngine.UI;

public class PauseMenuView : MonoBehaviour
{
    [SerializeField] private UIManager uiManager;
    [SerializeField] private GameObject popup;
    [SerializeField] private GameObject pauseContent;
    [SerializeField] private Canvas popupCanvas;
    private float previousTimeScale = 1f;

    public void Initialize(UIManager manager)
    {
        uiManager = manager;
        pauseContent.transform.Find("계속하기").GetComponent<Button>().onClick.AddListener(ContinueGame);
        pauseContent.transform.Find("게임 종료").GetComponent<Button>().onClick.AddListener(QuitGame);
        pauseContent.SetActive(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (pauseContent != null && pauseContent.activeSelf) ContinueGame();
            else if (popup != null && !popup.activeSelf) Open();
        }
    }



    private void Open()
    {
        previousTimeScale = Time.timeScale;
        Time.timeScale = 0f;
        pauseContent.SetActive(true);
        popup.SetActive(true);
        popupCanvas.overrideSorting = true;
        popupCanvas.sortingOrder = 1000;
        popup.transform.SetAsLastSibling();
    }

    private void ContinueGame()
    {
        pauseContent.SetActive(false);
        popup.SetActive(false);
        Time.timeScale = previousTimeScale;
    }

    private void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void OnDestroy()
    {
        if (pauseContent != null && pauseContent.activeSelf) Time.timeScale = previousTimeScale;
    }
}
