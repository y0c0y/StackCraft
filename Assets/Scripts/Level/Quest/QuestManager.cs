using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.SceneManagement;

public class QuestManager : MonoBehaviour
{
   public static QuestManager Instance {get; private set;}

   public GameObject questList;

   private int TotalQuestCnt {get; set;}
   private int CompletedQuestCnt {get; set;}

   public event Action<int, int> ChangeQuestProgress;
   public event Action<QuestData> ChangeQuestItemUI;

   public readonly Dictionary<string, QuestData> Quests = new();
   private readonly Dictionary<string, QuestProgress> _progresses = new();

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
      RecipeManager.Instance.OnRecipeFinished += CheckRecipe;

      BattleManager.Instance.CheckStageClear += OnCheckStageClear;
   }

   public async UniTask Init()
   {

      if (StageInfo.SelectedLevel != null)
      {
         var label = StageInfo.SelectedLevel.displayName + " Quests";
         Debug.Log(label);
        
         await LoadQuests(label);
      }
      else
      {
         await LoadQuests("Stage 1 Quests");
      }
   }

   private async UniTask LoadQuests(string stageLabel)
   {
      var handle = Addressables.LoadAssetsAsync<QuestData>(stageLabel, null);
      
      if (handle.Status == AsyncOperationStatus.Failed)
      {
         Debug.LogError($"로드 실패: {handle.OperationException} {stageLabel}");
      }
      
      var data = await handle.ToUniTask();

      for (var i = 0; i < data.Count; i++)
      {
         var quest = data[i];
         quest.idxInQuestList = i;
         Quests[quest.questID] = quest;
         _progresses[quest.questID] = new QuestProgress(quest.questID);
      } 
      
      TotalQuestCnt = Quests.Count;
      CompletedQuestCnt = 0;
      
      ChangeQuestProgress?.Invoke(TotalQuestCnt, CompletedQuestCnt);
   }
   
   public static void GameClear(bool isClear)
   {
      if (StageInfo.SelectedLevel == null)
      {
         Debug.Log("Test 중");
      }
      else
      {
         if (isClear)
         {
            var next = StageInfo.SelectedLevel.levelIndex + 1;
      
            if (next < 4)
            {
               PlayerPrefs.SetInt($"Stage_{next}", 1);
               PlayerPrefs.Save();
            }
            else
            {
               for (var i = 1; i <= 4; i++)
               {
                  PlayerPrefs.DeleteKey("Stage_" + next);
               }
            }
         }
         else
         {
            Debug.Log("클리어 실패");
         }
      }

      SceneManager.LoadScene("StageSelect");

   }
   
   private void OnCheckStageClear()
   {
      var allCards = GameTableManager.Instance.cardsOnTable;
      if (allCards == null || allCards.Count == 0) 
         return;
    
      var hasPlayer = allCards.Any(c => c.cardData.cardType == CardType.Person);
      var hasEnemy  = allCards.Any(c => c.cardData.cardType == CardType.Enemy);
    
      if (!hasPlayer)
      {
         Debug.Log("💀 게임 오버!");
         ChangeComplete(Quests[QuestInfo.GameOverQuestID]);
         return;
      }

      if (hasEnemy) return;
      
      Debug.Log("🎉 스테이지 클리어!");
      ChangeComplete(Quests[QuestInfo.GameClearQuestID]);
   }

   public bool IsCompleted(string questID)
   {
      return _progresses.TryGetValue(questID, out var progress) && progress.IsCompleted;
   }

   private void CheckRecipe(Recipe recipe)
   {
      foreach (var quest in Quests.Where(quest => quest.Value.questRecipe == recipe))
      {
         ChangeComplete(quest.Value);
      }
   }

   private void ChangeComplete(QuestData quest)
   {
      var questID = quest.questID;

      if (!_progresses.TryGetValue(questID, out var progress)) return;
      if (progress.IsCompleted) return;
      
      progress.IsCompleted = true;
      CompletedQuestCnt++;

      if (questID == QuestInfo.GameOverQuestID)
      {
         GameClear(false);
         return;
      }
      
      ChangeQuestProgress?.Invoke(TotalQuestCnt, CompletedQuestCnt);
      ChangeQuestItemUI?.Invoke(quest);
      
      if(questID ==  QuestInfo.GameClearQuestID) GameClear(true);
   }
}
