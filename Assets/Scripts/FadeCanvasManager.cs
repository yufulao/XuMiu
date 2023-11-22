using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FadeCanvasManager : MonoBehaviour
{
    public CanvasGroup fadeCanvasGroup;

    public void FadeAnimation(float fadeInTime, float fadeOutTime, float duringTime, float delayTime)
    {
        StartCoroutine(IFadeAnimation(fadeInTime, fadeOutTime, duringTime, delayTime));
    }
    IEnumerator IFadeAnimation(float fadeInTime, float fadeOutTime, float duringTime, float delayTime)
    {
        yield return new WaitForSeconds(delayTime);
        fadeCanvasGroup.alpha = 0;
        fadeCanvasGroup.gameObject.SetActive(true);
        Tweener tweener = fadeCanvasGroup.DOFade(1f, fadeInTime);
        yield return tweener.WaitForCompletion();
        yield return new WaitForSeconds(duringTime);
        tweener = fadeCanvasGroup.DOFade(0f, fadeOutTime);
        yield return tweener.WaitForCompletion();
        fadeCanvasGroup.gameObject.SetActive(false);
    }
}
