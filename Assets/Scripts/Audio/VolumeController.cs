using UnityEngine;
using UnityEngine.Audio;
using static Volume;

public class VolumeController: MonoBehaviour
{
    [SerializeField] private AudioMixer mixer;
    
    public void SetMasterVolume(float volume)
    {
        SaveMasterVolumes(volume);
        mixer.SetFloat("MasterVolume", Mathf.Log10(volume) * 20f);
    }
    
    public void SetBGMVolume(float volume)
    {
        SaveBGMVolumes(volume);
        mixer.SetFloat("BGMVolume", Mathf.Log10(volume) * 20);
    }
    
    public void SetSFXVolume(float volume)
    {
        SaveSFXVolumes(volume);
        mixer.SetFloat("SFXVolume", Mathf.Log10(volume) * 20);
    }
}
