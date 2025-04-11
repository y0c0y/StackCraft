using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelEndButton : MonoBehaviour
{
    public void OnButtonClick()
    {
        int next = StageInfo.selectedLevel.levelIndex + 1;
        PlayerPrefs.SetInt($"Stage_{next}", 1);
        PlayerPrefs.Save();
        SceneManager.LoadScene("StageSelect");
    }
}
