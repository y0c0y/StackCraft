using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Serialization;
using Random = System.Random;

public class BattleSystem : MonoBehaviour
{
    public static BattleSystem Instance;
    
    [Header("Prefabs")] 
    public BattleZone zone;
    public BattleEffectManager zoneUI;

    [Header("InBattleCards")]
    public List<Card> persons = new();
    public List<Card> enemies = new();

    public readonly float RemoveCardDelay = 0.2f;
    
    public event Action<BattleSystem> DeleteBattle;
    public event Action<Card, Card> AttackEffect;
    
    public event Action<Vector3, Vector3> SetCanvas;
    
    
    private Dictionary<Card, int> _cardHp = new();
    
    private readonly Random _random = new Random();
    
    public bool IsCardInBattle(Card card) => persons.Contains(card) || enemies.Contains(card);

    public bool IsEnemyNearby(Vector3 pos, float range)
    {
        return Vector3.Distance(zone.transform.position, pos) <= range;
    }

    private void Awake()
    {
        Instance = this;
        if (Instance != this)
        {
            Destroy(this);
        }
    }


    public async UniTask Init(List<Card> oriPerson, List<Card> oriEnemy)
    {
        await UniTask.WaitForFixedUpdate();
        
        
        persons.AddRange(oriPerson);
        enemies.AddRange(oriEnemy);
        
        var data = zone.ResizeBackground(enemies.Count, persons.Count);
        
        SetCanvas?.Invoke(data.Item1, data.Item2);
        
        
        InitHp();

        await TryStartBattle();
    }

    private void InitHp()
    {
        foreach (var card in persons)
        {
            _cardHp[card] = 5;
            // BattleCommon.UpdateCardHpUI(card, 5);
        }
        foreach (var card in enemies)
        {
            _cardHp[card] = 5;
            // BattleCommon.UpdateCardHpUI(card, 5);
        }
    }

    private async UniTask TryStartBattle()
    {
        await zone.ArrangeCard(persons, enemies);
        
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
        
        EndBattle(preemptiveFlag);
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
        
        var damage = _random.Next(1, 6);
        
        AttackEffect?.Invoke(attacker, targetCard);
        
        // Debug.Log($"{attacker.name} → {targetCard.name}에게 {damage} 피해");

        var hp = _cardHp[targetCard];
        hp -= damage;
        if (hp <= 0)
        {
            Debug.Log($"🟥 {targetCard.name} 파괴됨");
            await HandleRemove(targets, targetCard);
        }
        else
        {
            _cardHp[targetCard] = hp;
        }

        await UniTask.Delay(500);

        return targets.Count == 0;
    }

    
    private async UniTask HandleRemove(List<Card> group, Card card)
    {
        if (!group.Remove(card)) return;

        Debug.Log($"{card.cardData.cardType}");

        _cardHp.Remove(card);

        Debug.Log("Destroy");
        
        await UniTask.Delay(500);
        
        
        Destroy(card.gameObject);
        Destroy(card.owningStack.gameObject);
        Destroy(card);

    }
    
    public void RestoreCardComponents(List<Card> list, bool isEnemy)
    {
        foreach (var card in list)
        {
            if (card == null) continue;
            card.GetComponent<CardDrag>().enabled = true;
            card.owningStack.GetComponent<StackRepulsion>().enabled = true;

            // card.cardData.cardType = isEnemy ? CardType.Enemy : CardType.Person;

        }
    } 

    private void EndBattle(bool isEnemy)
    {
        Debug.Log("전투 종료");
        
        var list = isEnemy ? enemies : persons;
        RestoreCardComponents(list, isEnemy);
        
        DeleteBattle?.Invoke(this);
    }
}
