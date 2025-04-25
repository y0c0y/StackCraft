using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class BattleZone : MonoBehaviour
{
    public event Action<Vector3, Vector2> OnBackgroundSizeChanged;
    
    [SerializeField] private Transform enemyArea;
    [SerializeField] private Transform personArea;

    [Header("Card Layout")] 
    [SerializeField] private float cardWidth = 3f;
    [SerializeField] private float cardHeight = 4f;
    [SerializeField] private float verticalSpacing = 1f;
    [SerializeField] private float horizontalSpacing = 0.2f;
    
    private struct ComponentState
    {
        public bool SlowEnabled;
        public bool DragEnabled;
    }
    public bool IsInside(Vector3 worldPos)
    {
        return GetComponent<Collider2D>().bounds.Contains(worldPos);
    }
    
    private ComponentState DisableCardComponents(Card card)
    {
        var slow = card.GetComponent<SlowParentConstraint>();
        var drag = card.GetComponent<CardDrag>();
    
        var state = new ComponentState {
            SlowEnabled = slow.enabled,
            DragEnabled = drag.enabled
        };
    
        slow.enabled = false;
        drag.enabled = false;
        return state;
    }

    public void ArrangeCard(List<Card> cards, bool isEnemy)
    {
        int cnt = cards.Count;
        if (cnt == 0) return;

        var list = new List<(Card card, ComponentState state)>(cards.Count);
        float totalWidth     = (cards.Count - 1) * (cardWidth + horizontalSpacing);
        Vector3 centerOffset = new Vector3(totalWidth * 0.5f, 0f, 0f);
        Vector3 origin       = (isEnemy ? enemyArea : personArea).position;

        for (int i = 0; i < cards.Count; i++)
        {
            if(i >= cards.Count) continue;
            
            var card  = cards[i];
            if (card == null) continue;
            var state = DisableCardComponents(card);
            list.Add((card, state));

            Vector3 offset    = new Vector3(i * (cardWidth + horizontalSpacing), 0f, 0f);
            Vector3 targetPos = origin - centerOffset + offset;

            MoveCardInstant(card, targetPos);
        }
        
        foreach (var (card, state) in list)
        {
            if (card == null) continue;
            RestoreCardComponents(card, state);
        }
    }


    private void RestoreCardComponents(Card card, ComponentState state)
    {
        var slow = card.GetComponent<SlowParentConstraint>();
        var drag = card.GetComponent<CardDrag>();
    
        slow.enabled = state.SlowEnabled;
        drag.enabled = state.DragEnabled;
    }


    private void MoveCardInstant(Card card, Vector3 targetPos)
    {
        var rb = card.GetComponent<Rigidbody2D>();
        rb.position = targetPos;
    }

    public void ResizeBackground(int maxCardCount)
    {
        if (maxCardCount <= 0)
        {
            OnBackgroundSizeChanged?.Invoke(transform.position, Vector2.zero);
            return;
        }

      
        float totalWidth = (maxCardCount - 1) * (cardWidth + horizontalSpacing) + cardWidth;
        float totalHeight = cardHeight * 2 + verticalSpacing;

        var size = new Vector2(totalWidth, totalHeight);
        
        OnBackgroundSizeChanged?.Invoke(transform.position, size);
        
        var bc = GetComponent<BoxCollider2D>();
        if (bc != null)
            bc.size = size;
    }

}