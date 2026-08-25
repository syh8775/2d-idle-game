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
    private Button[] memberButtons = new Button[4];
    private string selectedId = string.Empty;

    private void Awake()
    {
        panel = transform.Find("Panel").gameObject;
        uiManager = GetComponentInParent<UIManager>();
        openButton = transform.Find("OpenButton").GetComponent<UIButton>();
        applyButton = transform.Find("Panel/ApplyButton").GetComponent<UIButton>();
        message = transform.Find("Panel/Message").GetComponent<Text>();

        openButton.Bind(Open);
        applyButton.Bind(Apply);

        for (int i = 0; i < slotButtons.Length; i++)
        {
            int slot = i + 1;
            slotButtons[i] = transform.Find("Panel/Board/Slot_" + slot.ToString("00")).GetComponent<Button>();
            slotButtons[i].onClick.AddListener(() => Move(slot));
        }

        for (int i = 0; i < memberButtons.Length; i++)
        {
            int index = i;
            memberButtons[i] = transform.Find("Panel/Members/Member_" + (i + 1).ToString("00")).GetComponent<Button>();
            memberButtons[i].onClick.AddListener(() => Pick(index));
        }

        panel.SetActive(false);
    }

    private void Start()
    {
        Refresh();
    }

    private void Open()
    {
        selectedId = string.Empty;

        if (!uiManager.Switch(UIType.Formation))
        {
            return;
        }

        SetMsg("Select a party member");
        Refresh();
    }

    private void Pick(int index)
    {
        PartyFormation formation = GameManager.Instance.Formation;

        if (index < 0 || index >= formation.Members.Count)
        {
            return;
        }

        selectedId = formation.Members[index].CharacterId;
        SetMsg(selectedId + " selected");
        Refresh();
    }

    private void Move(int slot)
    {
        if (string.IsNullOrEmpty(selectedId))
        {
            SetMsg("Select a party member first");
            return;
        }

        if (!GameManager.Instance.Formation.TryMove(selectedId, slot))
        {
            SetMsg("That slot is already occupied");
            return;
        }

        SetMsg(selectedId + " moved to slot " + slot);
        Refresh();
    }

    private void Apply()
    {
        applyButton.Lock(true);
        BattleManager battle = GameManager.Instance.Battle;

        if (battle != null)
        {
            battle.RestartCurrentStage();
        }

        uiManager.Switch(UIType.Battle);
        applyButton.Lock(false);
    }

    private void Refresh()
    {
        if (GameManager.Instance == null || GameManager.Instance.Formation == null)
        {
            return;
        }

        PartyFormation formation = GameManager.Instance.Formation;

        for (int i = 0; i < memberButtons.Length; i++)
        {
            Text label = memberButtons[i].GetComponentInChildren<Text>();
            Image image = memberButtons[i].GetComponent<Image>();

            if (i >= formation.Members.Count)
            {
                memberButtons[i].gameObject.SetActive(false);
                continue;
            }

            PartyMember member = formation.Members[i];
            memberButtons[i].gameObject.SetActive(true);
            label.text = member.CharacterId + "\nSlot " + member.FormationSlot;
            image.color = member.CharacterId == selectedId
                ? new Color(0.72f, 0.5f, 0.16f, 1f)
                : new Color(0.12f, 0.22f, 0.34f, 1f);
        }

        for (int slot = 1; slot <= slotButtons.Length; slot++)
        {
            PartyMember member = FindAt(slot);
            Text label = slotButtons[slot - 1].GetComponentInChildren<Text>();
            Image image = slotButtons[slot - 1].GetComponent<Image>();

            label.text = member == null
                ? slot.ToString()
                : slot + "\n" + member.CharacterId;

            image.color = member == null
                ? new Color(0.08f, 0.12f, 0.18f, 0.88f)
                : new Color(0.12f, 0.3f, 0.42f, 1f);
        }
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
}
