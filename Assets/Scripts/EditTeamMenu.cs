using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using PixelCrushers;

public class EditTeamMenu : MonoBehaviour
{
    public List<GameObject> teamList;
    public List<GameObject> characterList;

    public int currentTeamIndex;

    public CharacterData characterData;

    public Sprite nullTeamSiteSprite;
    public Image teamBG;
    public GameObject characterListScroll;
    public GameObject fixedTeamMask;
    public GameObject enterButton;
    public UIPanel editTeamPanel;
    private StageData.StageDataEntry stageData;

    public void OpenEditTeamPanel(StageData.StageDataEntry stageDataPre)
    {
        stageData = stageDataPre;
        SystemFacade.instance.PlayBGM("战前编队界面", 0f, 0.5f, 0f);
        InitUI();
        editTeamPanel.Open();
    }

    public void InitUI()
    {
        //设置背景
        ChapterMenu chapterMenu = gameObject.GetComponent<ChapterMenu>();
        if (chapterMenu.chapterData.isBranch)
            teamBG.sprite = gameObject.GetComponent<BranchMenu>().branchBG.sprite;
        else
            teamBG.sprite = chapterMenu.chapterBG.sprite;

        if (teamList.Count != characterData.teamIndexList.Count || characterList.Count != characterData.data.Count)
        {
            Debug.LogError("队伍列表数量和ui数量不匹配，或者选择角色列表数量与data角色数量不匹配");
            return;
        }

        //设置固定编队的mask
        if (stageData.fixedTeamIndexList.Count != 0)//有固定编队
        {
            if (stageData.fixedTeamIndexList.Count != characterData.teamIndexList.Count)
            {
                Debug.LogError("stageData固定编队数量与characterData数量不匹配");
                return;
            }

            for (int i = 0; i < characterData.data.Count; i++)
            {
                characterData.data[i].inTeam = false;
            }
            for (int i = 0; i < stageData.fixedTeamIndexList.Count; i++)
            {
                //if (stageData.fixedTeamIndexList[i] == -1)//空位置
                //{
                //    teamList[i].GetComponent<Image>().sprite = nullTeamSiteSprite;//设置编队空位
                //}
                //else//队内对应编号的角色的inTeam在队内为True
                //{
                //    teamList[i].GetComponent<Image>().sprite = characterData.data[stageData.fixedTeamIndexList[i]].teamSprite;
                //}
                int characterId = stageData.fixedTeamIndexList[i];
                characterData.teamIndexList[i] = characterId;
                if (characterId != -1)//如果不是编队空位
                {
                    characterData.data[characterId].inTeam = true;
                }
            }

            fixedTeamMask.SetActive(true);
        }
        else
        {
            fixedTeamMask.SetActive(false);
        }

        bool isEmpty = true;
        for (int i = 0; i < characterData.teamIndexList.Count; i++)//设置队内列表
        {
            if (characterData.teamIndexList[i] == -1)//空位置
            {
                teamList[i].GetComponent<Image>().sprite = nullTeamSiteSprite;//设置编队空位
            }
            else if (characterData.data[characterData.teamIndexList[i]].inTeam)//队内对应编号的角色的inTeam在队内为True
            {
                isEmpty = false;
                teamList[i].GetComponent<Image>().sprite = characterData.data[characterData.teamIndexList[i]].teamSprite;
            }
            else
            {
                Debug.Log("team数据错误,又不是空位置又没有对应的人");
            }
        }
        if (isEmpty)
            enterButton.SetActive(false);
        else
            enterButton.SetActive(true);
    }

    public void OpenCharacterListScroll(int siteIndex)//点击队伍位置时调用
    {
        currentTeamIndex = siteIndex;
        OpenCharacterListScroll();
    }

    private void OpenCharacterListScroll()
    {
        CharacterData.CharacterDataEntry character;
        for (int i = 0; i < characterData.data.Count; i++)//设置角色列表
        {
            character = characterData.data[i];
            if (character.unLocked)//角色已解锁
            {
                characterList[i].SetActive(true);
                characterList[i].transform.Find("RemoveMask").gameObject.SetActive(false);

                if (character.inTeam)//角色已经编入队伍内
                {
                    //打开TeamMask
                    characterList[i].transform.Find("TeamMask").gameObject.SetActive(true);
                }
                else
                {
                    characterList[i].transform.Find("TeamMask").gameObject.SetActive(false);
                }
            }
            else
            {
                characterList[i].SetActive(false);
            }
        }

        //给打开页面时点击的角色设置RemoveMask
        int characterIndex = characterData.teamIndexList[currentTeamIndex];
        if (characterIndex != -1)//如果不是编队空位
        {
            characterList[characterIndex].transform.Find("RemoveMask").gameObject.SetActive(true);
        }

        characterListScroll.GetComponent<UIPanel>().Open();
    }

    public void CloseCharacterListScroll(int selectCharacterIndex)//点击角色选择列表时调用
    {
        if (characterData.teamIndexList[currentTeamIndex] != -1)//如果不是编队空位
        {
            characterData.data[characterData.teamIndexList[currentTeamIndex]].inTeam = false;
        }
        characterData.data[selectCharacterIndex].inTeam = true;
        teamList[currentTeamIndex].GetComponent<Image>().sprite = characterData.data[selectCharacterIndex].teamSprite;
        characterData.teamIndexList[currentTeamIndex] = selectCharacterIndex;
        characterListScroll.GetComponent<UIPanel>().Close();

        enterButton.SetActive(true);
    }

    public void RemoveCharacterList()//给全部RemoveMask调用
    {
        teamList[currentTeamIndex].GetComponent<Image>().sprite = nullTeamSiteSprite;
        characterData.data[characterData.teamIndexList[currentTeamIndex]].inTeam = false;
        characterData.teamIndexList[currentTeamIndex] = -1;
        characterListScroll.GetComponent<UIPanel>().Close();

        bool isEmpty = true;
        for (int i = 0; i < characterData.teamIndexList.Count; i++)
        {
            if (characterData.teamIndexList[i] != -1)
            {
                isEmpty = false;
                break;
            }
        }
        if (isEmpty)
            enterButton.SetActive(false);
        else
            enterButton.SetActive(true);
    }

    public void EnterBattle()
    {
        SystemFacade.instance.EditTeamFinish();
    }
}
