using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class MoodFrames
{
    public Sprite[] character1Frames;
    public Sprite[] character2Frames;
}

[System.Serializable]
public class CaravanMoods
{
    public MoodFrames angryMood;
    public MoodFrames annoyedMood;
    public MoodFrames neutralMood;
    public MoodFrames happyMood;
}

public class CaravanVisuals : MonoBehaviour
{
    [Header("Caravan Moods")]
    [SerializeField] private CaravanMoods caravanMoods;
    [SerializeField] private int currentCharacterIndex = 0;

    [Header("Mafia Sprite (Animated)")]
    [SerializeField] private Sprite[] mafiaFrames;

    [Header("Display Images")]
    [SerializeField] private Image caravanImage;
    [SerializeField] private Image mafiaFullImage;

    [Header("Frame Animation Settings")]
    [SerializeField] private float frameRate = 10f;
    [SerializeField] private bool loopAnimation = true;

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

    private Vector2 slideBasePosition;
    private bool isAnimating = false;
    private bool isMafiaMode = false;
    private bool isPlayingFrameAnimation = false;

    private Coroutine frameAnimationCoroutine;

    private void Awake()
    {
        if (caravanRoot == null)
            caravanRoot = GetComponent<RectTransform>();
    }

    public void ShowCaravan()
    {
        isMafiaMode = false;

        StopFrameAnimations();

        if (mafiaFullImage != null)
            mafiaFullImage.enabled = false;

        if (caravanImage != null)
            caravanImage.enabled = true;

        // FIX: Randomize character on each show
        currentCharacterIndex = Random.Range(0, 2);
        Debug.Log($"[ShowCaravan] Selected Character {currentCharacterIndex}");

        SetEmotionDirect("neutral");

        caravanRoot.anchoredPosition = new Vector2(startXPosition, yPosition);

        StartCoroutine(SlideIn());
    }

    public void ShowMafia()
    {
        isMafiaMode = true;

        StopFrameAnimations();

        if (caravanImage != null)
            caravanImage.enabled = false;

        if (mafiaFullImage != null)
        {
            mafiaFullImage.enabled = true;
            if (mafiaFrames != null && mafiaFrames.Length > 0)
            {
                mafiaFullImage.sprite = mafiaFrames[0];
                frameAnimationCoroutine = StartCoroutine(AnimateMafiaFrames());
            }
        }

        caravanRoot.anchoredPosition = new Vector2(startXPosition, yPosition);

        StartCoroutine(SlideIn());

        Debug.Log("Mafia shown with animated frames");
    }

    public void HideCaravan()
    {
        StopAllCoroutines();
        StopFrameAnimations();
        StartCoroutine(SlideOut());
    }

    private void StopFrameAnimations()
    {
        isPlayingFrameAnimation = false;

        if (frameAnimationCoroutine != null)
        {
            StopCoroutine(frameAnimationCoroutine);
            frameAnimationCoroutine = null;
        }
    }

    private void ApplyMoodFrames(Sprite[] frames)
    {
        if (frames == null || frames.Length == 0)
        {
            Debug.LogError($"[CaravanVisuals] NULL/EMPTY FRAMES for Character {currentCharacterIndex}! Check Inspector assignments.");
            return;
        }

        StopFrameAnimations();

        if (caravanImage != null)
        {
            caravanImage.sprite = frames[0];
            caravanImage.enabled = true;
        }

        frameAnimationCoroutine = StartCoroutine(AnimateCaravanFrames(frames));

        Debug.Log($"[CaravanVisuals] Applied {frames.Length} frames for Character {currentCharacterIndex}");
    }

    private IEnumerator AnimateCaravanFrames(Sprite[] frames)
    {
        if (frames == null || frames.Length == 0 || caravanImage == null)
            yield break;

        isPlayingFrameAnimation = true;
        float frameDuration = 1f / frameRate;
        int currentFrameIndex = 0;

        while (isPlayingFrameAnimation)
        {
            yield return new WaitForSeconds(frameDuration);

            if (!isPlayingFrameAnimation || caravanImage == null)
                yield break;

            currentFrameIndex = (currentFrameIndex + 1) % frames.Length;
            caravanImage.sprite = frames[currentFrameIndex];

            if (!loopAnimation && currentFrameIndex == 0)
            {
                isPlayingFrameAnimation = false;
                yield break;
            }
        }
    }

