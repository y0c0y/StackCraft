using UnityEngine;
using UnityEngine.SceneManagement;

public class UnlockAllStageButton : MonoBehaviour
{
    public void OnButtonClick()
    {
        // TODO totalStages 수정 필요
        const int totalStages = 2;

        for (var i = 0; i < totalStages; i++)
        {
            PlayerPrefs.SetInt($"Stage_{i}", 1);
        }

        PlayerPrefs.Save();
        SceneManager.LoadScene("StageSelect");
    }
}
