using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadScene : MonoBehaviour
{
    private string mainScene = "Main";
    private string stageScene = "StageSelect";
    
    public void OnLoadMain()
    {
        StartCoroutine(Load(mainScene));
    }

    public void OnLoadStageSelect()
    {
        StartCoroutine(Load(stageScene));
    }
    
    public void OnLoadExit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit(); // 어플리케이션 종료
#endif
        
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
