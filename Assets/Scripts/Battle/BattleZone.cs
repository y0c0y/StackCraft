using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class BattleZone : MonoBehaviour
{
    public event Action<Vector3, Vector2> OnBackgroundSizeChanged;
    public event Action<Card> OnCardEntered;
    public event Action<Card> OnCardExited;
    
    [SerializeField] private Transform enemyArea;
    [SerializeField] private Transform personArea;

    [Header("Card Layout")] 
    [SerializeField] private float cardWidth = 3f;
    [SerializeField] private float cardHeight = 4f;
    [SerializeField] private float verticalSpacing = 1f;
    [SerializeField] private float horizontalSpacing = 0.2f;
    
    private List<(Card card, ComponentState state)> _list;
    
    private struct ComponentState
    {
        public bool SlowEnabled;
        public bool DragEnabled;
    }
    public bool IsInside(Vector3 worldPos)
    {
        return GetComponent<Collider2D>().bounds.Contains(worldPos);
    }
    
        
    public void RestoreCardComponents(List<Card> list)
    {
        foreach (var card in list)
        {
            if (card == null) continue;
            var slow = card.GetComponent<SlowParentConstraint>();
            var drag = card.GetComponent<CardDrag>();
            
            slow.enabled = false;
            drag.enabled = true;
        }
    } 
    
    public void ResizeBackground(int maxCardCount)
    {
        if (maxCardCount <= 0)
        {
            OnBackgroundSizeChanged?.Invoke(transform.position, Vector2.zero);
            return;
        }
        
        var totalWidth = (maxCardCount - 1) * (cardWidth + horizontalSpacing) + cardWidth;
        var totalHeight = cardHeight * 2 + verticalSpacing;

        var size = new Vector2(totalWidth, totalHeight);
        
        OnBackgroundSizeChanged?.Invoke(transform.position, size);
        
        var bc = GetComponent<BoxCollider2D>();
        if (bc != null)
            bc.size = size;
    }

    
    public void ArrangeCard(List<Card> persons, List<Card> enemies)
    {
        int maxCnt = Mathf.Max(persons.Count, enemies.Count);
        if (maxCnt == 0) return;

        var totalWidth = (maxCnt - 1) * (cardWidth + horizontalSpacing);
        var centerOffset = new Vector3(totalWidth * 0.5f, 0f, 0f);

        _ = ArrangeGroup(persons, transform.position + personArea.localPosition, centerOffset);
        _ = ArrangeGroup(enemies, transform.position + enemyArea.localPosition, centerOffset);
    }

    private async UniTask ArrangeGroup(List<Card> cards, Vector3 origin, Vector3 centerOffset)
    {
        for (int i = 0; i < cards.Count; i++)
        {
            var card = cards[i];
            if (card == null) continue;

            DisableConstraints(card);

            var offset = new Vector3(i * (cardWidth + horizontalSpacing), 0f, 0f);
            var targetPos = origin - centerOffset + offset;

            await MoveCardSmooth(card, targetPos, 0.3f); // ✨ 천천히 이동
        }
    }

    private void DisableConstraints(Card card)
    {
        var slow = card.GetComponent<SlowParentConstraint>();
        var drag = card.GetComponent<CardDrag>();
        if (slow != null) slow.enabled = false;
        if (drag != null) drag.enabled = false;
    }

    private async UniTask MoveCardSmooth(Card card, Vector3 targetPos, float duration)
    {
        var rb = card.GetComponent<Rigidbody2D>();
        if (rb == null) return;

        Vector3 startPos = rb.position;
        var elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            var t = Mathf.Clamp01(elapsed / duration);
            rb.MovePosition(Vector3.Lerp(startPos, targetPos, t));
            await UniTask.Yield();
        }

        rb.MovePosition(targetPos);
    }

}