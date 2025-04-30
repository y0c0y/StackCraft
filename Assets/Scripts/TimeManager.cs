using System;
using UnityEngine;

public class TimeManager : MonoBehaviour
{
    public static event Action OnResume;
    public static event Action OnPause;
    
    public static TimeManager Instance;

    public bool IsPausedByUser { get; private set; }
    public bool IsPausedByUI { get; private set; }
    public float CurrentSpeed { get; private set; } = 1f;
    
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        IsPausedByUser = false;
        IsPausedByUI = false;
        ApplyTimeScale();
    }

    private void OnEnable()
    {
        UIManager.OnUIChanged += UIChanged;
    }
    
    private void OnDisable()
    {
        UIManager.OnUIChanged -= UIChanged;
    }
    
    private void ApplyTimeScale()
    {
        if (IsPausedByUser || IsPausedByUI)
            Time.timeScale = 0f;
        else
            Time.timeScale = CurrentSpeed;
    }
    
    public void SetSpeed(float speed)
    {
        CurrentSpeed = speed;
        IsPausedByUser = Mathf.Approximately(speed, 0f);
        ApplyTimeScale();
    }
    
    public void ResumeTime()
    {
        IsPausedByUI = false;
        ApplyTimeScale();
        OnResume?.Invoke();
    }

    public void PauseTime()
    {
        IsPausedByUI = true;
        ApplyTimeScale();
        OnPause?.Invoke();
    }
    
    public void PauseTimeByUser()
    {
        IsPausedByUser = true;
        ApplyTimeScale();
        OnPause?.Invoke();
    }

    public void ResumeTimeByUser()
    {
        IsPausedByUser = false;
        ApplyTimeScale();
        OnResume?.Invoke();
    }

    private void UIChanged(bool isDefaultUI)
    {
        IsPausedByUI = !isDefaultUI;
        ApplyTimeScale();

        if (IsPausedByUI)
            PauseTime();
        else
            ResumeTime();
    }
}