    private IEnumerator AnimateMafiaFrames()
    {
        if (mafiaFrames == null || mafiaFrames.Length == 0 || mafiaFullImage == null)
            yield break;

        isPlayingFrameAnimation = true;
        float frameDuration = 1f / frameRate;
        int currentFrameIndex = 0;

        while (isPlayingFrameAnimation)
        {
            yield return new WaitForSeconds(frameDuration);

            if (!isPlayingFrameAnimation || mafiaFullImage == null)
                yield break;

            currentFrameIndex = (currentFrameIndex + 1) % mafiaFrames.Length;
            mafiaFullImage.sprite = mafiaFrames[currentFrameIndex];

            if (!loopAnimation && currentFrameIndex == 0)
            {
                isPlayingFrameAnimation = false;
                yield break;
            }
        }
    }

    public void SetEmotion(float normalizedValue)
    {
        if (isMafiaMode || caravanMoods == null)
            return;

        if (normalizedValue <= 0.33f)
        {
            SetEmotionDirect("angry");
        }
        else if (normalizedValue <= 0.66f)
        {
            SetEmotionDirect("annoyed");
        }
        else if (normalizedValue <= 0.95f)
        {
            SetEmotionDirect("neutral");
        }
        else
        {
            SetEmotionDirect("happy");
        }
    }

    public void SetEmotionDirect(string emotion)
    {
        if (isMafiaMode || caravanMoods == null)
            return;

        Sprite[] framesToApply = null;

        switch (emotion.ToLower())
        {
            case "angry":
                framesToApply = (currentCharacterIndex == 0)
                    ? caravanMoods.angryMood.character1Frames
                    : caravanMoods.angryMood.character2Frames;
                Debug.Log($"[CaravanVisuals] Setting ANGRY for Character {currentCharacterIndex}");
                break;
            case "annoyed":
                framesToApply = (currentCharacterIndex == 0)
                    ? caravanMoods.annoyedMood.character1Frames
                    : caravanMoods.annoyedMood.character2Frames;
                Debug.Log($"[CaravanVisuals] Setting ANNOYED for Character {currentCharacterIndex}");
                break;
            case "neutral":
                framesToApply = (currentCharacterIndex == 0)
                    ? caravanMoods.neutralMood.character1Frames
                    : caravanMoods.neutralMood.character2Frames;
                Debug.Log($"[CaravanVisuals] Setting NEUTRAL for Character {currentCharacterIndex}");
                break;
            case "happy":
                framesToApply = (currentCharacterIndex == 0)
                    ? caravanMoods.happyMood.character1Frames
                    : caravanMoods.happyMood.character2Frames;
                Debug.Log($"[CaravanVisuals] Setting HAPPY for Character {currentCharacterIndex}");
                break;
        }

        if (framesToApply != null && framesToApply.Length > 0)
        {
            ApplyMoodFrames(framesToApply);
        }
        else
        {
            Debug.LogError($"[CaravanVisuals] MISSING FRAMES: {emotion} for Character {currentCharacterIndex}. Check Inspector!");
        }
    }

    public void SetCharacter(int characterIndex)
    {
        if (characterIndex == 0 || characterIndex == 1)
        {
            Debug.Log($"[CaravanVisuals] Switching from Character {currentCharacterIndex} to Character {characterIndex}");
            currentCharacterIndex = characterIndex;
            SetEmotionDirect("neutral");
        }
        else
        {
            Debug.LogError($"Character index {characterIndex} out of range! Only 0 or 1 allowed.");
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
    }

    private IEnumerator SlideOut()
    {
        isAnimating = true;

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

        if (caravanImage != null)
            caravanImage.enabled = false;
        if (mafiaFullImage != null)
            mafiaFullImage.enabled = false;

        Debug.Log("Slid out to right");
    }

    public bool IsAnimating() => isAnimating;
    public int GetCurrentCaravanType() => currentCharacterIndex;
    public bool IsMafiaMode() => isMafiaMode;
}
