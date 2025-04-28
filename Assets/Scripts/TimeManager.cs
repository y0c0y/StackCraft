using UnityEngine;

public class TimeManager : MonoBehaviour
{
    public static TimeManager Instance;

    private float timeScale = 1f;
    private float stopTime = 0f;
    
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void OnEnable()
    {
        UIManager.OnUIChanged += UIChanged;
    }
    
    private void OnDisable()
    {
        UIManager.OnUIChanged -= UIChanged;
    }
    
    public void ResumeTime()
    {
        GameTableManager.Instance.SetTimeScale(timeScale);
    }

    public void PauseTime()
    {
        GameTableManager.Instance.SetTimeScale(stopTime);
    }

    public void SetTimeScale(float timeScale)
    {
        this.timeScale = timeScale;
    }

    private void UIChanged(bool isDefaultUI)
    {
        if (isDefaultUI)
        {
            ResumeTime();
        }
        else
        {
            PauseTime();
        }
    }
}
