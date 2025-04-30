using DG.Tweening;
using UnityEngine;

public class FadeInCanvasGroup : MonoBehaviour
{
    private CanvasGroup _canvasGroup;

    private void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
    }

    public void Run(float duration)
    {
        Debug.Log("Run");

        _canvasGroup.DOKill();
        _canvasGroup.alpha = 0f;
        _canvasGroup.DOFade(1f, duration)
                    .OnComplete(() =>
                                    {
                                        _canvasGroup.interactable = true;
                                        _canvasGroup.blocksRaycasts = true;
                                    })
                    .SetUpdate(false)
                    .SetLink(gameObject);
    }
}
