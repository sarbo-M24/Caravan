using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class CaravanBarterSystem : MonoBehaviour
{
    [Header("Resources")]
    [SerializeField] private List<Resource> resources = new List<Resource>();

    [Header("Caravan Settings")]
    [SerializeField] private float caravanArrivalInterval = 15f;
    [SerializeField] private int minTradeAmount = 5;
    [SerializeField] private int maxTradeAmount = 30;
    [SerializeField] private int minRejections = 2;
    [SerializeField] private int maxRejections = 4;

    [Header("UI References")]
    [SerializeField] private GameObject caravanPanel;
    
    [SerializeField] private TextMeshProUGUI sliderValueText;
    [SerializeField] private Slider barterSlider;
    [SerializeField] private Button acceptOfferButton;
    [SerializeField] private TextMeshProUGUI acceptOfferButtonText;
    [SerializeField] private Button rejectButton;
    [SerializeField] private TextMeshProUGUI feedbackText;
    [SerializeField] private TextMeshProUGUI rejectionCounterText;

    [Header("Resource Display")]
    [SerializeField] private TextMeshProUGUI resourceAText;
    [SerializeField] private TextMeshProUGUI resourceBText;
    [SerializeField] private TextMeshProUGUI resourceCText;
    [SerializeField] private TextMeshProUGUI resourceDText;

    [Header("Counter Offer Settings")]
    [SerializeField] private float minCounterOfferRatio = 0.3f;
    [SerializeField] private float maxCounterOfferRatio = 0.7f;

    [Header("Lowball Acceptance Settings")]
    [SerializeField] private float baseAcceptanceAtFinal = 0.8f;
    [SerializeField] private float baseAcceptanceAtDemand = 0.0f;
    [SerializeField] private AnimationCurve acceptanceCurve;

    [Header("Day Timer")]
    [SerializeField] private DayTimer dayTimer;

    [Header("Dialogue UI")]
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private TextMeshProUGUI tradeDisplayText;

    private DialogueSet currentDialogue;


    private CaravanTrade currentTrade;
    private bool caravanPresent = false;
    private int currentPlayerOffer;
    private bool hasAdjustedSlider = false;

    private void Start()
    {
        InitializeResources();
        SetupUI();
        UpdateResourceUI();
        StartCoroutine(SpawnCaravans());
    }

    private void InitializeResources()
    {
        if (resources.Count == 0)
        {
            resources.Add(new Resource { resourceName = "A", amount = 30 });
            resources.Add(new Resource { resourceName = "B", amount = 30 });
            resources.Add(new Resource { resourceName = "C", amount = 30 });
            resources.Add(new Resource { resourceName = "D", amount = 30 });
        }
    }

    private void SetupUI()
    {
        barterSlider.onValueChanged.AddListener(OnSliderChanged);
        acceptOfferButton.onClick.AddListener(OnAcceptOfferButtonClicked);
        rejectButton.onClick.AddListener(RejectTrade);

        if (caravanPanel != null)
            caravanPanel.SetActive(false);
    }

    private IEnumerator SpawnCaravans()
    {
        while (true)
        {
            yield return new WaitForSeconds(caravanArrivalInterval);

            // Only spawn if timer has time left (not during mafia collection)
            if (dayTimer != null && dayTimer.HasTimeLeft())
            {
                SpawnCaravan();
            }
        }
    }

    private void SpawnCaravan()
    {
        // Double-check time left (safety check)
        if (dayTimer != null && !dayTimer.HasTimeLeft())
        {
            Debug.Log("Day has ended, no more caravans!");
            return;
        }

        if (caravanPresent)
        {
            Debug.Log("Caravan already present");
            return;
        }

        CaravanTrade.TradeType tradeType = Random.value > 0.5f ? CaravanTrade.TradeType.Buy : CaravanTrade.TradeType.Sell;

        int index1 = Random.Range(0, 4);
        int index2 = Random.Range(0, 4);
        while (index2 == index1)
            index2 = Random.Range(0, 4);

        string resourceOffered = resources[index1].resourceName;
        string resourceRequested = resources[index2].resourceName;

        int offeredAmount = Random.Range(minTradeAmount, maxTradeAmount);
        int initialRequestAmount = Random.Range(minTradeAmount, maxTradeAmount);
        int finalOfferAmount = Mathf.Max(1, Mathf.RoundToInt(initialRequestAmount * 0.5f));
        int maxReject = Random.Range(minRejections, maxRejections + 1);

        currentTrade = new CaravanTrade(
            tradeType,
            resourceOffered,
            offeredAmount,
            resourceRequested,
            initialRequestAmount,
            finalOfferAmount,
            maxReject
        );

        DisplayCaravan();
    }

    private void DisplayCaravan()
    {
        caravanPresent = true;
        hasAdjustedSlider = false;
        caravanPanel.SetActive(true);

        // Get random dialogue for this caravan
        currentDialogue = CaravanDialogue.GetRandomDialogue(currentTrade.tradeType);

        // Display arrival dialogue
        string arrivalMessage = CaravanDialogue.FormatDialogue(
            currentDialogue.arrival,
            currentTrade.resourceOffered,
            currentTrade.resourceRequested
        );
        dialogueText.text = arrivalMessage;

        // Display trade info
        UpdateTradeDisplay();
        UpdateRejectionCounter();

        barterSlider.minValue = currentTrade.finalOffer;
        barterSlider.maxValue = currentTrade.initialOffer;
        barterSlider.value = currentTrade.amountRequested;

        currentPlayerOffer = currentTrade.amountRequested;
        UpdateSliderDisplay();
        UpdateButtonText();

        feedbackText.text = "Drag slider to make a counter-offer, or accept their current offer!";

        acceptOfferButton.interactable = true;
        rejectButton.interactable = true;
        barterSlider.interactable = true;

        Debug.Log($"Caravan displayed - Offering: {currentTrade.amountOffered} {currentTrade.resourceOffered}, Requesting: {currentTrade.amountRequested} {currentTrade.resourceRequested}");
    }



    private void UpdateTradeDisplay()
    {
        // Simple trade summary box
        tradeDisplayText.text = $"Get {currentTrade.amountOffered} {currentTrade.resourceOffered}  |  Pay {currentTrade.amountRequested} {currentTrade.resourceRequested}";
    }


    private void UpdateRejectionCounter()
    {
        int remaining = currentTrade.maxRejections - currentTrade.rejectionCount;
        rejectionCounterText.text = $"Patience: {remaining}/{currentTrade.maxRejections}";
    }

    private void OnSliderChanged(float value)
    {
        currentPlayerOffer = Mathf.RoundToInt(value);
        hasAdjustedSlider = true;
        UpdateSliderDisplay();
        UpdateButtonText();
    }

    private void UpdateSliderDisplay()
    {
        sliderValueText.text = $"Your Counter: {currentPlayerOffer} {currentTrade.resourceRequested}";

        if (currentPlayerOffer >= currentTrade.amountRequested)
        {
            sliderValueText.text += "\n(Matches their demand)";
        }
        else
        {
            int savings = currentTrade.amountRequested - currentPlayerOffer;
            sliderValueText.text += $"\n(Save {savings}, risk counter-offer)";
        }
    }

    private void UpdateButtonText()
    {
        if (hasAdjustedSlider)
        {
            acceptOfferButtonText.text = "Offer";
        }
        else
        {
            acceptOfferButtonText.text = "Accept";
        }
    }

    private void OnAcceptOfferButtonClicked()
    {
        Resource requestedRes = resources.Find(r => r.resourceName == currentTrade.resourceRequested);

        if (hasAdjustedSlider)
        {
            if (requestedRes.amount < currentPlayerOffer)
            {
                feedbackText.text = $"Not enough {currentTrade.resourceRequested}! Need {currentPlayerOffer}, have {requestedRes.amount}";
                return;
            }

            if (currentPlayerOffer >= currentTrade.amountRequested)
            {
                ExecuteTrade();
            }
            else
            {
                ProcessCounterOffer();
            }
        }
        else
        {
            if (requestedRes.amount < currentTrade.amountRequested)
            {
                feedbackText.text = $"Not enough {currentTrade.resourceRequested}! Need {currentTrade.amountRequested}, have {requestedRes.amount}";
                return;
            }

            ExecuteTrade();
        }
    }

    private float CalculateLowballAcceptanceChance()
    {
        // Calculate the range between their current demand and the best possible deal (finalOffer)
        float range = currentTrade.amountRequested - currentTrade.finalOffer;
        if (range == 0) return 0.95f; // If no room to negotiate, very high acceptance

        // How close is your offer to their current demand?
        // If you offer exactly their demand = 100% acceptance (but that's handled before this function)
        // If you offer exactly finalOffer = very low acceptance (5%)
        // Anywhere in between = scaled linearly

        float offerDistance = currentTrade.amountRequested - currentPlayerOffer;
        float normalizedPosition = offerDistance / range;

        // normalizedPosition:
        // 0.0 = you're offering exactly what they want (100% acceptance)
        // 1.0 = you're offering the absolute minimum finalOffer (5% acceptance)

        // Invert it: closer to their demand = higher chance
        float acceptChance = Mathf.Lerp(0.95f, 0.05f, normalizedPosition);

        // Optional: Apply curve for non-linear feel
        if (acceptanceCurve != null && acceptanceCurve.keys.Length > 0)
        {
            acceptChance = acceptanceCurve.Evaluate(1f - normalizedPosition);
        }

        return acceptChance;
    }


    private void ExecuteTradeWithCustomAmount(int customAmount)
    {
        Resource offeredRes = resources.Find(r => r.resourceName == currentTrade.resourceOffered);
        Resource requestedRes = resources.Find(r => r.resourceName == currentTrade.resourceRequested);

        requestedRes.amount -= customAmount;
        offeredRes.amount += currentTrade.amountOffered;

        feedbackText.text += $"\n\nTrade COMPLETED!\nGave: {customAmount} {currentTrade.resourceRequested}\n" +
                            $"Received: {currentTrade.amountOffered} {currentTrade.resourceOffered}\n" +
                            $"Rejections: {currentTrade.rejectionCount}/{currentTrade.maxRejections}";

        UpdateResourceUI();
        DisableButtons();
        StartCoroutine(CloseCaravanAfterDelay(3.5f));
    }

    private void ProcessCounterOffer()
    {
        float acceptanceChance = CalculateLowballAcceptanceChance();
        float roll = Random.value;

        if (roll <= acceptanceChance)
        {
            // Caravan accepts lowball
            dialogueText.text = currentDialogue.acceptOffer;

            feedbackText.text = $"Caravan accepts your offer!\nYou offered: {currentPlayerOffer} (they wanted {currentTrade.amountRequested})\n" +
                               $"Lucky! ({Mathf.RoundToInt(acceptanceChance * 100)}% chance)";

            ExecuteTradeWithCustomAmount(currentPlayerOffer);
            return;
        }

        currentTrade.rejectionCount++;

        if (currentTrade.rejectionCount >= currentTrade.maxRejections)
        {
            // Caravan leaves frustrated
            dialogueText.text = currentDialogue.leaving;
            feedbackText.text = "Caravan is fed up and leaves!";
            UpdateRejectionCounter();
            StartCoroutine(CloseCaravanAfterDelay(2.5f));
            DisableButtons();
            return;
        }

        // Caravan counters
        int range = currentTrade.amountRequested - currentPlayerOffer;
        float randomRatio = Random.Range(minCounterOfferRatio, maxCounterOfferRatio);
        int counterOffer = currentPlayerOffer + Mathf.RoundToInt(range * randomRatio);

        counterOffer = Mathf.Max(counterOffer, currentTrade.finalOffer);

        int previousDemand = currentTrade.amountRequested;
        currentTrade.amountRequested = counterOffer;

        // Show appropriate dialogue based on rejection count
        if (currentTrade.rejectionCount >= currentTrade.maxRejections - 1)
        {
            dialogueText.text = currentDialogue.frustrated;
        }
        else if (currentTrade.rejectionCount == 1)
        {
            dialogueText.text = currentDialogue.rejectOffer;
        }
        else
        {
            dialogueText.text = currentDialogue.counterOffer;
        }

        feedbackText.text = $"You offered: {currentPlayerOffer}\n" +
                           $"They counter: {counterOffer} (was {previousDemand})\n" +
                           $"Acceptance chance was {Mathf.RoundToInt(acceptanceChance * 100)}%";

        UpdateTradeDisplay();
        UpdateRejectionCounter();

        hasAdjustedSlider = false;
        barterSlider.minValue = currentTrade.finalOffer;
        barterSlider.maxValue = currentTrade.amountRequested;
        barterSlider.value = currentTrade.amountRequested;
        currentPlayerOffer = currentTrade.amountRequested;
        UpdateSliderDisplay();
        UpdateButtonText();
    }


    private void ExecuteTrade()
    {
        Resource offeredRes = resources.Find(r => r.resourceName == currentTrade.resourceOffered);
        Resource requestedRes = resources.Find(r => r.resourceName == currentTrade.resourceRequested);

        int finalTradeAmount = hasAdjustedSlider ? currentPlayerOffer : currentTrade.amountRequested;

        requestedRes.amount -= finalTradeAmount;
        offeredRes.amount += currentTrade.amountOffered;

        dialogueText.text = currentDialogue.acceptOffer;

        feedbackText.text = $"Trade COMPLETED!\nGave: {finalTradeAmount} {currentTrade.resourceRequested}\n" +
                           $"Received: {currentTrade.amountOffered} {currentTrade.resourceOffered}\n" +
                           $"Rejections: {currentTrade.rejectionCount}/{currentTrade.maxRejections}";

        UpdateResourceUI();
        DisableButtons();
        StartCoroutine(CloseCaravanAfterDelay(3f));
    }


    private void RejectTrade()
    {
        dialogueText.text = currentDialogue.leaving;
        feedbackText.text = "You rejected the trade. Caravan leaves.";
        DisableButtons();
        StartCoroutine(CloseCaravanAfterDelay(2f));
    }


    private void DisableButtons()
    {
        acceptOfferButton.interactable = false;
        rejectButton.interactable = false;
        barterSlider.interactable = false;
    }

    private IEnumerator CloseCaravanAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        caravanPanel.SetActive(false);
        caravanPresent = false;
        acceptOfferButton.interactable = true;
        rejectButton.interactable = true;
        barterSlider.interactable = true;
    }

    public bool IsCaravanPresent()
    {
        return caravanPresent;
    }

    public void UpdateResourceUI()
    {
        resourceAText.text = $"A: {resources[0].amount}";
        resourceBText.text = $"B: {resources[1].amount}";
        resourceCText.text = $"C: {resources[2].amount}";
        resourceDText.text = $"D: {resources[3].amount}";
    }

    public void ForceSpawnCaravan()
    {
        SpawnCaravan();
    }

    public List<Resource> GetResources()
    {
        return resources;
    }

    public int GetCurrentRejectionCount()
    {
        return currentTrade?.rejectionCount ?? 0;
    }
}
