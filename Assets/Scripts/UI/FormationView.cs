using UnityEngine;
using UnityEngine.UI;

public class FormationView : MonoBehaviour
{
    private GameObject panel;
    private UIManager uiManager;
    private UIButton openButton;
    private UIButton applyButton;
    private Text message;
    private Button[] slotButtons = new Button[9];
    private Image[] slotImages = new Image[9];
    private OwnedCharacterListView rosterList;
    private string selectedId = string.Empty;

private void Awake()
    {
        panel = transform.Find("Panel").gameObject;

        UIFrame.Build(panel.transform, new Vector2(900f, 1500f), Vector2.zero);


        uiManager = GetComponentInParent<UIManager>();

        Transform oldTitle = panel.transform.Find("Title");
        if (oldTitle != null)
        {
            oldTitle.gameObject.SetActive(false);
        }

        Transform guide = panel.transform.Find("Guide");
        if (guide != null)
        {
            guide.gameObject.SetActive(false);
        }

        UIFrame.MakeHeader(panel.transform, uiManager.UIFont, "편성", 645f);

        panel.transform.SetParent(uiManager.transform, false);
        panel.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, 60f);
        openButton = transform.Find("OpenButton").GetComponent<UIButton>();

        applyButton = panel.transform.Find("ApplyButton").GetComponent<UIButton>();
        applyButton.GetComponentInChildren<Text>().text = "적용하고 재시작";
        applyButton.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, -620f);
        RectTransform memberRoot = panel.transform.Find("Members").GetComponent<RectTransform>();
        memberRoot.anchoredPosition = new Vector2(0f, -270f);
        memberRoot.sizeDelta = new Vector2(820f, 560f);

        panel.transform.Find("Board").GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, 270f);
        message = panel.transform.Find("Message").GetComponent<Text>();

        Image panelImage = panel.GetComponent<Image>();
        if (panelImage == null)
        {
            panelImage = panel.AddComponent<Image>();
            panelImage.color = Color.clear;
        }
        panelImage.raycastTarget = true;

        UIFrame.MakeClose(panel.transform, uiManager.UIFont, new Vector2(390f, 645f), Close);

        openButton.Bind(Open);
        applyButton.Bind(Apply);

        Transform boardRoot = panel.transform.Find("Board");
        Image sourceFrame = boardRoot.Find("Slot_01").GetComponent<Image>();
        Sprite slotFrame = sourceFrame.overrideSprite != null ? sourceFrame.overrideSprite : sourceFrame.sprite;

        foreach (Transform child in boardRoot)
        {
            Destroy(child.gameObject);
        }

        GridLayoutGroup grid = boardRoot.GetComponent<GridLayoutGroup>();
        if (grid == null)
        {
            grid = boardRoot.gameObject.AddComponent<GridLayoutGroup>();
        }

        grid.cellSize = new Vector2(140f, 140f);
        grid.spacing = new Vector2(25f, 0f);
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = 3;
        grid.startCorner = GridLayoutGroup.Corner.UpperLeft;
        grid.startAxis = GridLayoutGroup.Axis.Horizontal;
        grid.childAlignment = TextAnchor.MiddleCenter;

        RectTransform boardRect = boardRoot.GetComponent<RectTransform>();
        boardRect.sizeDelta = new Vector2(470f, 420f);

        int[] layoutSlots = { 7, 4, 1, 8, 5, 2, 9, 6, 3 };
        for (int i = 0; i < layoutSlots.Length; i++)
        {
            int slot = layoutSlots[i];
            FormationSlotView slotView = FormationSlotView.Create(boardRoot, uiManager, slotFrame, slot, delegate { Move(slot); });
            slotButtons[slot - 1] = slotView.Button;
            slotImages[slot - 1] = slotView.Portrait;
        }

        panel.SetActive(false);
    }

    private void Start()
    {
        Transform memberRoot = panel.transform.Find("Members");
        foreach (Transform child in memberRoot)
        {
            Destroy(child.gameObject);
        }

        rosterList = OwnedCharacterListView.Create(memberRoot, uiManager, GameManager.Instance, true, Select);
        rosterList.transform.localScale = Vector3.one;
        Refresh();
    }

    private void Open()
    {
        selectedId = string.Empty;

        if (!uiManager.Switch(UIType.Formation))
        {
            return;
        }

        SetMsg("파티원을 선택하세요");
        Refresh();
    }

