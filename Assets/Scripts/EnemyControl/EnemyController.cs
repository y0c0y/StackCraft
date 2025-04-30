using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityTimer;
using Random = UnityEngine.Random;

public class EnemyController: MonoBehaviour
{
    public static EnemyController Instance;
    
    public event Action EnemyInvaded;
    
    public UnityEvent spawnEnemyEvent;
    public float spawnEnemyDelayMin = 15f;
    public float spawnEnemyDelayMax = 30f;
    public int enemyNeededToInvade = 5;
    public Transform enemyInvasionPosition;

    private List<Card> _enemiesInField = new();
    private Timer _timer;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(Instance.gameObject);
        }
        Instance = this;
    }

    private void Start()
    {
        _timer = this.AttachTimer(
            duration: Random.Range(spawnEnemyDelayMin, spawnEnemyDelayMax),
            onComplete: OnSpawnTimerComplete);
    }

    private void OnSpawnTimerComplete()
    {
        spawnEnemyEvent?.Invoke();

        if (CheckIfCanInvade())
        {
            Invade();
        }
        
        _timer = this.AttachTimer(
            duration: Random.Range(spawnEnemyDelayMin, spawnEnemyDelayMax),
            onComplete: OnSpawnTimerComplete);
    }

    private bool CheckIfCanInvade()
    {
        bool canInvade = false;
        var enemyFieldCards = GameTableManager.Instance.GetAllCardsInField(GameTableManager.FieldType.EnemyField);
        _enemiesInField = enemyFieldCards.Where(c => c.cardData.cardType == CardType.Enemy).ToList();

        canInvade = _enemiesInField.Count >= enemyNeededToInvade;
        return canInvade;
    }
    
    private void Invade()
    {
        _enemiesInField.RemoveRange(enemyNeededToInvade, _enemiesInField.Count - enemyNeededToInvade);
        GameTableManager.MoveCardsToField(GameTableManager.FieldType.PlayerField, _enemiesInField, enemyInvasionPosition.position);
        EnemyInvaded?.Invoke();
    }
}