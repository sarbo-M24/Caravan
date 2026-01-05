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
        timeRemaining = dayDuration;
        timerRunning = true;
        hasTimeLeft = true;
        waitingForCaravanToLeave = false;
        UpdateDayCounter();
        UpdateTimerDisplay();
    }

    private void EndDay()
    {
        timerRunning = false;
        OnDayEnd?.Invoke();
    }

    private void UpdateTimerDisplay()
    {
        if (circleTimerFill != null)
        {
            circleTimerFill.fillAmount = 1f - (timeRemaining / dayDuration);
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
    }

    public void ResumeTimer()
    {
        isPaused = false;
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
