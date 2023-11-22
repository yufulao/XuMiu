using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillSelectInfo
{
    public bool needSelect;
    public bool isBattleStartCommand;
    public BattleEntity battleEntity;
    public int skillIndex;
    public List<BattleEntity> selectedEntityList;


    //输入完Skill进EntityCommandList后再赋的值
    public int BpNeed;

    public SkillSelectInfo(bool needSelectT,bool isBattleStartCommandT, BattleEntity battleEntityT, int skillIndexT, List<BattleEntity> selectedEntityListT)
    {
        needSelect = needSelectT;
        isBattleStartCommand = isBattleStartCommandT;
        battleEntity = battleEntityT;
        skillIndex = skillIndexT;
        selectedEntityList = selectedEntityListT;
    }
}
