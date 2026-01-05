using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DialogueSet
{
    public string arrival;
    public string acceptOffer;
    public string rejectOffer;
    public string counterOffer;
    public string frustrated;
    public string leaving;
}

public static class CaravanDialogue
{
    private static List<DialogueSet> buyDialogues = new List<DialogueSet>
    {
        new DialogueSet
        {
            arrival = "Greetings! I'm looking to buy some {requested} for my {offered}.",
            acceptOffer = "That's a fair deal. Pleasure doing business!",
            rejectOffer = "Hmm, that's too low. How about this instead?",
            counterOffer = "I can meet you somewhere in the middle...",
            frustrated = "You're pushing your luck here...",
            leaving = "Forget it, I'll find another merchant."
        },
        new DialogueSet
        {
            arrival = "Hey there! I need {requested} and I've got {offered} to trade.",
            acceptOffer = "Perfect! Let's make this trade.",
            rejectOffer = "Not quite enough. Let me counter that offer.",
            counterOffer = "I think we can work something out...",
            frustrated = "This is getting ridiculous...",
            leaving = "I don't have time for this. Goodbye."
        },
        new DialogueSet
        {
            arrival = "Good day! I'm in the market for {requested}. I can offer {offered}.",
            acceptOffer = "Excellent! A deal's a deal.",
            rejectOffer = "That won't work for me. Here's my counter.",
            counterOffer = "Let's negotiate a better price...",
            frustrated = "My patience is wearing thin...",
            leaving = "I'm done here. See you never."
        },
        new DialogueSet
        {
            arrival = "Howdy! Looking to purchase {requested} with my {offered}.",
            acceptOffer = "Agreed! Thanks for being reasonable.",
            rejectOffer = "Too low, friend. Let me suggest something else.",
            counterOffer = "How about we split the difference?",
            frustrated = "You're testing me now...",
            leaving = "This isn't worth my time anymore."
        },
        new DialogueSet
        {
            arrival = "Salutations! I require {requested} and have {offered} for exchange.",
            acceptOffer = "Splendid! A mutually beneficial arrangement.",
            rejectOffer = "Insufficient. Allow me to propose an alternative.",
            counterOffer = "Perhaps we can find common ground...",
            frustrated = "My tolerance has limits...",
            leaving = "I shall take my business elsewhere."
        }
    };

    private static List<DialogueSet> sellDialogues = new List<DialogueSet>
    {
        new DialogueSet
        {
            arrival = "Hey friend! I've got {offered} to sell. Interested in trading for {requested}?",
            acceptOffer = "Done! Always a pleasure.",
            rejectOffer = "That's not enough for my goods. Counter-offer time.",
            counterOffer = "Let's find a price we both like...",
            frustrated = "Come on, don't lowball me...",
            leaving = "Forget it. I'll sell elsewhere."
        },
        new DialogueSet
        {
            arrival = "Good to see you! I'm selling {offered}. I'd like {requested} in return.",
            acceptOffer = "Great! You've got yourself a deal.",
            rejectOffer = "Nah, that's too little. How about this?",
            counterOffer = "We can negotiate something better...",
            frustrated = "You're really pushing it now...",
            leaving = "I'm out. This is a waste of time."
        },
        new DialogueSet
        {
            arrival = "Greetings! I have {offered} for sale. Looking for {requested}.",
            acceptOffer = "Perfect! Transaction complete.",
            rejectOffer = "Not acceptable. Here's my counter-proposal.",
            counterOffer = "Let's discuss a more reasonable price...",
            frustrated = "This is becoming tiresome...",
            leaving = "Enough. I'm leaving."
        },
        new DialogueSet
        {
            arrival = "Well met! Got {offered} to offload. Need {requested} from you.",
            acceptOffer = "Sold! Good doing business.",
            rejectOffer = "Too cheap. Let me adjust that for you.",
            counterOffer = "Maybe we can work out a compromise...",
            frustrated = "Stop wasting my time...",
            leaving = "I'm done negotiating. Farewell."
        },
        new DialogueSet
        {
            arrival = "Hello! I'm offering {offered} for trade. Seeking {requested}.",
            acceptOffer = "Wonderful! It's a deal.",
            rejectOffer = "That won't cover it. Here's what I need.",
            counterOffer = "Perhaps a different arrangement...",
            frustrated = "You're trying my patience...",
            leaving = "This conversation is over."
        }
    };

    public static DialogueSet GetRandomDialogue(CaravanTrade.TradeType tradeType)
    {
        if (tradeType == CaravanTrade.TradeType.Buy)
        {
            return buyDialogues[Random.Range(0, buyDialogues.Count)];
        }
        else
        {
            return sellDialogues[Random.Range(0, sellDialogues.Count)];
        }
    }

    public static string FormatDialogue(string template, string offered, string requested)
    {
        return template.Replace("{offered}", offered).Replace("{requested}", requested);
    }
}
