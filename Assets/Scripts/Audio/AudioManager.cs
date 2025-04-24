using System;
using UnityEngine;

[ExecuteInEditMode]
public class AudioManager : MonoBehaviour
{
    [SerializeField] private SoundList[] soundList;
    [SerializeField] private AudioSource sfxAudioSource;
    [SerializeField] private AudioSource bgmAudioSource;

    public static AudioManager Instance { get; private set; }
    
    private void Awake()
    {
        if (Application.isPlaying)
        {
            Debug.Assert(sfxAudioSource != null);
            Debug.Assert(bgmAudioSource != null);
            
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            
            DontDestroyOnLoad(this);
        }
    }

    public static void PlaySound(SoundType sound, float volume = 1f)
    {
        Instance.sfxAudioSource.PlayOneShot(Instance.soundList[(int)sound].sound, volume);
    }

#if UNITY_EDITOR
    private void OnEnable()
    {
        string[] names = Enum.GetNames(typeof(SoundType));
        Array.Resize(ref soundList, names.Length);
        for (int i = 0; i < soundList.Length; i++)
        {
            soundList[i].name = names[i];
        }
    }
#endif
}

[Serializable]
public struct SoundList
{
    [HideInInspector] public string name;
    [SerializeField] public AudioClip sound;
}