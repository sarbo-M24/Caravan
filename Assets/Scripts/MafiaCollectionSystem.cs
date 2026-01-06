using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MafiaCollectionSystem : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private DayTimer dayTimer;
    [SerializeField] private CaravanBarterSystem caravanSystem;
    [SerializeField] private CaravanVisuals caravanVisuals;

    [Header("Mafia Settings")]
    [SerializeField] private int baseDemandAmount = 10;
    [SerializeField] private float demandIncreasePerDay = 1.2f;

    [Header("Mafia Visuals")]
    [SerializeField] private Sprite mafiaTopSprite;
    [SerializeField] private Sprite mafiaBottomSprite;

    [Header("Caravan UI References")]
    [SerializeField] private GameObject caravanPanel;
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private TextMeshProUGUI tradeDisplayText;
    [SerializeField] private Button acceptButton;
    [SerializeField] private TextMeshProUGUI acceptButtonText;
    [SerializeField] private Button rejectButton;
    [SerializeField] private Slider barterSlider;
    [SerializeField] private GameObject sliderContainer; // Container with slider and related UI

    [Header("Game Over UI")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TextMeshProUGUI gameOverText;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button quitButton;

    [Header("Tutorial")]
    [SerializeField] private TutorialManager tutorialManager;

    private string demandedResourceName;
    private int demandedAmount;
    private List<Resource> resources;
    private bool isMafiaVisit = false;

    private void Start()
    {
        if (caravanSystem != null)
        {
            resources = caravanSystem.GetResources();
        }

        if (dayTimer != null)
        {
            dayTimer.OnDayEnd.AddListener(OnDayEnd);
        }

        if (acceptButton != null)
            acceptButton.onClick.AddListener(OnAcceptButtonPressed);

        if (restartButton != null)
            restartButton.onClick.AddListener(RestartGame);

        if (quitButton != null)
            quitButton.onClick.AddListener(QuitGame);

        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
    }

    private void OnDayEnd()
    {
        Debug.Log("Day ended! Mafia arrives...");

        if (dayTimer != null)
            dayTimer.PauseTimer();

        ShowMafiaDemand();
    }

    private void ShowMafiaDemand()
    {
        isMafiaVisit = true;
        int currentDay = dayTimer != null ? dayTimer.GetCurrentDay() : 1;

        // Calculate demand
        if (tutorialManager != null && tutorialManager.IsTutorialMode())
        {
            tutorialManager.OverrideMafiaDemand(out demandedResourceName, out demandedAmount);
            Debug.Log($"Tutorial mode: Demanding impossible {demandedAmount} {demandedResourceName}");
        }
        else
        {
            demandedAmount = Mathf.RoundToInt(baseDemandAmount * Mathf.Pow(demandIncreasePerDay, currentDay - 1));
            demandedResourceName = resources[Random.Range(0, resources.Count)].resourceName;
        }

        // Show caravan panel
        if (caravanPanel != null)
            caravanPanel.SetActive(true);

        // Show mafia sprite (no parameters - sprites are set in CaravanVisuals inspector)
        if (caravanVisuals != null)
        {
            caravanVisuals.ShowMafia();
        }

        // Set dialogue
        if (dialogueText != null)
        {
            dialogueText.text = "The mafia has arrived to collect protection money...";
        }

        // Set trade display
        if (tradeDisplayText != null)
        {
            tradeDisplayText.text = $"Pay {demandedAmount} {demandedResourceName}";
        }

        // Hide slider and reject button
        if (sliderContainer != null)
            sliderContainer.SetActive(false);

        if (rejectButton != null)
            rejectButton.gameObject.SetActive(false);

        // Configure accept button
        Resource demandedResource = resources.Find(r => r.resourceName == demandedResourceName);

        if (acceptButton != null)
        {
            if (demandedResource.amount >= demandedAmount)
            {
                acceptButtonText.text = "Pay";
                acceptButton.interactable = true;
            }
            else
            {
                acceptButtonText.text = $"Can't Pay ({demandedResource.amount}/{demandedAmount})";
                acceptButton.interactable = false;

                StartCoroutine(TriggerGameOverAfterDelay(5f));
            }
        }
    }

    private void OnAcceptButtonPressed()
    {
        if (isMafiaVisit)
        {
            AttemptPayMafia();
        }
        // If not mafia visit, CaravanBarterSystem handles it
    }

    private IEnumerator TriggerGameOverAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        GameOver();
    }

    private void AttemptPayMafia()
    {
        Resource demandedResource = resources.Find(r => r.resourceName == demandedResourceName);

        if (demandedResource.amount >= demandedAmount)
        {
            demandedResource.amount -= demandedAmount;

            if (dialogueText != null)
                dialogueText.text = "Pleasure doing business... See you tomorrow.";

            if (tradeDisplayText != null)
                tradeDisplayText.text = $"Paid {demandedAmount} {demandedResourceName}";

            if (acceptButton != null)
                acceptButton.interactable = false;

            if (caravanSystem != null)
            {
                caravanSystem.UpdateResourceUI();
            }

            StartCoroutine(ContinueToNextDay(2.5f));
        }
        else
        {
            GameOver();
        }
    }

    private IEnumerator ContinueToNextDay(float delay)
    {
        yield return new WaitForSeconds(delay);

        // Hide mafia
        if (caravanVisuals != null)
        {
            caravanVisuals.HideCaravan();
            yield return new WaitForSeconds(0.6f); // Wait for slide out
        }

        if (caravanPanel != null)
            caravanPanel.SetActive(false);

        // Re-enable slider and reject button for normal caravans
        if (sliderContainer != null)
            sliderContainer.SetActive(true);

        if (rejectButton != null)
            rejectButton.gameObject.SetActive(true);

        isMafiaVisit = false;

        if (dayTimer != null)
        {
            dayTimer.AdvanceToNextDay();
            dayTimer.ResumeTimer();
        }
    }

    private void GameOver()
    {
        if (caravanPanel != null)
            caravanPanel.SetActive(false);

        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);

        int survivedDays = dayTimer != null ? dayTimer.GetCurrentDay() : 0;
        bool isTutorial = tutorialManager != null && tutorialManager.IsTutorialMode();

        if (survivedDays == 1)
        {
            string additionalText = isTutorial ? "\n\nStarting real game..." : "";
            gameOverText.text = $"LESSON LEARNED\n\n" +
                               $"The mafia demanded: {demandedAmount} {demandedResourceName}\n" +
                               $"You only had: {resources.Find(r => r.resourceName == demandedResourceName)?.amount ?? 0}\n\n" +
                               $"Trade wisely during the day to survive the mafia's collection!" +
                               additionalText;
        }
        else
        {
            gameOverText.text = $"GAME OVER\n\nYou couldn't pay the mafia!\n\n" +
                               $"You survived {survivedDays} day{(survivedDays != 1 ? "s" : "")}.\n\n" +
                               $"They demanded: {demandedAmount} {demandedResourceName}";
        }

        if (restartButton != null)
        {
            restartButton.gameObject.SetActive(!isTutorial);
        }

        // Trigger tutorial transition
        if (isTutorial && tutorialManager != null)
        {
            tutorialManager.OnMafiaFirstVisit();
        }

        Debug.Log($"Game Over shown - Tutorial mode: {isTutorial}");
    }

    public void CloseGameOverPanel()
    {
        Debug.Log("CloseGameOverPanel called");

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }

        // Re-enable slider and reject for normal gameplay
        if (sliderContainer != null)
            sliderContainer.SetActive(true);

        if (rejectButton != null)
            rejectButton.gameObject.SetActive(true);

        isMafiaVisit = false;
    }

    private void RestartGame()
    {
        if (tutorialManager != null && !tutorialManager.IsTutorialMode())
        {
            return;
        }

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
