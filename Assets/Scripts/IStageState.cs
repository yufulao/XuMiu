using PixelCrushers.DialogueSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class IStageState : MonoBehaviour
{
    // ״̬����
    public string stateName;

    // ״̬ӵ����
    public StageFlowSystem stageFlowSystem;
    public IStageState nextState;//����������һ��State
    public StageData.StageDataEntry stageData;
    public List<StageData.StageDataEntry> unlockStages;

    // ����
    public IStageState(StageFlowSystem flowSystem, StageData.StageDataEntry stageDataRef, IStageState nextStateRef)
    {
        stageData = stageDataRef;
        stageFlowSystem = flowSystem;
        nextState = nextStateRef;
    }

    // ��ʼ
    public virtual void StateBegin()
    {
        if (nextState != null)
        {
            nextState.stageData = stageData;
            nextState.unlockStages = unlockStages;
        }
    }

    // �Y��
    //int.Parse(obj[0].ToString());
    public virtual void StateEnd()
    {
        StartCoroutine(stageFlowSystem.StateEnd());
    }


    // ����
    public virtual void StateUpdate()
    { }
}
