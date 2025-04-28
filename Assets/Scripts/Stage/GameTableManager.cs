using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameTableManager : MonoBehaviour
{
    public enum FieldType
    {
        PlayerField,
        EnemyField
    }
    
    public static GameTableManager Instance;
    
    public event Action<Card> CardAddedOnTable;
    public event Action<Card> CardRemovedFromTable;

    public event Action<Stack> StackAddedOnTable;
    public event Action<Stack> StackRemovedFromTable;

    public event Action<Field> FieldChanged;
    
    [SerializeField] public List<Card> cardsOnTable;
    [SerializeField] public List<Stack> stacksOnTable;
    [SerializeField] public List<Field> fields;
    
    [SerializeField] private GameObject cardPrefab;
    
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
        Mouse.current.WarpCursorPosition(new Vector2(Screen.width / 2, Screen.height / 2));
    }

    public Card AddNewCardToTable(CardData newCardData, Vector3 position)
    {
        var newCardGo = Instantiate(cardPrefab, position, Quaternion.identity);
        var newCard = newCardGo.GetComponent<Card>();
        newCard.cardData = newCardData;
        AddCardToTable(newCard);
        return newCard;
    }

    public void AddCardToTable(Card card)
    {
        if (!cardsOnTable.Contains(card))
        {
            cardsOnTable.Add(card);
            
#if UNITY_EDITOR
            card.name = card.cardData.cardName + " " + cardsOnTable.Count((c) => c.cardData == card.cardData);
#endif
            
            var cardDrag = card.GetComponent<CardDrag>();
            if (cardDrag != null)
            {
                cardDrag.CardDragStarted += OnCardDragStarted;
                cardDrag.CardDragEnded += OnCardDragEnded;
            }
            else
            {
                //Debug.LogError($"CardDrag component not found on {card.name}");
            }
                
            CardAddedOnTable?.Invoke(card);
        }
    }
    
    public void RemoveCardFromTable(Card card)
    {
        if (cardsOnTable.Contains(card))
        {
            cardsOnTable.Remove(card);
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
        if (!stacksOnTable.Contains(stack))
        {
            stacksOnTable.Add(stack);
            StackAddedOnTable?.Invoke(stack);
        }
    }
    
    public void RemoveStackFromTable(Stack stack)
    {
        if (stacksOnTable.Contains(stack))
        {
            stacksOnTable.Remove(stack);
            StackRemovedFromTable?.Invoke(stack);
        }
    }
    
    private void OnCardDragStarted(Card card)
    {
        //Debug.Log($"Card {card.name} started dragging");
        ShowCardCanStackIndicator(card);
        var owningStack = card.owningStack;
        if (owningStack)
        {
            foreach (var stackCard in owningStack.cards)
            {
                stackCard.gameObject.layer = LayerMask.NameToLayer("DraggingCard");
            }
        }
    }

    private void OnCardDragEnded(Card card)
    {
        //Debug.Log($"Card {obj.name} ended dragging");
        HideCardCanStackIndicator();
        
        var owningStack = card.owningStack;
        if (owningStack)
        {
            foreach (var stackCard in owningStack.cards)
            {
                stackCard.gameObject.layer = LayerMask.NameToLayer("Card");
            }
        }
    }
    
    private void ShowCardCanStackIndicator(Card card)
    {
        foreach (var c in cardsOnTable)
        {
            if (c == card) continue;
            if (!c.IsTopCard) continue;
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
        foreach (var c in cardsOnTable)
        { 
            c.HideCanStackOnIndicator();
        }
    }

    public void SetTimeScale(float timeScale)
    {
        Time.timeScale = timeScale;
    }

    public void ChangeField(FieldType fieldType)
    {
        if (fields[(int)fieldType] != null)
        {
            FieldChanged?.Invoke(fields[(int)fieldType]);
        }
        else
        {
            Debug.LogError("Field not found");
        }
    }

    public static void MoveCardToField(FieldType field, Card card)
    {
        if (card.owningStack)
        {
            card.owningStack.RemoveCard(card);
            var parentConstraint = card.GetComponent<SlowParentConstraint>();
            parentConstraint.target = null;
            parentConstraint.enabled = false;
        }
        
        var newStack = StackManager.Instance.AddNewStack();
        newStack.AddCard(card);
        newStack.currentField = GameTableManager.Instance.fields[(int)field];
        Instance.AddStackToTable(newStack);
        card.transform.position = GameTableManager.Instance.fields[(int)field].transform.position;
        CardColliderManager.Instance.ModifyColliders(newStack);
    }
}
