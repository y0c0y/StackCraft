using System;
using UnityEngine;

public class StackManager : MonoBehaviour
{
    public event Action CardAddedToStackByDrag;
        
    public static StackManager Instance;
   [SerializeField] public GameObject stacksHolderGameObject;
    
    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(Instance.gameObject);
        }
        Instance = this;
        
        GameTableManager.Instance.CardAddedOnTable += OnCardAddedToTable;
        GameTableManager.Instance.CardRemovedFromTable += OnCardRemovedFromTable;
    }

    private void Start()
    {
        var startingStacks = stacksHolderGameObject.GetComponents<Stack>();
        foreach (var stack in startingStacks)
        {
            for (int i = 0; i < stack.cards.Count; i++)
            {
                stack.cards[i].owningStack = stack;
                if (i > 0)
                {
                    var parentConstraint = stack.cards[i].GetComponent<SlowParentConstraint>();
                    if (parentConstraint)
                    {
                        parentConstraint.target = stack.cards[i - 1].gameObject.transform;
                        parentConstraint.enabled = true;
                    }
                }
                GameTableManager.Instance.AddCardToTable(stack.cards[i]);
                if (!stack.CardCounts.TryAdd(stack.cards[i].cardData, 1))
                {
                    stack.CardCounts[stack.cards[i].cardData]++;
                }
            }
            stack.ReorderZOrder();
            GameTableManager.Instance.AddStackToTable(stack);
        }
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
        
        newStack.AddMultipleCards(copyBuffer);
        
        // Remove the cards from the previous stack
        prevStack.RemoveRange(indexInStack, prevStack.Length - indexInStack);
        
        newStack.ReorderZOrder(1);
    }

    public void AddCardToStack(Card cardToAdd, Stack stack)
    {
        var lastCard = stack.LastCard;
        var draggingCardData = cardToAdd.cardData;
        var releasedCardData = lastCard.cardData;
        
        if (draggingCardData == null || releasedCardData == null)
        {
            Debug.LogError("Error while adding card to stack");
            return;
        }

        if (!StackingRules.CanStackByType(draggingCardData.cardType, releasedCardData.cardType))
        {
            //Debug.Log($"Can't stack type {draggingCardData.cardType} on {releasedCardData.cardType}");
            return;
        }
        
        var copyStack = cardToAdd.owningStack;
        
        lastCard.owningStack.AddMultipleCards(copyStack?.cards.ToArray() ?? new []{ cardToAdd });
        copyStack?.RemoveRange(0, copyStack.cards.Count);
        
        var slowParentCon = cardToAdd.gameObject.GetComponent<SlowParentConstraint>();
        slowParentCon.enabled = true;
        slowParentCon.target = lastCard.transform;
        
        CardAddedToStackByDrag?.Invoke();
    }

    private void OnCardReleasedOn(Card draggingCard, Card releasedCard)
    {
        if (draggingCard == releasedCard) return;
        AddCardToStack(draggingCard, releasedCard.owningStack);
    }
}
