using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class LevelScene : MonoBehaviour
{
    [SerializeField] private RectTransform scrollContent;
    
    private async void Start()
    {
        try
        {
            await UniTask.Yield();
        
            await QuestUIController.Instance.LoadQuestsUI();
        }
        catch (Exception e)
        {
            Debug.LogError($"[QuestUI Start] 예외 발생: {e.Message}\n{e.StackTrace}");
        }
    }
}
