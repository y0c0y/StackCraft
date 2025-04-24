using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;

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
    
    public bool IsInside(Vector3 worldPos)
    {
        return GetComponent<Collider2D>().bounds.Contains(worldPos);
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        var card = other.GetComponent<Card>();
        if (card == null || !BattleCommon.IsValidCardType(card)) return;


        OnCardEntered?.Invoke(card);
    }
    
    private void OnTriggerExit2D(Collider2D other)
    {
        var card = other.GetComponent<Card>();
        if (card == null || !BattleCommon.IsValidCardType(card)) return;

    
        OnCardExited?.Invoke(card);
    }

    public async UniTask ArrangeCard(List<Card> cards, bool isEnemy)
    {
        
        var cnt = cards.Count;
        if (cnt == 0) return;

        var totalWidth = (cnt - 1) * (cardWidth + horizontalSpacing);
        var centerOffset = new Vector3(totalWidth * 0.5f, 0f, 0f);
        var area = isEnemy ? enemyArea : personArea;
        var origin = transform.position + area.localPosition; 

        for (var i = 0; i < cnt; i++)
        {
            var offset = new Vector3(i * (cardWidth + horizontalSpacing), 0f, 0f);
            var targetPos = origin - centerOffset + offset;
            
            cards[i].GetComponent<Rigidbody2D>().MovePosition(targetPos);
            
            Debug.Log(cards[i].transform.position);
        }

        await UniTask.CompletedTask;
    }


    public void ResizeBackground(int maxCardCount)
    {
        var width = maxCardCount * (cardWidth + horizontalSpacing);
        var height = (cardHeight + verticalSpacing) * 2f;

        OnBackgroundSizeChanged?.Invoke(transform.position, new Vector2(width, height));
    }

}