using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class BattleManager : MonoBehaviour
{
    public event Action<BattleSystem> BattleFinished;
    public static BattleManager Instance;
    
    public GameObject battleSystemPrefab;
    public List<BattleSystem> battleSystems = new();

    public event Action CheckStageClear;
    
    [SerializeField] private GameObject cardBattlePrefab;
    
    private Collider2D[] _results = new Collider2D[5];
    
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

    private void OnCardAddedToTable(Card card)
    {
        if (!IsValidCardType(card)) return;
        
        var cb = card.GetComponentInChildren<CardBattle>();
        if (cb != null) return;
        
        var go = Instantiate(cardBattlePrefab, card.transform);
        cb = go.GetComponent<CardBattle>();
        cb.Setup(card);
    }

    private void OnCardRemovedFromTable(Card card)
    {
        if (!IsValidCardType(card)) return;
       if(card.TryGetComponent<CardBattle>(out var cb)) Destroy(cb.gameObject);
        
    }

    public bool Flag(Card c) => battleSystems.Any(bs => bs.IsCardInBattle(c));

    public bool IsValidCardType(Card card)
    {
        return card != null && 
               card.cardData.cardType is CardType.Person or CardType.Enemy;
    }
    
    private async UniTask<List<Card>> SplitStack(Stack stack)
    {
        var separatedCards = new List<Card>();

        if (stack.GetComponent<StackRepulsion>() is { } stackRepulsion)
        {
            stackRepulsion.enabled = false;
        }

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

    public async UniTaskVoid TryEngageBattle(Stack personStack, Stack enemyStack)
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
            BattleFinished?.Invoke(battleSystem);
            battleSystem.DeleteBattle -= OnDeleteBattle;
            battleSystems.Remove(battleSystem);
        
            Destroy(battleSystem.gameObject);
        }

        CheckStageClear?.Invoke();
    }
    
}