using UnityEngine;
using UnityEngine.UI;

public sealed class SkillCooldownView : MonoBehaviour
{
    [SerializeField] private Image cooldownOverlay;

    private float cooldownDuration;
    private float remainingTime;

    public bool IsCoolingDown => remainingTime > 0f;

    private void Awake()
    {
        if (cooldownOverlay == null)
        {
            cooldownOverlay = transform.Find("PortraitMask/CooldownOverlay")?.GetComponent<Image>();
        }

        Transform gauge = transform.Find("Gauge");
        if (gauge != null)
        {
            gauge.gameObject.SetActive(false);
        }

        CompleteCooldown();
    }

    private void Update()
    {
        remainingTime = Mathf.Max(0f, remainingTime - Time.deltaTime);
        ApplyProgress();

        if (remainingTime <= 0f)
        {
            enabled = false;
        }
    }

    public void StartCooldown(float seconds)
    {
        cooldownDuration = Mathf.Max(0.01f, seconds);
        remainingTime = cooldownDuration;
        enabled = true;
        ApplyProgress();
    }

    public void CompleteCooldown()
    {
        remainingTime = 0f;
        ApplyProgress();
        enabled = false;
    }

    private void ApplyProgress()
    {
        if (cooldownOverlay == null)
        {
            return;
        }

        cooldownOverlay.fillAmount = cooldownDuration > 0f ? remainingTime / cooldownDuration : 0f;
        cooldownOverlay.enabled = remainingTime > 0f;
    }
}