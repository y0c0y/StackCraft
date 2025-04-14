using System.Linq;
using UnityEngine;
using UnityEngine.Animations;

public class StackManager : MonoBehaviour
{
    void Start()
    {
        // TODO: how to find all cards?
        var cards = GameObject.FindGameObjectsWithTag("Card");
        
        foreach (var card in cards)
        {
            var cardComponent = card.GetComponent<Card>();
            if (cardComponent != null)
            {
                cardComponent.CardReleasedOn += OnCardReleasedOn;
                cardComponent.RequestSplitFromStack += OnCardSplit;
            }
        }
    }

    private void OnCardSplit(Card card)
    {
        var slowParentCon = card.GetComponent<SlowParentConstraint>();
        slowParentCon.enabled = false;
        slowParentCon.Target = null;
        
        var prevStack = card.owningStack;
        var newStack = new Stack();
        var indexInStack = card.IndexInStack;
        
        // Add the cards from splitting card to the new stack
        Card[] copyBuffer = new Card[prevStack.Length - indexInStack];
        prevStack.cards.CopyTo(indexInStack, copyBuffer, 0, prevStack.Length - indexInStack);

        Debug.Log(copyBuffer.Aggregate("", (s, c) => s += c.ToString()));
        
        for (int i = 0; i < copyBuffer.Length; i++)
        {
            newStack.AddCard(copyBuffer[i]);
        }
        
        // Remove the cards from the previous stack
        prevStack.cards.RemoveRange(indexInStack, prevStack.Length - indexInStack);
    }

    private void OnCardReleasedOn(Card draggingCard, Card releasedCard)
    {
        if (draggingCard == releasedCard) return;
        var lastCard = releasedCard.owningStack.LastCard;

        var copyStack = draggingCard.owningStack;
        for (int i = 0; i < copyStack.cards.Count; i++)
        {
            lastCard.owningStack.AddCard(copyStack.cards[i]);
        }
        
        var slowParentCon = draggingCard.gameObject.GetComponent<SlowParentConstraint>();
        slowParentCon.enabled = true;
        slowParentCon.Target = lastCard.gameObject;
    }
}
