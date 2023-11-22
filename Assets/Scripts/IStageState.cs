using PixelCrushers.DialogueSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class IStageState : MonoBehaviour
{
    // 状态名称
    public string stateName;

    // 状态拥有者
    public StageFlowSystem stageFlowSystem;
    public IStageState nextState;//用来设置下一个State
    public StageData.StageDataEntry stageData;
    public List<StageData.StageDataEntry> unlockStages;

    // 构造
    public IStageState(StageFlowSystem flowSystem, StageData.StageDataEntry stageDataRef, IStageState nextStateRef)
    {
        stageData = stageDataRef;
        stageFlowSystem = flowSystem;
        nextState = nextStateRef;
    }

    // 开始
    public virtual void StateBegin()
    {
        if (nextState != null)
        {
            nextState.stageData = stageData;
            nextState.unlockStages = unlockStages;
        }
    }

    // 結束
    //int.Parse(obj[0].ToString());
    public virtual void StateEnd()
    {
        StartCoroutine(stageFlowSystem.StateEnd());
    }


    // 更新
    public virtual void StateUpdate()
    { }
}