private void Select(string id)
    {
        selectedId = id;
        SetMsg(GetName(id) + " 선택됨");
        Refresh();
    }

    private void Move(int slot)
    {
        PartyFormation formation = GameManager.Instance.Formation;
        PartyMember placedMember = FindAt(slot);

        if (placedMember != null)
        {
            formation.TryRemove(placedMember.CharacterId);
            selectedId = string.Empty;
            SetMsg(GetName(placedMember.CharacterId) + " 편성 해제");
            Refresh();
            return;
        }

        if (string.IsNullOrEmpty(selectedId))
        {
            SetMsg("먼저 파티원을 선택하세요");
            return;
        }

        if (!formation.TryMove(selectedId, slot))
        {
            SetMsg("해당 슬롯에 배치할 수 없습니다");
            return;
        }

        SetMsg(GetName(selectedId) + "을(를) 슬롯 " + slot + "에 배치했습니다");
        selectedId = string.Empty;
        Refresh();
    }

    private void Apply()
    {
        foreach (PartyMember member in GameManager.Instance.Formation.Members)
        {
            if (member.FormationSlot == 0)
            {
                SetMsg("모든 파티원을 먼저 배치하세요");
                return;
            }
        }

        GameManager.Instance.SaveFormation();

        BattleManager battle = GameManager.Instance.Battle;

        if (battle != null)
        {
            battle.RestartStage();
        }

        uiManager.Switch(UIType.Battle);
    }

private void Refresh()
    {
        if (GameManager.Instance == null || GameManager.Instance.Formation == null)
        {
            return;
        }

        if (rosterList != null)
        {
            rosterList.Refresh();
        }

        for (int slot = 1; slot <= slotButtons.Length; slot++)
        {
            PartyMember member = FindAt(slot);
            Text label = slotButtons[slot - 1].GetComponentInChildren<Text>();
            Image background = slotButtons[slot - 1].GetComponent<Image>();
            Image portrait = slotImages[slot - 1];
            // 패널이 처음 활성화될 때 초기화되는 Canvas 정렬을 다시 적용합니다.
            Canvas parentCanvas = panel.GetComponentInParent<Canvas>(true);
            Canvas portraitCanvas = portrait.GetComponent<Canvas>();
            portraitCanvas.overrideSorting = true;
            portraitCanvas.sortingLayerID = parentCanvas.sortingLayerID;
            // 아래쪽 칸의 캐릭터가 위쪽 칸보다 앞에 보이도록 행별로 정렬합니다.
            portraitCanvas.sortingOrder = parentCanvas.sortingOrder + 1 + (slot - 1) % 3 * 2;
            Canvas labelCanvas = label.GetComponent<Canvas>();
            labelCanvas.overrideSorting = true;
            labelCanvas.sortingLayerID = parentCanvas.sortingLayerID;
            labelCanvas.sortingOrder = portraitCanvas.sortingOrder + 1;

            if (member == null)
            {
                background.color = new Color(0f, 0f, 0f, 0.08f);
                portrait.sprite = null;
                portrait.color = Color.clear;
                portrait.rectTransform.anchoredPosition = Vector2.zero;
                label.text = "슬롯 " + slot;
            }
            else
            {
                background.color = Color.clear;
                portrait.sprite = GetSprite(member.CharacterId);
                portrait.color = Color.white;
                Center(portrait, member.CharacterId);
                label.text = "슬롯 " + slot;
            }
        }
    }

private void Center(Image image, string characterId)
    {
        RectTransform rect = image.rectTransform;
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);

        if (image.sprite == null)
        {
            rect.anchoredPosition = Vector2.zero;
            return;
        }

        float scale = 118f / image.sprite.rect.height;
        float displayScale = 1f;

        if (image.sprite.name == "CHAR_002-trimmed")
        {
            displayScale = 1493f / 1324f;
        }

        float enlargement = characterId == "CHAR_002" || characterId == "CHAR_003" || characterId == "CHAR_009" ? 1.5f : 1.3f;
        rect.sizeDelta = image.sprite.rect.size * scale * displayScale * enlargement;
        // 확대 후에도 발끝은 칸 하단 안쪽의 같은 높이에 맞춥니다.
        rect.anchoredPosition = new Vector2(0f, (rect.sizeDelta.y - 118f) * 0.5f);
    }

private Sprite GetSprite(string id)
    {
        Sprite sprite = Resources.Load<Sprite>("UI/Formation/" + id + "-trimmed");
        return sprite != null ? sprite : uiManager.GetPortrait(id);
    }


    private PartyMember FindAt(int slot)
    {
        foreach (PartyMember member in GameManager.Instance.Formation.Members)
        {
            if (member.FormationSlot == slot)
            {
                return member;
            }
        }

        return null;
    }

    private void SetMsg(string value)
    {
        message.text = value;
    }


    private void Close()
    {
        uiManager.Switch(UIType.Battle);
    }


private string GetName(string id)
    {
        CharacterDefinition definition;
        if (GameManager.Instance != null && GameManager.Instance.Data.TryGetCharacter(id, out definition))
        {
            return definition.DisplayName;
        }

        return id;
    }
}
