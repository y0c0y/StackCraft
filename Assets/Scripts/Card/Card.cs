using System;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

public class Card : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
{
    public static readonly Vector2 CARD_SIZE = new Vector2(3f, 4f);

    // Events
    public event Action CardClicked;
    public event Action<Card, Card> CardReleasedOn;
    public event Action<Card> RequestSplitFromStack;
    public event Action<int, int> OnSortingLayerChanged;
    public event Action OnShowCanStackOnIndicator;
    public event Action OnHideCanStackOnIndicator;
    public event Action CardPointerEntered;
    public event Action CardPointerExited;

    public void RequestSplit() => RequestSplitFromStack?.Invoke(this);
    
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
    private CardDrag _drag;

    private ContactFilter2D _cardOverlapFilter2D;
    private Collider2D[] _cardOverlaps;

    private void Awake()
    {
        _drag = GetComponent<CardDrag>();
        _cardOverlapFilter2D = new ContactFilter2D();
        _cardOverlapFilter2D.SetLayerMask(LayerMask.GetMask("Card"));
        _cardOverlapFilter2D.useLayerMask = true;
        _cardOverlapFilter2D.useTriggers = true;
        _cardOverlaps = new Collider2D[10];
    }

    private void Start()
    {
        Debug.Assert(cardData != null, $"{name}에 카드 데이터가 설정되지 않음");

        if (owningStack) return;
        GameTableManager.Instance.AddCardToTable(this);
        owningStack ??= StackManager.Instance.AddNewStack();
        owningStack.AddCard(this);
    }

    private void OnDestroy()
    {
        if (owningStack)
        {
            owningStack.RemoveCard(this);
        }
        GameTableManager.Instance.RemoveCardFromTable(this);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        CardPointerEntered?.Invoke();
        
        var cardUiInfo = CardInfoUI.Instance;
        if (cardUiInfo)
        {
            cardUiInfo.ShowStackInfo(this);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        CardPointerExited?.Invoke();
        
        var cardUiInfo = CardInfoUI.Instance;
        if (cardUiInfo)
        {
            cardUiInfo.HideStackInfo();
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        _drag?.OnPointerDown(eventData);

        if (!IsTopCard)
        {
            RequestSplit();
        }
        
        CardClicked?.Invoke();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        _drag?.OnPointerUp(eventData);

        // OverlapBox가 자동으로 지워주지 않아서 null값으로 초기화해야함
        for (int i = 0; i < _cardOverlaps.Length; i++)
        {
            _cardOverlaps[i] = null;
        }
        
        var size = Physics2D.OverlapBox(
            transform.position,
            CARD_SIZE + new Vector2(0.05f, 0.05f),
            0f,
            _cardOverlapFilter2D,
            _cardOverlaps
            );
        if (size <= 0) return;
        
        
        // 자기 자신의 스택이 아닌 첫번째 카드
        var firstCard = _cardOverlaps?.FirstOrDefault(o => o &&
                                                               o.GetComponent<Card>()?.owningStack != this.owningStack);
        if (firstCard != null)
        {
            CardReleasedOn?.Invoke(this, firstCard.GetComponent<Card>());
        }
    }
    
    public void SetSortingLayer(int sortingOrder, int sortingLayerId = 0)
    {
        OnSortingLayerChanged?.Invoke(sortingOrder, sortingLayerId);
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
