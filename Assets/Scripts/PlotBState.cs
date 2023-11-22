using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PixelCrushers.DialogueSystem;
using System;

public class PlotBState : IStageState
{
    public PlotBState(StageFlowSystem flowSystem, StageData.StageDataEntry stageDataRef, IStageState nextState) : base(flowSystem, stageDataRef, nextState)
    {
    }

    public override void StateBegin()
    {
        base.StateBegin();
        DialogueManager.StopAllConversations();
        //¿ªÊ¼¾çÇé
        if (stageData.PlotBName != null && stageData.PlotBName != "")
        {
            StartPolt(stageData.PlotBName);
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
