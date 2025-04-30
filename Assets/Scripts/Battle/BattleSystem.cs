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
    public BattleZoneUI zoneUI;

    [Header("InBattleCards")]
    public List<Card> persons = new();
    public List<Card> enemies = new();

    public readonly float RemoveCardDelay = 0.2f;
    
    public event Action<BattleSystem> DeleteBattle;
    
    public event Action<Vector3, Vector3> SetCanvas;
    public event Action<Card, Card> CreateAttackEffect;


    public bool preemptiveFlag;
    
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
        
        preemptiveFlag = false;// 이거 수정해야함. (적 필드일 때, 나의 필드 일때)
    }
    
    public async UniTask Init(List<Card> oriPerson, List<Card> oriEnemy)
    {
        await UniTask.WaitForFixedUpdate();
        
        persons.AddRange(oriPerson);
        enemies.AddRange(oriEnemy);
        
        var data = zone.ResizeBackground(enemies.Count, persons.Count);
        
        SetCanvas?.Invoke(data.Item1, data.Item2);

        await TryStartBattle();
    }
    
    private async UniTask TryStartBattle()
    {
        await zone.ArrangeCard(persons, enemies);
        
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
        
        await UniTask.Delay(300);
        
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
        var target = targets[targetIdx];
        
        var attackerCb = attacker.GetComponentInChildren<CardBattle>();
        var targetCb = target.GetComponentInChildren<CardBattle>();
        
        if(attackerCb == null || targetCb == null) return false;
        
        var damage = attackerCb.ability.TotalDamage;
        CreateAttackEffect?.Invoke(attacker, target);

        targetCb.ability.CurrentHp -= damage;
        
        if (  targetCb.ability.CurrentHp <= 0)
        {
            Debug.Log($"🟥 {target.name} 파괴됨");
            await HandleRemove(targets, target);
        }
        else
        {
            targetCb.battleUI.ChangeHpText(targetCb.ability.CurrentHp);
            await UniTask.Delay(300);

        }

        await UniTask.Delay(500);
        
        return targets.Count == 0;
    }

    
    private async UniTask HandleRemove(List<Card> group, Card card)
    {
        if (!group.Remove(card)) return;

        Debug.Log($"{card.cardData.cardType}");
        
        Debug.Log("Destroy");
        
        await UniTask.Delay(500);
        
        Destroy(card.gameObject);
        Destroy(card.owningStack.gameObject);
        Destroy(card);

    }

    private void RestoreCardComponents(List<Card> list, bool isEnemy)
    {
        foreach (var card in list)
        {
            if (card == null) continue;
            card.GetComponent<CardDrag>().enabled = true;
            card.owningStack.GetComponent<StackRepulsion>().enabled = true;
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
