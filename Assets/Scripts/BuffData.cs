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
        public bool 相同buff是否可并行存在;
        public bool 是否可叠层;
        public int 最高层数;
        public bool 是否可驱散;
        public bool 重复释放是否刷新回合数;
    }

    public List<BuffDataEntry> data;
}

public enum BuffName
{
    星,
    激情,
    攻击力提升,
    攻击力下降,
    受伤加重,
    虚缪,
    苦旅,
    忍耐,
    反击架势,
    被掩护,
    增生,
    返生,
    眩晕,
    燃烬
}

public enum BuffType
{
    触发后消失类buff=0,
    触发后不消失类buff=1,
    印记buff=2,
    每回合恢复xx=3,
    追加类=4,
    标记类=5,
    属性加=6,
    属性减=7,
    易伤=8,
    hot=9,
    减伤=10,
    行动束缚=11,
    每回合失去xx = 12,
}


