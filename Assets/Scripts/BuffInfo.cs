using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BuffInfo
{
    public BuffName buffName;
    public BattleEntity entityGet;//�ܷ���
    public BattleEntity entitySet;//ʩ����
    public int roundDuring;//�����غ���
    public object[] buffValues;//������buff����ֵ
    public BuffData.BuffDataEntry data;
}
