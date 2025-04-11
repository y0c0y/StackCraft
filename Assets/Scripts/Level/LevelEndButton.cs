using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelEndButton : MonoBehaviour
{
    public void OnButtonClick()
    {
        int next = StageInfo.selectedLevel.levelIndex + 1;


        if (next < 4)
        {
            PlayerPrefs.SetInt($"Stage_{next}", 1);
            PlayerPrefs.Save();
        }
        else
        {
            for (int i = 1; i <= 4; i++)
            {

                PlayerPrefs.DeleteKey("Stage_" + next);
            }
        }
        
        SceneManager.LoadScene("StageSelect");
    }
}
