using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BuffInfo
{
    public BuffName buffName;
    public BattleEntity entityGet;//受法者
    public BattleEntity entitySet;//施法者
    public int roundDuring;//持续回合数
    public object[] buffValues;//增幅类buff的数值
    public BuffData.BuffDataEntry data;
}
