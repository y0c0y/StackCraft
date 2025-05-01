using UnityEngine;
using TMPro;

public class StageInfoPanel : MonoBehaviour
{
    public static StageInfoPanel Instance;

    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text descriptionText;
    
    private void Awake()
    {
        Instance = this;
        if (Instance != this)
        {
            Destroy(this);
        }
    }
    
    public void SetInfo(LevelData data)
    {
        if (data == null)
        {
            titleText.text = "";
            descriptionText.text = "";
            return;
        }
        
        titleText.text = data.stageName;
        descriptionText.text = data.description;
    }
}
