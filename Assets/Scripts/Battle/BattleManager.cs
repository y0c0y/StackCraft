using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class BattleManager : MonoBehaviour
{
    public static BattleManager Instance;
    
    public GameObject battleSystemPrefab;
    public List<BattleSystem> battleSystems = new();

    public event Action CheckStageClear;
    
    private void Awake()
    {
        Instance = this;
        if (Instance != this)
        {
            Destroy(this);
        }
    }
    
    
    private void Start()
    {
        GameTableManager.Instance.CardAddedOnTable +=  OnCardAddedToTable;
        GameTableManager.Instance.CardRemovedFromTable += OnCardRemovedFromTable;
    }

    private void OnDestroy()
    {
        GameTableManager.Instance.CardAddedOnTable -= OnCardAddedToTable;
        GameTableManager.Instance.CardRemovedFromTable -= OnCardRemovedFromTable;
    }

    private void  OnCardAddedToTable(Card card)
    {
        var drag = card.GetComponent<CardDrag>();
        if (drag != null)
        {
            drag.CardDragEnded -= OnCardDragEnded;
            drag.CardDragEnded += OnCardDragEnded;
        }
    }

    private void OnCardRemovedFromTable(Card card)
    {
        var drag = card.GetComponent<CardDrag>();
        if (drag != null)
        {
            drag.CardDragEnded -= OnCardDragEnded;
        }
    }

    private void OnCardDragEnded(Card card)
    {
        if (!BattleCommon.IsValidCardType(card)) return;
        if (battleSystems.Any(bs => bs.IsCardInBattle(card)))
            return;
        
        var results = new Collider2D[5];
        var filter = new ContactFilter2D().NoFilter();
        var count = Physics2D.OverlapCollider(card.GetComponent<Collider2D>(), filter, results);

        for (int i = 0; i < count; i++)
        {
            var enemy = results[i].GetComponent<Card>();
            if (enemy == null || enemy.cardData.cardType != CardType.None) continue;

            TryEngageBattle(card, enemy).Forget();
            return;
        }
    }

    private async UniTaskVoid TryEngageBattle(Card person, Card enemy)
    {
        
        await UniTask.NextFrame();

        
        var center = enemy.transform.position;
        
        var existingBattle = battleSystems.FirstOrDefault(bs =>
            bs.IsCardInBattle(person) ||
            bs.IsCardInBattle(enemy) ||
            bs.IsEnemyNearby(center, 1.5f));
        
        if (existingBattle != null)
        {
            if (!existingBattle.IsCardInBattle(person)) await existingBattle.AddCard(person);
            if (!existingBattle.IsCardInBattle(enemy)) await existingBattle.AddCard(enemy);
            return;
        }

        Debug.Log("새 전투 생성");

        var battleSystemObj = Instantiate(battleSystemPrefab, center, Quaternion.identity);
        var battleSystem = battleSystemObj.GetComponent<BattleSystem>();
        battleSystemObj.transform.SetParent(transform);
        
        battleSystem.DeleteBattle += OnDeleteBattle;
        battleSystems.Add(battleSystem);
        
        await battleSystem.Init(person, enemy);
        
    }

    private void OnDeleteBattle(BattleSystem battleSystem)
    {
        battleSystem.DeleteBattle -= OnDeleteBattle;
        battleSystems.Remove(battleSystem);
        
        Destroy(battleSystem.gameObject);
        
        CheckStageClear?.Invoke();
        
        
    }
    
}