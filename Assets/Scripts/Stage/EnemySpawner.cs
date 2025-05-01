using System.Linq;
using AYellowpaper.SerializedCollections;
using Cysharp.Threading.Tasks;
using Unity.Cinemachine;
using UnityEngine;

public class EnemySpawner: MonoBehaviour
{
    [SerializedDictionary("Card type to spawn", "Amount")]
    [SerializeField] private SerializedDictionary<CardData, int> enemyCardsToSpawn;
    [SerializeField] private Transform enemySpawnPoint;
    [SerializeField] private BoxCollider2D enemySpawnArea;
    [SerializeField] private CinemachineCamera zoomInCamera;
    [SerializeField] private float waitSeconds = 5f;

    public void ZoomInAndSpawnEnemy(bool shouldStack)
    {
        _ = ZoomInUniTask(shouldStack);
    }

    private async UniTaskVoid ZoomInUniTask(bool shouldStack)
    {
        SpawnEnemyInPosition(shouldStack);
        
        TimeManager.Instance.PauseTime();
        zoomInCamera.gameObject.SetActive(true);
        await UniTask.Delay((int)(waitSeconds * 1000), true);
        zoomInCamera.gameObject.SetActive(false);
        await UniTask.Delay((int)(waitSeconds * 1000), true);
        TimeManager.Instance.ResumeTime();
    }
    
    public void SpawnEnemyInPosition(bool shouldStack)
    {
        _ = SpawnTask(enemySpawnPoint.position + new Vector3(Random.Range(-4f, 4f), Random.Range(-4f, 4f), 0f), shouldStack);
    }
    
    public void SpawnEnemyInArea(bool shouldStack)
    {
        _ = SpawnTask(GetRandomPointInsideCollider(enemySpawnArea), shouldStack);
    }

    private async UniTaskVoid SpawnTask(Vector3 position, bool shouldStack)
    {
        foreach (var kv in enemyCardsToSpawn)
        {
            for (int i = 0; i < kv.Value; i++)
            {
                var newCard = GameTableManager.Instance.AddNewCardToTable(kv.Key, position);

                if (!shouldStack) continue;
                
                await UniTask.WaitUntil(() => newCard.owningStack != null);
                var stack = newCard.owningStack.GetRandomStackFromSameField();
                Debug.Log(stack);
                if (stack != null)
                {
                    StackManager.Instance.AddCardToStack(newCard, stack);
                }
            }
        }
    }
    
    public Vector2 GetRandomPointInsideCollider(BoxCollider2D boxCollider2D)
    {
        Vector2 colliderWorldPivot = (Vector2)boxCollider2D.transform.position + (boxCollider2D.offset * boxCollider2D.transform.lossyScale); 

        float colliderWidth = boxCollider2D.size.x * boxCollider2D.transform.lossyScale.x;
        float colliderHeight = boxCollider2D.size.y * boxCollider2D.transform.lossyScale.y;

        float randomPosX = Random.Range(colliderWorldPivot.x - colliderWidth / 2, colliderWorldPivot.x + colliderWidth / 2);
        float randomPosY = Random.Range(colliderWorldPivot.y - colliderHeight / 2, colliderWorldPivot.y + colliderHeight / 2);

        return new Vector2 (randomPosX, randomPosY);
    }
}