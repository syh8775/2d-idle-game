using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleUnitFeedbackView
{
    private const float DeathFadeDuration = 0.4f;

    private readonly MonoBehaviour coroutineHost;
    private readonly Sprite[] allyAttackFrames;
    private readonly Sprite[] char002AttackFrames;
    private readonly float char001AttackFrameDuration;
    private readonly float char002AttackFrameDuration;
    private readonly int char001AttackHitFrame;
    private readonly int char002AttackHitFrame;

    private readonly HashSet<BattleUnit> deathFadingUnits =
        new HashSet<BattleUnit>();
    private readonly Dictionary<Image, Color> originalCharacterColors =
        new Dictionary<Image, Color>();
    private readonly Dictionary<Image, Vector3> originalCharacterScales =
        new Dictionary<Image, Vector3>();
    private readonly Dictionary<Image, Sprite> originalCharacterSprites =
        new Dictionary<Image, Sprite>();
    private readonly Dictionary<RectTransform, Vector2> originalCharacterPositions =
        new Dictionary<RectTransform, Vector2>();
    private readonly Dictionary<RectTransform, int> hitReactionVersions =
        new Dictionary<RectTransform, int>();

    public BattleUnitFeedbackView(
        MonoBehaviour coroutineHost,
        Sprite[] allyAttackFrames,
        Sprite[] char002AttackFrames,
        float char001AttackFrameDuration,
        float char002AttackFrameDuration,
        int char001AttackHitFrame,
        int char002AttackHitFrame)
    {
        if (coroutineHost == null)
        {
            throw new System.Exception("전투 연출을 실행할 MonoBehaviour가 필요합니다.");
        }

        this.coroutineHost = coroutineHost;
        this.allyAttackFrames = allyAttackFrames ?? new Sprite[0];
        this.char002AttackFrames = char002AttackFrames ?? new Sprite[0];
        this.char001AttackFrameDuration = char001AttackFrameDuration;
        this.char002AttackFrameDuration = char002AttackFrameDuration;
        this.char001AttackHitFrame = char001AttackHitFrame;
        this.char002AttackHitFrame = char002AttackHitFrame;
    }

    public void ResetSession()
    {
        deathFadingUnits.Clear();
        hitReactionVersions.Clear();
    }

    public void RegisterCharacter(Image image)
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

        if (!originalCharacterSprites.ContainsKey(image))
        {
            originalCharacterSprites.Add(image, image.sprite);
        }

        if (!originalCharacterPositions.ContainsKey(image.rectTransform))
        {
            originalCharacterPositions.Add(
                image.rectTransform,
                image.rectTransform.anchoredPosition);
        }

        image.color = originalCharacterColors[image];
        image.rectTransform.localScale = originalCharacterScales[image];
        image.sprite = originalCharacterSprites[image];
        image.rectTransform.anchoredPosition =
            originalCharacterPositions[image.rectTransform];
    }

    public void StartDeathFade(BattleUnit unit, Image image)
    {
        if (unit == null || image == null || !deathFadingUnits.Add(unit))
        {
            return;
        }

        coroutineHost.StartCoroutine(FadeOutCharacter(image));
    }

    public IEnumerator PlayAttackFeedback(
        BattleUnit attacker,
        BattleUnit target,
        Image attackerImage,
        Image targetImage)
    {
        if (attackerImage == null || targetImage == null)
        {
            yield break;
        }

        Sprite[] attackFrames;
        float frameDuration;
        int hitFrame;
        GetAttackAnimation(
            attacker,
            out attackFrames,
            out frameDuration,
            out hitFrame);

        bool usesAllyAttackFrames =
            attacker.Side == BattleUnitSide.Ally &&
            attackFrames != null &&
            attackFrames.Length > 0;

        if (usesAllyAttackFrames)
        {
            coroutineHost.StartCoroutine(
                PlayAttackFrames(
                    attackerImage,
                    attackFrames,
                    frameDuration));
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

        if (usesAllyAttackFrames)
        {
            int hitFrameIndex = hitFrame - 1;

            if (hitFrameIndex < 0)
            {
                hitFrameIndex = 0;
            }

            yield return new WaitForSeconds(
                frameDuration * hitFrameIndex);
        }

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

    private void GetAttackAnimation(
        BattleUnit attacker,
        out Sprite[] attackFrames,
        out float frameDuration,
        out int hitFrame)
    {
        attackFrames = allyAttackFrames;
        frameDuration = char001AttackFrameDuration;
        hitFrame = char001AttackHitFrame;

        if (attacker != null && attacker.Id == "CHAR_002")
        {
            attackFrames = char002AttackFrames;
            frameDuration = char002AttackFrameDuration;
            hitFrame = char002AttackHitFrame;
        }
    }

    private IEnumerator FadeOutCharacter(Image image)
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

    private IEnumerator PlayAttackFrames(
        Image image,
        Sprite[] attackFrames,
        float frameDuration)
    {
        if (image == null ||
            attackFrames == null ||
            attackFrames.Length == 0)
        {
            yield break;
        }

        for (int i = 0; i < attackFrames.Length; i++)
        {
            if (image == null)
            {
                yield break;
            }

            if (attackFrames[i] != null)
            {
                image.sprite = attackFrames[i];
            }

            yield return new WaitForSeconds(frameDuration);
        }

        if (image != null &&
            originalCharacterSprites.TryGetValue(
                image,
                out Sprite originalSprite))
        {
            image.sprite = originalSprite;
        }
    }
}
