using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PixelCrushers;
using PixelCrushers.DialogueSystem.MenuSystem;
using UnityEngine.UI;

public class BranchMenu : MonoBehaviour
{
    public BranchData branchData;
    public List<GameObject> stageBtnList;
    public int currentSelectBranchIndex;
    public Text branchName;
    public Image branchBG;

    public Text stageInfoUI;
    public StageData stageData;
    public int currentSelectStageIndex;
    public GameObject enterBtn;

    public SaveHelper saveHelper;
    public UIPanel branchMenu;

    public void OpenBranchMenu()
    {
        currentSelectBranchIndex = branchData.currentBranchIndex;
        AfterNextLastBranch();
        UpdateStageBtns();
        enterBtn.SetActive(false);
        branchName.text = "剧情支线";
        currentSelectStageIndex = -1;
        branchMenu.Open();
    }

    public void NextBranch()
    {
        if (currentSelectBranchIndex < branchData.data.Count - 1)
        {
            currentSelectBranchIndex++;
        }
        else if (currentSelectBranchIndex == branchData.data.Count - 1)
        {
            currentSelectBranchIndex = 0;
        }
        AfterNextLastBranch();
    }

    public void LastBranch()
    {
        if (currentSelectBranchIndex > 0)
        {
            currentSelectBranchIndex--;
        }
        else if (currentSelectBranchIndex == 0)
        {
            currentSelectBranchIndex = branchData.data.Count - 1;
        }
        AfterNextLastBranch();
    }

    public void AfterNextLastBranch()
    {
        branchName.text = branchData.data[currentSelectBranchIndex].branchName;
        enterBtn.SetActive(false);
        stageInfoUI.text = "";
        UpdateStageBtns();
        branchData.currentBranchIndex= currentSelectBranchIndex;
    }

    public void UpdateStageBtns()
    {
        int stageCount = branchData.data[currentSelectBranchIndex].stageData.data.Count;
        for (int i = 0; i < stageCount; i++)
        {
            stageBtnList[i].SetActive(true);
            int stageIndex = i;
            stageBtnList[i].GetComponent<Button>().onClick.AddListener(() => { ChangeStageInfoUI(stageIndex); });
        }
        for (int i = stageCount; i < stageBtnList.Count; i++)
        {
            stageBtnList[i].SetActive(false);
        }
        stageData = branchData.data[currentSelectBranchIndex].stageData;
    }

    public void ChangeStageInfoUI(int stageIndex)
    {
        currentSelectStageIndex = stageIndex;
        //stageInfoUI.text = stageData.data[currentSelectStageIndex].stageInfo;
        if (!enterBtn.activeInHierarchy)
        {
            enterBtn.SetActive(true);
        }
    }

    public void EnterStage()
    {
        if (currentSelectStageIndex == -1)//未选择Stage
        {
            Debug.Log("未选择Stage但是Enter摁钮显示了");
            return;
        }
        StartCoroutine(IEnterStage());


    }
    IEnumerator IEnterStage()
    {
        List<StageData.StageDataEntry> unlockStages = null;
        if (currentSelectStageIndex + 1 >= branchData.data[currentSelectBranchIndex].stageData.data.Count)//当前关卡是当前章节的最后一关
        {
            if (currentSelectBranchIndex + 1 >= branchData.data.Count)//当前章节是最后一章
            {
                unlockStages = null;
            }
            else
            {
                unlockStages.Add(branchData.data[currentSelectBranchIndex + 1].stageData.data[0]);
            }
        }
        else
        {
            unlockStages.Add(branchData.data[currentSelectBranchIndex].stageData.data[currentSelectStageIndex + 1]);
        }

        //saveHelper.EnterStage(chapterData.data[currentSelectChapterIndex].stageData.data[currentSelectStageIndex]);
        float enterFadeTime = SystemFacade.instance.StageGameStart(branchData.data[currentSelectBranchIndex].stageData.data[currentSelectStageIndex], unlockStages);
        yield return new WaitForSeconds(enterFadeTime);
        branchMenu.Close();
        GameObject.Find("TitleMenuPanel").GetComponent<UIPanel>().Close();
    }



}
