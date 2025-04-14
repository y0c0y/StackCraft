using System;
using System.Linq;
using UnityEngine;

public class StackManager : MonoBehaviour
{
    public event Action CardAddedToStackByDrag;
        
    public static StackManager Instance;
    
    void Awake()
    {
        if (Instance != null)
        {
            Destroy(Instance.gameObject);
        }
        Instance = this;
        
        GameTableManager.Instance.CardAddedOnTable += OnCardAddedToTable;
        GameTableManager.Instance.CardRemovedFromTable += OnCardRemovedFromTable;
    }

    private void OnCardAddedToTable(Card card)
    {
        card.CardReleasedOn += OnCardReleasedOn;
        card.RequestSplitFromStack += OnCardSplit;
    }
    
    private void OnCardRemovedFromTable(Card card)
    {
        card.CardReleasedOn -= OnCardReleasedOn;
        card.RequestSplitFromStack -= OnCardSplit;
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

        //Debug.Log(copyBuffer.Aggregate("", (s, c) => s += c.ToString()));
        
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

        var draggingCardData = draggingCard.cardData;
        var releasedCardData = releasedCard.cardData;
        if (draggingCardData == null || releasedCardData == null)
        {
            Debug.LogError($"Card data is null. draggingCard: {draggingCard}, releasedCard: {releasedCard}");
            return;
        }

        if (!StackingRules.CanStackByType(draggingCardData.cardType, releasedCardData.cardType))
        {
            Debug.Log($"Can't stack type {draggingCardData.cardType} on {releasedCardData.cardType}");
            return;
        }
        
        var lastCard = releasedCard.owningStack.LastCard;

        var copyStack = draggingCard.owningStack;
        for (int i = 0; i < copyStack.cards.Count; i++)
        {
            lastCard.owningStack.AddCard(copyStack.cards[i]);
        }
        
        var slowParentCon = draggingCard.gameObject.GetComponent<SlowParentConstraint>();
        slowParentCon.enabled = true;
        slowParentCon.Target = lastCard.gameObject;
        
        CardAddedToStackByDrag?.Invoke();
    }
}
