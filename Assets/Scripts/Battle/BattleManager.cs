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
    
    public static Dictionary<Card, BattleAbility> BattleAbilities;
    public static Dictionary<Card, CardBattleUIController> UIMap;

    public event Action CheckStageClear;
    
    private void Awake()
    {
        Instance = this;
        if (Instance != this)
        {
            Destroy(this);
        }
        
        BattleAbilities = new Dictionary<Card, BattleAbility>();
        UIMap = new Dictionary<Card, CardBattleUIController>();
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

    private void OnCardAddedToTable(Card card)
    {
        if (!IsValidCardType(card)) return;
        
        if (!BattleAbilities.ContainsKey(card))
        {
            var ability = BattleAbility.FindAbility(card.cardData);
            BattleAbilities.Add(card, ability);
            
            var ui = card.GetComponentInChildren<CardBattleUIController>();
            ui.Init(card, ability);
            UIMap[card] = ui;
        }

        var drag = card.GetComponent<CardDrag>();
        if (drag != null)
        {
            drag.CardDragEnded -= OnCardDragEnded;
            drag.CardDragEnded += OnCardDragEnded;
        }
    }

    private void OnCardRemovedFromTable(Card card)
    {
        if (!IsValidCardType(card)) return;
        
        BattleAbilities.Remove(card);
        UIMap.Remove(card);
        
        var drag = card.GetComponent<CardDrag>();
        if (drag != null)
        {
            drag.CardDragEnded -= OnCardDragEnded;
        }
    }

    private bool Flag(Card c) => battleSystems.Any(bs => bs.IsCardInBattle(c));

    private static bool IsValidCardType(Card card)
    {
        return card != null && 
               card.cardData.cardType is CardType.Person or CardType.Enemy;
    }

    private void OnCardDragEnded(Card card)
    {
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
            
            Debug.Log($"Card {other.name}");

            var me = card.cardData.cardType;
            var you = other.cardData.cardType;
            if (me == you) continue;

            switch (me)
            {
                case CardType.Person:
                    TryEngageBattle(card.owningStack, other.owningStack).Forget();
                    break;
                case CardType.Enemy:
                    TryEngageBattle(other.owningStack, card.owningStack).Forget();
                    break;
            }

            return;
        }
    }

    
    private async UniTask<List<Card>> SplitStack(Stack stack)
    {
        var separatedCards = new List<Card>();

        stack.GetComponent<StackRepulsion>().enabled = false;

        foreach (var oldCard in stack.cards.ToList())
        {
            var slow = oldCard.GetComponent<SlowParentConstraint>();
            var drag = oldCard.GetComponent<CardDrag>();
            var rig = oldCard.GetComponent<Rigidbody2D>();
            
            if (slow != null)
            {
                slow.enabled = false;
                slow.target = null;
            }
            
            if (drag != null) drag.enabled = false;
            
            // if(rig != null) rig
            
                            
            stack.RemoveCard(oldCard);
            
            var newStack =  StackManager.Instance.AddNewStack();
            
            newStack.AddCard(oldCard);

            await UniTask.WaitForEndOfFrame();
            
            var sr = newStack.GetComponent<StackRepulsion>();
            if (sr != null)
            {
                Debug.Log("Stack Repulsion");
                sr.enabled = false;
            }
            
            separatedCards.Add(oldCard);
        }

        GameTableManager.Instance.RemoveStackFromTable(stack);

        Debug.Log($"SplitStack 완료 : {separatedCards.Count}개 카드 추출");
        return separatedCards;
    }

    private async UniTaskVoid TryEngageBattle(Stack personStack, Stack enemyStack)
    {
        await UniTask.WaitForFixedUpdate();
        
        var personsForBattle = await SplitStack(personStack);
        var enemiesForBattle = await SplitStack(enemyStack);
        
        Debug.Log($"{personsForBattle.Count}");
        
        Debug.Log("새 전투 생성");

        var center = new Vector3();
        if (enemiesForBattle[0] != null) center = enemiesForBattle[0].transform.position;
        
        var battleSystemObj = Instantiate(battleSystemPrefab, center, Quaternion.identity);
        var battleSystem = battleSystemObj.GetComponent<BattleSystem>();
        battleSystemObj.transform.SetParent(transform);
        
        battleSystem.DeleteBattle += OnDeleteBattle;
        battleSystems.Add(battleSystem);
        

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