using System;
using System.Linq;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;

public class QuestUIController : MonoBehaviour
{
    public static QuestUIController Instance;
    
    [SerializeField] private GameObject togglePrefab;
    [SerializeField] private Transform questListParent;
    [SerializeField] private TMP_Text progressText;
    
    private int _hideQuestIdx; 
    private string _hideQuestDescription;
    
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
        QuestManager.Instance.QuestProgressChanged += OnQuestProgressChanged;
        QuestManager.Instance.QuestCompleted += OnQuestCompleted;
    }

    public async UniTask LoadQuestsUI()
    {
        await QuestManager.Instance.Init();

        var goalOpened = false;
        var quests = QuestManager.Instance.Quests.Values
            .Where(q => q.questID != QuestInfo.GameOverQuestID);

        foreach (var data in quests)
        {
            var ui = Instantiate(togglePrefab, questListParent)
                .GetComponent<QuestItem>();

            goalOpened = goalOpened || data.questID == QuestInfo.GoalOpenQuestID;

            ui.Init(data);

            if (goalOpened && data.questID == QuestInfo.GameClearQuestID)
            {
                _hideQuestIdx         = data.idxInQuestList;
                _hideQuestDescription = data.description;
                ui.HideGoal();
            }
        }

        Canvas.ForceUpdateCanvases();
    }
    
    private void OnQuestCompleted(QuestData questData)
    {
        var item = FindQuestItem(questData.idxInQuestList);
        
        if (questData.questID == QuestInfo.GoalOpenQuestID)
        {
            var tmp = FindQuestItem(_hideQuestIdx);
            tmp.ShowGoal(_hideQuestDescription);
        }
        
        item.OnChange();
    }

    private void OnQuestProgressChanged(int total, int completed)
    {
        progressText.text = $"({completed}/{total})";
    }
    
    private QuestItem FindQuestItem(int index)
    {
        var item = questListParent.GetChild(index);
        return item.gameObject.GetComponent<QuestItem>();
    }
}
