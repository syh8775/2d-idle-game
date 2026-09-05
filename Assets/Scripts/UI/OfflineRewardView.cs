using UnityEngine;
using UnityEngine.UI;

public class OfflineRewardView : MonoBehaviour
{
    private GameManager gameManager;
    [SerializeField] private GameObject popup;
    [SerializeField] private Text timeText;
    [SerializeField] private Text goldText;
    [SerializeField] private Button receiveButton;
    [SerializeField] private GameObject recordPopup;
    [SerializeField] private Text stageRecordText;
    [SerializeField] private Text killRecordText;
    [SerializeField] private Text goldRecordText;
    [SerializeField] private Text damageRecordText;
    public bool IsPopupOpen { get { return (popup != null && popup.activeInHierarchy) || (recordPopup != null && recordPopup.activeInHierarchy); } }

    public void Initialize(UIManager uiManager, GameManager owner)
    {
        gameManager = owner;
        FindInHierarchy(uiManager.transform, "OfflineRewardButton").GetComponent<Button>().onClick.AddListener(Show);
        FindInHierarchy(uiManager.transform, "ExpeditionRecordCard").GetComponent<Button>().onClick.AddListener(ShowRecord);
        receiveButton.onClick.AddListener(Receive);
        popup.transform.Find("Panel/CloseButton").GetComponent<UIButton>().Bind(Hide);
        recordPopup.transform.Find("Panel/CloseButton").GetComponent<UIButton>().Bind(HideRecord);
        gameManager.ProgressChanged += Refresh;
        Refresh();
        if (gameManager.Progress.PendingOfflineGold > 0) Show();
    }

private void ShowRecord()
    {
        if (recordPopup == null || recordPopup.activeSelf) return;
        RefreshRecord();
        recordPopup.SetActive(true);
    }

    private void HideRecord()
    {
        if (recordPopup == null || !recordPopup.activeSelf) return;
        recordPopup.SetActive(false);
    }

    private void RefreshRecord()
    {
        if (gameManager == null || gameManager.Progress == null) return;
        stageRecordText.text = GetStageLabel(gameManager.Progress.LastClearedStageId);
        killRecordText.text = gameManager.Progress.TotalEnemyKills.ToString("N0");
        goldRecordText.text = gameManager.Progress.TotalGoldEarned.ToString("N0");
        damageRecordText.text = gameManager.Progress.HighestDamage.ToString("N0");
    }

    private string GetStageLabel(string stageId)
    {
        if (string.IsNullOrEmpty(stageId)) return "-";
        int separator = stageId.LastIndexOf('_');
        if (separator < 0 || !int.TryParse(stageId.Substring(separator + 1), out int stageNumber)) return stageId;
        return "1-" + stageNumber;
    }



public void Show()
    {
        if (popup == null || popup.activeSelf)
        {
            return;
        }

        Refresh();
        popup.SetActive(true);
    }

public void Hide()
    {
        if (popup == null || !popup.activeSelf)
        {
            return;
        }

        popup.SetActive(false);
    }

    private void Receive()
    {
        if (gameManager.ClaimOffline() > 0)
        {
            Hide();
        }
        else
        {
            Refresh();
        }
    }

    private void Refresh()
    {
        if (gameManager == null || gameManager.Progress == null)
        {
            return;
        }

        int seconds = gameManager.Progress.PendingOfflineSeconds;
        int hours = seconds / 3600;
        int minutes = seconds % 3600 / 60;
        string duration = hours > 0 ? hours + "시간 " + minutes + "분" : minutes + "분";
        int pendingGold = gameManager.Progress.PendingOfflineGold;

        if (timeText != null)
        {
            timeText.text = "누적 시간 " + duration;
        }

        if (goldText != null)
        {
            goldText.text = "Gold +" + pendingGold;
        }

        if (receiveButton != null)
        {
            receiveButton.interactable = pendingGold > 0;
        }
    }

    private Transform FindInHierarchy(Transform root, string targetName)
    {
        foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
        {
            if (child.name == targetName)
            {
                return child;
            }
        }

        return null;
    }

private void OnDestroy()
    {
        if (gameManager != null)
        {
            gameManager.ProgressChanged -= Refresh;
        }
    }
}
