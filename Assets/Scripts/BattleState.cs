using Spine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleState : IStageState
{
    public BattleState(StageFlowSystem flowSystem, StageData.StageDataEntry stageDataRef, IStageState nextState) : base(flowSystem, stageDataRef, nextState)
    {
    }

    public override void StateBegin()
    {
        base.StateBegin();
        SystemFacade.instance.BattleStart(stageData);
    }

    public override void StateEnd()//battleSystem关闭战斗界面已延迟0.5s
    {
        base.StateEnd();
    }
    public void StateEnd(bool isWin)
    {
        StageFlowSystem.PassAndUnlockStage(stageData, unlockStages);
        StateEnd();
    }


    public override void StateUpdate()
    {
        base.StateUpdate();
    }
}
