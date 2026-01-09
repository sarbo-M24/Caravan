using System;
using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Events;

public class DayTimer : MonoBehaviour
{
    [Header("Timer Settings")]
    [SerializeField] private float dayDuration = 120f;
    [SerializeField] private int currentDay = 1;

    [Header("UI References")]
    [SerializeField] private Image circleTimerFill;
    [SerializeField] private TextMeshProUGUI dayCountText;

    [Header("Events")]
    public UnityEvent OnDayEnd;

    [Header("Tutorial")]
    [SerializeField] private TutorialManager tutorialManager;

    private float currentDayDuration;
    private float timeRemaining;
    private bool timerRunning = false;
    private bool isPaused = false;
    private bool hasTimeLeft = true;
    private bool waitingForCaravanToLeave = false;

    private void Start()
    {
        StartNewDay();
    }

    private void Update()
    {
        if (!timerRunning || isPaused) return;

        timeRemaining -= Time.deltaTime;

        if (timeRemaining <= 0)
        {
            timeRemaining = 0;
            hasTimeLeft = false;

            if (!waitingForCaravanToLeave)
            {
                waitingForCaravanToLeave = true;
                StartCoroutine(WaitForCaravanThenEndDay());
            }
        }

        UpdateTimerDisplay();
    }

    private IEnumerator WaitForCaravanThenEndDay()
    {
        CaravanBarterSystem caravanSystem = FindObjectOfType<CaravanBarterSystem>();

        if (caravanSystem != null)
        {
            while (caravanSystem.IsCaravanPresent())
            {
                yield return new WaitForSeconds(0.5f);
            }
        }

        EndDay();
    }

    private void StartNewDay()
    {
        if (tutorialManager != null && tutorialManager.IsTutorialMode())
        {
            currentDayDuration = tutorialManager.GetTutorialDayDuration();
            timeRemaining = currentDayDuration;
        }
        else if (tutorialManager != null)
        {
            currentDayDuration = tutorialManager.GetNormalDayDuration();
            timeRemaining = currentDayDuration;
        }
        else
        {
            currentDayDuration = dayDuration;
            timeRemaining = currentDayDuration;
        }

        timerRunning = true;
        hasTimeLeft = true;
        waitingForCaravanToLeave = false;
        isPaused = false;
        UpdateDayCounter();
        UpdateTimerDisplay();

        Debug.Log($"[DayTimer] StartNewDay - Duration: {currentDayDuration}, TimeRemaining: {timeRemaining}, Running: {timerRunning}");
    }

    public void StartRealGameAfterTutorial()
    {
        Debug.Log("[DayTimer] StartRealGameAfterTutorial called");

        if (tutorialManager != null)
        {
            currentDayDuration = tutorialManager.GetNormalDayDuration();
            timeRemaining = currentDayDuration;
        }
        else
        {
            currentDayDuration = dayDuration;
            timeRemaining = currentDayDuration;
        }

        timerRunning = true;
        hasTimeLeft = true;
        waitingForCaravanToLeave = false;
        isPaused = false;
        UpdateTimerDisplay();

        Debug.Log($"[DayTimer] After tutorial - Duration: {currentDayDuration}, TimeRemaining: {timeRemaining}, Running: {timerRunning}, Paused: {isPaused}");
    }

    private void EndDay()
    {
        timerRunning = false;
        OnDayEnd?.Invoke();
        Debug.Log("[DayTimer] Day ended");
    }

    private void UpdateTimerDisplay()
    {
        if (circleTimerFill != null)
        {
            circleTimerFill.fillAmount = timeRemaining / currentDayDuration;
        }
    }

    private void UpdateDayCounter()
    {
        if (dayCountText != null)
        {
            dayCountText.text = $"Day {currentDay}";
        }
    }

    public void AdvanceToNextDay()
    {
        currentDay++;
        StartNewDay();
    }

    public void PauseTimer()
    {
        isPaused = true;
        Debug.Log("[DayTimer] Timer paused");
    }

    public void ResumeTimer()
    {
        isPaused = false;
        Debug.Log("[DayTimer] Timer resumed");
    }

    public int GetCurrentDay()
    {
        return currentDay;
    }

    public bool HasTimeLeft()
    {
        return hasTimeLeft;
    }

    public bool IsWaitingForCaravan()
    {
        return waitingForCaravanToLeave;
    }
}
