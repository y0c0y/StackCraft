using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityTimer;

[Serializable]
public class Stack: MonoBehaviour
{
    public event Action<Stack> OnStackModified;
    
    public List<Card> cards = new();
    public int Length => cards.Count;
    public Card TopCard => cards.Count > 0 ? cards[0] : null;
    public Card LastCard => cards.Count > 0 ? cards[^1] : null;

    public Dictionary<CardData, int> CardCounts = new();
    public bool IsOneKindOnly => CardCounts.Count == 1;
    
    public Field currentField;
    
    public Recipe producingRecipe;
    private Timer _produceTimer;
    public bool HasTimer => _produceTimer is { isDone: false };
    public Bounds bounds;
    private Vector2 _boundOffset = new Vector2(0.5f, 0.5f);
    private float _sizeY = Card.CARD_SIZE.y;

    private void Awake()
    {
        OnStackModified += AdjustSizeY;
        bounds = new Bounds
        {
            size = new Vector3(Card.CARD_SIZE.x + _boundOffset.x, Card.CARD_SIZE.y + _boundOffset.y, 1f)
        };
    }
    
    void DrawBounds(Bounds b, float delay=0)
    {
        // bottom
        var p1 = new Vector3(b.min.x, b.min.y, b.min.z);
        var p2 = new Vector3(b.max.x, b.min.y, b.min.z);
        var p3 = new Vector3(b.max.x, b.min.y, b.max.z);
        var p4 = new Vector3(b.min.x, b.min.y, b.max.z);

        Debug.DrawLine(p1, p2, Color.blue, delay);
        Debug.DrawLine(p2, p3, Color.red, delay);
        Debug.DrawLine(p3, p4, Color.yellow, delay);
        Debug.DrawLine(p4, p1, Color.magenta, delay);

        // top
        var p5 = new Vector3(b.min.x, b.max.y, b.min.z);
        var p6 = new Vector3(b.max.x, b.max.y, b.min.z);
        var p7 = new Vector3(b.max.x, b.max.y, b.max.z);
        var p8 = new Vector3(b.min.x, b.max.y, b.max.z);

        Debug.DrawLine(p5, p6, Color.blue, delay);
        Debug.DrawLine(p6, p7, Color.red, delay);
        Debug.DrawLine(p7, p8, Color.yellow, delay);
        Debug.DrawLine(p8, p5, Color.magenta, delay);

        // sides
        Debug.DrawLine(p1, p5, Color.white, delay);
        Debug.DrawLine(p2, p6, Color.gray, delay);
        Debug.DrawLine(p3, p7, Color.green, delay);
        Debug.DrawLine(p4, p8, Color.cyan, delay);
    }

    private void Update()
    {
        bounds.size = new Vector3(Card.CARD_SIZE.x + _boundOffset.x, _sizeY + _boundOffset.y, 1f);
        bounds.center =
            TopCard?.transform.position - new Vector3(0, (_sizeY - Card.CARD_SIZE.y) / 2f, 0) ?? new Vector3();
#if UNITY_EDITOR
        DrawBounds(bounds);
#endif
    }

    private void AdjustSizeY(Stack obj)
    {
        _sizeY = Card.CARD_SIZE.y;
        for (int i = 1; i < cards.Count; i++)
        {
            var slowParent = cards[i].GetComponent<SlowParentConstraint>();
            if (slowParent)
            {
                _sizeY += -(slowParent.offset.y);
            }
        }
    }

    public void Start()
    {
        GameTableManager.Instance.AddStackToTable(this);
        
        if (!TopCard.cardData.isStatic)
        {
            gameObject.AddComponent<StackRepulsion>();
        }
    }
    
    public void OnDestroy()
    {
        GameTableManager.Instance.RemoveStackFromTable(this);
    }
    
    public void AddCard(Card card)
    {
        cards.Add(card);
        card.owningStack = this;
        ReorderZOrder();

        if (card == TopCard)
        {
            currentField = GameTableManager.Instance.fields.FirstOrDefault(this.IsInsideField);
            if (currentField == null)
            {
                currentField = GameTableManager.Instance.fields[0];
            }
        }
        
        if (!CardCounts.TryAdd(card.cardData, 1))
        {
            CardCounts[card.cardData]++;
        }
        
        OnStackModified?.Invoke(this);
    }

    public void AddMultipleCards(Card[] inCards)
    {
        foreach (var card in inCards)
        {
            cards.Add(card);
            card.owningStack = this;
            
            if (card == TopCard)
            {
                currentField = GameTableManager.Instance.fields.FirstOrDefault(this.IsInsideField);
                if (currentField == null)
                {
                    currentField = GameTableManager.Instance.fields[0];
                }
            }
            
            if (!CardCounts.TryAdd(card.cardData, 1))
            {
                CardCounts[card.cardData]++;
            }
        }
        
        ReorderZOrder();
        OnStackModified?.Invoke(this);
    }
    
