using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

public class CardDataSprite : MonoBehaviour
{
    [SerializeField] private SpriteRenderer backgroundSprite;
    [SerializeField] private SpriteRenderer artworkSprite;
    [SerializeField] private TextMeshPro cardNameText;
    
    private Card _owner;
    private Dictionary<SpriteRenderer, int> _spriteInitialSortingOrder = new();
    
    private void Awake()
    {
        _owner = GetComponentInParent<Card>();
        Debug.Assert(_owner != null, $"CardDataSprite {name}가 카드에 붙어있지 않음");

        var sprites = GetComponentsInChildren<SpriteRenderer>();
        foreach (var sprite in sprites)
        {
            _spriteInitialSortingOrder.Add(sprite, sprite.sortingOrder);
        }
    }

    private void Start()
    {
        Debug.Assert(backgroundSprite && artworkSprite && cardNameText);
        StartCoroutine(SetupCardData());
        _owner.OnSortingLayerChanged += SetSortingLayer;
    }
    
    private IEnumerator SetupCardData()
    {
        yield return new WaitUntil(() => _owner.cardData != null);
        backgroundSprite.color = Random.ColorHSV();
        artworkSprite.sprite = _owner.cardData.sprite;
        cardNameText.text = _owner.cardData.cardName;
    }
    
    public void SetSortingLayer(int sortingOrder)
    {
        foreach (var sprite in _spriteInitialSortingOrder.Keys)
        {
            sprite.sortingOrder = _spriteInitialSortingOrder[sprite] + sortingOrder;
        }

        if (cardNameText != null)
        {
            cardNameText.sortingOrder = sortingOrder + 2;
        }
    }
}
