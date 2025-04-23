// ✅ 리팩토링된 BattleSystem: 전투 흐름 및 카드 상태 관리 전담

using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class BattleSystem : MonoBehaviour
{
    [Header("References")]
    public BattleZone zone;
    public BattleZoneUIController zoneUI;
    public Canvas canvas;

    public List<Card> persons = new();
    public List<Card> enemies = new();

    
    public event Action<BattleSystem> OnDeleteBattle;
    
    private int prevPersonCount;
    private int prevEnemyCount;

    public async UniTask Init(Card person, Card enemy)
    {
        zone.OnBackgroundSizeChanged -= zoneUI.ShowZone;
        zone.OnBackgroundSizeChanged += zoneUI.ShowZone;

        zone.OnCardEntered += AddCard;
        zone.OnCardExited += RemoveCard;

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

    public async UniTask TryStartBattle()
    {
        if (persons.Count > 0 && enemies.Count > 0)
        {
            await UniTask.CompletedTask;
        }
    }

    public void AddCard(Card card)
    {
        if (IsCardInBattle(card)) return;

        switch (card.cardData.cardType)
        {
            case CardType.Person:
                persons.Add(card);
                break;
            case CardType.Enemy:
                enemies.Add(card);
                break;
            default:
                return;
        }

        RepositionAllCards();
    }

    public void RemoveCard(Card card)
    {
        if (persons.Remove(card) || enemies.Remove(card))
        {
            RepositionAllCards();

            if (persons.Count == 0 || enemies.Count == 0)
                EndBattle();
        }
    }

    private void RepositionAllCards()
    { 
        if (persons.Count == prevPersonCount && enemies.Count == prevEnemyCount)
            return;

        prevPersonCount = persons.Count;
        prevEnemyCount = enemies.Count;

        for (int i = 0; i < enemies.Count; i++)
            zone.ArrangeCard(enemies[i], i, true);

        for (int i = 0; i < persons.Count; i++)
            zone.ArrangeCard(persons[i], i, false);

        zone.ResizeBackground(Mathf.Max(persons.Count, enemies.Count));
    }

    private void EndBattle()
    {
        Debug.Log("🏳 전투 종료");
        
        zone.OnBackgroundSizeChanged -= zoneUI.ShowZone;

        zone.OnCardEntered -= AddCard;
        zone.OnCardExited -= RemoveCard;
        
        OnDeleteBattle?.Invoke(this);
        
        Destroy(zone.gameObject);
    }
}
