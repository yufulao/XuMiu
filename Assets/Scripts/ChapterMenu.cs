using PixelCrushers;
using PixelCrushers.DialogueSystem.MenuSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class ChapterMenu : MonoBehaviour
{
    public ChapterData chapterData;
    public Transform stageContentTransform;
    public int currentSelectChapterIndex;
    public Image chapterTitle;
    public Image chapterBG;
    public GameObject downBG;
    public CanvasGroup fadeMask;//切换章节时的黑幕

    public Text stageInfoTitleText;
    public TextMeshProUGUI stageInfoTextText;
    public CanvasGroup stageInfoCanvasGroup;
    public int currentSelectStageIndex;
    public GameObject enterBtn;

    public SaveHelper saveHelper;
    public UIPanel chapterMenu;

    public void OpenChapterMenu()
    {
        currentSelectChapterIndex = chapterData.currentChapterIndex;
        CloseFadeMask();
        ResetChapterUI();
        SystemFacade.instance.PlayBGM(chapterData.data[currentSelectChapterIndex].chapterBgmName, 1f, 0.5f, 0f);//切换背景音乐
        chapterMenu.Open();
    }

    private void CloseFadeMask()
    {
        fadeMask.alpha = 0;
        fadeMask.gameObject.SetActive(false);
    }

    public void ResetChapterUI()
    {
        for (int i = 0; i < stageContentTransform.childCount; i++)
        {
            Destroy(stageContentTransform.GetChild(i).gameObject);
        }

        GameObject chapterContent = Instantiate(chapterData.data[currentSelectChapterIndex].stageContent, stageContentTransform);//装载data中的stages位置
        UpdateStageBtns(chapterContent);

        chapterBG.sprite = chapterData.data[currentSelectChapterIndex].chapterBG;
        chapterTitle.sprite = chapterData.data[currentSelectChapterIndex].chapterTitle;
        currentSelectStageIndex = -1;
        stageInfoTextText.text = "";
        stageInfoTitleText.text = "";
        enterBtn.SetActive(false);
        downBG.GetComponent<CanvasGroup>().alpha = 0;
        downBG.SetActive(false);

        chapterData.currentChapterIndex = currentSelectChapterIndex;
    }

    public void UpdateStageBtns(GameObject chapterContent)
    {
        StageData stageData = chapterData.data[currentSelectChapterIndex].stageData;
        for (int i = 0; i < stageData.data.Count; i++)//一定要确定stageData中的stage数量和ChapterContent中的StageObj数量一致
        {
            GameObject stageObj = chapterContent.transform.GetChild(i).gameObject;
            stageObj.transform.Find("PassTip").gameObject.SetActive(stageData.data[i].isPassed);
            stageObj.SetActive(stageData.data[i].isUnlock);
            stageObj.transform.Find("SelectMask").gameObject.SetActive(false);

            int stageIndex = i;
            stageObj.transform.Find("ButtonSelect").GetComponent<Button>().onClick.AddListener(() => { ChangeStageInfoUI(stageIndex); });
        }
    }

    public void NextChapter()
    {
        if (currentSelectChapterIndex < chapterData.data.Count - 1)
        {
            currentSelectChapterIndex++;
        }
        else if (currentSelectChapterIndex == chapterData.data.Count - 1)
        {
            currentSelectChapterIndex = 0;
        }
        ChangeChapterAfter();
    }

    public void LastChapter()
    {
        if (currentSelectChapterIndex > 0)
        {
            currentSelectChapterIndex--;
        }
        else if (currentSelectChapterIndex == 0)
        {
            currentSelectChapterIndex = chapterData.data.Count - 1;
        }
        ChangeChapterAfter();
    }

    private void ChangeChapterAfter()
    {
        SystemFacade.instance.PlaySe("上一章下一章");
        SystemFacade.instance.PlayBGM(chapterData.data[currentSelectChapterIndex].chapterBgmName, 1f, 0.5f, 0f);//切换背景音乐
        StartCoroutine(FadeMaskAnimation());
    }
    IEnumerator FadeMaskAnimation()
    {
        fadeMask.alpha = 0;
        fadeMask.gameObject.SetActive(true);
        Tweener tweener = fadeMask.DOFade(1f, 0.5f);
        yield return tweener.WaitForCompletion();
        ResetChapterUI();
        tweener = fadeMask.DOFade(0f, 0.5f);
        yield return tweener.WaitForCompletion();
        fadeMask.gameObject.SetActive(false);
    }

    public void ChangeStageInfoUI(int stageIndex)
    {
        Transform chapterContent = stageContentTransform.GetChild(0);

        if (currentSelectStageIndex != -1)//关闭上一个stageBtn的selectMask
        {
            chapterContent.GetChild(currentSelectStageIndex).transform.Find("SelectMask").gameObject.SetActive(false);
        }

        currentSelectStageIndex = stageIndex;
        chapterContent.GetChild(currentSelectStageIndex).transform.Find("SelectMask").gameObject.SetActive(true);//打开当前的selectMask

        stageInfoCanvasGroup.alpha = 0;
        StageData.StageDataEntry stageData = chapterData.data[currentSelectChapterIndex].stageData.data[currentSelectStageIndex];
        stageInfoTitleText.text = stageData.stageInfoTitle;
        stageInfoTextText.text = stageData.stageInfoText;
        stageInfoCanvasGroup.DOFade(1, 0.2f);

        if (!enterBtn.activeInHierarchy)
        {
            enterBtn.SetActive(true);
        }
        if (!downBG.activeInHierarchy)
        {
            downBG.SetActive(true);
            downBG.GetComponent<CanvasGroup>().DOFade(1, 0.2f);
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
        //saveHelper.EnterStage(chapterData.data[currentSelectChapterIndex].stageData.data[currentSelectStageIndex]);
        List<StageData.StageDataEntry> unlockStages = new List<StageData.StageDataEntry>();
        if (currentSelectStageIndex + 1 >= chapterData.data[currentSelectChapterIndex].stageData.data.Count)//当前关卡是当前章节的最后一关
        {
            if (currentSelectChapterIndex+1>= chapterData.data.Count)//当前章节是最后一章
            {
                unlockStages = null;
            }
            else
            {
                unlockStages.Add(chapterData.data[currentSelectChapterIndex+1].stageData.data[0]);
            }
        }
        else
        {
            unlockStages.Add(chapterData.data[currentSelectChapterIndex].stageData.data[currentSelectStageIndex + 1]);
        }

        float enterFadeTime = SystemFacade.instance.StageGameStart(chapterData.data[currentSelectChapterIndex].stageData.data[currentSelectStageIndex], unlockStages);
        yield return new WaitForSeconds(enterFadeTime);
        chapterMenu.Close();
        GameObject.Find("Menu System").transform.Find("TitleMenuPanel").GetComponent<UIPanel>().Close();
    }
}
