using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class SkillInfo
{
    public string skillName;
    public bool isUnlock;
    public bool needSelect;
    public bool isBattleStartCommand;
    [TextArea(1, 30)]
    public string skillDescription;
    [HideInInspector]
    public SelectType selectType;
    [HideInInspector]
    public bool cameraLookAt;//falseÎª¿´Ïòenemy
    [HideInInspector]
    public int selectCount;
}
