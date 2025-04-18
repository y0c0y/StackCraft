using System;
using UnityEngine;

public class StackManager : MonoBehaviour
{
    public event Action CardAddedToStackByDrag;
        
    public static StackManager Instance;
   [SerializeField] public GameObject stacksHolderGameObject;
    
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

    public Stack AddNewStack()
    {
        return stacksHolderGameObject.AddComponent<Stack>();
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
        slowParentCon.target = null;
        
        var prevStack = card.owningStack;
        var newStack = AddNewStack();
        var indexInStack = card.IndexInStack;
        
        // Add the cards from splitting card to the new stack
        Card[] copyBuffer = new Card[prevStack.Length - indexInStack];
        prevStack.cards.CopyTo(indexInStack, copyBuffer, 0, prevStack.Length - indexInStack);

        //Debug.Log(copyBuffer.Aggregate("", (s, c) => s += c.ToString()));
        
        /*
        for (int i = 0; i < copyBuffer.Length; i++)
        {
            newStack.AddCard(copyBuffer[i]);
        }
        */
        
        newStack.AddMultipleCards(copyBuffer);
        
        // Remove the cards from the previous stack
        prevStack.RemoveRange(indexInStack, prevStack.Length - indexInStack);
    }

    private void OnCardReleasedOn(Card draggingCard, Card releasedCard)
    {
        if (draggingCard == releasedCard) return;
        
        var lastCard = releasedCard.owningStack.LastCard;
        
        var draggingCardData = draggingCard.cardData;
        var releasedCardData = lastCard.cardData;
        if (draggingCardData == null || releasedCardData == null)
        {
            Debug.LogError($"Card data is null. draggingCard: {draggingCard}, releasedCard: {releasedCard}");
            return;
        }

        if (!StackingRules.CanStackByType(draggingCardData.cardType, releasedCardData.cardType))
        {
            //Debug.Log($"Can't stack type {draggingCardData.cardType} on {releasedCardData.cardType}");
            return;
        }

        var copyStack = draggingCard.owningStack;
        
        /*
        for (int i = 0; i < copyStack.cards.Count; i++)
        {
            lastCard.owningStack.AddCard(copyStack.cards[i]);
        }
        */
        lastCard.owningStack.AddMultipleCards(copyStack.cards.ToArray());
        
        copyStack.RemoveRange(0, copyStack.cards.Count);
        
        var slowParentCon = draggingCard.gameObject.GetComponent<SlowParentConstraint>();
        slowParentCon.enabled = true;
        slowParentCon.target = lastCard.transform;
        
        CardAddedToStackByDrag?.Invoke();
    }
}
