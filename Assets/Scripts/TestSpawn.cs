using System.Linq;
using UnityEngine;

public class TestSpawn : MonoBehaviour
{
    [SerializeField] private CardData[] allyCardDatas;
    [SerializeField] private CardData[] enemyCardDatas;
    
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            GameTableManager.Instance.AddNewCardToTable(allyCardDatas[Random.Range(0, allyCardDatas.Length)], Vector3.zero);
        }
        if (Input.GetKeyDown(KeyCode.E))
        {
            var newCard = GameTableManager.Instance.AddNewCardToTable(enemyCardDatas[Random.Range(0, enemyCardDatas.Length)],
                Vector3.zero);
            
            var stack = GameTableManager.Instance.GetAllStacksInField(GameTableManager.FieldType.PlayerField)
                .Where(stack => stack != newCard.owningStack &&
                                !BattleManager.Instance.Flag(stack.TopCard) &&
                                newCard.CanStackOn(stack.LastCard))
                .ToList()
                .Random();
            if (stack != null)
            {
                StackManager.Instance.AddCardToStack(newCard, stack);
            }
        }
    }
}
