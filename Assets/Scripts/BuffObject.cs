using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

[System.Serializable]
public class BuffObject : MonoBehaviour
{
    public BuffInfo buffInfo;
    public Text layersText;
    private int m_buffLayers;

    private void Start()
    {
        Utility.AddTriggersListener(GetComponent<EventTrigger>(), EventTriggerType.PointerEnter, OnPointEnter);
        Utility.AddTriggersListener(GetComponent<EventTrigger>(), EventTriggerType.PointerExit, OnPointExit);
    }

    private void OnPointEnter(BaseEventData baseEventData)
    {
        GameObject.Find("BattleSystem").GetComponent<BattleSystem>().OpenBuffDescribe(this);
    }
    private void OnPointExit(BaseEventData baseEventData)
    {
        GameObject.Find("BattleSystem").GetComponent<BattleSystem>().CloseBuffDescribe();
    }

    public void UpdateLayersText()
    {
        layersText.text = m_buffLayers.ToString();
    }

    public int GetLayers() { return m_buffLayers; }
    public void SetLayers(int layers)
    {
        m_buffLayers = layers;
        UpdateLayersText();
    }
    public void UpdateLayers(int layersAddition)
    {
        m_buffLayers += layersAddition;
        UpdateLayersText();
    }
}
