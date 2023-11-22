using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleCommandInfo
{
    public bool isEnemy;
    public BattleCommandType commandType;
    public bool isBattleStartCommand;
    public int bpNeed;
    public List<BattleEntity> selectEntityList;//ѡ��Ŀ���б�
    public int battleId;//ָ�����

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
