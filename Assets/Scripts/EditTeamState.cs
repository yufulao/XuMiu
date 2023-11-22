using PixelCrushers.DialogueSystem;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EditTeamState : IStageState
{
    public EditTeamState(StageFlowSystem flowSystem, StageData.StageDataEntry stageDataRef,IStageState nextState) : base(flowSystem,stageDataRef, nextState)
    {
    }

    public override void StateBegin()
    {
        base.StateBegin();
        //进入队伍编辑界面
        GameObject.Find("Menu System").GetComponent<EditTeamMenu>().OpenEditTeamPanel(stageData);
    }

    public override void StateEnd()
    {
        base.StateEnd();
        StartCoroutine(IStateEnd());
    }
    private IEnumerator IStateEnd()
    {
        yield return new WaitForSeconds(0.5f);
        GameObject.Find("Menu System").GetComponent<EditTeamMenu>().editTeamPanel.Close();
    }

    public override void StateUpdate()
    {
        base.StateUpdate();
    }
}
