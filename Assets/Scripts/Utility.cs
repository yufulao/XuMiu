using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Utility
{
    //播放动画协程
    public static IEnumerator PlayAnimation(Animator animator, string animationName)
    {
        if (animator != null && animationName != null && animationName != "")
        {
            animator.Play(animationName, 0, 0f);
            yield return new WaitForSeconds(GetAnimatorLength(animator, animationName));
        }
    }

    //eventTrigger添加委托
    public static void AddTriggersListener(EventTrigger trigger, EventTriggerType eventID, UnityEngine.Events.UnityAction<BaseEventData> callback)
    {
        if (trigger.triggers.Count == 0)
        {
            trigger.triggers = new List<EventTrigger.Entry>();
        }

        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = eventID;
        entry.callback.AddListener(callback);
        trigger.triggers.Add(entry);
    }

    //强制更新ui布局
    public static void ForceUpdateContentSizeFilter(Transform rootTransform)
    {
        foreach (ContentSizeFitter child in rootTransform.GetComponentsInChildren<ContentSizeFitter>(true))
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(child.GetComponent<RectTransform>());
        }
        LayoutRebuilder.ForceRebuildLayoutImmediate(rootTransform.GetComponent<RectTransform>());
    }

    //获取当前播放动画的长度
    public static float GetAnimatorLength(Animator animator, string name)
    {
        //动画片段时间长度
        float length = 0;

        AnimationClip[] clips = animator.runtimeAnimatorController.animationClips;
        foreach (AnimationClip clip in clips)
        {
            if (clip.name.Equals(name))
            {
                length = clip.length;
                break;
            }
        }
        return length;
    }

    //飘字
    public static void TextFly(Graphic graphic,Vector3 originalRect)
    {
        RectTransform rectTransform = graphic.rectTransform;
        Color originalColor = graphic.color;

        rectTransform.anchoredPosition = originalRect;
        //Debug.Log(rectTransform.localPosition);
        originalColor.a = 0;
        graphic.color = originalColor;

        Sequence mySequence = DOTween.Sequence();
        Tweener move1 = rectTransform.DOMoveY(rectTransform.position.y + 50, 0.5f);
        Tweener move2 = rectTransform.DOMoveY(rectTransform.position.y + 50, 0.5f);
        Tweener alpha1 = graphic.DOColor(new Color(originalColor.r, originalColor.g, originalColor.b, 1), 0.5f);
        Tweener alpha2 = graphic.DOColor(new Color(originalColor.r, originalColor.g, originalColor.b, 0), 0.5f);

        mySequence.Append(move1);
        mySequence.Join(alpha1);
        mySequence.AppendInterval(0.5f);
        mySequence.Append(move2);
        mySequence.Join(alpha2);
    }
}
