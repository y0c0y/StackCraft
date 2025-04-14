using UnityEngine;

public class LevelScene : MonoBehaviour
{
    private void Start()
    {
        Debug.Log($"Loaded: {StageInfo.SelectedLevel.displayName}");
    }

    
}
