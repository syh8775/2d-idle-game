using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button), typeof(Image))]
public sealed class AutoToggleButton : MonoBehaviour
{
    [SerializeField] private bool isAutoEnabled;
    [SerializeField] private Color enabledColor = new Color(0.2f, 0.82f, 0.96f, 1f);
    [SerializeField] private Color disabledColor = new Color(0.16f, 0.34f, 0.38f, 1f);

    private Button button;
    private Image buttonImage;
    private Text label;

    public bool IsAutoEnabled => isAutoEnabled;

    private void Awake()
    {
        button = GetComponent<Button>();
        buttonImage = GetComponent<Image>();
        label = GetComponentInChildren<Text>();

        button.onClick.AddListener(ToggleAuto);
        ApplyState();
    }

    private void OnDestroy()
    {
        if (button != null)
        {
            button.onClick.RemoveListener(ToggleAuto);
        }
    }

    public void ToggleAuto()
    {
        isAutoEnabled = !isAutoEnabled;
        ApplyState();
    }

    private void ApplyState()
    {
        buttonImage.color = isAutoEnabled ? enabledColor : disabledColor;
        label.text = "AUTO";
        label.color = isAutoEnabled ? Color.white : new Color(0.55f, 0.65f, 0.67f, 1f);
    }
}