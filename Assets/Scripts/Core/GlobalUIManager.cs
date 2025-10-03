using DG.Tweening;
using UnityEngine;

public class GlobalUIManager : Singleton<GlobalUIManager>
{
    [SerializeField] private CanvasGroup fadeCanvasGroup;

    private void OnEnable()
    {
        if (fadeCanvasGroup == null)
        {
            Debug.LogError("Fade Canvas Group is not assigned in the inspector.");
            return;
        }
        
        fadeCanvasGroup.alpha = 0f;
        fadeCanvasGroup.blocksRaycasts = false;
        fadeCanvasGroup.interactable = false;
    }

    public Tween FadeIn(float duration)
    {
        fadeCanvasGroup.blocksRaycasts = true;
        fadeCanvasGroup.interactable = true;
        return fadeCanvasGroup.DOFade(1f, duration);
    }
    
    public Tween FadeOut(float duration)
    {
        fadeCanvasGroup.blocksRaycasts = false;
        fadeCanvasGroup.interactable = false;
        return fadeCanvasGroup.DOFade(0f, duration);
    }
}