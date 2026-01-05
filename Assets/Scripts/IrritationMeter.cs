using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class IrritationMeter : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image fillImage;
    [SerializeField] private RectTransform meterTransform;

    [Header("Fill Settings")]
    [SerializeField] private Color lowIrritationColor = Color.green;
    [SerializeField] private Color mediumIrritationColor = Color.yellow;
    [SerializeField] private Color highIrritationColor = Color.red;
    [SerializeField] private float fillSpeed = 3f;

    [Header("Vibration (2 chances left)")]
    [SerializeField] private float vibrateAmount = 2f;
    [SerializeField] private float vibrateSpeed = 15f;

    [Header("Pulse (1 chance left)")]
    [SerializeField] private float pulseScaleMin = 1.0f;
    [SerializeField] private float pulseScaleMax = 1.15f;
    [SerializeField] private float pulseSpeed = 5f;

    private float currentFill = 0f;
    private float targetFill = 0f;
    private Vector2 basePosition;
    private Vector3 baseScale;
    private bool isAnimating = false;

    private int currentRejections = 0;
    private int maxRejections = 3;
    private int chancesLeft = 3;

    private void Awake()
    {
        if (fillImage == null)
            fillImage = GetComponentInChildren<Image>();

        if (meterTransform == null)
            meterTransform = GetComponent<RectTransform>();

        basePosition = meterTransform.anchoredPosition;
        baseScale = meterTransform.localScale;
    }

    private void Start()
    {
        fillImage.fillAmount = 0f;
        currentFill = 0f;
        targetFill = 0f;
    }

    public void ShowMeter()
    {
        gameObject.SetActive(true);
        ResetMeter();

        if (!isAnimating)
        {
            isAnimating = true;
            StartCoroutine(AnimateMeter());
        }
    }

    public void HideMeter()
    {
        isAnimating = false;
        StopAllCoroutines();
        gameObject.SetActive(false);
    }

    public void SetIrritation(int rejections, int max)
    {
        currentRejections = rejections;
        maxRejections = max;
        chancesLeft = max - rejections;

        targetFill = (float)rejections / max;
        targetFill = Mathf.Clamp01(targetFill);

        Debug.Log($"Irritation: {rejections}/{max}, Chances left: {chancesLeft}");
    }

    public void SetIrritationDirect(float normalizedValue)
    {
        targetFill = Mathf.Clamp01(normalizedValue);
    }

    public void ResetMeter()
    {
        currentFill = 0f;
        targetFill = 0f;
        currentRejections = 0;
        chancesLeft = maxRejections;
        fillImage.fillAmount = 0f;
        UpdateColor();
    }

    private void Update()
    {
        if (!isAnimating) return;

        // Smoothly fill to target
        if (Mathf.Abs(currentFill - targetFill) > 0.01f)
        {
            currentFill = Mathf.Lerp(currentFill, targetFill, Time.deltaTime * fillSpeed);
            fillImage.fillAmount = currentFill;
            UpdateColor();
        }
        else
        {
            currentFill = targetFill;
            fillImage.fillAmount = currentFill;
        }
    }

    private IEnumerator AnimateMeter()
    {
        while (isAnimating)
        {
            float time = Time.time;
            Vector2 finalPosition = basePosition;
            Vector3 finalScale = baseScale;

            // Check conditions
            bool shouldVibrate = chancesLeft == 2;
            bool shouldPulse = chancesLeft == 1;

            // Vibration (2 chances left)
            if (shouldVibrate || shouldPulse)
            {
                float vibrateX = Mathf.Sin(time * vibrateSpeed) * vibrateAmount;
                float vibrateY = Mathf.Cos(time * vibrateSpeed * 1.3f) * vibrateAmount * 0.5f;
                finalPosition += new Vector2(vibrateX, vibrateY);
            }

            // Pulsation (1 chance left)
            if (shouldPulse)
            {
                float pulse = Mathf.Lerp(pulseScaleMin, pulseScaleMax,
                    (Mathf.Sin(time * pulseSpeed) + 1f) * 0.5f);
                finalScale = baseScale * pulse;
            }

            meterTransform.anchoredPosition = finalPosition;
            meterTransform.localScale = finalScale;

            yield return null;
        }

        // Reset to base when stopped
        meterTransform.anchoredPosition = basePosition;
        meterTransform.localScale = baseScale;
    }

    private void UpdateColor()
    {
        if (currentFill <= 0.33f)
        {
            fillImage.color = Color.Lerp(lowIrritationColor, mediumIrritationColor, currentFill * 3f);
        }
        else if (currentFill <= 0.66f)
        {
            float t = (currentFill - 0.33f) * 3f;
            fillImage.color = Color.Lerp(mediumIrritationColor, highIrritationColor, t);
        }
        else
        {
            float t = (currentFill - 0.66f) * 3f;
            fillImage.color = Color.Lerp(highIrritationColor, Color.red * 1.2f, t);
        }
    }

    public float GetCurrentIrritation()
    {
        return currentFill;
    }

    public int GetChancesLeft()
    {
        return chancesLeft;
    }
}
