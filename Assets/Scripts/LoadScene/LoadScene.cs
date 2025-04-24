using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadScene : MonoBehaviour
{
    private const string MAIN_SCENE_NAME = "Main";
    private const string STAGE_SELECT_SCENE_NAME = "StageSelect";
    
    public void OnLoadMain()
    {
        var allCanvas = FindObjectsByType<Canvas>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        foreach (var canvas in allCanvas)
        {
            if (canvas.renderMode != RenderMode.ScreenSpaceCamera)
            {
                canvas.enabled = false;
            }
        }
        
        var sceneTransition = FindFirstObjectByType<SceneTransition>();
        if (sceneTransition)
        {
            sceneTransition.onFadeOutTransitionDone.AddListener(() => StartLoading(MAIN_SCENE_NAME));
            sceneTransition.StartFadeOutTransition();    
        }
        else
        {
            StartLoading(MAIN_SCENE_NAME);
        }
    }

    public void OnLoadStageSelect()
    {
        var allCanvas = FindObjectsByType<Canvas>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        foreach (var canvas in allCanvas)
        {
            if (canvas.renderMode != RenderMode.ScreenSpaceCamera)
            {
                canvas.enabled = false;
            }
        }
        
        var sceneTransition = FindFirstObjectByType<SceneTransition>();
        if (sceneTransition)
        {
            sceneTransition.onFadeOutTransitionDone.AddListener(() => StartLoading(STAGE_SELECT_SCENE_NAME));
            sceneTransition.StartFadeOutTransition();    
        }
        else
        {
            StartLoading(STAGE_SELECT_SCENE_NAME);
        }
    }
    
    public void OnLoadExit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit(); // 어플리케이션 종료
#endif
    }

    private void StartLoading(string sceneName)
    {
        StartCoroutine(Load(sceneName));
    }

    private IEnumerator Load(string sceneName)
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);

        while (!asyncLoad.isDone)
        {
            yield return null;
        }
    }
}
