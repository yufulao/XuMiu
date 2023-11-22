using System.Collections.Generic;
using UnityEngine;
using System;

[CreateAssetMenu(fileName = "BuffData", menuName = "ScriptableObjects/BuffData", order = 1)]
public class BuffData : ScriptableObject
{
    [Serializable]
    public class BuffDataEntry
    {
        public BuffName buffName;
        public bool isDebuff;
        [TextArea(1, 30)]
        public string description;
        public BuffType buffType;
        public bool ��ͬbuff�Ƿ�ɲ��д���;
        public bool �Ƿ�ɵ���;
        public int ��߲���;
        public bool �Ƿ����ɢ;
        public bool �ظ��ͷ��Ƿ�ˢ�»غ���;
    }

    public List<BuffDataEntry> data;
}

public enum BuffName
{
    ��,
    ����,
    ����������,
    �������½�,
    ���˼���,
    ����,
    ����,
    ����,
    ��������,
    ���ڻ�,
    ����,
    ����,
    ѣ��,
    ȼ��
}

public enum BuffType
{
    ��������ʧ��buff=0,
    ��������ʧ��buff=1,
    ӡ��buff=2,
    ÿ�غϻָ�xx=3,
    ׷����=4,
    �����=5,
    ���Լ�=6,
    ���Լ�=7,
    ����=8,
    hot=9,
    ����=10,
    �ж�����=11,
    ÿ�غ�ʧȥxx = 12,
}


