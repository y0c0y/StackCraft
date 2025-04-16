using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class GameTableManager : MonoBehaviour
{
    public static GameTableManager Instance;
    public event Action<Card> CardAddedOnTable;
    public event Action<Card> CardRemovedFromTable;
    
    [SerializeField] public List<Card> CardsOnTable;
    [SerializeField] public List<Stack> StacksOnTable;

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
    }

    public void AddNewCardToTable(CardData newCardData, Vector3 position)
    {
        var newCard = Instantiate(cardPrefab, position, Quaternion.identity);
        newCard.GetComponent<Card>().cardData = newCardData;
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
            stack.OnStackModified += OnStackModified;
        }
    }
    
    public void RemoveStackFromTable(Stack stack)
    {
        if (StacksOnTable.Contains(stack))
        {
            StacksOnTable.Remove(stack);
            stack.OnStackModified -= OnStackModified;
        }
    }
    
    private void OnStackModified(Stack stack)
    {
        if (stack.HasTimer)
        {
            if (!RecipeManager.Instance.CheckRecipe(stack, stack.ProducingRecipe))
            {
                stack.RemoveTimer();
            }
        }
        
        // Don't bother checking for recipes if the stack is empty or has only one card
        if (stack.Length <= 1) return;
        
        Debug.Log($"Stack modified: {stack.name}. Checking for recipes...");
        if (RecipeManager.Instance.TryFindMatchingRecipe(stack, out var matchedRecipe, out var consumedCards))
        {
            Debug.Log($"Recipe matched: {matchedRecipe.name} with {stack.name}, consumed cards: {string.Join(", ", consumedCards.Select(c => c.name))}");
            
            if (matchedRecipe.produceTime > 0)
            {
                stack.AddTimer(matchedRecipe, consumedCards);
            }
            else
            {
                ApplyRecipe(stack, matchedRecipe, consumedCards);
            }
        }
    }

    public void ApplyRecipe(Stack stack, Recipe recipe, List<Card> consumedCards)
    {
        var originStackPos = stack.cards[0].transform.position;
        
        if (recipe.ConsumeInputs)
        {
            stack.ConsumeCards(consumedCards);
        }

        var outputCards = recipe.outputCards;
            
        foreach (var spawningCard in outputCards)
        {
            var randomDirection = Random.insideUnitCircle.normalized;
            var randomUnitCircle = randomDirection * 3f;
            
            var spawningPos = originStackPos + new Vector3(randomUnitCircle.x, 0, randomUnitCircle.y);
            AddNewCardToTable(spawningCard, spawningPos);
        }
    }
    
    private void OnCardDragStarted(Card card)
    {
        //Debug.Log($"Card {card.name} started dragging");
        ShowCardCanStackIndicator(card);
    }

    private void OnCardDragEnded(Card card)
    {
        //Debug.Log($"Card {obj.name} ended dragging");
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
