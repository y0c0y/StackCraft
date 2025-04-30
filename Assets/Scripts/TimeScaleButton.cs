using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TimeScaleButton : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private Sprite[] sprites;
    
    [SerializeField] private GameObject StopPanel;
    private readonly int[] _routine = { 0, 1, 2, 4 };
    private int _index;

    private void Start()
    {
        _index = 1;
        SetUI(_routine[_index]);
    }

    private void OnEnable()
    {
        TimeManager.OnResume += ResumeTime;
        TimeManager.OnPause += PauseTime;
    }

    private void OnDisable()
    {
        TimeManager.OnResume -= ResumeTime;
        TimeManager.OnPause -= PauseTime;
    }

    public void OnButtonClick()
    {
        if (Time.timeScale == 0) _index = 0;
        
        _index = (_index + 1) % _routine.Length;
        StopPanel.SetActive(_index == 0);
        SetUI(_routine[_index]);
        TimeManager.Instance.SetSpeed(_routine[_index]);
    }

    private void SetUI(int value)
    {
        int spriteIndex = System.Array.IndexOf(_routine, value);
        if (spriteIndex >= 0 && spriteIndex < sprites.Length)
        {
            if (iconImage != null)
                iconImage.sprite = sprites[spriteIndex];
        }
    }

    private void ResumeTime()
    {
        StopPanel.SetActive(Time.timeScale == 0);
        SetUI((int)Time.timeScale);
    }

    private void PauseTime()
    {
        StopPanel.SetActive(true);
        SetUI(0);
    }
}
