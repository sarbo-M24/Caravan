using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class TutorialManager : MonoBehaviour
{
    [Header("Tutorial Settings")]
    [SerializeField] private bool forceTutorialMode = false;

    [Header("References")]
    [SerializeField] private DayTimer dayTimer;
    [SerializeField] private MafiaCollectionSystem mafiaSystem;
    [SerializeField] private GameObject timerUI;
    [SerializeField] private GameObject gameOverPanel; // Direct reference

    [Header("Tutorial Timing")]
    [SerializeField] private float tutorialDayDuration = 5f;
    [SerializeField] private float normalDayDuration = 120f;
    [SerializeField] private float gameOverDisplayTime = 3f; // How long to show game over

    [Header("Forced Failure")]
    [SerializeField] private string forcedDemandResource = "Pepper";
    [SerializeField] private int impossibleDemandAmount = 200;

    private static bool tutorialCompleted = false;
    private bool isTutorialMode = false;
    private bool transitioningToRealGame = false;

    private void Awake()
    {
        if (forceTutorialMode)
        {
            isTutorialMode = true;
            tutorialCompleted = false;
        }
        else
        {
            isTutorialMode = !tutorialCompleted;
        }

        Debug.Log($"Tutorial mode: {isTutorialMode}, Completed: {tutorialCompleted}");
    }

    private void Start()
    {
        if (isTutorialMode)
        {
            StartTutorial();
        }
        else
        {
            if (timerUI != null)
            {
                timerUI.SetActive(true);
            }
        }
    }

    private void StartTutorial()
    {
        if (timerUI != null)
        {
            timerUI.SetActive(false);
        }

        Debug.Log("Tutorial started - timer hidden, 5 second day");
    }

    public void OnMafiaFirstVisit()
    {
        Debug.Log($"OnMafiaFirstVisit called - isTutorialMode: {isTutorialMode}, transitioningToRealGame: {transitioningToRealGame}");

        if (isTutorialMode && !transitioningToRealGame)
        {
            isTutorialMode = false;
            tutorialCompleted = true;
            transitioningToRealGame = true;

            Debug.Log("Tutorial complete - starting transition to real game");

            StartCoroutine(ContinueToRealGame());
        }
        else
        {
            Debug.Log("OnMafiaFirstVisit called but conditions not met or already transitioning");
        }
    }


    private IEnumerator ContinueToRealGame()
    {
        Debug.Log($"Waiting {gameOverDisplayTime} seconds before closing game over...");

        // Wait for game over screen to show
        yield return new WaitForSeconds(gameOverDisplayTime);

        Debug.Log("Closing game over panel...");

        // Close game over panel
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
            Debug.Log("Game over panel closed");
        }
        else
        {
            Debug.LogError("Game Over Panel reference is null!");
        }

        // Also try via mafia system
        if (mafiaSystem != null)
        {
            mafiaSystem.CloseGameOverPanel();
            Debug.Log("Called CloseGameOverPanel on mafia system");
        }

        // Show timer UI
        if (timerUI != null)
        {
            timerUI.SetActive(true);
            Debug.Log("Timer UI shown");
        }

        // Start a new day with normal duration
        if (dayTimer != null)
        {
            dayTimer.StartRealGameAfterTutorial();
            Debug.Log("Real game day started");
        }
        else
        {
            Debug.LogError("DayTimer reference is null!");
        }

        transitioningToRealGame = false;
        Debug.Log("Transition to real game complete");
    }

    public void OverrideMafiaDemand(out string resource, out int amount)
    {
        resource = forcedDemandResource;
        amount = impossibleDemandAmount;
    }

    public bool IsTutorialMode()
    {
        return isTutorialMode;
    }

    public float GetTutorialDayDuration()
    {
        return tutorialDayDuration;
    }

    public float GetNormalDayDuration()
    {
        return normalDayDuration;
    }

    [ContextMenu("Reset Tutorial")]
    public void ResetTutorial()
    {
        tutorialCompleted = false;
        transitioningToRealGame = false;
        Debug.Log("Tutorial reset - will play on next scene reload");
    }
}
