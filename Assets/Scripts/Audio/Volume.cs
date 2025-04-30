using UnityEngine;

public static class Volume
{
    public static float MasterVolume = 0.5f;
    public static float BGMVolume = 0.5f;
    public static float SFXVolume = 0.5f;

    public enum VolumeType
    {
        Master, 
        BGM, 
        SFX,
    }
    
    public static void SaveMasterVolumes(float volume)
    {
        MasterVolume = volume;
        PlayerPrefs.SetFloat("MasterVolume", MasterVolume);
        PlayerPrefs.Save();
    }
    
    public static void SaveBGMVolumes(float volume)
    {
        BGMVolume = volume;
        PlayerPrefs.SetFloat("BGMVolume", BGMVolume);
        PlayerPrefs.Save();
    }
    
    public static void SaveSFXVolumes(float volume)
    {
        SFXVolume = volume;
        PlayerPrefs.SetFloat("SFXVolume", SFXVolume);
        PlayerPrefs.Save();
    }

    public static void LoadVolumes()
    {
        MasterVolume = PlayerPrefs.GetFloat("MasterVolume", 0.5f);
        BGMVolume = PlayerPrefs.GetFloat("BGMVolume", 0.5f);
        SFXVolume = PlayerPrefs.GetFloat("SFXVolume", 0.5f);
    }
}
