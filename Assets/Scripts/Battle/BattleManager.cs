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
        if (!BattleCommon.IsValidCardType(card)) 
            return;
    
        var results = new Collider2D[5];
        var filter  = new ContactFilter2D().NoFilter();
        var count   = Physics2D.OverlapCollider(card.GetComponent<Collider2D>(), filter, results);

        for (int i = 0; i < count; i++)
        {
            var other = results[i].GetComponent<Card>();
            if (other == null) 
                continue;
            if (!BattleCommon.IsValidCardType(other)) 
                continue;

            var me   = card.cardData.cardType;
            var you  = other.cardData.cardType;
            if (me == you) continue;  
            
            if (battleSystems.Any(bs => bs.IsCardInBattle(card)))
                return;

            TryEngageBattle(card.owningStack, other.owningStack).Forget();
            return;
        }
    }


    private async UniTaskVoid TryEngageBattle(Stack persons, Stack enemies)
    {
        var center = enemies.TopCard.transform.position * 0.5f;

        Debug.Log("새 전투 생성");

        var battleSystemObj = Instantiate(battleSystemPrefab, center, Quaternion.identity);
        var battleSystem = battleSystemObj.GetComponent<BattleSystem>();
        battleSystemObj.transform.SetParent(transform);
        
        battleSystem.DeleteBattle += OnDeleteBattle;
        battleSystems.Add(battleSystem);
        
        await battleSystem.Init(persons, enemies);
    }

    private void OnDeleteBattle(BattleSystem battleSystem)
    {
        if (battleSystem != null)
        {
            battleSystem.DeleteBattle -= OnDeleteBattle;
            battleSystems.Remove(battleSystem);
        
            Destroy(battleSystem.gameObject);
        }

        CheckStageClear?.Invoke();
    }
    
}