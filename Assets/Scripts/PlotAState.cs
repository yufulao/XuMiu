using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PixelCrushers.DialogueSystem;
using System;

public class PlotAState : IStageState
{
    public PlotAState(StageFlowSystem flowSystem, StageData.StageDataEntry stageDataRef, IStageState nextState) : base(flowSystem, stageDataRef, nextState)
    {
    }

    public override void StateBegin()
    {
        base.StateBegin();
        //¿ªÊ¼¾çÇé
        DialogueManager.StopAllConversations();
        if (stageData.PlotAName != null && stageData.PlotAName != "")
        {
            StartPolt(stageData.PlotAName);
        }
    }

    public override void StateEnd()
    {
        base.StateEnd();
    }

    public override void StateUpdate()
    {
        base.StateUpdate();
    }

    public void StartPolt(string plotName)
    {
        DialogueManager.instance.StopAllConversations();
        DialogueManager.instance.StartConversation(plotName);
    }
}
