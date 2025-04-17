using System;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;

public class QuestUIController : MonoBehaviour
{
    
    public static QuestUIController Instance;
    
    [SerializeField] private GameObject togglePrefab;
    [SerializeField] private Transform questListParent;
    
    [SerializeField] private TMP_Text progressText;
    
    
    private Transform _hideQuest;
    private QuestData _hideQuestData;
    
    
    private void Awake()
    {
        Instance = this;
        if (Instance != this)
        {
            Destroy(this);
        }
    }

    public void OpenTheGoal()
    {
        var item = questListParent.GetChild(questListParent.childCount - 1);
        var tmp = item.gameObject.GetComponent<QuestItem>();
        tmp.ShowGoal(_hideQuestData);
    }

    public void ChangeQuestProgress(int total, int completed)
    {
        progressText.text = $"({completed}/{total})";
    }
    
    
    public async UniTask LoadQuestsUI()
    {
        await QuestManager.Instance.Init();
        
        var quests = QuestManager.Instance.Quests;
        var flag = false;

        foreach (var quest in quests)
        {
            var go = Instantiate(togglePrefab, questListParent);
            var questUI = go.GetComponent<QuestItem>();
            
            if(quest.Key == QuestInfo.GoalOpenQuestID) flag = true;
            questUI.Init(quest.Value);
            if (flag)
            {
                if (quest.Key == QuestInfo.GameClearQuestID)
                {
                    _hideQuestData = quest.Value;
                    questUI.HideGoal(quest.Value);
                }
            }
            
            Debug.Log($"Loaded: {quest.Value.description}");
        }
        
        Canvas.ForceUpdateCanvases(); 
    }
}