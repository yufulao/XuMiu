using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using static UnityEngine.EventSystems.EventTrigger;

public class SfxManager : MonoBehaviour
{
    public SfxData sfxData;
    public AudioMixerGroup sfxMixerGroup;

    private Dictionary<string, SfxData.SFXDataEntry> dataDictionary;
    private Dictionary<SfxData.SFXDataEntry, AudioSource> sfxItems;

    private void Awake()
    {
        Init();
    }
    private void Init()
    {
        GameObject sfxObjTemp = new GameObject("SfxItem");
        AudioSource sfxObjAudioSource = sfxObjTemp.AddComponent<AudioSource>();

        sfxObjAudioSource.outputAudioMixerGroup = sfxMixerGroup;
        sfxObjAudioSource.playOnAwake = false;

        dataDictionary = new Dictionary<string, SfxData.SFXDataEntry>();
        sfxItems = new Dictionary<SfxData.SFXDataEntry, AudioSource>();
        for (int i = 0; i < sfxData.data.Count; i++)
        {
            if (!string.IsNullOrEmpty(sfxData.data[i].m_name))
            {
                AudioSource audioSourceTemp = GameObject.Instantiate(sfxObjTemp, base.transform).GetComponent<AudioSource>();
                sfxItems.Add(sfxData.data[i], audioSourceTemp);
                dataDictionary.Add(sfxData.data[i].m_name, sfxData.data[i]);
            }
        }
    }

    public void PlaySFX(string name, float volumeBase, bool isLoop = false)
    {
        if (dataDictionary.ContainsKey(name))
        {
            SfxData.SFXDataEntry entry = dataDictionary[name];
            if (entry.m_oneShot)
            {
                sfxItems[entry].PlayOneShot(entry.m_audioClips[UnityEngine.Random.Range(0, entry.m_audioClips.Count)], volumeBase);
                return;
            }
            sfxItems[entry].Stop();
            sfxItems[entry].clip = entry.m_audioClips[UnityEngine.Random.Range(0, entry.m_audioClips.Count)];
            sfxItems[entry].loop = isLoop;

            sfxItems[entry].Play();
            sfxItems[entry].volume = volumeBase;
        }
        else
        {
            Debug.LogError("播放sfx时，没有该SFX名字" + name);
            return;
        }
    }

    public void Stop(string name)
    {
        if (dataDictionary.ContainsKey(name))
        {
            SfxData.SFXDataEntry entry = dataDictionary[name];
            sfxItems[entry].Stop();
        }
        else
        {
            Debug.LogError("暂停sfx时，没有该SFX名字" + name);
            return;
        }
    }

    public void SetVolumeRuntime(float volumePre)
    {
        foreach (var sfxItem in sfxItems)
        {
            sfxItem.Value.volume = volumePre;
        }
    }
}
