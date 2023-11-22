using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using PixelCrushers;
using TMPro;
using DG.Tweening;
using UnityEngine.Events;
using static UnityEngine.EventSystems.EventTrigger;
using Unity.VisualScripting;

public class BattleSystem : MonoBehaviour
{
    //�������
    public SkillsManager skillsManager;
    public BuffManager buffManager;
    public List<CharacterInfoBtn> characterInfoUiList;
    public List<CharacterInfoBtn> enemyInfoUiList;
    public CharacterData characterData;
    public EnemyData enemyData;
    private StageData.StageDataEntry stageData;
    public List<Transform> characterEntityTransList;
    public List<Transform> enemyEntityTransList;
    public GameObject battlePanel;
    public GameObject battleScene;
    public GameObject entityObjContainer;
    public GameObject entityUiContainer;
    public ToggleGroup selectToggleGroup;
    public GameObject entityUI;
    public List<GameObject> menuPanelList;
    public GameObject continueButton;
    public GameObject backButton;
    public GameObject selectEntityPanel;
    public GameObject selectEntityMainMask;//�����ڷ�attackѡ��׶��ڵ�����ֹ���spine
    private BattleCommandType selectMenuType;
    public GameObject menusContainer;
    public List<Sprite> bpSpriteList;
    public GameObject skillMenuMask;//���skillButtonʱ��ȫ�����֣�ͬʱ����skillMenu
    public List<SkillObject> skillObjs;//����ͼ���б�
    private SkillSelectInfo skillSelectInfo;
    public GameObject buffDescribe;
    public GameObject skillDescribe;

    //˽��
    private bool inCommandExcuting;//�Ƿ�����ִ��ָ���������
    private bool inBattleStartExcuting;//�Ƿ���������ָ���������
    [ReadOnly]
    public int enemyNumber, characterNumber;
    [ReadOnly]
    private int characterCount;//��ɫ����
    [ReadOnly]
    public List<BattleEntity> allEntities = new List<BattleEntity>();
    private List<BattleEntity> sortEntities = new List<BattleEntity>();//�����ٶ�������entityIdList
    [ReadOnly]
    public int currentBattleId;//��ǰ����ָ���entity��battleId
    [ReadOnly]
    public int currentRound;
    [ReadOnly]
    public int currentMenuLastIndex;//Brave����������������menus��
    [ReadOnly]
    public int canSelectCount;
    [HideInInspector]
    public List<BattleCommandInfo> commandInfoList = new List<BattleCommandInfo>();
    private List<BattleCommandInfo> sortCommandInfoList = new List<BattleCommandInfo>();
    private List<IEnumerator> allCommandList = new List<IEnumerator>();
    private List<IEnumerator> battleStartAllCommandList = new List<IEnumerator>();
    public List<BattleEntity> selectedEntityList = new List<BattleEntity>();
    private List<Toggle> activedToggleList = new List<Toggle>();//���Խ���ѡ���Ŀ��toggle
    private UnityEvent RoundEndEvent = new UnityEvent();


    private void Start()
    {
        InitButtonCallback();//��ʼ��menu������button�¼���isDisable
        RoundEndEvent.AddListener(() =>
        {
            DoRoundEndBuffEffect();
            CheckDefault();
        });
    }

    public void StartBattle(StageData.StageDataEntry stageDataT)//��װ�ã���¶��facade
    {
        InitBattle(stageDataT);
    }

    private void InitBattle(StageData.StageDataEntry stageDataT)
    {
        stageData = stageDataT;
        SystemFacade.instance.PlayBGM("ս��a", 0f, 0.5f, 0f, () => { SystemFacade.instance.PlayBGM("ս��b", 0f, 0.5f, 0f); });
        currentRound = 1;
        InitEntity();//��ʼ����ɫ�͵�������
        InitUI();//��ʼ����ɫ�͵��˵�ui������վλ
        InitCommand();//���ָ���б�
        SortEntityList();
        UpdateAllEntityUiInfo();
        UpdateMenuInfo();
        StartCoroutine(AllEntityPlayEnterAnimation());
    }

    private void InitEntity()//��ʼ����ɫ�͵�������
    {
        List<int> teamList = characterData.teamIndexList;
        //��յ��˺ͽ�ɫentity�б�
        characterNumber = 0;
        characterCount = 0;
        enemyNumber = 0;
        allEntities.Clear();
        for (int i = 0; i < entityObjContainer.transform.childCount; i++)
        {
            Destroy(entityObjContainer.transform.GetChild(i).gameObject);
        }
        for (int i = 0; i < entityUiContainer.transform.childCount; i++)
        {
            Destroy(entityUiContainer.transform.GetChild(i).gameObject);
        }

        //���ý�ɫ
        if (characterEntityTransList.Count < teamList.Count)
        {
            Debug.LogError("�����ɫ�������ڿ���ս��λ������");
        }
        for (int i = 0; i < teamList.Count; i++)
        {
            if (teamList[i] != -1)
            {
                CharacterData.CharacterDataEntry characterDataPiece = characterData.data[teamList[i]];
                //���ɽ�ɫ
                GameObject characterObj = Instantiate(characterDataPiece.battleObjPrefab);
                //���ý�ɫվλ
                characterObj.transform.position = characterEntityTransList[i].position;
                characterObj.transform.SetParent(entityObjContainer.transform);


                //����entity����
                BattleEntity characterEntity = Instantiate(entityUI).GetComponent<BattleEntity>();
                characterEntity.transform.SetParent(entityUiContainer.transform);
                characterEntity.isDie = false;
                characterEntity.SetHp(characterDataPiece.maxHp);
                characterEntity.mp = 0;
                characterEntity.SetBp(characterDataPiece.originalBp);
                characterEntity.bpPreview = 0;
                characterEntity.braveCount = 0;
                characterEntity.hadUniqueSkill = false;
                characterEntity.hatred = 0;

                characterEntity.damage = characterDataPiece.damage;
                characterEntity.defend = characterDataPiece.defend;
                characterEntity.maxHp = characterDataPiece.maxHp;
                characterEntity.damageRate = characterDataPiece.damageRate;
                characterEntity.hurtRate = characterDataPiece.hurtRate;
                characterEntity.hatred = characterDataPiece.originalHatred;
                characterEntity.speed = characterDataPiece.speed;

                characterEntity.isEnemy = false;
                characterEntity.characterId = teamList[i];
                characterEntity.selectToggle = characterEntity.transform.Find("SelectToggle").GetComponent<Toggle>();
                characterEntity.animator = characterObj.GetComponent<Animator>();
                characterEntity.skillVfxAnim = characterObj.transform.Find("SkillVfx").GetComponent<Animator>();
                characterEntity.buffVfxAnim = characterObj.transform.Find("BuffVfx").GetComponent<Animator>();
                characterEntity.entityInfoUi = characterInfoUiList[i];
                characterEntity.entityObj = characterObj;
                characterEntity.meshRenderer = characterObj.GetComponent<MeshRenderer>();

                characterEntity.hurtToEntity = null;

                characterEntity.hpSlider.maxValue = characterDataPiece.maxHp;
                characterEntity.hpSlider.value = characterDataPiece.maxHp;
                characterEntity.selectToggle.isOn = false;

                //�����·���ɫ�б�
                characterEntity.entityInfoUi.portrait.sprite = characterDataPiece.battlePortrait;
                characterEntity.entityInfoUi.readyText.SetActive(false);
                characterEntity.entityInfoUi.hpSlider.maxValue = characterDataPiece.maxHp;
                characterEntity.entityInfoUi.hpSlider.value = characterDataPiece.maxHp;
                characterEntity.entityInfoUi.mpSlider.maxValue = 100;
                characterEntity.entityInfoUi.mpSlider.value = 0;
                characterEntity.entityInfoUi.bpText.sprite = bpSpriteList[0 + 4];
                characterEntity.entityInfoUi.healthPoint.text = "0";
                characterEntity.entityInfoUi.magicPoint.text = "0";

                //��װui
                FollowWorldObj followScript = characterEntity.gameObject.AddComponent<FollowWorldObj>();
                followScript.objFollowed = characterObj.transform.Find("FollowPoint");
                followScript.rectFollower = characterEntity.GetComponent<RectTransform>();
                followScript.offset = new Vector2(0f, 15f);
                characterEntity.EntityInit();

                //һЩ��ʼ����
                EntityOutline(characterEntity, false);

                //�ӽ�entityList
                characterNumber++;
                characterCount++;
                allEntities.Add(characterEntity);
            }
        }

        //���ɵ���
        if (stageData.enemyTeam.Count > enemyEntityTransList.Count)
        {
            Debug.LogError("��������λ��");
        }
        for (int i = 0; i < stageData.enemyTeam.Count; i++)
        {
            if (stageData.enemyTeam[i] == -1)
            {
                continue;
            }
            EnemyData.EnemyDataEntry enemyDataPiece = enemyData.data[stageData.enemyTeam[i]];
            //���ɵ���
            GameObject enemyObj = Instantiate(enemyDataPiece.battleObjPrefab);
            //���õ���վλ
            enemyObj.transform.position = enemyEntityTransList[i].position;
            enemyObj.transform.SetParent(entityObjContainer.transform);

            //����entity����
            BattleEntity enemyEntity = Instantiate(entityUI).GetComponent<BattleEntity>();//======================================
            enemyEntity.transform.SetParent(entityUiContainer.transform);
            enemyEntity.isDie = false;
            enemyEntity.SetHp(enemyDataPiece.maxHp);
            enemyEntity.mp = 0;
            enemyEntity.hatred = 0;

            enemyEntity.damage = enemyDataPiece.damage;
            enemyEntity.defend = enemyDataPiece.defend;
            enemyEntity.maxHp = enemyDataPiece.maxHp;
            enemyEntity.damageRate = enemyDataPiece.damageRate;
            enemyEntity.hurtRate = enemyDataPiece.hurtRate;
            enemyEntity.speed = enemyDataPiece.speed;

            enemyEntity.isEnemy = true;
            enemyEntity.selectToggle = enemyEntity.transform.Find("SelectToggle").GetComponent<Toggle>();
            enemyEntity.animator = enemyObj.GetComponent<Animator>();
            enemyEntity.skillVfxAnim = enemyObj.transform.Find("SkillVfx").GetComponent<Animator>();
            enemyEntity.buffVfxAnim = enemyObj.transform.Find("BuffVfx").GetComponent<Animator>();
            enemyEntity.entityInfoUi = enemyInfoUiList[i];
            enemyEntity.entityObj = enemyObj;
            enemyEntity.meshRenderer = enemyObj.GetComponent<MeshRenderer>();
            enemyEntity.enemyDataEntry = enemyDataPiece;

            enemyEntity.hurtToEntity = null;

            enemyEntity.hpSlider.maxValue = enemyDataPiece.maxHp;
            enemyEntity.hpSlider.value = enemyDataPiece.maxHp;
            enemyEntity.selectToggle.isOn = false;

            //�������Ͻǵ����б�
            //enemyEntity.entityInfoUi.portrait.sprite = enemyDataPiece.battlePortrait;
            //enemyEntity.entityInfoUi.readyText.SetActive(false);
            enemyEntity.entityInfoUi.hpSlider.maxValue = enemyDataPiece.maxHp;
            enemyEntity.entityInfoUi.hpSlider.value = enemyDataPiece.maxHp;
            enemyEntity.entityInfoUi.mpSlider.maxValue = 100;
            enemyEntity.entityInfoUi.mpSlider.value = 0;
            enemyEntity.entityInfoUi.healthPoint.text = "0";
            enemyEntity.entityInfoUi.magicPoint.text = "0";

            //��װui
            FollowWorldObj followScript = enemyEntity.gameObject.AddComponent<FollowWorldObj>();
            followScript.objFollowed = enemyObj.transform.Find("FollowPoint");
            followScript.rectFollower = enemyEntity.GetComponent<RectTransform>();
            followScript.offset = new Vector2(0f, 15f);
            enemyEntity.EntityInit();

            //һЩ��ʼ����
            EntityOutline(enemyEntity, false);

            //�ӽ�entityList
            enemyNumber++;
            allEntities.Add(enemyEntity);
        }
    }

