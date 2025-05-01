using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;

public class CardDataSprite : MonoBehaviour
{
    [SerializeField] public SpriteRenderer artworkSprite;
    [SerializeField] public SpriteRenderer shadowSprite;
    [SerializeField] private GameObject canStackOnIndicator;

    public int LastSortingOrder => _spriteInitialSortingOrder.Keys.Select(spriteRenderer => spriteRenderer.sortingOrder).Max() + 1;
    
    private float artNormalY = 0f;
    private float artHoverY = 0.07f;
    private float artDragY = 0.15f;
    private float tweenDuration = 0.3f;

    private Tween _tween;
    private Card _ownerCard;
    private readonly Dictionary<SpriteRenderer, int> _spriteInitialSortingOrder = new();
    
    private void Awake()
    {
        _ownerCard = GetComponentInParent<Card>();
        Debug.Assert(_ownerCard != null, $"CardDataSprite {name}가 카드에 붙어있지 않음");
        
        _ownerCard.OnSortingLayerChanged += SetSortingLayer;
        _ownerCard.OnShowCanStackOnIndicator += () => canStackOnIndicator?.SetActive(true);
        _ownerCard.OnHideCanStackOnIndicator += () => canStackOnIndicator?.SetActive(false);
        _ownerCard.CardPointerEntered += OnCardPointerEnter;
        _ownerCard.CardPointerExited += OnCardPointerExit;
        
        var cardDrag = GetComponentInParent<CardDrag>();
        if (cardDrag)
        {
            cardDrag.CardDragStarted += (c) => OnCardDragStarted();
            cardDrag.CardDragEnded += (c) => OnCardDragEnded();
        }
        
        var sprites = GetComponentsInChildren<SpriteRenderer>();
        foreach (var sprite in sprites)
        {
            _spriteInitialSortingOrder.Add(sprite, sprite.sortingOrder);
        }
    }
    
    private void Start()
    {
        Debug.Assert(artworkSprite);
        StartCoroutine(SetupCardData());
        transform.DOPunchScale(new Vector3(0.2f, 0f, 0f), 0.5f)
                                .SetUpdate(true)
                                .SetLink(gameObject);
        artworkSprite.DOColor(Color.black, 0.075f)
                                .SetEase(Ease.InOutElastic)
                                .SetLoops(2, LoopType.Yoyo)
                                .SetUpdate(true)
                                .SetLink(gameObject);
    }

    private void Update()
    {
        shadowSprite.transform.localPosition = -artworkSprite.transform.localPosition;
        var shadowPos = shadowSprite.transform.position;
        shadowPos.z = 0f;
        shadowSprite.transform.position = shadowPos;
    }

    public void OnCardPointerEnter()
    {
        _tween?.Kill();

        _tween = 
            artworkSprite.transform.DOLocalMoveY(artHoverY, tweenDuration)
                 .SetEase(Ease.OutBack)
                 .SetUpdate(true)
                 .SetLink(gameObject);
        
        if (_ownerCard.IndexInStack + 1 < _ownerCard.owningStack.cards.Count)
        {
            _ownerCard.owningStack.cards[_ownerCard.IndexInStack + 1].GetComponentInChildren<CardDataSprite>()?.OnCardPointerEnter();
        }
    }
    
    public void OnCardPointerExit()
    {
        _tween?.Kill();

        _tween = 
            artworkSprite.transform.DOLocalMoveY(artNormalY, tweenDuration)
                .SetEase(Ease.OutBack)
                .SetUpdate(true)
                .SetLink(gameObject);
        
        if (_ownerCard.IndexInStack + 1 < _ownerCard.owningStack.cards.Count)
        {
            _ownerCard.owningStack.cards[_ownerCard.IndexInStack + 1].GetComponentInChildren<CardDataSprite>()?.OnCardPointerExit();
        }
    }
    
    public void OnCardDragStarted()
    {
        _tween?.Kill();

        _tween = 
            artworkSprite.transform.DOLocalMoveY(artDragY, tweenDuration)
                .SetEase(Ease.OutBack)
                .SetUpdate(true)
                .SetLink(gameObject);
        
        if (_ownerCard.IndexInStack + 1 < _ownerCard.owningStack.cards.Count)
        {
            _ownerCard.owningStack.cards[_ownerCard.IndexInStack + 1].GetComponentInChildren<CardDataSprite>()?.OnCardDragStarted();
        }
    }
    
    public void OnCardDragEnded()
    {
        _tween?.Kill();

        _tween = 
            artworkSprite.transform.DOLocalMoveY(artHoverY, tweenDuration)
                .SetEase(Ease.OutBack)
                .SetUpdate(true)
                .SetLink(gameObject);
        
        if (_ownerCard.IndexInStack + 1 < _ownerCard.owningStack.cards.Count)
        {
            _ownerCard.owningStack.cards[_ownerCard.IndexInStack + 1].GetComponentInChildren<CardDataSprite>()?.OnCardDragEnded();
        }
    }
    
    private IEnumerator SetupCardData()
    {
        yield return new WaitUntil(() => _ownerCard.cardData != null);
        artworkSprite.sprite = _ownerCard.cardData.sprite;
    }
    
    private void SetSortingLayer(int sortingOrder, int sortingLayerId = 0)
    {
        foreach (var sprite in _spriteInitialSortingOrder.Keys)
        {
            sprite.sortingLayerID = SortingLayer.layers[sortingLayerId].id;
            sprite.sortingOrder = _spriteInitialSortingOrder[sprite] + sortingOrder;
        }
    }
}
