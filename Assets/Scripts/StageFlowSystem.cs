using PixelCrushers;
using PixelCrushers.DialogueSystem;
using PixelCrushers.DialogueSystem.MenuSystem;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class StageFlowSystem : MonoBehaviour
{
    private IStageState state = null;

    public float enterGameFadeDuringTime;

    public EditTeamState editTeamState;
    public PlotAState plotStateA;
    public BattleState battleState;
    public PlotBState plotStateB;

    private void Update()
    {
        if (state != null)
        {
            state.StateUpdate();
        }
    }

    public float StageGameStart(StageData.StageDataEntry stageDataRef, List<StageData.StageDataEntry> unlockStagesT)//Game统一入口,每进去一次就把全部State的nextState设为null
    {
        StartCoroutine(IStageGameStart(stageDataRef, unlockStagesT));
        return 0.5f;
    }
    IEnumerator IStageGameStart(StageData.StageDataEntry stageDataRef, List<StageData.StageDataEntry> unlockStagesT)
    {
        SystemFacade.instance.FadeAnimation(0.5f, 0.5f, enterGameFadeDuringTime, 0f);
        yield return new WaitForSeconds(0.5f);
        SetAndStartGameFlow(stageDataRef, unlockStagesT);
    }

    public IEnumerator StateEnd()
    {
        if (state.nextState != null)//进入下一个State
        {
            SystemFacade.instance.FadeAnimation(0.5f, 0.5f, 1f);
            yield return new WaitForSeconds(0.5f);
            DialogueManager.StopAllConversations();
            StartState(state.nextState);
        }
        else//没有下一个State，流程结束，返回章节选择界面
        {
            SystemFacade.instance.FadeAnimation(0.5f, 0.5f, enterGameFadeDuringTime, 0f);
            yield return new WaitForSeconds(0.5f);
            DialogueManager.StopAllConversations();
            SystemFacade.instance.ReturnToChapter();
        }
    }

    // 设定状态
    private void StartState(IStageState stateRef)
    {
        // 设定
        state = stateRef;
        state.StateBegin();
    }

    private void SetAndStartGameFlow(StageData.StageDataEntry stageDataT, List<StageData.StageDataEntry> unlockStagesT)
    {
        switch (stageDataT.stageFlow)
        {
            case StageFlowEnum.EditPlotBattlePlot:
                SetStateValue(editTeamState, stageDataT, unlockStagesT);
                editTeamState.nextState = plotStateA;
                plotStateA.nextState = battleState;
                battleState.nextState = plotStateB;
                plotStateB.nextState = null;
                StartState(editTeamState);
                break;

            case StageFlowEnum.Plot:
                SetStateValue(plotStateA, stageDataT, unlockStagesT);
                plotStateA.nextState = null;
                PassAndUnlockStage(stageDataT, unlockStagesT);
                StartState(plotStateA);
                break;

            case StageFlowEnum.EditBattle:
                SetStateValue(editTeamState, stageDataT, unlockStagesT);
                editTeamState.nextState = battleState;
                battleState.nextState = null;
                StartState(editTeamState);
                break;

            case StageFlowEnum.EditPlotBattle:
                SetStateValue(editTeamState, stageDataT, unlockStagesT);
                editTeamState.nextState = plotStateA;
                plotStateA.nextState = battleState;
                battleState.nextState = null;
                StartState(editTeamState);
                break;

            case StageFlowEnum.EditBattlePlot:
                SetStateValue(editTeamState, stageDataT, unlockStagesT);
                editTeamState.nextState = battleState;
                battleState.nextState = plotStateB;
                plotStateB.nextState = null;
                StartState(editTeamState);
                break;
        }
    }

    private void SetStateValue(IStageState stateT, StageData.StageDataEntry stageDataT, List<StageData.StageDataEntry> unlockStagesT)
    {
        stateT.stageData = stageDataT;
        stateT.unlockStages = unlockStagesT;
    }

    public static void PassAndUnlockStage(StageData.StageDataEntry stageDataT, List<StageData.StageDataEntry> unlockStagesT)
    {
        stageDataT.isPassed = true;
        stageDataT.isUnlock = true;
        if (unlockStagesT != null)
        {
            for (int i = 0; i < unlockStagesT.Count; i++)
            {
                unlockStagesT[i].isUnlock = true;
            }
        }

        PassStageActionSwitch(stageDataT);
    }
    private static void PassStageActionSwitch(StageData.StageDataEntry stageData)//通关特定关卡的事件
    {
        switch (stageData.stageName)
        {
            case "t-1":
                SystemFacade.instance.UnlockCharacterSkill(1, 2);
                break;
            case "1-5":
                SystemFacade.instance.UnlockCharacterSkill(0, 3);
                SystemFacade.instance.UnlockCharacterSkill(1, 3);
                break;
            case "2-5":
                SystemFacade.instance.UnlockCharacterSkill(0, 4);
                break;
            case "3-5":
                SystemFacade.instance.UnlockCharacterSkill(0, 5);
                SystemFacade.instance.UnlockCharacterSkill(1, 4);
                break;
            case "5-5":
                SystemFacade.instance.UnlockCharacterSkill(1, 5);
                break;
        }
    }

    public void EditStateEnd()
    {
        editTeamState.StateEnd();
    }
    public void PlotAStateEnd()
    {
        plotStateA.StateEnd();
    }
    public void BattleStateEnd(bool isWin)
    {
        battleState.StateEnd(isWin);
    }
    public void PlotBStateEnd()
    {
        plotStateB.StateEnd();
    }
}
