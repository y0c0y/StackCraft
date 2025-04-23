using UnityEngine;
using UnityEngine.Serialization;

public class StageManager : MonoBehaviour
{
    [SerializeField] public LevelButton[] levelButtons;

    public void Start()
    {
        foreach (var btn in levelButtons)
        {
            btn.Init();
            btn.OnHovered += StageInfoPanel.Instance.SetInfo;
        }
    }

}