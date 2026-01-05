using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class CaravanEmotionSprites
{
    public Sprite angryTop;
    public Sprite angryBottom;
    public Sprite annoyedTop;
    public Sprite annoyedBottom;
    public Sprite neutralTop;
    public Sprite neutralBottom;
    public Sprite happyTop;
    public Sprite happyBottom;
}

public class CaravanVisuals : MonoBehaviour
{
    [Header("Test Man Emotions")]
    [SerializeField] private CaravanEmotionSprites manEmotions;

    [Header("Image References")]
    [SerializeField] private RectTransform topHalfTransform;
    [SerializeField] private RectTransform bottomHalfTransform;
    [SerializeField] private Image topHalfImage;
    [SerializeField] private Image bottomHalfImage;

    [Header("Slide Animation")]
    [SerializeField] private RectTransform caravanRoot;
    [SerializeField] private float slideInDuration = 0.5f;
    [SerializeField] private float slideOutDuration = 0.5f;
    [SerializeField] private float startXPosition = -1000f;
    [SerializeField] private float centerXPosition = 0f;
    [SerializeField] private float endXPosition = 1000f;
    [SerializeField] private float yPosition = 0f;
    [SerializeField] private AnimationCurve slideInCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] private AnimationCurve slideOutCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Idle Bob Animation")]
    [SerializeField] private bool enableIdleAnimation = true;
    [SerializeField] private float topBobAmount = 8f;
    [SerializeField] private float bottomBobAmount = 5f;
    [SerializeField] private float bobSpeed = 1f;
    [SerializeField] private float bobPhaseOffset = 0.5f; // Offset in radians (0.5 ≈ slight delay)

    private Vector2 slideBasePosition;
    private Vector2 topBasePosition;
    private Vector2 bottomBasePosition;
    private bool isIdling = false;
    private bool isAnimating = false;

    private void Awake()
    {
        if (caravanRoot == null)
            caravanRoot = GetComponent<RectTransform>();

        if (topHalfTransform == null && topHalfImage != null)
            topHalfTransform = topHalfImage.GetComponent<RectTransform>();

        if (bottomHalfTransform == null && bottomHalfImage != null)
            bottomHalfTransform = bottomHalfImage.GetComponent<RectTransform>();
    }

    public void ShowCaravan()
    {
        // Start with neutral emotion
        ApplyEmotionSprites(manEmotions.neutralTop, manEmotions.neutralBottom);

        // Reset position to start (off-screen left)
        caravanRoot.anchoredPosition = new Vector2(startXPosition, yPosition);
        topHalfImage.enabled = true;
        bottomHalfImage.enabled = true;

        // Store base positions for bobbing
        topBasePosition = topHalfTransform.anchoredPosition;
        bottomBasePosition = bottomHalfTransform.anchoredPosition;

        // Slide in
        StartCoroutine(SlideIn());
    }

    public void HideCaravan()
    {
        StopAllCoroutines();
        isIdling = false;
        StartCoroutine(SlideOut());
    }

    private void ApplyEmotionSprites(Sprite top, Sprite bottom)
    {
        if (topHalfImage != null)
            topHalfImage.sprite = top;
        if (bottomHalfImage != null)
            bottomHalfImage.sprite = bottom;

        Debug.Log($"Applied emotion - Top: {top?.name}, Bottom: {bottom?.name}");
    }

    public void SetEmotion(float normalizedValue)
    {
        if (manEmotions == null)
        {
            Debug.LogWarning("Man emotions not assigned!");
            return;
        }

        if (normalizedValue <= 0.33f)
        {
            ApplyEmotionSprites(manEmotions.angryTop, manEmotions.angryBottom);
            Debug.Log($"Emotion: ANGRY (value: {normalizedValue:F2})");
        }
        else if (normalizedValue <= 0.66f)
        {
            ApplyEmotionSprites(manEmotions.annoyedTop, manEmotions.annoyedBottom);
            Debug.Log($"Emotion: ANNOYED (value: {normalizedValue:F2})");
        }
        else if (normalizedValue <= 0.95f)
        {
            ApplyEmotionSprites(manEmotions.neutralTop, manEmotions.neutralBottom);
            Debug.Log($"Emotion: NEUTRAL (value: {normalizedValue:F2})");
        }
        else
        {
            ApplyEmotionSprites(manEmotions.happyTop, manEmotions.happyBottom);
            Debug.Log($"Emotion: HAPPY (value: {normalizedValue:F2})");
        }
    }

    public void SetEmotionDirect(string emotion)
    {
        if (manEmotions == null) return;

        switch (emotion.ToLower())
        {
            case "angry":
                ApplyEmotionSprites(manEmotions.angryTop, manEmotions.angryBottom);
                Debug.Log("Emotion set to: ANGRY");
                break;
            case "annoyed":
                ApplyEmotionSprites(manEmotions.annoyedTop, manEmotions.annoyedBottom);
                Debug.Log("Emotion set to: ANNOYED");
                break;
            case "neutral":
                ApplyEmotionSprites(manEmotions.neutralTop, manEmotions.neutralBottom);
                Debug.Log("Emotion set to: NEUTRAL");
                break;
            case "happy":
                ApplyEmotionSprites(manEmotions.happyTop, manEmotions.happyBottom);
                Debug.Log("Emotion set to: HAPPY");
                break;
        }
    }

    private IEnumerator SlideIn()
    {
        isAnimating = true;
        float elapsed = 0f;
        Vector2 startPos = new Vector2(startXPosition, yPosition);
        Vector2 centerPos = new Vector2(centerXPosition, yPosition);

        while (elapsed < slideInDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / slideInDuration;
            float curveValue = slideInCurve.Evaluate(t);
            caravanRoot.anchoredPosition = Vector2.Lerp(startPos, centerPos, curveValue);
            yield return null;
        }

        caravanRoot.anchoredPosition = centerPos;
        slideBasePosition = centerPos;
        isAnimating = false;

        Debug.Log("Caravan slid in to center");

        if (enableIdleAnimation)
        {
            isIdling = true;
            StartCoroutine(IdleAnimation());
        }
    }

    private IEnumerator SlideOut()
    {
        isAnimating = true;
        isIdling = false;

        // Reset to base positions before sliding out
        topHalfTransform.anchoredPosition = topBasePosition;
        bottomHalfTransform.anchoredPosition = bottomBasePosition;

        float elapsed = 0f;
        Vector2 startPos = caravanRoot.anchoredPosition;
        Vector2 endPos = new Vector2(endXPosition, yPosition);

        while (elapsed < slideOutDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / slideOutDuration;
            float curveValue = slideOutCurve.Evaluate(t);
            caravanRoot.anchoredPosition = Vector2.Lerp(startPos, endPos, curveValue);
            yield return null;
        }

        caravanRoot.anchoredPosition = endPos;
        isAnimating = false;
        topHalfImage.enabled = false;
        bottomHalfImage.enabled = false;

        Debug.Log("Caravan slid out to right");
    }

    private IEnumerator IdleAnimation()
    {
        while (isIdling && !isAnimating)
        {
            float time = Time.time * bobSpeed;

            // Top half bobs with larger amplitude
            float topBobOffset = Mathf.Sin(time) * topBobAmount;
            topHalfTransform.anchoredPosition = topBasePosition + new Vector2(0f, topBobOffset);

            // Bottom half bobs with smaller amplitude and phase offset
            float bottomBobOffset = Mathf.Sin(time + bobPhaseOffset) * bottomBobAmount;
            bottomHalfTransform.anchoredPosition = bottomBasePosition + new Vector2(0f, bottomBobOffset);

            yield return null;
        }

        // Reset to base positions when stopped
        if (!isAnimating)
        {
            topHalfTransform.anchoredPosition = topBasePosition;
            bottomHalfTransform.anchoredPosition = bottomBasePosition;
        }
    }

    public bool IsAnimating() => isAnimating;

    public int GetCurrentCaravanType() => 0;
}
