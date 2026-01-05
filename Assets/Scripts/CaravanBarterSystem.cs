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
    [SerializeField] private TextMeshProUGUI tradeInfoText;
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
            SpawnCaravan();
        }
    }

    private void SpawnCaravan()
    {
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

        UpdateTradeDisplay();
        UpdateRejectionCounter();

        barterSlider.minValue = currentTrade.finalOffer;
        barterSlider.maxValue = currentTrade.initialOffer;
        barterSlider.value = currentTrade.finalOffer;

        currentPlayerOffer = currentTrade.finalOffer;
        UpdateSliderDisplay();
        UpdateButtonText();

        feedbackText.text = "Drag slider to make a counter-offer, or accept their current offer!";

        acceptOfferButton.interactable = true;
        rejectButton.interactable = true;
        barterSlider.interactable = true;
    }

    private void UpdateTradeDisplay()
    {
        string tradeTypeText = currentTrade.tradeType == CaravanTrade.TradeType.Buy ? "wants to BUY" : "wants to SELL";

        tradeInfoText.text = $"Caravan {tradeTypeText}\n\n" +
                            $"Offers: {currentTrade.amountOffered} {currentTrade.resourceOffered}\n" +
                            $"Wants: {currentTrade.amountRequested} {currentTrade.resourceRequested}";
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
            // Making a counter-offer
            if (requestedRes.amount < currentPlayerOffer)
            {
                feedbackText.text = $"Not enough {currentTrade.resourceRequested}! Need {currentPlayerOffer}, have {requestedRes.amount}";
                return;
            }

            if (currentPlayerOffer >= currentTrade.amountRequested)
            {
                // Offer meets or exceeds demand, accept immediately
                ExecuteTrade();
            }
            else
            {
                // Low-ball offer, trigger counter-offer
                ProcessCounterOffer();
            }
        }
        else
        {
            // Accepting caravan's current offer
            if (requestedRes.amount < currentTrade.amountRequested)
            {
                feedbackText.text = $"Not enough {currentTrade.resourceRequested}! Need {currentTrade.amountRequested}, have {requestedRes.amount}";
                return;
            }

            ExecuteTrade();
        }
    }

    private void ProcessCounterOffer()
    {
        currentTrade.rejectionCount++;

        if (currentTrade.rejectionCount >= currentTrade.maxRejections)
        {
            feedbackText.text = "Caravan is fed up and leaves!";
            UpdateRejectionCounter();
            StartCoroutine(CloseCaravanAfterDelay(2.5f));
            DisableButtons();
            return;
        }

        int range = currentTrade.amountRequested - currentPlayerOffer;
        float randomRatio = Random.Range(minCounterOfferRatio, maxCounterOfferRatio);
        int counterOffer = currentPlayerOffer + Mathf.RoundToInt(range * randomRatio);

        counterOffer = Mathf.Max(counterOffer, currentTrade.finalOffer);

        int previousDemand = currentTrade.amountRequested;
        currentTrade.amountRequested = counterOffer;

        feedbackText.text = $"Caravan counters!\nYou offered: {currentPlayerOffer}\n" +
                           $"They counter: {counterOffer} (was {previousDemand})";

        UpdateTradeDisplay();
        UpdateRejectionCounter();

        // Reset slider state after counter-offer
        hasAdjustedSlider = false;
        barterSlider.minValue = currentTrade.finalOffer;
        barterSlider.maxValue = currentTrade.amountRequested;
        barterSlider.value = currentTrade.finalOffer;
        currentPlayerOffer = currentTrade.finalOffer;
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

        feedbackText.text = $"Trade COMPLETED!\nGave: {finalTradeAmount} {currentTrade.resourceRequested}\n" +
                           $"Received: {currentTrade.amountOffered} {currentTrade.resourceOffered}\n" +
                           $"Rejections: {currentTrade.rejectionCount}/{currentTrade.maxRejections}";

        UpdateResourceUI();
        DisableButtons();
        StartCoroutine(CloseCaravanAfterDelay(3f));
    }

    private void RejectTrade()
    {
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

    private void UpdateResourceUI()
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

    public int GetCurrentRejectionCount()
    {
        return currentTrade?.rejectionCount ?? 0;
    }
}
