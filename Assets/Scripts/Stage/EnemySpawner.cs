using AYellowpaper.SerializedCollections;
using Cysharp.Threading.Tasks;
using Unity.Cinemachine;
using UnityEngine;

public class EnemySpawner: MonoBehaviour
{
    [SerializeField] private SerializedDictionary<CardData, int> enemyCardsToSpawn;
    [SerializeField] private Transform enemySpawnPoint;
    [SerializeField] private CinemachineCamera zoomInCamera;
    [SerializeField] private float waitSeconds = 5f;

    public void ZoomInAndSpawnEnemy()
    {
        _ = ZoomInUniTask();
    }

    private async UniTaskVoid ZoomInUniTask()
    {
        SpawnEnemy();
        
        TimeManager.Instance.PauseTime();
        zoomInCamera.gameObject.SetActive(true);
        await UniTask.Delay((int)(waitSeconds * 1000), true);
        zoomInCamera.gameObject.SetActive(false);
        await UniTask.Delay((int)(waitSeconds * 1000), true);
        TimeManager.Instance.ResumeTime();
    }
    
    public void SpawnEnemy()
    {
        foreach (var kv in enemyCardsToSpawn)
        {
            for (int i = 0; i < kv.Value; i++)
            {
                var position = enemySpawnPoint.position + new Vector3(Random.Range(-4f, 4f), Random.Range(-4f, 4f), 0);
                GameTableManager.Instance.AddNewCardToTable(kv.Key, position);
            }
        }
    }
}