    public IEnumerator AllEntityPlayEnterAnimation()
    {
        yield return new WaitForSeconds(1f);
        for (int i = 0; i < allEntities.Count; i++)
        {
            try
            {
                StartCoroutine(Utility.PlayAnimation(allEntities[i].animator, "start"));
            }
            catch (System.Exception)
            {
            }
        }
        yield return new WaitForSeconds(Utility.GetAnimatorLength(allEntities[0].animator, "start"));
    }

    private void InitCommand()//��ʼ����ÿ�ֶ���������Ϻ����
    {
        inCommandExcuting = false;
        inBattleStartExcuting = false;

        for (int i = 0; i < allEntities.Count; i++)
        {
            if (!allEntities[i].isDie)
            {
                currentBattleId = i;//����һ������entity����ΪcurrentBattleId
                break;
            }
        }

        for (int i = 0; i < allEntities.Count; i++)//�������entity��ָ��List
        {
            allEntities[i].entityCommandList.Clear();
            allEntities[i].entitybattleStartCommandList.Clear();
        }

        commandInfoList.Clear();
        sortCommandInfoList.Clear();
        allCommandList.Clear();
        battleStartAllCommandList.Clear();

        continueButton.GetComponent<Animator>().Play("Idle");
        backButton.gameObject.SetActive(false);
        for (int i = 0; i < allEntities.Count; i++)
        {
            if (allEntities[i].GetBp() < 3)
            {
                allEntities[i].UpdateBp(1);
            }

            allEntities[i].bpPreview = allEntities[i].GetBp();
            allEntities[i].braveCount = 0;
            allEntities[i].hadUniqueSkill = false;
        }

        currentMenuLastIndex = -1;
        SetCommand();
    }

    private void InitUI()
    {
        for (int i = 1; i < menuPanelList.Count; i++)//�رն���Ľ���
        {
            menuPanelList[i].SetActive(false);
            menuPanelList[i].GetComponent<RectTransform>().anchoredPosition = new Vector2(500f, 0f);
            menuPanelList[i].transform.Find("MenuMask").gameObject.SetActive(false);
        }

        List<int> teamList = characterData.teamIndexList;//�����·���ɫ�б�
        for (int i = 0; i < teamList.Count; i++)
        {
            if (teamList[i] != -1)
            {
                characterInfoUiList[i].gameObject.SetActive(true);
                buffManager.ClearBuff(characterInfoUiList[i]);
            }
            else
            {
                characterInfoUiList[i].gameObject.SetActive(false);
            }
        }
        for (int i = teamList.Count; i < characterInfoUiList.Count; i++)
        {
            characterInfoUiList[i].gameObject.SetActive(false);
        }
        //�������Ͻǵ����б�
        for (int i = 0; i < stageData.enemyTeam.Count; i++)
        {
            if (stageData.enemyTeam[i] != -1)
            {
                enemyInfoUiList[i].gameObject.SetActive(true);
                buffManager.ClearBuff(enemyInfoUiList[i]);
            }
            else
            {
                enemyInfoUiList[i].gameObject.SetActive(false);
            }
        }
        for (int i = stageData.enemyTeam.Count; i < enemyInfoUiList.Count; i++)
        {
            enemyInfoUiList[i].gameObject.SetActive(false);
        }

        battlePanel.GetComponent<UIPanel>().Open();
        battleScene.SetActive(true);

        InitEntityToggleList();//��ʼ��entity�ĸ�ѡ��
    }

    private void InitButtonCallback()//��ʼ��menu������button�¼���isDisable
    {
        for (int i = 0; i < menuPanelList.Count; i++)
        {
            menuPanelList[i].transform.Find("Buttons").Find("Attack").GetComponent<BattleMenuButton>().SetEnable(() => AttackButton());
            menuPanelList[i].transform.Find("Buttons").Find("Brave").GetComponent<BattleMenuButton>().SetEnable(() => BraveButton());
            menuPanelList[i].transform.Find("Buttons").Find("Skill").GetComponent<BattleMenuButton>().SetEnable(() => SkillButton());
            menuPanelList[i].transform.Find("Buttons").Find("UniqueSkill").GetComponent<BattleMenuButton>().SetDisable();
            menuPanelList[i].transform.Find("Buttons").Find("Default").GetComponent<BattleMenuButton>().SetDisable();
        }
        menuPanelList[0].transform.Find("Buttons").Find("Default").GetComponent<BattleMenuButton>().SetEnable(() => DefaultButton());
        menuPanelList[3].transform.Find("Buttons").Find("Brave").GetComponent<BattleMenuButton>().SetDisable();

        //��skillMenuMask��ӻص�
        Utility.AddTriggersListener(skillMenuMask.GetComponent<EventTrigger>(), EventTriggerType.PointerClick, new UnityAction<BaseEventData>(CloseSkillMenu));
        for (int i = 0; i < skillObjs.Count; i++)
        {
            int index = i;
            skillObjs[index].GetComponent<Button>().onClick.AddListener(() => { SkillMenuButton(index); });
        }

        selectEntityPanel.transform.Find("SwitchCameraToggle").GetComponent<Toggle>().onValueChanged.AddListener(SwitchCameraToggle);
        SystemFacade.instance.SwitchObjCamera(CameraManager.ObjCameraState.IdleCommand, 0f);
    }

    private void InitEntityToggleList()//��ʼ��entity�ĸ�ѡ��
    {
        selectEntityPanel.SetActive(false);
        selectEntityMainMask.SetActive(true);
        activedToggleList.Clear();
        for (int i = 0; i < allEntities.Count; i++)
        {
            int index = i;
            allEntities[index].selectToggle.onValueChanged.AddListener((isOn) => SelectEntity(isOn, allEntities[index]));
        }
        ResetSelectEntity();
    }

    private void SortEntityList()//����entity��Speed��allEntities����
    {
        sortEntities.Clear();
        for (int i = 0; i < allEntities.Count; i++)
        {
            sortEntities.Add(allEntities[i]);
        }

        sortEntities.Sort((x, y) => { return y.speed.CompareTo(x.speed); });
        //��������
        string log = "������character˳��";
        for (int i = 0; i < sortEntities.Count; i++)
        {
            if (!sortEntities[i].isEnemy)
            {
                log += sortEntities[i].characterId + 1 + "--";
            }
        }
        Debug.Log(log);
    }
    private void SortCommandInfoList()//����entity��Speed��CommandInfoList����
    {
        sortCommandInfoList.Clear();
        for (int i = 0; i < commandInfoList.Count; i++)
        {
            sortCommandInfoList.Add(commandInfoList[i]);
        }
        sortCommandInfoList.Sort((x, y) => { return allEntities[y.battleId].speed.CompareTo(allEntities[x.battleId].speed); });
        //��������
        string originalLog = "����ǰ��commandInfoList˳��";
        for (int i = 0; i < commandInfoList.Count; i++)
        {
            originalLog += "\n" + commandInfoList[i].battleId + "--��ָ��--" + commandInfoList[i].commandType;
        }
        string newLog = "������sortCommandInfoList˳��";
        for (int i = 0; i < sortCommandInfoList.Count; i++)
        {
            newLog += "\n" + sortCommandInfoList[i].battleId + "--��ָ��--" + sortCommandInfoList[i].commandType;
        }
        Debug.Log(originalLog + "\n" + newLog);

        //�������entity������ָ�
        originalLog = "";
        for (int i = 0; i < allEntities.Count; i++)
        {
            originalLog += "\n" + i + "������ָ��Ϊ\n";
            for (int j = 0; j < allEntities[i].entitybattleStartCommandList.Count; j++)
            {
                originalLog += j + ".->" + allEntities[i].entitybattleStartCommandList[j] + "\n";
            }
        }
        Debug.Log("ȫ��entity������ָ�" + originalLog);

        //�������entity��ָ�
        originalLog = "";
        for (int i = 0; i < allEntities.Count; i++)
        {
            originalLog += "\n" + i + "��ָ��Ϊ\n";
            for (int j = 0; j < allEntities[i].entityCommandList.Count; j++)
            {
                originalLog += j + ".->" + allEntities[i].entityCommandList[j] + "\n";
            }
        }
        Debug.Log("ȫ��entity��ָ�" + originalLog);
    }

