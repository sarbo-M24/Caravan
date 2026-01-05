using System;
using UnityEngine;

[System.Serializable]
public class CaravanTrade
{
    public enum TradeType { Buy, Sell }

    public TradeType tradeType;
    public string resourceOffered;
    public int amountOffered;
    public string resourceRequested;
    public int amountRequested;         // Current offer from caravan
    public int initialOffer;            // Starting unfavorable offer
    public int finalOffer;              // Best possible offer (won't go below this)
    public int rejectionCount;          // How many times player offer was rejected
    public int maxRejections;           // Max rejections before caravan leaves

    public CaravanTrade(TradeType type, string offered, int offeredAmt, string requested, int requestedAmt, int final, int maxReject)
    {
        tradeType = type;
        resourceOffered = offered;
        amountOffered = offeredAmt;
        resourceRequested = requested;
        amountRequested = requestedAmt;
        initialOffer = requestedAmt;
        finalOffer = final;
        rejectionCount = 0;
        maxRejections = maxReject;
    }
}
