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
        SystemFacade.instance.PlayBGM("սǰ��ӽ���", 0f, 0.5f, 0f);
        InitUI();
        editTeamPanel.Open();
    }

    public void InitUI()
    {
        //���ñ���
        ChapterMenu chapterMenu = gameObject.GetComponent<ChapterMenu>();
        if (chapterMenu.chapterData.isBranch)
            teamBG.sprite = gameObject.GetComponent<BranchMenu>().branchBG.sprite;
        else
            teamBG.sprite = chapterMenu.chapterBG.sprite;

        if (teamList.Count != characterData.teamIndexList.Count || characterList.Count != characterData.data.Count)
        {
            Debug.LogError("�����б�������ui������ƥ�䣬����ѡ���ɫ�б�������data��ɫ������ƥ��");
            return;
        }

        //���ù̶���ӵ�mask
        if (stageData.fixedTeamIndexList.Count != 0)//�й̶����
        {
            if (stageData.fixedTeamIndexList.Count != characterData.teamIndexList.Count)
            {
                Debug.LogError("stageData�̶����������characterData������ƥ��");
                return;
            }

            for (int i = 0; i < characterData.data.Count; i++)
            {
                characterData.data[i].inTeam = false;
            }
            for (int i = 0; i < stageData.fixedTeamIndexList.Count; i++)
            {
                //if (stageData.fixedTeamIndexList[i] == -1)//��λ��
                //{
                //    teamList[i].GetComponent<Image>().sprite = nullTeamSiteSprite;//���ñ�ӿ�λ
                //}
                //else//���ڶ�Ӧ��ŵĽ�ɫ��inTeam�ڶ���ΪTrue
                //{
                //    teamList[i].GetComponent<Image>().sprite = characterData.data[stageData.fixedTeamIndexList[i]].teamSprite;
                //}
                int characterId = stageData.fixedTeamIndexList[i];
                characterData.teamIndexList[i] = characterId;
                if (characterId != -1)//������Ǳ�ӿ�λ
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
        for (int i = 0; i < characterData.teamIndexList.Count; i++)//���ö����б�
        {
            if (characterData.teamIndexList[i] == -1)//��λ��
            {
                teamList[i].GetComponent<Image>().sprite = nullTeamSiteSprite;//���ñ�ӿ�λ
            }
            else if (characterData.data[characterData.teamIndexList[i]].inTeam)//���ڶ�Ӧ��ŵĽ�ɫ��inTeam�ڶ���ΪTrue
            {
                isEmpty = false;
                teamList[i].GetComponent<Image>().sprite = characterData.data[characterData.teamIndexList[i]].teamSprite;
            }
            else
            {
                Debug.Log("team���ݴ���,�ֲ��ǿ�λ����û�ж�Ӧ����");
            }
        }
        if (isEmpty)
            enterButton.SetActive(false);
        else
            enterButton.SetActive(true);
    }

    public void OpenCharacterListScroll(int siteIndex)//�������λ��ʱ����
    {
        currentTeamIndex = siteIndex;
        OpenCharacterListScroll();
    }

    private void OpenCharacterListScroll()
    {
        CharacterData.CharacterDataEntry character;
        for (int i = 0; i < characterData.data.Count; i++)//���ý�ɫ�б�
        {
            character = characterData.data[i];
            if (character.unLocked)//��ɫ�ѽ���
            {
                characterList[i].SetActive(true);
                characterList[i].transform.Find("RemoveMask").gameObject.SetActive(false);

                if (character.inTeam)//��ɫ�Ѿ����������
                {
                    //��TeamMask
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

        //����ҳ��ʱ����Ľ�ɫ����RemoveMask
        int characterIndex = characterData.teamIndexList[currentTeamIndex];
        if (characterIndex != -1)//������Ǳ�ӿ�λ
        {
            characterList[characterIndex].transform.Find("RemoveMask").gameObject.SetActive(true);
        }

        characterListScroll.GetComponent<UIPanel>().Open();
    }

    public void CloseCharacterListScroll(int selectCharacterIndex)//�����ɫѡ���б�ʱ����
    {
        if (characterData.teamIndexList[currentTeamIndex] != -1)//������Ǳ�ӿ�λ
        {
            characterData.data[characterData.teamIndexList[currentTeamIndex]].inTeam = false;
        }
        characterData.data[selectCharacterIndex].inTeam = true;
        teamList[currentTeamIndex].GetComponent<Image>().sprite = characterData.data[selectCharacterIndex].teamSprite;
        characterData.teamIndexList[currentTeamIndex] = selectCharacterIndex;
        characterListScroll.GetComponent<UIPanel>().Close();

        enterButton.SetActive(true);
    }

    public void RemoveCharacterList()//��ȫ��RemoveMask����
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
