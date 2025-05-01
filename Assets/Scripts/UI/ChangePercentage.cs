using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Volume;

public class ChangePercentage : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private VolumeType volumeType;
    private Slider slider;

    private void Awake()
    {
        slider = GetComponent<Slider>();
    }

    private void Start()
    {
        LoadVolumes();
        switch (volumeType)
        {
            case VolumeType.Master:
                slider.value = MasterVolume;
                break;
            case VolumeType.BGM:
                slider.value = BGMVolume;
                break;
            case VolumeType.SFX:
                slider.value = SFXVolume;
                break;
        }
        text.text = $"{slider.value * 100:N0} %";
    }

    public void OnValueChanged()
    {
        text.text = $"{slider.value* 100:N0} %";
    }
}
