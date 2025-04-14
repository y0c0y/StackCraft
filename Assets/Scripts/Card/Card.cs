using System;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

public class Card : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    // Events
    public event Action<Card, Card> CardReleasedOn;
    public event Action<Card> RequestSplitFromStack;

    // Stack
    public Stack owningStack;
    public bool IsTopCard => owningStack?.TopCard == this;
    public bool IsLastCard => owningStack?.LastCard == this;
    public int IndexInStack => owningStack?.cards.IndexOf(this) ?? -1;
    public bool IsChild => !IsTopCard;
    
    // Components
    private SpriteRenderer _sprite;
    private CardDrag _drag;
    private Collider2D _collider2D;

    private void Awake()
    {
        _sprite = GetComponentInChildren<SpriteRenderer>();
        _drag = GetComponent<CardDrag>();
        _collider2D = GetComponent<Collider2D>();
        
        owningStack ??= new Stack();
        owningStack.AddCard(this);
    }
    
    public void OnPointerDown(PointerEventData eventData)
    {
        _drag.OnPointerDown(eventData);

        if (!IsTopCard)
        {
            RequestSplitFromStack?.Invoke(this);
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        _drag.OnPointerUp(eventData);

        // TODO: non alloc / find better way?
        var overlapBox = Physics2D.OverlapBoxAll(transform.position,
            new Vector2(transform.localScale.x, transform.localScale.y) + new Vector2(0.1f, 0.1f),
            0f,
            LayerMask.GetMask("Card")
            );
        // 자기 자신이 아니거나 맨 위 카드가 아닌 카드 (첫번째 맨 위 카드)
        var FirstTopCard = overlapBox?.FirstOrDefault(o => o != _collider2D  && (o.GetComponent<Card>()?.IsTopCard ?? false));
        if (FirstTopCard != null)
        {
            CardReleasedOn?.Invoke(this, FirstTopCard.GetComponent<Card>());
        }
    }
    
    public void SetSortingLayer(int sortingLayer)
    {
        _sprite.sortingOrder = sortingLayer;
    }
}
