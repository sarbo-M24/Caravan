using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class TradeUIManager : MonoBehaviour
{
    [Header("Player Resource Icons")]
    [SerializeField] private Image playerPepperIcon;
    [SerializeField] private Image playerCinnamonIcon;
    [SerializeField] private Image playerTurmericIcon;
    [SerializeField] private Image playerChilliIcon;

    [Header("Caravan Resource Icons")]
    [SerializeField] private Image caravanPepperIcon;
    [SerializeField] private Image caravanCinnamonIcon;
    [SerializeField] private Image caravanTurmericIcon;
    [SerializeField] private Image caravanChilliIcon;

    [Header("Visual Settings")]
    [SerializeField] private float normalAlpha = 1.0f;
    [SerializeField] private float fadedAlpha = 0.3f;
    [SerializeField] private float transitionSpeed = 5f;

    private Image[] playerIcons;
    private Image[] caravanIcons;

    private void Awake()
    {
        // Store icons in arrays for easy iteration
        playerIcons = new Image[] { playerPepperIcon, playerCinnamonIcon, playerTurmericIcon, playerChilliIcon };
        caravanIcons = new Image[] { caravanPepperIcon, caravanCinnamonIcon, caravanTurmericIcon, caravanChilliIcon };
    }

    public void UpdateTradeDisplay(string playerResource, string caravanResource)
    {
        // Update player icons
        SetIconState(playerPepperIcon, playerResource == "Pepper");
        SetIconState(playerCinnamonIcon, playerResource == "Cinnamon");
        SetIconState(playerTurmericIcon, playerResource == "Turmeric");
        SetIconState(playerChilliIcon, playerResource == "Chilli");

        // Update caravan icons
        SetIconState(caravanPepperIcon, caravanResource == "Pepper");
        SetIconState(caravanCinnamonIcon, caravanResource == "Cinnamon");
        SetIconState(caravanTurmericIcon, caravanResource == "Turmeric");
        SetIconState(caravanChilliIcon, caravanResource == "Chilli");
    }

    public void UpdateTradeDisplayAnimated(string playerResource, string caravanResource)
    {
        StopAllCoroutines();

        // Animate player icons
        StartCoroutine(AnimateIconState(playerPepperIcon, playerResource == "Pepper"));
        StartCoroutine(AnimateIconState(playerCinnamonIcon, playerResource == "Cinnamon"));
        StartCoroutine(AnimateIconState(playerTurmericIcon, playerResource == "Turmeric"));
        StartCoroutine(AnimateIconState(playerChilliIcon, playerResource == "Chilli"));

        // Animate caravan icons
        StartCoroutine(AnimateIconState(caravanPepperIcon, caravanResource == "Pepper"));
        StartCoroutine(AnimateIconState(caravanCinnamonIcon, caravanResource == "Cinnamon"));
        StartCoroutine(AnimateIconState(caravanTurmericIcon, caravanResource == "Turmeric"));
        StartCoroutine(AnimateIconState(caravanChilliIcon, caravanResource == "Chilli"));
    }

    private void SetIconState(Image icon, bool isActive)
    {
        if (icon == null) return;

        Color color = icon.color;
        color.a = isActive ? normalAlpha : fadedAlpha;
        icon.color = color;
    }

    private IEnumerator AnimateIconState(Image icon, bool isActive)
    {
        if (icon == null) yield break;

        float targetAlpha = isActive ? normalAlpha : fadedAlpha;
        Color color = icon.color;

        while (Mathf.Abs(color.a - targetAlpha) > 0.01f)
        {
            color.a = Mathf.Lerp(color.a, targetAlpha, Time.deltaTime * transitionSpeed);
            icon.color = color;
            yield return null;
        }

        color.a = targetAlpha;
        icon.color = color;
    }

    public void ResetAllIcons()
    {
        // Set all icons to normal alpha
        foreach (Image icon in playerIcons)
        {
            if (icon != null)
                SetIconState(icon, true);
        }

        foreach (Image icon in caravanIcons)
        {
            if (icon != null)
                SetIconState(icon, true);
        }
    }

    public void HideAllIcons()
    {
        // Fade all icons
        foreach (Image icon in playerIcons)
        {
            if (icon != null)
                SetIconState(icon, false);
        }

        foreach (Image icon in caravanIcons)
        {
            if (icon != null)
                SetIconState(icon, false);
        }
    }
}
