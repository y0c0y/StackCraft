// ✅ BattleZone: 카드 위치 관리 전담 (UI는 따로)
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class BattleZone : MonoBehaviour
{

    public event Action<Vector3, Vector2> OnBackgroundSizeChanged;
    public event Action<Card> OnCardEntered;
    public event Action<Card> OnCardExited;

    [Header("Zone Layout")] [SerializeField]
    private float cardWidth = 3f;

    [SerializeField] private float cardHeight = 4f;
    [SerializeField] private float verticalSpacing = 1f;
    [SerializeField] private float horizontalSpacing = 0.2f;
    
    public bool IsInside(Vector3 worldPos)
    {
        return GetComponent<Collider2D>().bounds.Contains(worldPos);
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        var card = other.GetComponent<Card>();
        if (card == null) return;
        OnCardEntered?.Invoke(card);
    }
    
    private void OnTriggerExit2D(Collider2D other)
    {
        var card = other.GetComponent<Card>();
        if (card == null) return;
    
        OnCardExited?.Invoke(card);
    }
    
    public void ArrangeCard(Card card, int index, bool isEnemy)
    {
        var xOffset = index * (cardWidth + horizontalSpacing);
        var yOffset = isEnemy
            ? (cardHeight + verticalSpacing)
            : -(cardHeight + verticalSpacing);

        card.transform.position = transform.position + new Vector3(xOffset, yOffset, 0f);
    }

    public void ResizeBackground(int maxCardCount)
    {
        var width = maxCardCount * (cardWidth + horizontalSpacing);
        var height = (cardHeight + verticalSpacing) * 2f;

        OnBackgroundSizeChanged?.Invoke(transform.position, new Vector2(width, height));
    }

}