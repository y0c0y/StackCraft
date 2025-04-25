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
    
    private Dictionary<Card, int> _cardHp = new();
    
    private bool _isInBattle;

    private readonly Random _random = new Random();
    
    public async UniTask Init(Stack person, Stack enemy)// 여기가 스택이 되어야 하네
    {
        zone.OnBackgroundSizeChanged -= zoneUI.ShowZone;
        zone.OnBackgroundSizeChanged += zoneUI.ShowZone;

        if (canvas.renderMode == RenderMode.WorldSpace && canvas.worldCamera == null)
            canvas.worldCamera = Camera.main;

        AddStack(person);
        AddStack(enemy);

        await TryStartBattle();
    }

    public bool IsCardInBattle(Card card) => persons.Contains(card) || enemies.Contains(card);

    public bool IsEnemyNearby(Vector3 pos, float range)
    {
        return Vector3.Distance(zone.transform.position, pos) <= range;
    }
    
    private async UniTask<bool> Attack(List<Card> attackers, List<Card> targets)
    {
        var attackerIdx = _random.Next(0, attackers.Count);
        var targetIdx = _random.Next(0, targets.Count);

        if (attackerIdx >= attackers.Count || targetIdx >= targets.Count)
        {
            attackerIdx = 0;
            targetIdx = 0;
        }
        
        var attacker  = attackers[attackerIdx];
        var targetCard = targets[targetIdx];
        
        var damage = _random.Next(0, 6);
        Debug.Log($"{attacker.name} → {targetCard.name}에게 {damage} 피해");

        int hp = _cardHp[targetCard];
        
        
        hp -= damage;
        if (hp <= 0)
        {
            Debug.Log($"🟥 {targetCard.name} 파괴됨");
            HandleRemove(targetCard, true);
        }
        else
        {
            _cardHp[targetCard] = hp;
        }

        await UniTask.Delay(500);

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
        
        await UniTask.Delay(500);
        
        EndBattle();
    }
    
    public void AddStack(Stack stack)
    {
        if (IsCardInBattle(card)) return;
        
        var (group, isEnemy) = BattleCommon.GetCardTargetList(card, persons, enemies);
        group.Add(card);
        _cardHp[card] = 5;
        // BattleCommon.UpdateCardHpUI(card, 5);

    }
    
    private void HandleRemove(Card card, bool shouldDestroy)
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

    }

    private void EndBattle()
    {
        Debug.Log("전투 종료");

        zone.OnBackgroundSizeChanged -= zoneUI.ShowZone;
        
        DeleteBattle?.Invoke(this);
    }
}