    private void SetCommand()
    {
        StartCoroutine(ISetCommand());
    }
    IEnumerator ISetCommand()
    {
        bool isSwitchCharacter = false;
        int lastEntityId = currentBattleId;
        yield return new WaitForSeconds(0.1f);

        if (commandInfoList.Count != 0)//����back��ť
        {
            backButton.SetActive(true);
        }

        if (currentMenuLastIndex == 0)//����menu�ر�
        {
            CharacterReadyEvent(lastEntityId);
            Tweener tweener = menuPanelList[0].GetComponent<RectTransform>().DOAnchorPosX(500f, 0.3f);
            yield return tweener.WaitForCompletion();
            for (int i = 0; i < menuPanelList.Count; i++)
            {
                menuPanelList[i].SetActive(false);
            }

            currentMenuLastIndex = -1;

            currentBattleId++;
            for (int i = currentBattleId; i < characterCount + 1; i++)//ѡ��û������bp������û��ѣ��buff�������ȵĽ�ɫ
            {
                if (!allEntities[currentBattleId].isDie && allEntities[currentBattleId].GetBp() >= 0)
                {
                    if (buffManager.CheckBuff(allEntities[currentBattleId].entityInfoUi, BuffName.ѣ��).Count <= 0)
                    {
                        break;
                    }
                }
                currentBattleId++;
            }
            isSwitchCharacter = true;
            //Debug.Log(currentBallteId);
        }
        else if (currentMenuLastIndex > 0)
        {
            menuPanelList[currentMenuLastIndex].GetComponent<RectTransform>().DOAnchorPos(new Vector2(500f, 0f), 0.3f);

            currentMenuLastIndex--;
            for (int i = 0; i < currentMenuLastIndex + 1; i++)
            {
                float originalX = menuPanelList[i].GetComponent<RectTransform>().anchoredPosition.x;
                menuPanelList[i].GetComponent<RectTransform>().DOAnchorPosX(originalX + 20f, 0.3f);
            }
        }
        else if (currentMenuLastIndex == -1)//�տ�ʼ��Ϸ���ݳ�����ʱΪ-1
        {
            for (int i = currentBattleId; i < characterCount + 1; i++)//ѡ��û������bp������û��ѣ��buff�������ȵĽ�ɫ
            {
                if (!allEntities[currentBattleId].isDie && allEntities[currentBattleId].GetBp() >= 0)
                {
                    if (buffManager.CheckBuff(allEntities[currentBattleId].entityInfoUi, BuffName.ѣ��).Count <= 0)
                    {
                        break;
                    }
                }
                currentBattleId++;
            }
        }


        if (currentBattleId >= characterCount)//��ʱcurrentBallteId�Ǳ�allEntity�����index��1
        {
            currentBattleId--;
            if (characterNumber > 0)//������Ϸ���Ӯ����ʾ
            {
                for (int i = 0; i < characterInfoUiList.Count; i++)
                {
                    if (!characterInfoUiList[i].gameObject.activeInHierarchy)
                    {
                        continue;
                    }
                    AnimatorStateInfo stateinfo = characterInfoUiList[i].animator.GetCurrentAnimatorStateInfo(0);
                    if (stateinfo.IsName("SelectedBgOpen"))
                    {
                        characterInfoUiList[i].CloseSelectedBg();
                        break;
                    }
                }

                continueButton.GetComponent<Animator>().SetTrigger("open");//��ʾContinueButton
            }

            for (int i = 0; i < menuPanelList.Count; i++)
            {
                menuPanelList[i].transform.Find("Buttons").Find("Brave").GetComponent<BattleMenuButton>().SetEnable(() => BraveButton());
            }
            menuPanelList[3].transform.Find("Buttons").Find("Brave").GetComponent<BattleMenuButton>().SetDisable();
            menuPanelList[0].transform.Find("Buttons").Find("Default").GetComponent<BattleMenuButton>().SetEnable(() => DefaultButton());
        }
        else//��ǰ���ǽ�ɫ
        {
            AnimatorStateInfo stateinfo;
            if (isSwitchCharacter)
            {
                for (int i = 0; i < characterInfoUiList.Count; i++)
                {
                    if (characterInfoUiList[i].gameObject.activeInHierarchy)
                    {
                        stateinfo = characterInfoUiList[i].animator.GetCurrentAnimatorStateInfo(0);
                        if (i != currentBattleId && stateinfo.IsName("SelectedBgOpen"))
                        {
                            characterInfoUiList[i].CloseSelectedBg();
                            break;
                        }
                    }
                }
                characterInfoUiList[currentBattleId].OpenSelectedBg();
            }

            if (currentMenuLastIndex == -1)//��ǰmenuһ����û��
            {
                menuPanelList[0].GetComponent<RectTransform>().anchoredPosition = new Vector2(500f, 0f);
                menuPanelList[0].SetActive(true);
                menuPanelList[0].GetComponent<RectTransform>().DOAnchorPos(new Vector2(0f, 0f), 0.3f);
                currentMenuLastIndex = 0;

                for (int i = 0; i < menuPanelList.Count; i++)
                {
                    menuPanelList[i].transform.Find("Buttons").Find("Brave").GetComponent<BattleMenuButton>().SetEnable(() => BraveButton());
                    if (allEntities[currentBattleId].mp >= 100)
                    {
                        menuPanelList[i].transform.Find("Buttons").Find("UniqueSkill").GetComponent<BattleMenuButton>().SetEnable(() => { UniqueSkillButton(); });
                    }
                    else
                    {
                        menuPanelList[i].transform.Find("Buttons").Find("UniqueSkill").GetComponent<BattleMenuButton>().SetDisable();
                    }
                }
                menuPanelList[3].transform.Find("Buttons").Find("Brave").GetComponent<BattleMenuButton>().SetDisable();
                menuPanelList[0].transform.Find("Buttons").Find("Default").GetComponent<BattleMenuButton>().SetEnable(() => DefaultButton());
            }
            else//�����ǰ���д򿪵�menu��˵����brave�׶�
            {
                for (int i = 0; i < menuPanelList.Count - 1; i++)
                {
                    menuPanelList[i].transform.Find("Buttons").Find("Brave").GetComponent<BattleMenuButton>().SetDisable();
                }
                menuPanelList[0].transform.Find("Buttons").Find("Default").GetComponent<BattleMenuButton>().SetDisable();
            }

            UpdateMenuInfo();
            UpdateAllEntityUiInfo();
        }
    }

    private void CharacterReadyEvent(int entityId)//��ɫ�������ָ��󣬵��¼�
    {
        if (commandInfoList[commandInfoList.Count - 1].commandType == BattleCommandType.Default)
        {
            allEntities[entityId].animator.Play("default_start");
            allEntities[entityId].animator.SetBool("default", true);
        }
        else
        {
            allEntities[entityId].animator.Play("ready_start");
            allEntities[entityId].animator.SetBool("ready", true);
        }
    }
    private void CharacterUnreadyEvent(int entityId)//��ɫȡ����������ɵ�ָ��󣬵��¼�
    {
        allEntities[entityId].animator.Play("idle");
        allEntities[entityId].animator.SetBool("ready", false);
        allEntities[entityId].animator.SetBool("default", false);
    }

    private void EntityOutline(BattleEntity entity, bool actived)
    {
        //MaterialPropertyBlock materialPB = new MaterialPropertyBlock();
        //if (actived)
        //{
        //    materialPB.SetFloat("_OutlineReferenceTexWidth", 1024);
        //}
        //else
        //{
        //    materialPB.SetFloat("_OutlineReferenceTexWidth", 0);
        //}
        //for (int i = 0; i < entity.meshRenderer.materials.Length; i++)
        //{
        //    entity.meshRenderer.SetPropertyBlock(materialPB);
        //}

        if (actived)
        {
            entity.outlineComponent.enabled = true;
        }
        else
        {
            entity.outlineComponent.enabled = false;
        }
    }

    public void ContinueButton()
    {
        continueButton.GetComponent<Animator>().SetTrigger("close");
        backButton.SetActive(false);

        currentBattleId++;
        EnemySetCommandAI();
    }

    private void EnemySetCommandAI()//��������ָ���ai
    {
        List<BattleEntity> liveCharacters = new List<BattleEntity>();
        for (int i = 0; i < allEntities.Count; i++)
        {
            if (!allEntities[i].isEnemy && !allEntities[i].isDie)
            {
                liveCharacters.Add(allEntities[i]);
                break;
            }
        }
        if (liveCharacters.Count == 0)
        {
            Debug.LogError("�����Ҳ���û���Ľ�ɫ");
        }
        //��hatred�趨ѡȡ����
        List<BattleEntity> liveCharactersForHatredList = new List<BattleEntity>();
        for (int i = 0; i < liveCharacters.Count; i++)
        {
            for (int j = 0; j < liveCharacters[i].hatred; j++)
            {
                liveCharactersForHatredList.Add(liveCharacters[i]);
            }
        }
        //Debug.Log(liveCharactersForHatredList.Count);
        BattleEntity characterEntity = liveCharactersForHatredList[Random.Range(0, liveCharactersForHatredList.Count)];

        //�Ļغ�����һ�μ���1
        if (currentRound % 4 == 0)
        {
            allEntities[currentBattleId].entityCommandList.Add(skillsManager.EnemyASkill1(allEntities[currentBattleId], characterEntity));
            commandInfoList.Add(new BattleCommandInfo(true, BattleCommandType.Skill, false, 0, new List<BattleEntity> { characterEntity }, currentBattleId));
        }
        else
        {
            allEntities[currentBattleId].entityCommandList.Add(EnemyAttack(currentBattleId, new List<BattleEntity> { characterEntity }));
            commandInfoList.Add(new BattleCommandInfo(true, BattleCommandType.Attack, false, 0, new List<BattleEntity> { characterEntity }, currentBattleId));
        }
        Debug.Log("ʵ��" + currentBattleId + "������ָ��" + allEntities[currentBattleId].entityCommandList[allEntities[currentBattleId].entityCommandList.Count - 1] + "��commandInfo������Ϊ" + commandInfoList.Count);

        currentBattleId++;

        if (currentBattleId == allEntities.Count)//��ʱcurrentBallteId�Ǳ�allEntity�����index��1
        {
            SetCommandReachEnd();
            return;
        }

        EnemySetCommandAI();
    }

    private void SetCommandReachEnd()//��������entity��������ָ����
    {
        Debug.Log("��������entity��������ָ����");
        PrepareAllCommandList();
    }

    private void PrepareAllCommandList()
    {
        StartCoroutine(IPrepareAllCommandList());
    }
    IEnumerator IPrepareAllCommandList()
    {
        SystemFacade.instance.SwitchObjCamera(CameraManager.ObjCameraState.IdleAnimation, 0.3f);
        yield return new WaitForSeconds(0.3f);

        //���ٶ�����Ȼ���������entity��commandList�ӽ�allCommandList
        for (int i = 0; i < sortEntities.Count; i++)
        {
            if (!sortEntities[i].isDie)
            {
                //Debug.Log(sortEntities[i].entitybattleStartCommandList.Count);
                for (int j = 0; j < sortEntities[i].entityCommandList.Count; j++)
                {
                    allCommandList.Add(sortEntities[i].entityCommandList[j]);
                }
                for (int j = 0; j < sortEntities[i].entitybattleStartCommandList.Count; j++)
                {
                    //Debug.Log(sortEntities[i].entitybattleStartCommandList[j]);
                    battleStartAllCommandList.Add(sortEntities[i].entitybattleStartCommandList[j]);
                }
            }
        }

        SortCommandInfoList();//��commandInfoList����
        ExcuteBattleStartCommandList();
    }

