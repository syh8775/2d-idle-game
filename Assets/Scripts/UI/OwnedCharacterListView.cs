using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OwnedCharacterListView : MonoBehaviour
{
    [SerializeField] private UIManager uiManager;
    private GameManager gameManager;
    private Action<string> selectAction;
    [SerializeField] private bool selectable;
    private string selectedId = string.Empty;
    [SerializeField] private List<string> characterIds = new List<string>();
    [SerializeField] private List<Button> memberButtons = new List<Button>();
    [SerializeField] private List<Image> memberImages = new List<Image>();
    [SerializeField] private List<Image> memberFrames = new List<Image>();

    public string FirstId
    {
        get { return characterIds.Count > 0 ? characterIds[0] : string.Empty; }
    }

    public void Initialize(UIManager manager, GameManager owner, bool canSelect, Action<string> onSelect)
    {
        uiManager = manager;
        gameManager = owner;
        selectable = canSelect;
        selectAction = onSelect;
        for (int i = 0; i < memberButtons.Count; i++)
        {
            string id = characterIds[i];
            memberButtons[i].interactable = selectable;
            if (selectable) memberButtons[i].onClick.AddListener(delegate { Select(id); });
        }
        selectedId = FirstId;
        gameManager.ProgressChanged += Refresh;
        Refresh();
    }

    public void SetSelected(string id)
    {
        selectedId = id;
        Refresh();
    }

    public void Refresh()
    {
        for (int i = 0; i < characterIds.Count; i++)
        {
            CharacterProgressModel progress = gameManager.Progress.GetCharacter(characterIds[i]);
            bool selected = selectable && characterIds[i] == selectedId;
            memberFrames[i].color = selected
                ? new Color(0.08f, 0.22f, 0.25f, 1f)
                : new Color(0.02f, 0.055f, 0.095f, 0.98f);

            Text[] labels = memberButtons[i].GetComponentsInChildren<Text>();
            if (labels.Length > 1 && progress != null)
            {
                labels[1].text = "Lv." + progress.Level;
            }

            Canvas parentCanvas = GetComponentInParent<Canvas>(true);
            Canvas portraitCanvas = memberImages[i].transform.parent.GetComponent<Canvas>();
            portraitCanvas.overrideSorting = true;
            portraitCanvas.sortingLayerID = parentCanvas != null ? parentCanvas.sortingLayerID : 0;
            portraitCanvas.sortingOrder = parentCanvas != null ? parentCanvas.sortingOrder + 1 : 1;
            memberImages[i].sprite = uiManager.GetPortrait(characterIds[i]);
        }
    }

    private void Select(string id)
    {
        selectedId = id;
        Refresh();

        if (selectAction != null)
        {
            selectAction(id);
        }
    }

    private void OnDestroy()
    {
        if (gameManager != null)
        {
            gameManager.ProgressChanged -= Refresh;
        }
    }
}
