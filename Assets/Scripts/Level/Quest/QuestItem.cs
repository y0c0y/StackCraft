using System;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class QuestItem : MonoBehaviour
{
    [SerializeField] private Toggle toggle;
    [SerializeField] private TMP_Text label;

    public event Action<string> OnQuestCompleted;
    private QuestData _questData;
    
    public void Init(QuestData data)
    {
        _questData = data;
        
        label.text = data.description;
        toggle.isOn = QuestManager.Instance.IsCompleted(data.questID);

        OnQuestCompleted += QuestManager.Instance.CompleteQuest;

        // toggle.interactable = false;
    }

    private void SetCompleted()
    {
        toggle.isOn = true;
        Debug.Log("Completed");
        OnQuestCompleted?.Invoke(_questData.questID);
    }

    public void OnChange()
    {
        label.text = $"<s>{label.text}</s>";
        label.alpha = 0.5f;
        
        SetCompleted();
    }

    public void HideGoal(QuestData questData)
    {
        Init(questData);
        label.text = "???";
    }

    public void ShowGoal(QuestData questData)
    {
        label.text = $"{questData.description}";
    }
    
}
