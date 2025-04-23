using System;
using UnityEngine;

[RequireComponent(typeof(AudioSource)), ExecuteInEditMode]
public class AudioManager : MonoBehaviour
{
    [SerializeField] private SoundList[] soundList;
    public static AudioManager Instance { get; private set; }
    private AudioSource _audioSource;
    
    private void Awake()
    {
        if (Application.isPlaying)
        {
            if (Instance != null)
            {
                Destroy(gameObject);
            }
            Instance = this;
            
            DontDestroyOnLoad(this);
        }
    }

    private void Start()
    {
        _audioSource = GetComponent<AudioSource>();
    }

    public static void PlaySound(SoundType sound, float volume = 1f)
    {
        Instance._audioSource.PlayOneShot(Instance.soundList[(int)sound].sound, volume);
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