    public void ExcuteBattleStartCommandList()//ִ������ָ��
    {
        //Debug.Log(battleStartAllCommandList.Count);
        if (!inBattleStartExcuting)
        {
            inBattleStartExcuting = true;
        }
        if (battleStartAllCommandList.Count == 0)//ָ��ȫ��ִ�����
        {
            ExcuteBattleStartCommandReachEnd();//��ǰ�ִε�ָ���б�ȫ��ִ�����
            return;
        }

        BattleEntity entity = allEntities[sortCommandInfoList[0].battleId];
        if (buffManager.CheckBuff(entity.entityInfoUi, BuffName.ѣ��).Count > 0)
        {
            Debug.Log(sortCommandInfoList[0].battleId + "��Ϊѣ�Σ�����ִ������ָ��" + battleStartAllCommandList[0]);
            battleStartAllCommandList.RemoveAt(0);
            ExcuteBattleStartCommandList();
        }
        else
        {
            Debug.Log(sortCommandInfoList[0].battleId + "ִ������ָ��" + battleStartAllCommandList[0]);
            StartCoroutine(battleStartAllCommandList[0]);
            battleStartAllCommandList.RemoveAt(0);
            if ((sortCommandInfoList.Count == 1) || (sortCommandInfoList[0].battleId != sortCommandInfoList[1].battleId))
            {
                if (!allEntities[sortCommandInfoList[0].battleId].isDie)
                {
                    allEntities[sortCommandInfoList[0].battleId].animator.SetBool("ready", false);
                }
            }
        }
        //sortCommandInfoList.RemoveAt(0);//����ɾ���������Ǹ���ռλ�ģ���������ָ���info������ָ��û��info
    }
    private void ExcuteBattleStartCommandReachEnd()//��ǰ�غϵ�����ָ���б�ȫ��ִ�����
    {
        inBattleStartExcuting = false;
        Debug.Log("����ָ��ȫ��ִ�����");
        UpdateAllEntityUiInfo();
        Debug.Log("��ʼִ�г���ָ��");
        ExcuteCommandList();//ִ��ָ���б���еĶ���
        inCommandExcuting = true;
    }
    public void ExcuteCommandList()//ִ��ָ���б���еĶ���
    {
        StartCoroutine(IExcuteCommandList());
    }
    IEnumerator IExcuteCommandList()
    {
        yield return new WaitForSeconds(0.7f);

        if (!inCommandExcuting)
        {
            inCommandExcuting = true;
        }
        if (allCommandList.Count == 0)//ָ��ȫ��ִ�����
        {
            ExcuteCommandReachEnd();//��ǰ�ִε�ָ���б�ȫ��ִ�����
            UpdateAllEntityUiInfo();
            yield break;
        }

        BattleEntity entity = allEntities[sortCommandInfoList[0].battleId];
        if (buffManager.CheckBuff(entity.entityInfoUi, BuffName.ѣ��).Count > 0)
        {
            Debug.Log(sortCommandInfoList[0].battleId + "��Ϊѣ�Σ�����ִ��ָ��" + allCommandList[0]);
            SkipCommand();
            yield break;
        }
        else
        {
            //���bp����������
            if (allEntities[sortCommandInfoList[0].battleId].GetBp() - sortCommandInfoList[0].bpNeed < -4)
            {
                Debug.Log(sortCommandInfoList[0].battleId + "��Ϊbp����������ִ��ָ��" + allCommandList[0]);
                SkipCommand();
                yield break;
            }

            Debug.Log(sortCommandInfoList[0].battleId + "ִ��ָ��" + allCommandList[0]);

            StartCoroutine(allCommandList[0]);
            allCommandList.RemoveAt(0);
            if ((sortCommandInfoList.Count == 1) || (sortCommandInfoList[0].battleId != sortCommandInfoList[1].battleId))
            {
                if (!allEntities[sortCommandInfoList[0].battleId].isDie)
                {
                    allEntities[sortCommandInfoList[0].battleId].animator.SetBool("ready", false);
                }
            }
            sortCommandInfoList.RemoveAt(0);
        }
        UpdateAllEntityUiInfo();
    }
    private void SkipCommand()
    {
        allCommandList.RemoveAt(0);
        sortCommandInfoList.RemoveAt(0);
        ExcuteCommandList();
    }
    private void ExcuteCommandReachEnd()//��ǰ�ִε�ָ���б�ȫ��ִ�����
    {
        currentRound++;
        RoundEndEvent.Invoke();
        Debug.Log("ָ��ȫ��ִ�����");

        InitCommand();
        for (int i = 0; i < menuPanelList.Count - 1; i++)//��Ϊ���һ��menu���Բ�����Brave������Ҫ-1
        {
            menuPanelList[i].transform.Find("Buttons").Find("Brave").GetComponent<BattleMenuButton>().SetEnable(() => BraveButton());
        }
        menuPanelList[0].transform.Find("Buttons").Find("Default").GetComponent<BattleMenuButton>().SetEnable(() => DefaultButton());

        SystemFacade.instance.SwitchObjCamera(CameraManager.ObjCameraState.IdleCommand);

        bool otherCondition = true;
        for (int i = 0; i < allEntities.Count; i++)//�ݳ��������л�����
        {
            if (!allEntities[i].isDie && otherCondition)
            {
                allEntities[i].animator.Play("idle");
                allEntities[i].animator.SetBool("default", false);
                allEntities[i].animator.SetBool("ready", false);
            }
        }

        UpdateAllEntityUiInfo();
        UpdateMenuInfo();
    }

    public void DefaultButton()
    {
        //��������
        backButton.GetComponent<Button>().interactable = false;
        for (int i = 0; i < menuPanelList.Count; i++)
        {
            menuPanelList[i].transform.Find("MenuMask").gameObject.SetActive(true);
        }

        allEntities[currentBattleId].entitybattleStartCommandList.Add(CharacterDefend(currentBattleId));
        allEntities[currentBattleId].entityCommandList.Add(BattleStartCommandInCommandList(currentBattleId));//ռλ
        commandInfoList.Add(new BattleCommandInfo(false, BattleCommandType.Default, true, 0, null, currentBattleId));
        Debug.Log("ʵ��" + currentBattleId + "������ָ��" + allEntities[currentBattleId].entityCommandList[allEntities[currentBattleId].entityCommandList.Count - 1] + "��commandInfo������Ϊ" + commandInfoList.Count);

        SetCommand();

        //��������
        backButton.GetComponent<Button>().interactable = true;
        for (int i = 0; i < menuPanelList.Count; i++)
        {
            menuPanelList[i].transform.Find("MenuMask").gameObject.SetActive(false);
        }
    }
    private void CheckDefault()//ÿ�غϽ���ʱ���default
    {
        for (int i = 0; i < commandInfoList.Count; i++)
        {
            if (commandInfoList[i].commandType == BattleCommandType.Default)
            {
                if (!allEntities[commandInfoList[i].battleId].isDie)
                {
                    allEntities[commandInfoList[i].battleId].defend -= 500;
                }
            }
        }
    }
    public void BraveButton()//��ִ������¼�˵��Brave���ʱbraveCount��û����4
    {
        //��������
        backButton.GetComponent<Button>().interactable = false;
        for (int i = 0; i < menuPanelList.Count; i++)
        {
            menuPanelList[i].transform.Find("MenuMask").gameObject.SetActive(true);
        }

        StartCoroutine(Utility.PlayAnimation(allEntities[currentBattleId].buffVfxAnim, "Brave"));

        currentMenuLastIndex++;

        for (int i = 0; i < currentMenuLastIndex; i++)//�Ѵ�����ָ���menuȫ�������ƶ�
        {
            float originalX = menuPanelList[i].GetComponent<RectTransform>().anchoredPosition.x;
            menuPanelList[i].GetComponent<RectTransform>().DOAnchorPosX(originalX - 20f, 0.3f);
        }

        menuPanelList[currentMenuLastIndex].GetComponent<RectTransform>().anchoredPosition = new Vector2(500f, 0f);
        menuPanelList[currentMenuLastIndex].SetActive(true);
        menuPanelList[currentMenuLastIndex].GetComponent<RectTransform>().DOAnchorPos(new Vector2(0f, 0f), 0.3f);

        allEntities[currentBattleId].entityCommandList.Add(CharacterBrave(currentBattleId));
        commandInfoList.Add(new BattleCommandInfo(false, BattleCommandType.Brave, false, 1, null, currentBattleId));
        Debug.Log("ʵ��" + currentBattleId + "������ָ��" + allEntities[currentBattleId].entityCommandList[allEntities[currentBattleId].entityCommandList.Count - 1] + "��commandInfo������Ϊ" + commandInfoList.Count);

        if (allEntities[currentBattleId].braveCount > 0)
        {
            allEntities[currentBattleId].bpPreview--;
        }
        else if (allEntities[currentBattleId].braveCount == 0)//bp{review=-4�Ͳ���brave
        {
            allEntities[currentBattleId].bpPreview -= 2;
        }

        if (allEntities[currentBattleId].bpPreview <= -4)
        {
            for (int i = 0; i < menuPanelList.Count; i++)
            {
                menuPanelList[i].transform.Find("Buttons").Find("Brave").GetComponent<BattleMenuButton>().SetDisable();
            }
        }

        allEntities[currentBattleId].braveCount++;
        UpdateMenuInfo();
        backButton.SetActive(true);

        //��������
        backButton.GetComponent<Button>().interactable = true;
        for (int i = 0; i < menuPanelList.Count; i++)
        {
            menuPanelList[i].transform.Find("MenuMask").gameObject.SetActive(false);
        }
    }

    public void BackButton()//����
    {
        StartCoroutine(IBackButton());
    }
    IEnumerator IBackButton()
    {
        //��������
        backButton.GetComponent<Button>().interactable = false;
        for (int i = 0; i < menuPanelList.Count; i++)
        {
            menuPanelList[i].transform.Find("MenuMask").gameObject.SetActive(true);
        }

        Tweener tweener;
        //�жϵ�ǰcontinueButton�Ƿ���open״̬
        AnimatorStateInfo stateinfo = continueButton.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0);
        if (stateinfo.IsName("ContinueButtonOpen"))
        {
            continueButton.GetComponent<Animator>().SetTrigger("close");
            CharacterUnreadyEvent(currentBattleId);
        }

        if (commandInfoList.Count > 1 && currentBattleId != commandInfoList[commandInfoList.Count - 1].battleId)//������һ��ָ�����Ҫ�л���ɫ
        {
            currentBattleId = commandInfoList[commandInfoList.Count - 1].battleId;//��һ������ָ��Ľ�ɫ

            CharacterUnreadyEvent(currentBattleId);

            tweener = menuPanelList[0].GetComponent<RectTransform>().DOAnchorPosX(500f, 0.3f);
            yield return tweener.WaitForCompletion();
            menuPanelList[0].SetActive(false);

            characterInfoUiList[currentBattleId].OpenSelectedBg();
            if (currentBattleId + 1 < characterInfoUiList.Count)
            {
                characterInfoUiList[currentBattleId + 1].CloseSelectedBg();
            }
        }
        else if (commandInfoList.Count == 1 && currentBattleId != commandInfoList[0].battleId/*commandInfoList[0].commandType != BattleCommandType.Brave && characterNumber > 1*/)//��������ĵ�һ��ָ����Ҳ���brave
        {
            currentBattleId = commandInfoList[0].battleId;
            CharacterUnreadyEvent(currentBattleId);
            Debug.Log(currentBattleId);

            tweener = menuPanelList[0].GetComponent<RectTransform>().DOAnchorPosX(500f, 0.3f);
            yield return tweener.WaitForCompletion();
            menuPanelList[0].SetActive(false);
            currentMenuLastIndex = -1;

            characterInfoUiList[currentBattleId].OpenSelectedBg();
            if (currentBattleId + 1 < characterInfoUiList.Count)
            {
                characterInfoUiList[currentBattleId + 1].CloseSelectedBg();
            }
        }

        bool isBraveCommand = false;//���ָ���Ƿ�����brave���������attack��ָ��
        //��ʼ��
        if (currentMenuLastIndex == -1)
        {
            if (allEntities[currentBattleId].braveCount > 0)
            {
                yield return StartCoroutine(InitDoneMenuForBack());
            }
            else if (allEntities[currentBattleId].braveCount == 0)
            {
                for (int i = 1; i < menuPanelList.Count; i++)
                {
                    menuPanelList[i].GetComponent<RectTransform>().anchoredPosition = new Vector2(500f, 0f);
                    menuPanelList[i].SetActive(false);
                }
                menuPanelList[0].GetComponent<RectTransform>().anchoredPosition = new Vector2(500f, 0f);
                menuPanelList[0].SetActive(true);
                tweener = menuPanelList[0].GetComponent<RectTransform>().DOAnchorPos(new Vector2(0f, 0f), 0.3f);
                yield return tweener.WaitForCompletion();
                currentMenuLastIndex = 0;

                menuPanelList[0].transform.Find("Buttons").Find("Brave").GetComponent<BattleMenuButton>().SetEnable(() => BraveButton());
                menuPanelList[0].transform.Find("Buttons").Find("Default").GetComponent<BattleMenuButton>().SetEnable(() => DefaultButton());
                RefreshUniqueSkillButton();
            }

            characterInfoUiList[currentBattleId].OpenSelectedBg();
        }
        else if (currentMenuLastIndex == 0)
        {
            if (allEntities[currentBattleId].braveCount > 0)
            {
                if (menuPanelList[1].activeInHierarchy)
                {
                    isBraveCommand = true;
                }
                else
                {
                    yield return StartCoroutine(InitDoneMenuForBack());
                }
            }
            else if (allEntities[currentBattleId].braveCount == 0)
            {
                menuPanelList[0].SetActive(true);
                tweener = menuPanelList[0].GetComponent<RectTransform>().DOAnchorPosX(0f, 0.3f);
                yield return tweener.WaitForCompletion();

                menuPanelList[0].transform.Find("Buttons").Find("Brave").GetComponent<BattleMenuButton>().SetEnable(() => BraveButton());
                menuPanelList[0].transform.Find("Buttons").Find("Default").GetComponent<BattleMenuButton>().SetEnable(() => DefaultButton());
                RefreshUniqueSkillButton();
            }
        }

