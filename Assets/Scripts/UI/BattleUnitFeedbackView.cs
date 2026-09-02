using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleUnitFeedbackView
{
    public const float DeathFadeDuration = 0.4f;

    private readonly MonoBehaviour coroutineHost;

    private readonly HashSet<BattleUnit> deathFadingUnits =
        new HashSet<BattleUnit>();
    private readonly Dictionary<Image, Color> originalCharacterColors =
        new Dictionary<Image, Color>();
    private readonly Dictionary<Image, Vector3> originalCharacterScales =
        new Dictionary<Image, Vector3>();
    private readonly Dictionary<RectTransform, Vector2> originalCharacterPositions =
        new Dictionary<RectTransform, Vector2>();
    private readonly Dictionary<RectTransform, int> hitReactionVersions =
        new Dictionary<RectTransform, int>();

    public BattleUnitFeedbackView(MonoBehaviour coroutineHost)
    {
        if (coroutineHost == null)
        {
            throw new System.Exception("사망 연출을 실행할 MonoBehaviour가 필요합니다. ");
        }

        this.coroutineHost = coroutineHost;
    }

    public void ResetSession()
    {
        deathFadingUnits.Clear();
        hitReactionVersions.Clear();
    }

    public void RegisterChar(Image image)
    {
        if (image == null)
        {
            return;
        }

        if (!originalCharacterColors.ContainsKey(image))
        {
            originalCharacterColors.Add(image, image.color);
        }

        if (!originalCharacterScales.ContainsKey(image))
        {
            originalCharacterScales.Add(
                image,
                image.rectTransform.localScale);
        }

        if (!originalCharacterPositions.ContainsKey(image.rectTransform))
        {
            originalCharacterPositions.Add(
                image.rectTransform,
                image.rectTransform.anchoredPosition);
        }

        image.color = originalCharacterColors[image];
        image.rectTransform.localScale = originalCharacterScales[image];
        image.rectTransform.anchoredPosition =
            originalCharacterPositions[image.rectTransform];
    }

    public void StartDeathFade(BattleUnit unit, Image image)
    {
        if (unit == null || image == null || !deathFadingUnits.Add(unit))
        {
            return;
        }

        coroutineHost.StartCoroutine(FadeCharacter(image));
    }

    public IEnumerator PlayAttackFx(
        BattleUnit attacker,
        BattleUnit target,
        Image attackerImage,
        Image targetImage)
    {
        if (attackerImage == null || targetImage == null)
        {
            yield break;
        }

        Vector3 originalScale = attackerImage.rectTransform.localScale;
        Color originalColor;

        if (!originalCharacterColors.TryGetValue(
                targetImage,
                out originalColor))
        {
            originalColor = targetImage.color;
        }

        RectTransform targetTransform = targetImage.rectTransform;
        Vector2 originalTargetPosition;

        if (!originalCharacterPositions.TryGetValue(
                targetTransform,
                out originalTargetPosition))
        {
            originalTargetPosition = targetTransform.anchoredPosition;
        }

        attackerImage.rectTransform.localScale = originalScale * 1.12f;

        bool canReact = !deathFadingUnits.Contains(target);
        int reactionVersion = 0;

        if (canReact)
        {
            reactionVersion = 1;

            if (hitReactionVersions.ContainsKey(targetTransform))
            {
                reactionVersion = hitReactionVersions[targetTransform] + 1;
            }

            hitReactionVersions[targetTransform] = reactionVersion;
            targetImage.color = new Color(
                1f,
                0.45f,
                0.45f,
                originalColor.a);
        }

        const float HitReactionDistance = 8f;
        const float HitReactionDuration = 0.12f;
        float reactionDirection =
            attacker.Side == BattleUnitSide.Ally ? 1f : -1f;
        float elapsed = 0f;

        while (elapsed < HitReactionDuration)
        {
            if (targetImage == null)
            {
                yield break;
            }

            bool isCurrentReaction =
                canReact &&
                hitReactionVersions.ContainsKey(targetTransform) &&
                hitReactionVersions[targetTransform] == reactionVersion;

            if (isCurrentReaction && !deathFadingUnits.Contains(target))
            {
                float progress = Mathf.Clamp01(
                    elapsed / HitReactionDuration);
                float recoil =
                    Mathf.Sin(progress * Mathf.PI) *
                    HitReactionDistance *
                    reactionDirection;
                targetTransform.anchoredPosition =
                    originalTargetPosition +
                    new Vector2(recoil, 0f);
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        if (attackerImage != null)
        {
            attackerImage.rectTransform.localScale = originalScale;
        }

        if (targetImage != null)
        {
            bool isCurrentReaction =
                canReact &&
                hitReactionVersions.ContainsKey(targetTransform) &&
                hitReactionVersions[targetTransform] == reactionVersion;

            if (isCurrentReaction)
            {
                targetTransform.anchoredPosition = originalTargetPosition;

                if (!deathFadingUnits.Contains(target))
                {
                    targetImage.color = originalColor;
                }

                hitReactionVersions.Remove(targetTransform);
            }
        }
    }

    private IEnumerator FadeCharacter(Image image)
    {
        if (image == null)
        {
            yield break;
        }

        Color startColor = image.color;
        float elapsed = 0f;

        while (elapsed < DeathFadeDuration)
        {
            if (image == null)
            {
                yield break;
            }

            elapsed += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsed / DeathFadeDuration);
            Color color = startColor;
            color.a = Mathf.Lerp(startColor.a, 0f, progress);
            image.color = color;
            yield return null;
        }

        if (image == null)
        {
            yield break;
        }

        Color finalColor = image.color;
        finalColor.a = 0f;
        image.color = finalColor;
        image.gameObject.SetActive(false);
    }
}
