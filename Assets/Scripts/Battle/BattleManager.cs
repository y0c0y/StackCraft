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

    private bool IsValidCardType(Card card)
    {
        return card != null && 
               card.cardData.cardType is CardType.Person or CardType.Enemy;
    }

    private void OnCardDragEnded(Card card)
    {
        if (!IsValidCardType(card)) 
            return;
        if (Flag(card)) return;

        var results = new Collider2D[5];
        var filter = new ContactFilter2D().NoFilter();
        var count = Physics2D.OverlapCollider(card.GetComponent<Collider2D>(), filter, results);

        for (var i = 0; i < count; i++)
        {
            var other = results[i].GetComponent<Card>();
            if (other == null) continue;
            if (!IsValidCardType(other)) continue;
            if (Flag(other)) continue;

            var me = card.cardData.cardType;
            var you = other.cardData.cardType;
            if (me == you) continue;

            if (me == CardType.Person)
            {
                TryEngageBattle(card.owningStack, other.owningStack).Forget();
            }
            else
            {
                TryEngageBattle(other.owningStack, card.owningStack).Forget();
            }

            return;
        }
    }

    
    private List<Card> SplitStack(Stack stack)
    {
        var separatedCards = new List<Card>();

        foreach (var oldCard in stack.cards.ToList())
        {
            var slow = oldCard.GetComponent<SlowParentConstraint>();
            var drag = oldCard.GetComponent<CardDrag>();
            
            if (slow != null)
            {
                slow.enabled = false;
                slow.target = null;
            }
            
            if (drag != null) drag.enabled = false;
                            
            stack.RemoveCard(oldCard);
            
            var newStack =  StackManager.Instance.AddNewStack();
            
            newStack.AddCard(oldCard);
            
            separatedCards.Add(oldCard);
        }

        GameTableManager.Instance.RemoveStackFromTable(stack);

        Debug.Log($"SplitStack 완료 : {separatedCards.Count}개 카드 추출");
        return separatedCards;
    }

    private async UniTaskVoid TryEngageBattle(Stack personStack, Stack enemyStack)
    {
        var center = enemyStack.TopCard.transform.position * 0.5f;

        Debug.Log("새 전투 생성");

        var battleSystemObj = Instantiate(battleSystemPrefab, center, Quaternion.identity);
        var battleSystem = battleSystemObj.GetComponent<BattleSystem>();
        battleSystemObj.transform.SetParent(transform);
        
        battleSystem.DeleteBattle += OnDeleteBattle;
        battleSystems.Add(battleSystem);
        
        var personsForBattle = SplitStack(personStack);
        var enemiesForBattle = SplitStack(enemyStack);

        await battleSystem.Init(personsForBattle, enemiesForBattle);
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