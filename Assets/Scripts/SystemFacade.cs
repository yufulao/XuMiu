using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class SystemFacade : MonoBehaviour
{
    public SystemMediator mediator;
    [HideInInspector]
    public MyAlert myAlert;
    public static SystemFacade instance;

    private void Awake()
    {
        if (instance != null)
            Destroy(gameObject);
        else
            instance = this;
        DontDestroyOnLoad(this);
    }

    public float StageGameStart(StageData.StageDataEntry stageData, List<StageData.StageDataEntry> unlockStagesT)
    {
        return mediator.stageFlowSystem.StageGameStart(stageData, unlockStagesT);
    }

    public void BattleStart(StageData.StageDataEntry stageData)
    {
        mediator.battleSystem.StartBattle(stageData);
    }
    //public void BattleWin(params object[] objs)
    //{
    //    mediator.stageFlowSystem.BattleStateEnd(objs);
    //}
    //public void BattleLoss(params object[] objs)
    //{
    //    mediator.stageFlowSystem.StageLoss(objs);
    //}
    public void BattleFinish(bool isWin)
    {
        mediator.stageFlowSystem.BattleStateEnd(isWin);
    }
    public void EditTeamFinish()
    {
        mediator.stageFlowSystem.EditStateEnd();
    }
    public void PlotAFinish()
    {
        mediator.stageFlowSystem.PlotAStateEnd();
    }
    public void PlotBFinish()
    {
        mediator.stageFlowSystem.PlotBStateEnd();
    }

    public void RegisterMyAlert(MyAlert t)
    {
        if (myAlert == null)
        {
            myAlert = t;
        }
    }

    public void PlayBGM(string name, float delayTime, float fadeOutTime, float fadeInTime, UnityAction callback = null)
    {
        mediator.bgmManager.PlayBgmFadeDelay(name, PlayerPrefs.GetFloat("bgmAudioSourceVolume"), delayTime, fadeOutTime, fadeInTime, callback);
    }
    public void StopBGM()
    {
        mediator.bgmManager.Stop();
    }
    public void SetBgmAudioSourceVolume(float volumePre)
    {
        mediator.bgmManager.SetVolumeRuntime(volumePre);
        PlayerPrefs.SetFloat("bgmAudioSourceVolume", volumePre);
    }

    public void PlayBgmForTitle()
    {
        mediator.bgmManager.PlayBgmFadeDelay("主菜单和章节选择界面", PlayerPrefs.GetFloat("bgmAudioSourceVolume"), 0.5f, 0.5f, 0f);
    }

    public void PlaySe(string name, bool isLoop = false)
    {
        mediator.seManager.PlaySFX(name, PlayerPrefs.GetFloat("seAudioSourceVolume"), isLoop);
    }
    public void PlayVoice(string name, bool isLoop = false)
    {
        mediator.voiceManager.PlaySFX(name, PlayerPrefs.GetFloat("voiceAudioSourceVolume"), isLoop);
    }
    public void SetSeAudioSourceVolume(float volumePre)
    {
        mediator.seManager.SetVolumeRuntime(volumePre);
    }
    public void SetVoiceAudioSourceVolume(float volumePre)
    {
        mediator.voiceManager.SetVolumeRuntime(volumePre);
    }

    public float FadeAnimation(float fadeInTime, float fadeOutTime, float duringTime = 0f, float delayTime = 0f)
    {
        mediator.fadeCanvasManager.FadeAnimation(fadeInTime, fadeOutTime, duringTime, delayTime);
        return fadeInTime + fadeOutTime + duringTime + delayTime;
    }

    public void ReturnToChapter()
    {
        if (GameObject.Find("Menu System").GetComponent<ChapterMenu>().chapterData.isBranch)
        {
            GameObject.Find("Menu System").GetComponent<BranchMenu>().OpenBranchMenu();
        }
        else
        {
            GameObject.Find("Menu System").GetComponent<ChapterMenu>().OpenChapterMenu();
        }
    }

    public void SwitchObjCamera(CameraManager.ObjCameraState nextState, float during = 0.3f)
    {
        CameraManager.SwitchObjCamera(nextState, during);
    }

    public void UnlockCharacterSkill(int characterIndex,int skillIndex)
    {
        mediator.battleSystem.characterData.data[characterIndex].skills[skillIndex].isUnlock = true;
    }
}