        else if (currentMenuLastIndex > 0)
        {
            if (currentMenuLastIndex + 1 < menuPanelList.Count && menuPanelList[currentMenuLastIndex + 1].activeInHierarchy)
            {
                isBraveCommand = true;
            }
        }
        UpdateMenuInfo();
        UpdateAllEntityUiInfo();

        BattleCommandType commandType = commandInfoList[commandInfoList.Count - 1].commandType;

        Tweener tweener2;
        switch (commandType)
        {
            case BattleCommandType.Attack:
            case BattleCommandType.Skill:
                if (isBraveCommand)
                {
                    float newWaitTime = 0f;
                    for (int i = 0; i < currentMenuLastIndex + 1; i++)
                    {
                        float originalX = menuPanelList[i].GetComponent<RectTransform>().anchoredPosition.x;
                        tweener2 = menuPanelList[i].GetComponent<RectTransform>().DOAnchorPosX(originalX - 20f, 0.3f);
                        newWaitTime += tweener2.Duration();
                    }
                    menuPanelList[currentMenuLastIndex + 1].GetComponent<RectTransform>().DOAnchorPosX(0f, 0.3f);
                    yield return new WaitForSeconds(newWaitTime);

                    currentMenuLastIndex++;
                }
                break;

            case BattleCommandType.UniqueSkill:
                if (isBraveCommand)
                {
                    float newWaitTime = 0f;
                    for (int i = 0; i < currentMenuLastIndex + 1; i++)
                    {
                        float originalX = menuPanelList[i].GetComponent<RectTransform>().anchoredPosition.x;
                        tweener2 = menuPanelList[i].GetComponent<RectTransform>().DOAnchorPosX(originalX - 20f, 0.3f);
                        newWaitTime += tweener2.Duration();
                    }
                    menuPanelList[currentMenuLastIndex + 1].GetComponent<RectTransform>().DOAnchorPosX(0f, 0.3f);
                    yield return new WaitForSeconds(newWaitTime);

                    currentMenuLastIndex++;
                }

                allEntities[currentBattleId].hadUniqueSkill = false;
                RefreshUniqueSkillButton();
                break;

            case BattleCommandType.Brave:
                if (allEntities[currentBattleId].braveCount > 1)
                {
                    allEntities[currentBattleId].bpPreview += 1;//����bp
                }
                else if (allEntities[currentBattleId].braveCount == 1)
                {
                    allEntities[currentBattleId].bpPreview += 2;//����bp
                    for (int i = 0; i < menuPanelList.Count - 1; i++)
                    {
                        menuPanelList[i].transform.Find("Buttons").Find("Brave").GetComponent<BattleMenuButton>().SetEnable(() => BraveButton());
                    }
                    menuPanelList[0].transform.Find("Buttons").Find("Default").GetComponent<BattleMenuButton>().SetEnable(() => DefaultButton());
                }

                allEntities[currentBattleId].braveCount--;

                float waitTime = 0;
                for (int i = 0; i < currentMenuLastIndex; i++)
                {
                    float originalX = menuPanelList[i].GetComponent<RectTransform>().anchoredPosition.x;
                    tweener2 = menuPanelList[i].GetComponent<RectTransform>().DOAnchorPosX(originalX + 20f, 0.3f);
                    waitTime += tweener2.Duration();
                }
                menuPanelList[currentMenuLastIndex].GetComponent<RectTransform>().DOAnchorPosX(500f, 0.3f);
                yield return new WaitForSeconds(waitTime);

                //����Ҫ�����棬��Ȼ��ʱ��ȼ��˺�ִ��ǰ��Ļص�,����Ҫ�ڶ���ʱ�ر�backButton��Ȼ����
                menuPanelList[currentMenuLastIndex].SetActive(false);
                currentMenuLastIndex--;

                UpdateMenuInfo();
                break;

            case BattleCommandType.Default:
                break;
        }

        //���������������ָ��
        if (commandInfoList[commandInfoList.Count - 1].isBattleStartCommand)
        {
            allEntities[currentBattleId].entitybattleStartCommandList.RemoveAt(allEntities[currentBattleId].entitybattleStartCommandList.Count - 1);
        }
        Debug.Log("ʵ��" + currentBattleId + "����������ָ��" + allEntities[currentBattleId].entityCommandList[allEntities[currentBattleId].entityCommandList.Count - 1] + "��commandInfo������+1Ϊ" + commandInfoList.Count);

        //ͳһ��������������ָ���и�ռλ����commandList��
        Debug.Log("ʵ��" + currentBattleId + "������ָ��" + allEntities[currentBattleId].entityCommandList[allEntities[currentBattleId].entityCommandList.Count - 1] + "��commandInfo������+1Ϊ" + commandInfoList.Count);
        allEntities[currentBattleId].entityCommandList.RemoveAt(allEntities[currentBattleId].entityCommandList.Count - 1);
        commandInfoList.RemoveAt(commandInfoList.Count - 1);

        if (commandInfoList.Count == 0)
        {
            backButton.SetActive(false);
        }

