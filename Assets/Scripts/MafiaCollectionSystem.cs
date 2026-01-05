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

    [Header("Mafia Settings")]
    [SerializeField] private int baseDemandAmount = 10;
    [SerializeField] private float demandIncreasePerDay = 1.2f;

    [Header("UI References")]
    [SerializeField] private GameObject mafiaPanel;
    [SerializeField] private TextMeshProUGUI mafiaDialogueText;
    [SerializeField] private TextMeshProUGUI demandText;
    [SerializeField] private Button payButton;
    [SerializeField] private TextMeshProUGUI payButtonText;

    [Header("Game Over UI")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TextMeshProUGUI gameOverText;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button quitButton;

    private string demandedResourceName;
    private int demandedAmount;
    private List<Resource> resources;

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

        if (payButton != null)
            payButton.onClick.AddListener(AttemptPayMafia);

        if (restartButton != null)
            restartButton.onClick.AddListener(RestartGame);

        if (quitButton != null)
            quitButton.onClick.AddListener(QuitGame);

        if (mafiaPanel != null)
            mafiaPanel.SetActive(false);

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
        int currentDay = dayTimer != null ? dayTimer.GetCurrentDay() : 1;
        demandedAmount = Mathf.RoundToInt(baseDemandAmount * Mathf.Pow(demandIncreasePerDay, currentDay - 1));

        demandedResourceName = resources[Random.Range(0, resources.Count)].resourceName;

        mafiaPanel.SetActive(true);

        mafiaDialogueText.text = "The mafia has arrived to collect...";
        demandText.text = $"We want {demandedAmount} of resource {demandedResourceName}.\n\nPay up, or else...";

        Resource demandedResource = resources.Find(r => r.resourceName == demandedResourceName);

        if (demandedResource.amount >= demandedAmount)
        {
            payButtonText.text = "Pay";
            payButton.interactable = true;
        }
        else
        {
            payButtonText.text = $"Can't Pay ({demandedResource.amount}/{demandedAmount})";
            payButton.interactable = false;

            StartCoroutine(TriggerGameOverAfterDelay(2f));
        }
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

            mafiaDialogueText.text = "Pleasure doing business...";
            demandText.text = $"You paid {demandedAmount} {demandedResourceName}.\n\nSee you tomorrow.";

            payButton.interactable = false;

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

        mafiaPanel.SetActive(false);

        // REMOVED: caravanPanel.SetActive(true) - let caravans control their own panel

        if (dayTimer != null)
        {
            dayTimer.AdvanceToNextDay();
            dayTimer.ResumeTimer();
        }
    }

    private void GameOver()
    {
        mafiaPanel.SetActive(false);
        gameOverPanel.SetActive(true);

        int survivedDays = dayTimer != null ? dayTimer.GetCurrentDay() : 0;

        gameOverText.text = $"GAME OVER\n\nYou couldn't pay the mafia!\n\n" +
                           $"You survived {survivedDays} day{(survivedDays != 1 ? "s" : "")}.\n\n" +
                           $"They demanded: {demandedAmount} {demandedResourceName}";

        Debug.Log("Player beaten by mafia - Game Over!");
    }

    private void RestartGame()
    {
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
