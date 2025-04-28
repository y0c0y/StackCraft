using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Unity.VisualScripting;
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

    private bool Flag(Card c) => battleSystems.Any(bs => bs.IsCardInBattle(c));

    private void OnCardDragEnded(Card card)
    {
        if (!BattleCommon.IsValidCardType(card)) 
            return;
    
        var results = new Collider2D[5];
        var filter  = new ContactFilter2D().NoFilter();
        var count   = Physics2D.OverlapCollider(card.GetComponent<Collider2D>(), filter, results);

        for (var i = 0; i < count; i++)
        {
            var other = results[i].GetComponent<Card>();
            if (other == null) 
                continue;
            if (!BattleCommon.IsValidCardType(other)) 
                continue;

            if (Flag(card)) return;
            
            
            var me   = card.cardData.cardType;
            var you  = other.cardData.cardType;
            if (me == you) continue;

            if (me == CardType.Person)
            {
                TryEngageBattle(card.owningStack, other.owningStack).Forget();
            }
            else
            {
                TryEngageBattle(other.owningStack, card.owningStack).Forget();
            }
        }
    }


    private async UniTaskVoid TryEngageBattle(Card person, Card enemy)
    {
        await UniTask.Yield(PlayerLoopTiming.Update);
        
        var center = enemy.transform.position;
        
        var existingBattle = battleSystems
            .FirstOrDefault(bs => bs.IsCardInBattle(person) || bs.IsCardInBattle(enemy));

        if (existingBattle == null)
        {
            existingBattle = battleSystems
                .FirstOrDefault(bs => bs.IsEnemyNearby(center, 1.5f));
        }

        Debug.Log("새 전투 생성");

        var battleSystemObj = Instantiate(battleSystemPrefab, center, Quaternion.identity);
        var battleSystem = battleSystemObj.GetComponent<BattleSystem>();
        battleSystemObj.transform.SetParent(transform);
        
        battleSystem.DeleteBattle += OnDeleteBattle;
        battleSystems.Add(battleSystem);
        
        await battleSystem.Init(persons.cards, enemies.cards);
    }

    private void OnDeleteBattle(BattleSystem battleSystem)
    {
        battleSystem.DeleteBattle -= OnDeleteBattle;
        battleSystems.Remove(battleSystem);
        
        Destroy(battleSystem.gameObject);
        
        CheckStageClear?.Invoke();
    }
    
}