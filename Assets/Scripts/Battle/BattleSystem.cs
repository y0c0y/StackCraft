using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Serialization;
using Random = System.Random;

public class BattleSystem : MonoBehaviour
{
    [Header("Prefabs")] 
    public BattleZone zone;
    public BattleZoneUIController zoneUI;
    public Canvas canvas;

    [Header("InBattleCards")]
    public List<Card> persons = new();
    public List<Card> enemies = new();

    public readonly float RemoveCardDelay = 0.2f;
    
    public event Action<BattleSystem> DeleteBattle;

    private int _preEnemiesCount;
    private int _prePersonCount;
    
    private Dictionary<Card, int> _cardHp = new();

    readonly Random _random =  new Random();

    private void HandleCardEnter(Card card) => _ = AddCard(card);
    private void HandleCardExit(Card card)
    {
        _ = TryRemoveAfterDelay(card);
    }
    
    private async UniTaskVoid TryRemoveAfterDelay(Card card)
    {
        await UniTask.Delay(200);

        if (card == null || card.gameObject == null) return;

        if (!zone.IsInside(card.transform.position))
        {
            Debug.Log($"📤 {card.name} 실제로 전투존 이탈 → 제거");
            await HandleRemove(card, false);
        }
        else
        {
            Debug.Log($"↩ {card.name} 다시 진입함 → 유지");
        }
    }

    
    public async UniTask Init(Card person, Card enemy)
    {
        zone.OnBackgroundSizeChanged -= zoneUI.ShowZone;
        zone.OnBackgroundSizeChanged += zoneUI.ShowZone;

        zone.OnCardEntered += HandleCardEnter;
        zone.OnCardExited  += HandleCardExit;

        if (canvas.renderMode == RenderMode.WorldSpace && canvas.worldCamera == null)
            canvas.worldCamera = Camera.main;

        await AddCard(person);
        await AddCard(enemy);

        await TryStartBattle();
    }

    public bool IsCardInBattle(Card card) => persons.Contains(card) || enemies.Contains(card);

    public bool IsEnemyNearby(Vector3 pos, float range)
    {
        return Vector3.Distance(zone.transform.position, pos) <= range;
    }
    
    private async UniTask<bool> Attack(List<Card> attackers, List<Card> targets)
    {
        int targetIdx = 0;

        for (int i = 0; i < attackers.Count; i++)
        {
            if (targetIdx >= targets.Count) break;
            
            var targetCard = targets[targetIdx];
            int damage = _random.Next(0, 6);

            Debug.Log($"{attackers[i].name} → {targetCard.name}에게 {damage} 피해");

            _cardHp[targetCard] -= damage;
            // BattleCommon.UpdateCardHpUI(targetCard, _cardHp[targetCard]);

            if (_cardHp[targetCard] <= 0)
            {
                await HandleRemove(targetCard, true);
                // Debug.Log($"🟥 {targetCard.name} 파괴됨");
            }
            else
            {
                targetIdx++; // 다음 타겟으로
            }

            await UniTask.Delay(300);
        }

        return targets.Count == 0;
    }
    

    private async UniTask TryStartBattle()
    {
        var preemptiveFlag = false;

        while (true)
        {
            Debug.Log($"🕛 { (preemptiveFlag ? "적" : "아군")} 턴 시작");

            bool battleOver = preemptiveFlag
                    ? await Attack(enemies, persons)
                : await Attack(persons, enemies);

            if (battleOver)
            {
                Debug.Log(preemptiveFlag ? "👿 적 승리" : "🎉 플레이어 승리");
                break;
            }

            preemptiveFlag = !preemptiveFlag;
        }

        await UniTask.Delay(1000);
        EndBattle();
    }
    public async UniTask AddCard(Card card)
    {
        if (IsCardInBattle(card)) return;

        var (group, isEnemy) = BattleCommon.GetCardTargetList(card, persons, enemies);
        group.Add(card);
        _cardHp[card] = 5;
        // BattleCommon.UpdateCardHpUI(card, 5);

        await RepositionAllCards(group, isEnemy);
    }
    
    private async UniTask HandleRemove(Card card, bool shouldDestroy)
    {
        var (group, isEnemy) = BattleCommon.GetCardTargetList(card, persons, enemies);
        if (!group.Remove(card)) return;

        _cardHp.Remove(card);

        if (shouldDestroy)
        {
            Debug.Log("Destroy");
            Destroy(card.gameObject);
            Destroy(card);
        }

        await RepositionAllCards(group, isEnemy);
    }

    private async UniTask RepositionAllCards(List<Card> cards, bool flag)
    {
        var pre = flag ? _preEnemiesCount : _prePersonCount;
        
        if(cards.Count == pre) return;
        
        if (flag)
        {
            _preEnemiesCount = cards.Count;
        }
        else
        {
            _prePersonCount = cards.Count;
        }

        await zone.ArrangeCard(cards, flag);

        zone.ResizeBackground(Mathf.Max(persons.Count, enemies.Count));
    }


    private void EndBattle()
    {
        Debug.Log("전투 종료");

        zone.OnBackgroundSizeChanged -= zoneUI.ShowZone;

        zone.OnCardEntered -= HandleCardEnter;
        zone.OnCardExited  -= HandleCardExit;
        
        Destroy(zone.gameObject);
        
        DeleteBattle?.Invoke(this);
    }
}
