using System;
using System.Linq;
using AYellowpaper.SerializedCollections;
using UnityEngine;
using UnityEngine.Events;

public class StageScenarioManager: MonoBehaviour
{
    [SerializedDictionary] public SerializedDictionary<QuestData, UnityEvent> questEvents;

    private void Start()
    {
        QuestManager.Instance.QuestCompleted += OnQuestCompleted;
    }

    private void OnQuestCompleted(QuestData questData)
    {
        var questKv = questEvents.FirstOrDefault(x => x.Key.questID == questData.questID);
        if (questKv.Key != null)
        {
            questKv.Value?.Invoke();
        }
    }
}