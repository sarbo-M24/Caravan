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

    [Header("Mafia Sprite (Single)")]
    [SerializeField] private Sprite mafiaSprite; // Just one sprite
    [SerializeField] private Image mafiaFullImage; // Full image for mafia

    [Header("Split Sprite Images (For Caravan)")]
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
    [SerializeField] private float bobPhaseOffset = 0.5f;

    private Vector2 slideBasePosition;
    private Vector2 topBasePosition;
    private Vector2 bottomBasePosition;
    private bool isIdling = false;
    private bool isAnimating = false;
    private bool isMafiaMode = false;

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
        isMafiaMode = false;

        // Hide mafia image, show split images
        if (mafiaFullImage != null)
            mafiaFullImage.enabled = false;

        if (topHalfImage != null)
            topHalfImage.enabled = true;

        if (bottomHalfImage != null)
            bottomHalfImage.enabled = true;

        // Start with neutral emotion
        ApplyEmotionSprites(manEmotions.neutralTop, manEmotions.neutralBottom);

        // Reset position to start (off-screen left)
        caravanRoot.anchoredPosition = new Vector2(startXPosition, yPosition);

        // Store base positions for bobbing
        topBasePosition = topHalfTransform.anchoredPosition;
        bottomBasePosition = bottomHalfTransform.anchoredPosition;

        // Slide in
        StartCoroutine(SlideIn());
    }

    public void ShowMafia()
    {
        isMafiaMode = true;

        // Hide split images, show mafia full image
        if (topHalfImage != null)
            topHalfImage.enabled = false;

        if (bottomHalfImage != null)
            bottomHalfImage.enabled = false;

        if (mafiaFullImage != null)
        {
            mafiaFullImage.enabled = true;
            mafiaFullImage.sprite = mafiaSprite;
        }

        // Reset position to start (off-screen left)
        caravanRoot.anchoredPosition = new Vector2(startXPosition, yPosition);

        // Slide in
        StartCoroutine(SlideIn());

        Debug.Log("Mafia shown with single sprite");
    }

    public void HideCaravan()
    {
        StopAllCoroutines();
        isIdling = false;
        StartCoroutine(SlideOut());
    }

    private void ApplyEmotionSprites(Sprite top, Sprite bottom)
    {
        if (top == null || bottom == null)
        {
            Debug.LogError($"Trying to apply null sprites!");
            return;
        }

        if (topHalfImage != null)
            topHalfImage.sprite = top;
        if (bottomHalfImage != null)
            bottomHalfImage.sprite = bottom;

        Debug.Log($"Applied sprites - Top: {top.name}, Bottom: {bottom.name}");
    }

    public void SetEmotion(float normalizedValue)
    {
        // Only apply emotions to caravan, not mafia
        if (isMafiaMode || manEmotions == null)
            return;

        if (normalizedValue <= 0.33f)
        {
            ApplyEmotionSprites(manEmotions.angryTop, manEmotions.angryBottom);
        }
        else if (normalizedValue <= 0.66f)
        {
            ApplyEmotionSprites(manEmotions.annoyedTop, manEmotions.annoyedBottom);
        }
        else if (normalizedValue <= 0.95f)
        {
            ApplyEmotionSprites(manEmotions.neutralTop, manEmotions.neutralBottom);
        }
        else
        {
            ApplyEmotionSprites(manEmotions.happyTop, manEmotions.happyBottom);
        }
    }

    public void SetEmotionDirect(string emotion)
    {
        // Only apply emotions to caravan, not mafia
        if (isMafiaMode || manEmotions == null)
            return;

        switch (emotion.ToLower())
        {
            case "angry":
                ApplyEmotionSprites(manEmotions.angryTop, manEmotions.angryBottom);
                break;
            case "annoyed":
                ApplyEmotionSprites(manEmotions.annoyedTop, manEmotions.annoyedBottom);
                break;
            case "neutral":
                ApplyEmotionSprites(manEmotions.neutralTop, manEmotions.neutralBottom);
                break;
            case "happy":
                ApplyEmotionSprites(manEmotions.happyTop, manEmotions.happyBottom);
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

        Debug.Log("Slid in to center");

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
        if (!isMafiaMode)
        {
            topHalfTransform.anchoredPosition = topBasePosition;
            bottomHalfTransform.anchoredPosition = bottomBasePosition;
        }

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

        // Hide all images
        if (topHalfImage != null)
            topHalfImage.enabled = false;
        if (bottomHalfImage != null)
            bottomHalfImage.enabled = false;
        if (mafiaFullImage != null)
            mafiaFullImage.enabled = false;

        Debug.Log("Slid out to right");
    }

    private IEnumerator IdleAnimation()
    {
        while (isIdling && !isAnimating)
        {
            float time = Time.time * bobSpeed;

            if (isMafiaMode)
            {
                // Simple bob for mafia full image (bob the entire root)
                float bobOffset = Mathf.Sin(time) * topBobAmount;
                caravanRoot.anchoredPosition = slideBasePosition + new Vector2(0f, bobOffset);
            }
            else
            {
                // Split bob for caravan top/bottom
                float topBobOffset = Mathf.Sin(time) * topBobAmount;
                topHalfTransform.anchoredPosition = topBasePosition + new Vector2(0f, topBobOffset);

                float bottomBobOffset = Mathf.Sin(time + bobPhaseOffset) * bottomBobAmount;
                bottomHalfTransform.anchoredPosition = bottomBasePosition + new Vector2(0f, bottomBobOffset);
            }

            yield return null;
        }

        // Reset positions
        if (!isAnimating)
        {
            if (!isMafiaMode)
            {
                topHalfTransform.anchoredPosition = topBasePosition;
                bottomHalfTransform.anchoredPosition = bottomBasePosition;
            }
            else
            {
                caravanRoot.anchoredPosition = slideBasePosition;
            }
        }
    }

    public bool IsAnimating() => isAnimating;
    public int GetCurrentCaravanType() => 0;
    public bool IsMafiaMode() => isMafiaMode;
}
