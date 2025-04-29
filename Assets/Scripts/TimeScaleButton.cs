using System;
using UnityEngine;
using UnityEngine.UI;

public class TimeScaleButton : MonoBehaviour
{
    [SerializeField] private float timeScale = 1f;
    private Button _button;

    private void Awake()
    {
        _button = GetComponent<Button>();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _button.onClick.AddListener(() => GameTableManager.Instance.SetTimeScale(timeScale));
        var timeManager = TimeManager.Instance;
        if (timeManager)
        {
            _button.onClick.AddListener(() => timeManager.SetTimeScale(timeScale));
        }
    }
}
