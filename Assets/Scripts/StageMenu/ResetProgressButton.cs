using UnityEngine;
using UnityEngine.SceneManagement;

public class ResetProgressButton : MonoBehaviour
{
    [SerializeField] private SceneTransition sceneTransition;
    
    public void OnButtonClick()
    {
        for (var i = 1; i < 4; i++)
        {
            PlayerPrefs.DeleteKey($"Stage_{i}");
        }

        PlayerPrefs.Save();
        sceneTransition.onFadeOutTransitionDone.AddListener(LoadScene);
        sceneTransition.StartFadeOutTransition();
    }

    public void LoadScene()
    {
        SceneManager.LoadScene("StageSelect");
    }
}
