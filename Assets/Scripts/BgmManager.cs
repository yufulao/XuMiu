using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using DG.Tweening;
using UnityEngine.Events;

[RequireComponent(typeof(AudioSource))]
public class BgmManager : MonoBehaviour
{
    public BgmData bgmData;
    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    private void PlayBGM(string name,float baseVolume)
    {
        foreach (BgmData.BGMDataEntry datum in bgmData.data)
        {
            if (datum.m_name == name)
            {
                audioSource.Stop();
                audioSource.clip = datum.m_AudioClip;
                audioSource.Play();
                audioSource.volume = baseVolume;
                return;
            }
        }
        Debug.LogError("没有该BGM名字 " + name);
    }

    public void PlayBgmFadeDelay(string name, float baseVolume, float delayTime, float fadeOutTime, float fadeInTime, UnityAction callback = null)
    {
        audioSource.loop = true;
        StopAllCoroutines();
        audioSource.DOFade(0, fadeOutTime);//音量降为0
        StartCoroutine(IPlayBgmDelay(name, baseVolume, delayTime, fadeOutTime, fadeInTime, callback));
    }
    IEnumerator IPlayBgmDelay(string name, float baseVolume, float delayTime, float fadeOutTime, float fadeInTime, UnityAction callback)
    {
        yield return new WaitForSeconds(fadeOutTime + delayTime);
        PlayBGM(name, 0f);
        audioSource.DOFade(baseVolume, fadeInTime);
        if (callback != null)
        {
            audioSource.loop = false;
            yield return new WaitForSeconds(audioSource.clip.length);
            callback.Invoke();
        }
    }

    public void Stop()
    {
        audioSource.Stop();
    }

    public void SetVolumeRuntime(float volumePre)
    {
        audioSource.volume = volumePre;
    }
}
