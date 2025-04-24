using UnityEngine;
using UnityEngine.Audio;

public class VolumeController: MonoBehaviour
{
    [SerializeField] private AudioMixer mixer;
    
    public void SetMasterVolume(float volume)
    {
        mixer.SetFloat("MasterVolume", Mathf.Log10(volume) * 20f);
    }
    
    public void SetBGMVolume(float volume)
    {
        mixer.SetFloat("BGMVolume", Mathf.Log10(volume) * 20);
    }
    
    public void SetSFXVolume(float volume)
    {
        mixer.SetFloat("SFXVolume", Mathf.Log10(volume) * 20);
    }
}
