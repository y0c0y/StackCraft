using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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
    
    private bool _isInBattle;

    private readonly Random _random = new Random();
    
    public async UniTask Init(Card person, Card enemy)
    {
        zone.OnBackgroundSizeChanged -= zoneUI.ShowZone;
        zone.OnBackgroundSizeChanged += zoneUI.ShowZone;

        zone.OnCardEntered += AddCard;
        zone.OnCardExited  += TryRemoveAfterDelay;

        if (canvas.renderMode == RenderMode.WorldSpace && canvas.worldCamera == null)
            canvas.worldCamera = Camera.main;

        AddCard(person);
        AddCard(enemy);

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
            if (targets.Count == 0) 
                break;
            if (targetIdx >= targets.Count) 
                targetIdx = 0;

            var attacker  = attackers[i];
            var targetCard = targets[targetIdx];

            if (!_cardHp.TryGetValue(targetCard, out var hp))
            {
                continue;
            }

            int damage = _random.Next(0, 6);
            Debug.Log($"{attacker.name} → {targetCard.name}에게 {damage} 피해");

            hp -= damage;
            if (hp <= 0)
            {
                Debug.Log($"🟥 {targetCard.name} 파괴됨");
                HandleRemove(targetCard, true);
            }
            else
            {
                _cardHp[targetCard] = hp;
                targetIdx++;
            }

            await UniTask.Delay(300);
        }

        return targets.Count == 0;
    }

    private async UniTask TryStartBattle()
    {
        _isInBattle = true;
        var preemptiveFlag = false;

        while (true)
        {
            Debug.Log($"🕛 { (preemptiveFlag ? "적" : "아군")} 턴 시작");

            var battleOver = preemptiveFlag
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
    
    private void TryRemoveAfterDelay(Card card)
    {
        if (card == null || card.gameObject == null) return;

        if (!zone.IsInside(card.transform.position))
        {
            Debug.Log($"📤 {card.name} 실제로 전투존 이탈 → 제거");
            HandleRemove(card, false);
        }
        else
        {
            Debug.Log($"↩ {card.name} 다시 진입함 → 유지");
        }
    }
    
    public void AddCard(Card card)
    {
        if (IsCardInBattle(card)) return;
        
        var (group, isEnemy) = BattleCommon.GetCardTargetList(card, persons, enemies);
        group.Add(card);
        _cardHp[card] = 5;
        // BattleCommon.UpdateCardHpUI(card, 5);

        Debug.Log("시작");
        
        RepositionAllCards(group, isEnemy);
        
        Debug.Log("끛");

    }
    
    private void HandleRemove(Card card, bool shouldDestroy)
    {
        var (group, isEnemy) = BattleCommon.GetCardTargetList(card, persons, enemies);
        if (!group.Remove(card)) return;

        _cardHp.Remove(card);
        
        Debug.Log("시작");
        
        RepositionAllCards(group, isEnemy);
        
        Debug.Log("끛");

        if (shouldDestroy)
        {
            Debug.Log("Destroy");
            Destroy(card.gameObject);
            Destroy(card);
        }

    }
    

    private void RepositionAllCards(List<Card> cards, bool flag)
    {
        Debug.Log("하는 중");
        
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

        zone.ArrangeCard(cards, flag);

        zone.ResizeBackground(Mathf.Max(persons.Count, enemies.Count));
    }


    private void EndBattle()
    {
        Debug.Log("전투 종료");

        zone.OnBackgroundSizeChanged -= zoneUI.ShowZone;

        zone.OnCardEntered -= AddCard;
        zone.OnCardExited  -= TryRemoveAfterDelay;
        
        Destroy(zone.gameObject);
        
        DeleteBattle?.Invoke(this);
    }
}
