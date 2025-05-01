using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

public class BattleZone : MonoBehaviour
{
    [SerializeField] private Transform enemyArea;
    [SerializeField] private Transform personArea;
    [SerializeField] private GameObject background;

    [Header("Card Layout")] 
    [SerializeField] private float cardWidth = 3f;
    [SerializeField] private float cardHeight = 4f;
    [SerializeField] private float verticalSpacing = 1f;
    [SerializeField] private float horizontalSpacing = 0.2f;
    
    public (Vector3,Vector3) ResizeBackground(int personsCount, int enemiesCount)
    {
        var maxCardCount = Mathf.Max(personsCount, enemiesCount);

        var width = maxCardCount * (cardWidth);
        var height = (cardHeight) * 2;
        
        background.transform.localScale = new Vector3(maxCardCount, 2.5f, 0f);
        
        var zoneCollider = GetComponent<BoxCollider2D>();
        var bounds       = zoneCollider.bounds;
        
        return (bounds.size, bounds.center);
    }
    
    public async UniTask ArrangeCard(List<Card> persons, List<Card> enemies)
    {
        int maxCnt = Mathf.Max(persons.Count, enemies.Count);
        if (maxCnt == 0) return;

        var totalWidth = (maxCnt - 1) * (cardWidth + horizontalSpacing);
        var centerOffset = new Vector3(totalWidth * 0.5f, 0f, 0f);

        await ArrangeGroup(enemies, transform.position + enemyArea.localPosition, centerOffset);
        await ArrangeGroup(persons, transform.position + personArea.localPosition, centerOffset);
    }

    private async UniTask ArrangeGroup(List<Card> cards, Vector3 origin, Vector3 centerOffset)
    {
        for (int i = 0; i < cards.Count; i++)
        {
            var card = cards[i];
            if (card == null) continue;
            
            var offset = new Vector3(i * (cardWidth + horizontalSpacing), 0f, 0f);
            var targetPos = origin - centerOffset + offset;
            targetPos.z = 0f;
            await MoveCardSmooth(card, targetPos, 0.3f);
        }
    }

    private async UniTask MoveCardSmooth(Card card, Vector3 targetPos, float duration)
    {
        // Rigidbody2D 물리 적용이 필요 없거나, z도 같이 움직여야 한다면
        await card.transform
            .DOMove(targetPos, duration)
            .SetEase(Ease.Linear)
            .AsyncWaitForCompletion();
        
        
        BattleManager.Instance.CardBattles[card].ResetSpriteLocalPos();
        // await UniTask.NextFrame();
    }

}