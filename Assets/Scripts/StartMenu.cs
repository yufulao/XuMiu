using PixelCrushers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class StartMenu : MonoBehaviour
{
    public Image infomationUI;
    public float infoImageSwitchSpeed;
    public GameObject enterBtn;
    public int currentSelect;
    public List<Sprite> infoImageList;
    public List<GameObject> selectBtns;

    public ChapterMenu chapterMenu;
    public BranchMenu branchMenu;
    public UIPanel startMenu;

    public void OpenStartMenu()
    {
        InitSelectBtn();
        currentSelect = -1;
        enterBtn.SetActive(false);
        infomationUI.color = new Color(1, 1, 1, 0);
        startMenu.Open();
    }

    public void InitSelectBtn()
    {
        for (int i = 0; i < selectBtns.Count; i++)
        {
            int index = i;
            selectBtns[i].GetComponent<OnceClickButton>().onClick.AddListener(() => { UpdateBtn(index); });
            selectBtns[i].GetComponent<OnceClickButton>().EnableInteractable();
        }
        selectBtns[0].transform.SetAsLastSibling();
    }

    public void UpdateBtn(int selectIndex)
    {
        infomationUI.color = new Color(1, 1, 1, 0);
        infomationUI.sprite = infoImageList[selectIndex];
        DOTween.Kill("imageFade");
        infomationUI.DOFade(1f, 0.6f).SetId<Tween>("imageFade");
        currentSelect = selectIndex;
        if (!enterBtn.activeInHierarchy)
        {
            enterBtn.SetActive(true);
        }
    }
    public void EnterBtn()
    {
        if (currentSelect == -1)
        {
            Debug.Log("currentSelect为-1");
            return;
        }
        startMenu.Close();
        if (currentSelect == 0)//主线
        {
            chapterMenu.chapterData.isBranch = false;
            chapterMenu.OpenChapterMenu();
        }
        else if (currentSelect == 1)//支线
        {
            chapterMenu.chapterData.isBranch = true;
            branchMenu.OpenBranchMenu();
        }
    }
}
