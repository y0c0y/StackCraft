using System;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

public class Card : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    // Events
    public event Action<Card, Card> CardReleasedOn;
    public event Action<Card> RequestSplitFromStack;
    public event Action<int> OnSortingLayerChanged;
    public event Action OnShowCanStackOnIndicator;
    public event Action OnHideCanStackOnIndicator;

    // Data
    [SerializeField] public CardData cardData;
    
    // Stack
    public Stack owningStack;
    public bool IsTopCard => owningStack?.TopCard == this;
    public bool IsLastCard => owningStack?.LastCard == this;
    public int IndexInStack => owningStack?.cards.IndexOf(this) ?? -1;
    public bool IsChild => !IsTopCard;
    
    // Components
    [SerializeField] public CardTimerUI cardTimerUI;
    private SpriteRenderer[] _sprites;
    private CardDrag _drag;
    private Collider2D _collider2D;

    private ContactFilter2D _cardOverlapFilter2D;
    private Collider2D[] _cardOverlaps;

    private void Awake()
    {
        _sprites = GetComponentsInChildren<SpriteRenderer>();
        _drag = GetComponent<CardDrag>();
        _collider2D = GetComponent<Collider2D>();
        _cardOverlapFilter2D = new ContactFilter2D();
        _cardOverlapFilter2D.SetLayerMask(LayerMask.GetMask("Card"));
        _cardOverlapFilter2D.useLayerMask = true;
        _cardOverlapFilter2D.useTriggers = true;
        _cardOverlaps = new Collider2D[10];
    }

    private void Start()
    {
        Debug.Assert(cardData != null, $"{name}에 카드 데이터가 설정되지 않음");
        GameTableManager.Instance.AddCardToTable(this);

        owningStack ??= StackManager.Instance.AddNewStack();
        owningStack.AddCard(this);
    }

    private void OnDestroy()
    {
        GameTableManager.Instance.RemoveCardFromTable(this);
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

        for (int i = 0; i < _cardOverlaps.Length; i++)
        {
            _cardOverlaps[i] = null;
        }
        
        var size = Physics2D.OverlapBox(
            transform.position,
            new Vector2(transform.localScale.x, transform.localScale.y) + new Vector2(0.1f, 0.1f),
            0f,
            _cardOverlapFilter2D,
            _cardOverlaps
            );
        if (size <= 0) return;
        
        
        // 자기 자신이 아니거나 맨 위 카드가 아닌 카드 (첫번째 맨 위 카드)
        var firstTopCard = _cardOverlaps?.FirstOrDefault(o => o && o != _collider2D  && (o.GetComponent<Card>()?.IsTopCard ?? false));
        if (firstTopCard != null)
        {
            CardReleasedOn?.Invoke(this, firstTopCard.GetComponent<Card>());
        }
    }
    
    public void SetSortingLayer(int sortingLayer)
    {
        OnSortingLayerChanged?.Invoke(sortingLayer);
    }

    public void ShowCanStackOnIndicator()
    {
        OnShowCanStackOnIndicator?.Invoke();
    }

    public void HideCanStackOnIndicator()
    {
        OnHideCanStackOnIndicator?.Invoke();
    }
}
