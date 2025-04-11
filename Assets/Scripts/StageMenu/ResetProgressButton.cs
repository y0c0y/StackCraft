using UnityEngine;
using UnityEngine.SceneManagement;

public class ResetProgressButton : MonoBehaviour
{
    public void OnButtonClick()
    {
        // Stage_1 ~ Stage_3 삭제
        for (int i = 1; i < 4; i++)
        {
            PlayerPrefs.DeleteKey($"Stage_{i}");
        }

        PlayerPrefs.Save();

        Debug.Log("진행 데이터 초기화 완료!");
        SceneManager.LoadScene("StageSelect");
    }
}
