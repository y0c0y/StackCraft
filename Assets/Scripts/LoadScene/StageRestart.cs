using UnityEngine;

public class StageRestart : MonoBehaviour
{
    [SerializeField] private LevelData levelData;
    public void OnButtonClick()
    {
        StageInfo.SelectedLevel = levelData;
        
        var sceneTransition = FindFirstObjectByType<SceneTransition>();
        sceneTransition.onFadeOutTransitionDone.AddListener(LoadScene);
        sceneTransition.StartFadeOutTransition();
    }
    
    private void LoadScene()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(levelData.sceneName);
    }
}