    public void RemoveCard(Card card)
    {
        if (card.owningStack != this || !cards.Contains(card))
        {
            return;
        }
        
        cards.Remove(card);
        ReorderZOrder();
        if (cards.Count == 0)
        {
            Debug.Log("Stack count 0. Destroying stack...");
            Destroy(gameObject);
            return;
        }

        if (CardCounts.TryGetValue(card.cardData, out var count))
        {
            if (count > 1)
            {
                CardCounts[card.cardData]--;
            }
            else
            {
                CardCounts.Remove(card.cardData);
            }
        }
        
        OnStackModified?.Invoke(this);
    }
    
    public void RemoveRange(int indexInStack, int prevStackLength)
    { 
        for (int i = indexInStack; i < indexInStack + prevStackLength; i++)
        {
            if (!CardCounts.TryGetValue(cards[i].cardData, out var count))
            {
                Debug.LogError($"Card {cards[i].cardData} not found in CardCounts");
                continue;
            }
            if (count > 1)
            {
                CardCounts[cards[i].cardData]--;
            }
            else
            {
                CardCounts.Remove(cards[i].cardData);
            }
        }
        
        cards.RemoveRange(indexInStack,  prevStackLength); 
        if (cards.Count == 0)
        {
            Destroy(gameObject);
            return;
        }
        
        ReorderZOrder();
        OnStackModified?.Invoke(this);
    }
    
    public void ConsumeCard(Card card)
    {
        if (!card.cardData.isConsumable) return;
        
        cards.Remove(card);
        
        if (CardCounts.TryGetValue(card.cardData, out var count))
        {
            if (count > 1)
            {
                CardCounts[card.cardData]--;
            }
            else
            {
                CardCounts.Remove(card.cardData);
            }
        }
        
        Destroy(card.gameObject);
    }

    public void ConsumeCards(List<Card> cardsToConsume)
    {
        if (cardsToConsume.Count <= 0) return;
        
        foreach (var card in cardsToConsume)
        {
            Debug.Log($"Consuming card {card}");
            ConsumeCard(card);
        }
        
        Debug.Log($"Card count after consume : {cards.Count}");

        if (cards.Count == 0)
        {
            Destroy(this);
            return;
        }
        
        ReorderZOrder();
        ResetFollowTargets();
        
        OnStackModified?.Invoke(this);
    }

    private void ResetFollowTargets()
    {
        for (int i = 0; i < cards.Count; i++)
        {
            var follow = cards[i].GetComponent<SlowParentConstraint>();
            if (follow)
            {
                if (i == 0)
                {
                    follow.enabled = false;
                    follow.target = null;
                }
                else
                {
                    follow.target = cards[i - 1].transform;
                }
            }
        }
    }

    public void ReorderZOrder(int sortingLayerId = 0)
    {
        float topz = 0f;
        for (int i = 0; i < cards.Count; i++)
        {
            cards[i].SetSortingLayer(i * 2, sortingLayerId);
            if (i == 0)
            {
                topz = cards[0].transform.position.z;
            }
            else
            {
                var pos = cards[i].transform.position;
                pos.z = topz;
                cards[i].transform.position = pos;
            }

            if (cards[i].GetComponentInChildren<CardBattleUI>() is { } cardBattleUI
                && cards[i].IsLastCard)
            {
                var lastSortingOrder = cards[i].GetComponentInChildren<CardDataSprite>().LastSortingOrder; 
                cardBattleUI.canvas.sortingOrder = lastSortingOrder + 1;
                cardBattleUI.canvas.sortingLayerID = SortingLayer.layers[sortingLayerId].id;
            }
        }
    }

    public void AddTimer(Recipe matchedRecipe, List<Card> consumedCards)
    {
        if (producingRecipe != null && producingRecipe == matchedRecipe)
        {
            return;
        }
        
        Timer.Cancel(_produceTimer);
        Debug.Log($"Starting recipe {matchedRecipe.recipeName} timer for {matchedRecipe.produceTime} seconds");
        producingRecipe = matchedRecipe;
        TopCard.cardTimerUI.gameObject.SetActive(true);
        _produceTimer = this.AttachTimer(matchedRecipe.produceTime,
            () => ApplyRecipe(matchedRecipe, consumedCards),
            onUpdate: (t) => this.TopCard.cardTimerUI.SetValue(t / matchedRecipe.produceTime),
            useRealTime: false,
            isLooped: false);
    }

    private void ApplyRecipe(Recipe matchedRecipe, List<Card> consumedCards)
    {
        RecipeManager.Instance.ApplyRecipe(this, matchedRecipe, consumedCards);

        if (!RecipeManager.Instance.CheckRecipe(this, matchedRecipe))
        {
            RemoveTimer();
        }
        else
        {
            _produceTimer = this.AttachTimer(matchedRecipe.produceTime,
                () => ApplyRecipe(matchedRecipe, RecipeManager.Instance.FindConsumedCards(cards, CardCounts)),
                onUpdate: (t) => this.TopCard.cardTimerUI.SetValue(t / matchedRecipe.produceTime),
                useRealTime: false,
                isLooped: false);
        }
    }

    public void RemoveTimer()
    {
        Debug.Log($"Removing timer");
        TopCard.cardTimerUI.gameObject.SetActive(false);
        Timer.Cancel(_produceTimer);
        producingRecipe = null;
    }
}