        //��������
        backButton.GetComponent<Button>().interactable = true;
        for (int i = 0; i < menuPanelList.Count; i++)
        {
            menuPanelList[i].transform.Find("MenuMask").gameObject.SetActive(false);
        }
    }

    IEnumerator InitDoneMenuForBack()
    {
        Tweener tweener1;

        int braveCount = allEntities[currentBattleId].braveCount;//�����ɫ���������㣬����2������ʱ�����ֵ����2
        for (int i = 1; i < braveCount + 1; i++)//�������1,2
        {
            menuPanelList[i].GetComponent<RectTransform>().anchoredPosition = new Vector2(500f, 0f);
            menuPanelList[i].SetActive(true);
        }
        for (int i = braveCount + 1; i < menuPanelList.Count; i++)//�����3
        {
            menuPanelList[i].GetComponent<RectTransform>().anchoredPosition = new Vector2(500f, 0f);
            menuPanelList[i].SetActive(false);
        }
        menuPanelList[0].GetComponent<RectTransform>().anchoredPosition = new Vector2(500f, 0f);
        menuPanelList[0].SetActive(true);//�������0
        tweener1 = menuPanelList[0].GetComponent<RectTransform>().DOAnchorPosX(0f, 0.3f);
        yield return tweener1.WaitForCompletion();
        currentMenuLastIndex = 0;

        for (int i = 0; i < menuPanelList.Count - 1; i++)
        {
            menuPanelList[i].transform.Find("Buttons").Find("Brave").GetComponent<BattleMenuButton>().SetDisable();
        }
        menuPanelList[0].transform.Find("Buttons").Find("Default").GetComponent<BattleMenuButton>().SetDisable();
    }

    private void RefreshUniqueSkillButton()
    {
        if (allEntities[currentBattleId].hadUniqueSkill)
        {
            for (int i = 0; i < menuPanelList.Count; i++)
            {
                menuPanelList[i].transform.Find("Buttons").Find("UniqueSkill").GetComponent<BattleMenuButton>().SetDisable();
            }
        }
        else
        {
            for (int i = 0; i < menuPanelList.Count; i++)
            {
                menuPanelList[i].transform.Find("Buttons").Find("UniqueSkill").GetComponent<BattleMenuButton>().SetEnable(() => UniqueSkillButton());
            }
        }
    }

    IEnumerator CharacterAttack(int battleId, int bpNeed, List<BattleEntity> selectEntity)//���enemyIndexListֻ��1��Ԫ��
    {
        float damagePoint = allEntities[battleId].damage;
        //Debug.Log(selectEntity.Count);
        for (int i = 0; i < selectEntity.Count; i++)
        {
            //Debug.Log(selectEntity[i]);

            if (selectEntity[i].isDie)
            {
                //�޷�Ӧ
            }
            else
            {
                SystemFacade.instance.SwitchObjCamera(CameraManager.ObjCameraState.Character, 0f);
                yield return StartCoroutine(Utility.PlayAnimation(allEntities[battleId].animator, "attack"));

                yield return StartCoroutine(CameraManager.SwitchCamera(selectEntity[i]));

                EntityGetDamage(selectEntity[i], allEntities[battleId], damagePoint);
                CheckBpSubstract(allEntities[battleId], bpNeed);
            }
            UpdateAllEntityUiInfo();
        }

        ExcuteCommandList();
    }
    IEnumerator BattleStartCommandInCommandList(int battleId)//����ָ��ӽ�commandList��ռλ��
    {
        yield return null;
        ExcuteCommandList();
    }
    IEnumerator CharacterDefend(int battleId)//�ӽ�BattleStartCommandList��ִ�е�
    {
        allEntities[battleId].defend += 500;//��RoundListener��ע�����¼�������Ƿ�����default����ȥdefend
        yield return null;
        ExcuteBattleStartCommandList();
    }
    IEnumerator CharacterBrave(int battleId)//�ӽ�commandList��ռλ��
    {
        yield return null;
        ExcuteCommandList();
    }

    //entity
    public void EntityGetDamage(BattleEntity entityGet, BattleEntity entitySet, float damagePoint, float damageRateAddition = 0)
    {
        if (entityGet.isDie)//�����ɫ�Ѿ�����
        {
            return;
        }
        else//buff+Damage()
        {
            //Debug.Log(entityGet.characterInfoBtn != null);
            //Debug.Log(entityGet.characterInfoBtn != null && entityGet.characterInfoBtn.CheckBuff(BuffName.���ڻ�) > 0);
            //Debug.Log(entityGet.hurtToEntity != null);
            if (entityGet.entityInfoUi != null && buffManager.CheckBuff(entityGet.entityInfoUi, BuffName.���ڻ�).Count > 0 && entityGet.hurtToEntity != null)//���ڻ���
            {
                Debug.Log(entityGet + "������ת�Ƹ�" + entityGet.hurtToEntity);
                entityGet = entityGet.hurtToEntity;
            }

            EntityGetDamageNoReDamageBuffCheck(entityGet, entitySet, damagePoint, damageRateAddition);

            if (entityGet.entityInfoUi != null && buffManager.CheckBuff(entityGet.entityInfoUi, BuffName.��������).Count > 0)//�з�������
            {
                float reDamagePoint = entityGet.damage * 0.7f;
                EntityGetDamageNoReDamageBuffCheck(entitySet, entitySet, reDamagePoint, damageRateAddition);
            }
        }
    }
    private void EntityGetDamageNoReDamageBuffCheck(BattleEntity entityGet, BattleEntity entitySet, float damagePoint, float damageRateAddition)
    {
        entityGet.animator.Play("damaged");
        int hurtPoint = (int)(Mathf.Clamp((damagePoint - entityGet.defend), 0f, 1000000f) * entityGet.hurtRate * (entitySet.damageRate + damageRateAddition));
        //Debug.Log("damage=" + damagePoint + ",defand=" + entityGet.defend + ",hurtRate=" + entityGet.hurtRate + ",damageRate=" + entitySet.damageRate + "\n" +
        //    "�˺�-����=" + (int)(Mathf.Clamp((damagePoint - entityGet.defend), 0f, 1000000f)) + "\n" +
        //    "�˺�rate*����rate=" + entityGet.hurtRate * entitySet.damageRate + "\n" +
        //    "���˺�=" + (int)(Mathf.Clamp((damagePoint - entityGet.defend), 0f, 1000000f) * entityGet.hurtRate * entitySet.damageRate));
        entityGet.hurtPointText.text = hurtPoint.ToString();
        Utility.TextFly(entityGet.hurtPointText, Vector3.zero);

        if (!entityGet.isEnemy)
        {
            //��ʱ�趨�ɰ�����10��MP==================================================================================
            entityGet.mp += 10;
            if (entityGet.mp > 100)
            {
                entityGet.mp = 100;
            }
        }
        DescreaseEntityHp(entityGet, entitySet, hurtPoint);
    }
    public void DescreaseEntityHp(BattleEntity entityGet, BattleEntity entitySet, int hpDescreaseCount)//ǿ�Ƽ���hp
    {
        entityGet.UpdateHp(-hpDescreaseCount);

        if (entityGet.GetHp() <= 0)
        {
            if (buffManager.CheckBuffObj(entityGet.entityInfoUi, BuffName.����).Count > 0)//�з���buff
            {
                BuffObject buffObj = buffManager.CheckBuffObj(entityGet.entityInfoUi, BuffName.����)[0];
                buffManager.RemoveBuffEffectSwitch(buffObj);
                UpdateAllEntityUiInfo();
                return;
            }
            StartCoroutine(IEntityDie(entityGet));
        }

        UpdateAllEntityUiInfo();//�����·���ɫ��Ϣ�б�
    }
    public void RecoverEntityHp(BattleEntity entity, int hpAddValue)//���buff�����ظ�entity����ֵ�������buff��ֱ�ӵ���entity.UpdateHp()
    {
        if (buffManager.CheckBuff(entity.entityInfoUi, BuffName.ȼ��).Count <= 0)
        {
            entity.UpdateHp(hpAddValue);
        }
    }
    public void CheckBpSubstract(BattleEntity characterEntity, int bpNeed)
    {
        buffManager.CheckBpSubstract(characterEntity, bpNeed);
    }

    IEnumerator IEntityDie(BattleEntity entityEntity)
    {
        entityEntity.isDie = true;
        entityEntity.SetHp(0);
        entityEntity.hpSlider.value = 0;
        entityEntity.selectToggle.group = null;
        entityEntity.animator.Play("die");

        yield return new WaitForSeconds(1f);
        entityEntity.gameObject.SetActive(false);
        if (entityEntity.isEnemy)
        {
            enemyNumber--;
            if (enemyNumber == 0)
            {
                GameWin();
            }
        }
        else
        {
            characterNumber--;
            if (characterNumber == 0)
            {
                GameLoss();
            }
        }
    }
    IEnumerator EnemyAttack(int battleId, List<BattleEntity> entityList)
    {
        if (allEntities[battleId].isDie)
        {
            yield return null;
            ExcuteCommandList();
        }
        else
        {
            SystemFacade.instance.SwitchObjCamera(CameraManager.ObjCameraState.Enemy, 0f);
            yield return StartCoroutine(Utility.PlayAnimation(allEntities[battleId].animator, "attack"));

            float damagePoint = allEntities[battleId].damage;
            for (int i = 0; i < entityList.Count; i++)
            {
                yield return StartCoroutine(CameraManager.SwitchCamera(entityList[i]));

                if (entityList[i].isEnemy)
                {
                    Debug.LogError("���˹��������ˣ�������");
                }
                else
                {
                    EntityGetDamage(entityList[i], allEntities[battleId], damagePoint);
                }
            }
            yield return new WaitForSeconds(0.5f);
            ExcuteCommandList();
        }
    }

    public void AttackButton()
    {
        SelectMenuOpen(BattleCommandType.Attack, SelectType.All, false, 1);
    }

    public void SkillButton()
    {
        InitSkillMenu();
        skillMenuMask.SetActive(true);
    }
    private void InitSkillMenu()
    {
        //��ʼ����ɫ�ļ��ܱ�=====================================================================
        List<SkillInfo> skillList = characterData.data[allEntities[currentBattleId].characterId].skills;
        for (int i = 0; i < skillList.Count; i++)
        {
            skillObjs[i].skillInfo = skillList[i];

            skillObjs[i].skillObjText.text = skillObjs[i].skillInfo.skillName;
            if (skillList[i].isUnlock)
            {
                skillObjs[i].gameObject.SetActive(true);
            }
            else
            {
                skillObjs[i].gameObject.SetActive(false);
            }
        }
        for (int i = skillList.Count; i < skillObjs.Count; i++)
        {
            skillObjs[i].gameObject.SetActive(false);
        }

        for (int i = 0; i < menuPanelList.Count; i++)
        {
            menuPanelList[i].transform.Find("Buttons").Find("Skill").GetComponent<Image>().color = new Color32(24, 13, 255, 255);
        }
    }
    private void CloseSkillMenu(BaseEventData baseEventData)//�ر�SkillMenu��������
    {
        for (int i = 0; i < menuPanelList.Count; i++)
        {
            Transform skillButtonTransform = menuPanelList[i].transform.Find("Buttons").Find("Skill");
            skillButtonTransform.GetComponent<Image>().color = new Color32(255, 255, 255, 255);
        }
        skillMenuMask.SetActive(false);
    }
    private void SkillMenuButton(int skillIndex)//��������˼���
    {
        //����skillSelectInfo
        SkillInfo skillInfo = skillObjs[skillIndex].skillInfo;
        skillSelectInfo = new SkillSelectInfo(skillInfo.needSelect, skillInfo.isBattleStartCommand, allEntities[currentBattleId], skillIndex, null);
        if (skillSelectInfo.needSelect)
        {
            SelectMenuOpen(BattleCommandType.Skill, skillInfo.selectType, skillInfo.cameraLookAt, skillInfo.selectCount);
        }
        else
        {
            skillSelectInfo = AddSkill(skillSelectInfo);
            commandInfoList.Add(new BattleCommandInfo(false, BattleCommandType.Skill, skillSelectInfo.isBattleStartCommand, skillSelectInfo.BpNeed, selectedEntityList, currentBattleId));
            SetCommand();

            Debug.Log("ʵ��" + currentBattleId + "������ָ��" + allEntities[currentBattleId].entityCommandList[allEntities[currentBattleId].entityCommandList.Count - 1] + "��commandInfo������Ϊ" + commandInfoList.Count);
            SystemFacade.instance.SwitchObjCamera(CameraManager.ObjCameraState.IdleCommand);
            backButton.SetActive(true);
        }

        CloseSkillMenu(null);
        skillDescribe.GetComponent<CanvasGroup>().alpha = 0f;
        //Debug.Log(skillIndex);
    }
    public void UniqueSkillButton()
    {
        //��������
        backButton.GetComponent<Button>().interactable = false;
        for (int i = 0; i < menuPanelList.Count; i++)
        {
            menuPanelList[i].transform.Find("MenuMask").gameObject.SetActive(true);
        }

        SkillInfo skillInfo = characterData.data[allEntities[currentBattleId].characterId].uniqueSkill;
        skillSelectInfo = new SkillSelectInfo(skillInfo.needSelect, skillInfo.isBattleStartCommand, allEntities[currentBattleId], 100, null);
        if (skillSelectInfo.needSelect)
        {
            SelectMenuOpen(BattleCommandType.UniqueSkill, skillInfo.selectType, skillInfo.cameraLookAt, skillInfo.selectCount);
        }
        else
        {
            allEntities[currentBattleId].hadUniqueSkill = true;
            RefreshUniqueSkillButton();

            skillSelectInfo = AddSkill(skillSelectInfo);
            commandInfoList.Add(new BattleCommandInfo(false, BattleCommandType.UniqueSkill, skillSelectInfo.isBattleStartCommand, skillSelectInfo.BpNeed, selectedEntityList, currentBattleId));
            SetCommand();

            Debug.Log("ʵ��" + currentBattleId + "������ָ��" + allEntities[currentBattleId].entityCommandList[allEntities[currentBattleId].entityCommandList.Count - 1] + "��commandInfo������Ϊ" + commandInfoList.Count);
            SystemFacade.instance.SwitchObjCamera(CameraManager.ObjCameraState.IdleCommand);
            backButton.SetActive(true);
        }

        //��������
        backButton.GetComponent<Button>().interactable = true;
        for (int i = 0; i < menuPanelList.Count; i++)
        {
            menuPanelList[i].transform.Find("MenuMask").gameObject.SetActive(false);
        }
    }
    private SkillSelectInfo AddSkill(SkillSelectInfo skillSelectInfo)
    {
        if (skillSelectInfo.needSelect && skillSelectInfo.selectedEntityList == null)
        {
            Debug.LogError("ѡ����Ŀ�굫Ŀ���б�Ϊ��");
        }

        List<BattleEntity> selectedEntity = new List<BattleEntity>();//list���������ͣ�����Ҫ���
        for (int i = 0; i < selectedEntityList.Count; i++)
        {
            selectedEntity.Add(selectedEntityList[i]);
        }

        switch (skillSelectInfo.battleEntity.characterId)
        {
            case 0://ά������
                switch (skillSelectInfo.skillIndex)
                {
                    case 0://�������ƣ�����ָ��
                        skillSelectInfo.BpNeed = 1;
                        skillSelectInfo.battleEntity.entityCommandList.Add(skillsManager.VectoriaSkill1(allEntities[currentBattleId], skillSelectInfo.BpNeed, selectedEntity));
                        break;
                    case 1:
                        skillSelectInfo.BpNeed = 1;
                        skillSelectInfo.battleEntity.entityCommandList.Add(skillsManager.VectoriaSkill2(allEntities[currentBattleId], skillSelectInfo.BpNeed, selectedEntity));
                        break;
                    case 2:
                        skillSelectInfo.BpNeed = 1;
                        skillSelectInfo.battleEntity.entityCommandList.Add(skillsManager.VectoriaSkill3(allEntities[currentBattleId], skillSelectInfo.BpNeed, allEntities));
                        break;
                    case 3:
                        skillSelectInfo.BpNeed = 2;
                        skillSelectInfo.battleEntity.entityCommandList.Add(skillsManager.VectoriaSkill4(allEntities[currentBattleId], skillSelectInfo.BpNeed, allEntities));
                        break;
                    case 4:
                        skillSelectInfo.BpNeed = 1;
                        skillSelectInfo.battleEntity.entityCommandList.Add(skillsManager.VectoriaSkill5(allEntities[currentBattleId], skillSelectInfo.BpNeed, selectedEntity));
                        break;
                    case 5:
                        skillSelectInfo.BpNeed = 1;
                        skillSelectInfo.battleEntity.entityCommandList.Add(skillsManager.VectoriaSkill6(allEntities[currentBattleId], skillSelectInfo.BpNeed));
                        break;
                    case 100:
                        skillSelectInfo.BpNeed = 1;
                        skillSelectInfo.battleEntity.entityCommandList.Add(skillsManager.VectoriaUniqueSkill(allEntities[currentBattleId], skillSelectInfo.BpNeed, allEntities));
                        break;
                }
                break;

            case 1://ά�˶�
                switch (skillSelectInfo.skillIndex)
                {
                    case 0://�������ƣ�����ָ��
                        skillSelectInfo.BpNeed = 1;
                        skillSelectInfo.battleEntity.entityCommandList.Add(BattleStartCommandInCommandList(currentBattleId));
                        skillSelectInfo.battleEntity.entitybattleStartCommandList.Add(skillsManager.VectorSkill1(allEntities[currentBattleId], skillSelectInfo.BpNeed));
                        break;
                    case 1:
                        skillSelectInfo.BpNeed = 1;
                        skillSelectInfo.battleEntity.entityCommandList.Add(skillsManager.VectorSkill2(allEntities[currentBattleId], skillSelectInfo.BpNeed, selectedEntity));
                        break;
                    case 2:
                        skillSelectInfo.BpNeed = 1;
                        skillSelectInfo.battleEntity.entityCommandList.Add(skillsManager.VectorSkill3(allEntities[currentBattleId], skillSelectInfo.BpNeed, selectedEntity));
                        break;
                    case 3:
                        skillSelectInfo.BpNeed = 2;
                        skillSelectInfo.battleEntity.entityCommandList.Add(skillsManager.VectorSkill4(allEntities[currentBattleId], skillSelectInfo.BpNeed));
                        break;
                    case 4:
                        skillSelectInfo.BpNeed = 1;
                        skillSelectInfo.battleEntity.entityCommandList.Add(skillsManager.VectorSkill5(allEntities[currentBattleId], skillSelectInfo.BpNeed, selectedEntity));
                        break;
                    case 5:
                        skillSelectInfo.BpNeed = 3;
                        skillSelectInfo.battleEntity.entityCommandList.Add(skillsManager.VectorSkill6(allEntities[currentBattleId], skillSelectInfo.BpNeed, allEntities));
                        break;
                    case 100:
                        skillSelectInfo.BpNeed = 1;
                        skillSelectInfo.battleEntity.entityCommandList.Add(skillsManager.VectorUniqueSkill(allEntities[currentBattleId], skillSelectInfo.BpNeed, selectedEntity));
                        break;
                }
                break;

            case 2://
            case 3://
            case 4://
            case 5://
                break;
        }

        return skillSelectInfo;//���ñ��������Ի��������Switch�б��޸�Ȼ�󴫳�
    }

    //Buff
    private void DoRoundEndBuffEffect()//ÿ�غ����ִ������buffЧ��
    {
        for (int i = 0; i < allEntities.Count; i++)
        {
            if (!allEntities[i].isDie)
            {
                for (int j = 0; j < allEntities[i].entityInfoUi.buffs.Count; j++)//ִ��buffЧ��
                {
                    switch (allEntities[i].entityInfoUi.buffs[j].buffInfo.buffName)
                    {
                        case BuffName.����:
                            for (int k = 0; k < commandInfoList.Count; k++)
                            {
                                if (commandInfoList[k].battleId == i && commandInfoList[k].commandType == BattleCommandType.Default)
                                {
                                    allEntities[i].UpdateBp(2);
                                    break;
                                }
                            }
                            break;
                        case BuffName.����:
                            RecoverEntityHp(allEntities[i], (int)(allEntities[i].maxHp * 0.3f));
                            if (allEntities[i].GetHp() > allEntities[i].maxHp)
                            {
                                allEntities[i].SetHp(allEntities[i].maxHp);
                            }
                            break;
                    }
                }
                for (int j = 0; j < allEntities[i].entityInfoUi.debuffs.Count; j++)
                {
                    switch (allEntities[i].entityInfoUi.debuffs[j].buffInfo.buffName)
                    {
                        case BuffName.����:
                            float damagePoint = allEntities[i].entityInfoUi.debuffs[j].buffInfo.entitySet.damage * 0.7f;
                            EntityGetDamage(allEntities[i], allEntities[i].entityInfoUi.debuffs[j].buffInfo.entitySet, damagePoint);
                            break;
                    }
                }
                buffManager.SubstractRoundDuring(allEntities[i]);//ִ�����buffʱ��Ч��
                buffDescribe.SetActive(false);
            }
        }
        UpdateAllEntityUiInfo();
    }

    //ѡ��Ŀ��ģ��
    private void SelectMenuOpen(BattleCommandType battleCommandType, SelectType selectType, bool cameraLookAt, int canSelectCountT)//��ʼ��ѡ��Ŀ��
    {
        activedToggleList.Clear();
        canSelectCount = canSelectCountT;
        selectMenuType = battleCommandType;

        selectEntityPanel.SetActive(true);
        selectEntityMainMask.SetActive(false);
        backButton.SetActive(false);

        switch (selectType)
        {
            case SelectType.All:
                selectEntityPanel.transform.Find("SwitchCameraToggle").GetComponent<Toggle>().isOn = cameraLookAt;
                SwitchCameraToggle(cameraLookAt);
                for (int i = 0; i < allEntities.Count; i++)
                {
                    if (!allEntities[i].isDie)
                    {
                        allEntities[i].selectToggle.isOn = false;
                        allEntities[i].selectToggle.gameObject.SetActive(true);
                        activedToggleList.Add(allEntities[i].selectToggle);
                        EntityOutline(allEntities[i], true);
                    }
                }
                break;
            case SelectType.Enemy:
                selectEntityPanel.transform.Find("SwitchCameraToggle").GetComponent<Toggle>().isOn = cameraLookAt;
                SwitchCameraToggle(cameraLookAt);
                for (int i = 0; i < allEntities.Count; i++)
                {
                    if (!allEntities[i].isDie)
                    {
                        if (allEntities[i].isEnemy)
                        {
                            allEntities[i].selectToggle.isOn = false;
                            allEntities[i].selectToggle.gameObject.SetActive(true);
                            activedToggleList.Add(allEntities[i].selectToggle);
                            EntityOutline(allEntities[i], true);
                        }
                        else
                        {
                            allEntities[i].selectToggle.isOn = false;
                            allEntities[i].selectToggle.gameObject.SetActive(false);
                            EntityOutline(allEntities[i], false);
                        }
                    }
                }
                break;
            case SelectType.Character:
                selectEntityPanel.transform.Find("SwitchCameraToggle").GetComponent<Toggle>().isOn = cameraLookAt;
                SwitchCameraToggle(cameraLookAt);
                for (int i = 0; i < allEntities.Count; i++)
                {
                    if (!allEntities[i].isDie)
                    {
                        if (!allEntities[i].isEnemy)
                        {
                            allEntities[i].selectToggle.isOn = false;
                            allEntities[i].selectToggle.gameObject.SetActive(true);
                            activedToggleList.Add(allEntities[i].selectToggle);
                            EntityOutline(allEntities[i], true);
                        }
                        else
                        {
                            allEntities[i].selectToggle.isOn = false;
                            allEntities[i].selectToggle.gameObject.SetActive(false);
                            EntityOutline(allEntities[i], false);
                        }
                    }
                }
                break;
            case SelectType.AllExceptSelf:
                selectEntityPanel.transform.Find("SwitchCameraToggle").GetComponent<Toggle>().isOn = cameraLookAt;
                SwitchCameraToggle(cameraLookAt);
                for (int i = 0; i < allEntities.Count; i++)
                {
                    if (!allEntities[i].isDie)
                    {
                        allEntities[i].selectToggle.isOn = false;
                        allEntities[i].selectToggle.gameObject.SetActive(true);
                        activedToggleList.Add(allEntities[i].selectToggle);
                        EntityOutline(allEntities[i], true);
                    }
                }

                allEntities[currentBattleId].selectToggle.isOn = false;
                allEntities[currentBattleId].selectToggle.gameObject.SetActive(false);
                EntityOutline(allEntities[currentBattleId], false);

                break;
            case SelectType.EnemyExceptSelf:
                selectEntityPanel.transform.Find("SwitchCameraToggle").GetComponent<Toggle>().isOn = cameraLookAt;
                SwitchCameraToggle(cameraLookAt);
                for (int i = 0; i < allEntities.Count; i++)
                {
                    if (!allEntities[i].isDie)
                    {
                        if (allEntities[i].isEnemy)
                        {
                            allEntities[i].selectToggle.isOn = false;
                            allEntities[i].selectToggle.gameObject.SetActive(true);
                            activedToggleList.Add(allEntities[i].selectToggle);
                            EntityOutline(allEntities[i], true);
                        }
                        else
                        {
                            allEntities[i].selectToggle.isOn = false;
                            allEntities[i].selectToggle.gameObject.SetActive(false);
                            EntityOutline(allEntities[i], false);
                        }
                    }
                }

                allEntities[currentBattleId].selectToggle.isOn = false;
                allEntities[currentBattleId].selectToggle.gameObject.SetActive(false);
                EntityOutline(allEntities[currentBattleId], false);

                break;
            case SelectType.CharacterExceptSelf:
                selectEntityPanel.transform.Find("SwitchCameraToggle").GetComponent<Toggle>().isOn = cameraLookAt;
                SwitchCameraToggle(cameraLookAt);
                for (int i = 0; i < allEntities.Count; i++)
                {
                    if (!allEntities[i].isDie)
                    {
                        if (!allEntities[i].isEnemy)
                        {
                            allEntities[i].selectToggle.isOn = false;
                            allEntities[i].selectToggle.gameObject.SetActive(true);
                            activedToggleList.Add(allEntities[i].selectToggle);
                            EntityOutline(allEntities[i], true);
                        }
                        else
                        {
                            allEntities[i].selectToggle.isOn = false;
                            allEntities[i].selectToggle.gameObject.SetActive(false);
                            EntityOutline(allEntities[i], false);
                        }
                    }
                }

                allEntities[currentBattleId].selectToggle.isOn = false;
                allEntities[currentBattleId].selectToggle.gameObject.SetActive(false);
                EntityOutline(allEntities[currentBattleId], false);

                break;
        }

        for (int i = 0; i < allEntities.Count; i++)
        {
            if (allEntities[i].isDie)
            {
                allEntities[i].selectToggle.isOn = false;
                allEntities[i].selectToggle.gameObject.SetActive(false);
            }
        }

        if (canSelectCountT == 1)
        {
            for (int i = 0; i < activedToggleList.Count; i++)
            {
                activedToggleList[i].group = selectToggleGroup;
            }
        }
        else if (canSelectCountT > 1)
        {
            for (int i = 0; i < activedToggleList.Count; i++)
            {
                activedToggleList[i].group = null;
            }
        }

        selectedEntityList.Clear();
    }
    public void SelectOkButton()//��ѡ��Ŀ�������ok
    {
        //��������
        backButton.GetComponent<Button>().interactable = false;
        for (int i = 0; i < menuPanelList.Count; i++)
        {
            menuPanelList[i].transform.Find("MenuMask").gameObject.SetActive(true);
        }

        //Debug.Log(currentBallteId);
        //Debug.Log(selectedEntityList.Count);
        if (selectedEntityList.Count <= 0)//�ж�ѡ����Ŀ��û
        {
            return;//һ����ûѡ
        }

        for (int i = 0; i < allEntities.Count; i++)//�ر�entity���
        {
            if (!allEntities[i].isDie)
            {
                EntityOutline(allEntities[i], false);
            }
        }

        List<BattleEntity> selectedEntity = new List<BattleEntity>();//list���������ͣ�����Ҫ���
        for (int i = 0; i < selectedEntityList.Count; i++)
        {
            selectedEntity.Add(selectedEntityList[i]);
        }

        switch (selectMenuType)
        {
            case BattleCommandType.Attack:
                allEntities[currentBattleId].entityCommandList.Add(CharacterAttack(currentBattleId, 1, selectedEntity));
                commandInfoList.Add(new BattleCommandInfo(false, BattleCommandType.Attack, false, 1, selectedEntity, currentBattleId));
                break;
            case BattleCommandType.Skill:
                skillSelectInfo.selectedEntityList = selectedEntity;
                AddSkill(skillSelectInfo);
                commandInfoList.Add(new BattleCommandInfo(false, BattleCommandType.Skill, skillSelectInfo.isBattleStartCommand, skillSelectInfo.BpNeed, selectedEntity, currentBattleId));
                CloseSkillMenu(null);
                break;
            case BattleCommandType.UniqueSkill:
                allEntities[currentBattleId].hadUniqueSkill = true;
                RefreshUniqueSkillButton();

                skillSelectInfo.selectedEntityList = selectedEntity;
                AddSkill(skillSelectInfo);
                commandInfoList.Add(new BattleCommandInfo(false, BattleCommandType.UniqueSkill, skillSelectInfo.isBattleStartCommand, skillSelectInfo.BpNeed, selectedEntity, currentBattleId));
                break;
            default:
                Debug.LogError("Ŀ�����������µ�CommandType");
                break;
        }

        Debug.Log("ʵ��" + currentBattleId + "������ָ��" + allEntities[currentBattleId].entityCommandList[allEntities[currentBattleId].entityCommandList.Count - 1] + "��commandInfo������Ϊ" + commandInfoList.Count);

        SetCommand();
        selectEntityPanel.SetActive(false);
        SystemFacade.instance.SwitchObjCamera(CameraManager.ObjCameraState.IdleCommand);
        selectEntityMainMask.SetActive(true);
        backButton.SetActive(true);
        ResetSelectEntity();

        //��������
        backButton.GetComponent<Button>().interactable = true;
        for (int i = 0; i < menuPanelList.Count; i++)
        {
            menuPanelList[i].transform.Find("MenuMask").gameObject.SetActive(false);
        }
    }
    public void SelectUndoButton()//��ѡ��Ŀ�������undo
    {
        //��������
        backButton.GetComponent<Button>().interactable = false;
        for (int i = 0; i < menuPanelList.Count; i++)
        {
            menuPanelList[i].transform.Find("MenuMask").gameObject.SetActive(true);
        }

        if (commandInfoList.Count == 0)
        {
            backButton.SetActive(false);
        }
        else
        {
            backButton.SetActive(true);
        }

        for (int i = 0; i < allEntities.Count; i++)//�ر�entity���
        {
            if (!allEntities[i].isDie)
            {
                EntityOutline(allEntities[i], false);
            }
        }

        selectEntityPanel.SetActive(false);
        SystemFacade.instance.SwitchObjCamera(CameraManager.ObjCameraState.IdleCommand);
        selectEntityMainMask.SetActive(true);
        menusContainer.SetActive(true);
        ResetSelectEntity();

        //��������
        backButton.GetComponent<Button>().interactable = true;
        for (int i = 0; i < menuPanelList.Count; i++)
        {
            menuPanelList[i].transform.Find("MenuMask").gameObject.SetActive(false);
        }
    }
    private void SelectEntity(bool isOn, BattleEntity entity)//��selectAttackButton����
    {
        if (isOn)
        {
            selectedEntityList.Add(entity);
            canSelectCount--;
            if (canSelectCount <= 0 && activedToggleList[0].group == null)//activedToggleList[0].group != null���ж��ǲ���ֻ����ѡ����,��Ϊѡ����Ҫ�趨group
            {
                for (int i = 0; i < activedToggleList.Count; i++)
                {
                    if (!activedToggleList[i].isOn)
                    {
                        activedToggleList[i].gameObject.SetActive(false);
                    }
                }
            }
        }
        else
        {
            selectedEntityList.Remove(entity);
            if (canSelectCount == 0)
            {
                for (int i = 0; i < activedToggleList.Count; i++)
                {
                    activedToggleList[i].gameObject.SetActive(true);
                }
            }
            canSelectCount++;
        }
    }
    private void ResetSelectEntity()//����ѡ��Ŀ��
    {
        for (int i = 0; i < allEntities.Count; i++)
        {
            allEntities[i].selectToggle.isOn = false;
            allEntities[i].selectToggle.gameObject.SetActive(false);
        }
        selectedEntityList.Clear();
        canSelectCount = -1;
    }


    public void OpenBuffDescribe(BuffObject buffObj)
    {
        buffDescribe.transform.position = buffObj.transform.position + new Vector3(18f, 17f, 0f);
        buffDescribe.SetActive(true);//ǿ�Ƹ���contentSizeFilter��Ҫ����gameObj

        //inspector���ڵ�string����ʱ���Զ���\nת��\\n ������Ҫת��������Ȼ�����У�ܳ
        string buffValueString = "";

        if (buffObj.buffInfo.buffValues.Length != 0)
        {
            buffValueString = buffObj.buffInfo.buffValues[0].ToString();
            if (buffObj.buffInfo.buffValues[0].GetType() == typeof(BattleEntity))
            {
                BattleEntity entity = buffObj.buffInfo.buffValues[0] as BattleEntity;
                buffValueString = entity.isEnemy ? entity.enemyDataEntry.enemyName : characterData.data[entity.characterId].characterName;
            }
        }
        Debug.Log(buffObj.buffInfo.data.description);
        buffDescribe.transform.Find("InfoText").GetComponent<TextMeshProUGUI>().text = string.Format(buffObj.buffInfo.data.description.Replace("\\n", "\n"), buffValueString);
        //ǿ�Ƹ���contentSizeFilter����Ȼ���´�С���ӳ�
        Utility.ForceUpdateContentSizeFilter(buffDescribe.transform);

        buffDescribe.GetComponent<CanvasGroup>().alpha = 1;
    }
    public void CloseBuffDescribe()
    {
        buffDescribe.GetComponent<CanvasGroup>().alpha = 0;
        buffDescribe.SetActive(false);
    }

    public void OpenSkillDescribe(SkillObject skillObj)
    {
        //ͨ����skillDescribe������xy��Ϊ-1��Ȼ�����������xy��Ϊ-1��ʵ�������½���չ
        skillDescribe.transform.position = skillObj.transform.position + new Vector3(-17f, 32.4f, 0f);
        skillDescribe.SetActive(true);//ǿ�Ƹ���contentSizeFilter��Ҫ����gameObj

        //Debug.Log(skillObj.skillInfo.skillDescription);
        skillDescribe.transform.Find("InfoText").GetComponent<TextMeshProUGUI>().text = skillObj.skillInfo.skillDescription.Replace("\\n", "\n");
        //ǿ�Ƹ���contentSizeFilter����Ȼ���´�С���ӳ�
        Utility.ForceUpdateContentSizeFilter(skillDescribe.transform);

        skillDescribe.GetComponent<CanvasGroup>().alpha = 1;
    }
    public void CloseSkillDescribe()
    {
        skillDescribe.GetComponent<CanvasGroup>().alpha = 0;
        skillDescribe.SetActive(false);
    }

    public void UpdateAllEntityUiInfo()//�����·���ɫ��Ϣ�б�
    {
        //������Ϣ
        for (int i = 0; i < allEntities.Count; i++)
        {
            if (allEntities[i].isDie)//��ɫ����
            {
                allEntities[i].entityInfoUi.hpSlider.value = 0;
                allEntities[i].entityInfoUi.mpSlider.value = 0;
                allEntities[i].entityInfoUi.healthPoint.text = "0";
                allEntities[i].entityInfoUi.magicPoint.text = "0";
                allEntities[i].hpSlider.value = 0f;

                if (!allEntities[i].isEnemy)//����ǽ�ɫ
                {
                    allEntities[i].entityInfoUi.bpText.sprite = bpSpriteList[0 + 4];
                    allEntities[i].entityInfoUi.portrait.color = new Color(0.5f, 0.5f, 0.5f, 1f);
                }
            }
            else//��ɫû��
            {
                allEntities[i].entityInfoUi.hpSlider.value = allEntities[i].GetHp();
                allEntities[i].entityInfoUi.mpSlider.value = allEntities[i].mp;
                allEntities[i].entityInfoUi.healthPoint.text = allEntities[i].GetHp().ToString();
                allEntities[i].entityInfoUi.magicPoint.text = allEntities[i].mp.ToString();
                allEntities[i].hpSlider.value = allEntities[i].GetHp();

                if (!allEntities[i].isEnemy)//����ǽ�ɫ
                {
                    allEntities[i].entityInfoUi.bpText.sprite = bpSpriteList[allEntities[i].GetBp() + 4];
                    allEntities[i].entityInfoUi.portrait.color = new Color(1f, 1f, 1f, 1f);
                }
            }
        }
    }
    private void UpdateMenuInfo()//���½�ɫָ�������Ϣ
    {
        for (int i = 0; i < menuPanelList.Count; i++)
        {
            menuPanelList[i].transform.Find("Portrait").GetComponent<Image>().sprite = characterData.data[allEntities[currentBattleId].characterId].battlePortrait;
            menuPanelList[i].transform.Find("PortraitName").GetComponent<Image>().sprite = characterData.data[allEntities[currentBattleId].characterId].battlePortraitName;
            menuPanelList[i].transform.Find("BravePoint").GetComponent<Image>().sprite = bpSpriteList[allEntities[currentBattleId].GetBp() + 4];
            menuPanelList[i].transform.Find("BravePointPreview").GetComponent<Image>().sprite = bpSpriteList[allEntities[currentBattleId].bpPreview + 4];
            menuPanelList[i].transform.Find("MpSlider").GetComponent<Slider>().value = allEntities[currentBattleId].mp;
            menuPanelList[i].transform.Find("MagicPoint").GetComponent<TextMeshProUGUI>().text = allEntities[currentBattleId].mp.ToString();
        }
    }

    public void SwitchCameraToggle(bool on)
    {
        if (on)
        {
            SystemFacade.instance.SwitchObjCamera(CameraManager.ObjCameraState.Character);
        }
        else
        {
            SystemFacade.instance.SwitchObjCamera(CameraManager.ObjCameraState.Enemy);
        }
    }

    private void GameWin()
    {
        StartCoroutine(IGameFinish(true));
    }
    private void GameLoss()
    {
        StartCoroutine(IGameFinish(false));
    }
    IEnumerator IGameFinish(bool isWin)
    {
        if (isWin)
        {
            Debug.Log("ս��ʤ��");
        }
        else
        {
            Debug.Log("ս��ʧ��");
        }
        yield return new WaitForSeconds(2f);
        SystemFacade.instance.BattleFinish(isWin);
        yield return new WaitForSeconds(0.5f);
        battlePanel.GetComponent<UIPanel>().Close();
        battleScene.SetActive(false);
    }

}
