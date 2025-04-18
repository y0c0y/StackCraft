using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.SceneManagement;
using static QuestUIController;

public class QuestManager : MonoBehaviour
{
   public static QuestManager Instance {get; private set;}

   public GameObject questList;

   public int TotalQuestCnt {get; private set;}
   public int CompletedQuestCnt {get; private set;}

   public event Action OnGoalQuestOpen;

   public readonly Dictionary<string, QuestData> Quests = new();
   public readonly Dictionary<string, QuestProgress> Progresses = new();

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
      OnGoalQuestOpen += QuestUIController.Instance.OpenTheGoal;
   }

   public async UniTask Init()
   {
      var label = StageInfo.SelectedLevel.displayName + " Quests";
      Debug.Log(label);
      var check = LoadQuests(label);
      
      await LoadQuests(label);
      
      // await LoadQuests("Stage 1 Quests");

   }

   private async UniTask LoadQuests(string stageLabel)
   {
      var handle = Addressables.LoadAssetsAsync<QuestData>(stageLabel, null);
      
      if (handle.Status == AsyncOperationStatus.Failed)
      {
         Debug.LogError($"로드 실패: {handle.OperationException} {stageLabel}");
      }
      
      var data = await handle.ToUniTask();

      foreach (var d in data)
      {
         Quests[d.questID] = d;
         Progresses[d.questID] = new QuestProgress(d.questID);
      }
      
      TotalQuestCnt = Quests.Count;
      CompletedQuestCnt = 0;

      Debug.Log($"총 퀘스트 {TotalQuestCnt}개 로드 완료");
   }

   public void CompleteQuest(string questID)
   {
      if (!Progresses.TryGetValue(questID, out var progress)) return;
      if (progress.IsCompleted) return;
      
      progress.IsCompleted = true;
      CompletedQuestCnt++;

      switch (questID)
      {
         case QuestInfo.GoalOpenQuestID:
            OnGoalQuestOpen?.Invoke();
            break;
         case QuestInfo.GameClearQuestID:
            GameClear();
            break;
      }
   }

   public static void GameClear()
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

      SceneManager.LoadScene("StageSelect");

   }

   public bool IsCompleted(string questID)
   {
      return Progresses.TryGetValue(questID, out var progress) && progress.IsCompleted;
   }

   
}
