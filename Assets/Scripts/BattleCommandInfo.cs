using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleCommandInfo
{
    public bool isEnemy;
    public BattleCommandType commandType;
    public bool isBattleStartCommand;
    public int bpNeed;
    public List<BattleEntity> selectEntityList;//选择目标列表
    public int battleId;//指令发起者

    public BattleCommandInfo(bool isEnemyT, BattleCommandType commandTypeT,bool isBattleStartCommandT, int bpNeedT, List<BattleEntity> selectEntityListT, int entityIdT)
    {
        isEnemy = isEnemyT;
        commandType = commandTypeT;
        isBattleStartCommand = isBattleStartCommandT;
        bpNeed = bpNeedT;
        selectEntityList = selectEntityListT;
        battleId = entityIdT;
    }
}
