using UnityEngine;

public class StageManager : MonoBehaviour
{
    [SerializeField] private LevelButton[] levelButtons;

    void Start()
    {
        foreach (var btn in levelButtons)
        {
            btn.Init();
        }
    }
}