using UnityEngine;

public class LevelScene : MonoBehaviour
{
    void Start()
    {
        Debug.Log($"Loaded: {StageInfo.selectedLevel.displayName}");
    }

    
}
