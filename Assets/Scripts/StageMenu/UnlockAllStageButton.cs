using UnityEngine;
using UnityEngine.SceneManagement;

public class UnlockAllStageButton : MonoBehaviour
{
    public void OnButtonClick()
    {
        const int totalStages = 4; // 현재 스테이지 수에 맞게 수정

        for (var i = 0; i < totalStages; i++)
        {
            PlayerPrefs.SetInt($"Stage_{i}", 1);
        }

        PlayerPrefs.Save();

        Debug.Log("모든 스테이지가 해금되었습니다!");
        SceneManager.LoadScene("StageSelect"); // 새로고침
    }
}
