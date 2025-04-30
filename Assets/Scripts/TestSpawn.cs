using UnityEngine;

public class TestSpawn : MonoBehaviour
{
    [SerializeField] private CardData[] allyCardDatas;
    [SerializeField] private CardData[] enemyCardDatas;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            GameTableManager.Instance.AddNewCardToTable(allyCardDatas[Random.Range(0, allyCardDatas.Length)], Vector3.zero);
        }
        if (Input.GetKeyDown(KeyCode.E))
        {
            GameTableManager.Instance.AddNewCardToTable(enemyCardDatas[Random.Range(0, enemyCardDatas.Length)], Vector3.zero);
        }
    }
}
