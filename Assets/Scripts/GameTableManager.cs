using System;
using System.Collections.Generic;
using UnityEngine;

public class GameTableManager : MonoBehaviour
{
    public static GameTableManager Instance;
    public event Action<Card> CardAddedOnTable;
    public event Action<Card> CardRemovedFromTable;

    [SerializeField] public List<Card> CardsOnTable;
    [SerializeField] public List<Stack> StacksOnTable;
    
    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(Instance.gameObject);
        }
        Instance = this;
    }

    private void Start()
    {
        StackManager.Instance.CardAddedToStackByDrag += HideCardCanStackIndicator;
    }

    public void AddCardToTable(Card card)
    {
        if (!CardsOnTable.Contains(card))
        {
            CardsOnTable.Add(card);
            var cardDrag = card.GetComponent<CardDrag>();
            if (cardDrag != null)
            {
                cardDrag.CardDragStarted += OnCardDragStarted;
                cardDrag.CardDragEnded += OnCardDragEnded;
            }
            else
            {
                Debug.LogError($"CardDrag component not found on {card.name}");
            }
                
            CardAddedOnTable?.Invoke(card);
        }
    }

    
    public void RemoveCardFromTable(Card card)
    {
        if (CardsOnTable.Contains(card))
        {
            CardsOnTable.Remove(card);
            var cardDrag = card.GetComponent<CardDrag>();
            if (cardDrag != null)
            {
                cardDrag.CardDragStarted -= OnCardDragStarted;
                cardDrag.CardDragEnded -= OnCardDragEnded;
            }
            
            CardRemovedFromTable?.Invoke(card);
        }
    }
    
    public void AddStackToTable(Stack stack)
    {
        if (!StacksOnTable.Contains(stack))
        {
            StacksOnTable.Add(stack);
        }
    }
    
    public void RemoveStackFromTable(Stack stack)
    {
        if (StacksOnTable.Contains(stack))
        {
            StacksOnTable.Remove(stack);
        }
    }
    
    private void OnCardDragStarted(Card card)
    {
        Debug.Log($"Card {card.name} started dragging");
        ShowCardCanStackIndicator(card);
    }

    private void OnCardDragEnded(Card obj)
    {
        Debug.Log($"Card {obj.name} ended dragging");
        HideCardCanStackIndicator();
    }
    
    private void ShowCardCanStackIndicator(Card card)
    {
        foreach (var c in CardsOnTable)
        {
            if (c == card) continue;
            if (!c.IsLastCard) continue;
            if (c.owningStack == card.owningStack) continue;
            
            var draggingCardData = card.cardData;
            var releasedCardData = c.cardData;

            if (StackingRules.CanStackByType(draggingCardData.cardType, releasedCardData.cardType))
            {
                c.ShowCanStackOnIndicator();
            }
        }
    }
    
    private void HideCardCanStackIndicator()
    {
        foreach (var c in CardsOnTable)
        { 
            c.HideCanStackOnIndicator();
        }
    }
}
