using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class SkillObject : MonoBehaviour
{
    public TextMeshProUGUI skillObjText;
    [ReadOnly]
    public SkillInfo skillInfo;

    private void Start()
    {
        Utility.AddTriggersListener(GetComponent<EventTrigger>(), EventTriggerType.PointerEnter, OnPointEnter);
        Utility.AddTriggersListener(GetComponent<EventTrigger>(), EventTriggerType.PointerExit, OnPointExit);
    }

    private void OnPointEnter(BaseEventData baseEventData)
    {
        GameObject.Find("BattleSystem").GetComponent<BattleSystem>().OpenSkillDescribe(this);
    }
    private void OnPointExit(BaseEventData baseEventData)
    {
        GameObject.Find("BattleSystem").GetComponent<BattleSystem>().CloseSkillDescribe();
    }
}
