using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class BattleManager : MonoBehaviour
{
    public GameObject battleSystemPrefab;
    public List<BattleSystem> battleSystems = new();

    private void Start()
    {
        GameTableManager.Instance.CardAddedOnTable += RegisterCard;
        GameTableManager.Instance.CardRemovedFromTable += UnregisterCard;
    }

    private void OnDestroy()
    {
        GameTableManager.Instance.CardAddedOnTable -= RegisterCard;
        GameTableManager.Instance.CardRemovedFromTable -= UnregisterCard;
    }

    private void RegisterCard(Card card)
    {
        var drag = card.GetComponent<CardDrag>();
        if (drag != null)
        {
            drag.CardDragEnded -= OnCardDragEnded;
            drag.CardDragEnded += OnCardDragEnded;
        }
    }

    private void UnregisterCard(Card card)
    {
        var drag = card.GetComponent<CardDrag>();
        if (drag != null)
        {
            drag.CardDragEnded -= OnCardDragEnded;
        }
    }

    private void OnCardDragEnded(Card card)
    {
        if (card.cardData.cardType != CardType.Person) return;
        
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
        var center = (person.transform.position + enemy.transform.position) / 2f;
        
        var existingBattle = battleSystems.FirstOrDefault(bs =>
            bs.IsCardInBattle(person) ||
            bs.IsCardInBattle(enemy) ||
            bs.IsEnemyNearby(center, 1.5f));
        
        if (existingBattle != null)
        {
            if (!existingBattle.IsCardInBattle(person)) existingBattle.AddCard(person);
            if (!existingBattle.IsCardInBattle(enemy)) existingBattle.AddCard(enemy);
            return;
        }

        Debug.Log("[⚔ 새 전투 생성]");

        var battleSystemObj = Instantiate(battleSystemPrefab, center, Quaternion.identity);
        var battleSystem = battleSystemObj.GetComponent<BattleSystem>();
        battleSystemObj.transform.SetParent(transform);
        
        battleSystem.OnDeleteBattle += DeleteBattle;
        
        await battleSystem.Init(person, enemy);
        battleSystems.Add(battleSystem);
    }

    private void DeleteBattle(BattleSystem battleSystem)
    {
        Destroy(battleSystem.gameObject);
        battleSystem.OnDeleteBattle -= DeleteBattle;
       battleSystems.Remove(battleSystem);
    }
    
}