using UnityEngine;

public class Stage1Scenario: MonoBehaviour
{
    [SerializeField] private CardData enemyCard;
    [SerializeField] private Transform enemySpawnPoint;
    
    public void SpawnEnemy()
    {
        if (enemyCard == null)
        {
            Debug.LogError("Enemy card is not set.");
            return;
        }

        GameTableManager.Instance.AddNewCardToTable(enemyCard, enemySpawnPoint.position);
    }
}