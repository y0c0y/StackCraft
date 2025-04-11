using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LevelButton : MonoBehaviour
{
    [SerializeField] private LevelData levelData;
    [SerializeField] private Button button;
    [SerializeField] private TMP_Text label;
    [SerializeField] private GameObject lockIcon;

    private bool _isUnlocked;
    
    public void Init()
    {
        label.text = levelData.displayName;
        _isUnlocked = PlayerPrefs.GetInt($"Stage_{levelData.levelIndex}", levelData.unlockedByDefault ? 1 : 0) == 1;

        button.interactable = _isUnlocked;
        lockIcon.SetActive(!_isUnlocked);
    }

    public void OnButtonClick()
    {
        if(!_isUnlocked) return;
        StageInfo.selectedLevel = levelData;
        UnityEngine.SceneManagement.SceneManager.LoadScene(levelData.sceneName);
    }
    
}