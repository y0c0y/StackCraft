using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class SceneTransition : MonoBehaviour
{
    public UnityEvent onFadeInTransitionDone;
    public UnityEvent onFadeOutTransitionDone;
    
    private static readonly int ProgressID = Shader.PropertyToID("_Progress");
    
    [SerializeField] private bool doFadeIn = true;
    [SerializeField] private float fadeInTransitionTime = 1f;
    [SerializeField] private float fadeOutTransitionTime = 1f;

    private Material _transitionMaterial;

    private void Awake()
    {
        var image = GetComponentInChildren<Image>();
        _transitionMaterial = Instantiate(image.material);
        image.material = _transitionMaterial;
    }

    private void Start()
    {
        if (!doFadeIn)
        {
            _transitionMaterial.SetFloat(ProgressID, 0f);
        }
        else
        {
            _transitionMaterial.SetFloat(ProgressID, 1f);
            Run(fadeInTransitionTime, false).Forget();
        }
    }

    public void StartFadeOutTransition()
    {
        _transitionMaterial.SetFloat(ProgressID, 0f);
        Run(fadeOutTransitionTime).Forget();
    }
    
    private async UniTask Run(float time, bool isFadeOut = true)
    {
        float progress = 0f;
        float currentTime = 0f;
        
        while (currentTime < time)
        {
            currentTime += Time.unscaledDeltaTime;
            progress = Mathf.Clamp01(currentTime / time);
            
            if (!isFadeOut)
            {
                progress = 1f - progress;
            }
            
            _transitionMaterial.SetFloat(ProgressID, progress);
            await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate);
        }

        if (isFadeOut)
        {
            onFadeOutTransitionDone?.Invoke();
        }
        else
        {
            onFadeInTransitionDone?.Invoke();
        }
    }
}
