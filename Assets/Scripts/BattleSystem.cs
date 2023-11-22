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
    //设置组件
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
    public GameObject selectEntityMainMask;//用来在非attack选择阶段遮挡，阻止点击spine
    private BattleCommandType selectMenuType;
    public GameObject menusContainer;
    public List<Sprite> bpSpriteList;
    public GameObject skillMenuMask;//点击skillButton时的全屏遮罩，同时包含skillMenu
    public List<SkillObject> skillObjs;//技能图标列表
    private SkillSelectInfo skillSelectInfo;
    public GameObject buffDescribe;
    public GameObject skillDescribe;

    //私有
    private bool inCommandExcuting;//是否正在执行指令的流程中
    private bool inBattleStartExcuting;//是否正在先制指令的流程中
    [ReadOnly]
    public int enemyNumber, characterNumber;
    [ReadOnly]
    private int characterCount;//角色总数
    [ReadOnly]
    public List<BattleEntity> allEntities = new List<BattleEntity>();
    private List<BattleEntity> sortEntities = new List<BattleEntity>();//根据速度排序后的entityIdList
    [ReadOnly]
    public int currentBattleId;//当前输入指令的entity的battleId
    [ReadOnly]
    public int currentRound;
    [ReadOnly]
    public int currentMenuLastIndex;//Brave计数器，用来控制menus的
    [ReadOnly]
    public int canSelectCount;
    [HideInInspector]
    public List<BattleCommandInfo> commandInfoList = new List<BattleCommandInfo>();
    private List<BattleCommandInfo> sortCommandInfoList = new List<BattleCommandInfo>();
    private List<IEnumerator> allCommandList = new List<IEnumerator>();
    private List<IEnumerator> battleStartAllCommandList = new List<IEnumerator>();
    public List<BattleEntity> selectedEntityList = new List<BattleEntity>();
    private List<Toggle> activedToggleList = new List<Toggle>();//可以进行选择的目标toggle
    private UnityEvent RoundEndEvent = new UnityEvent();


    private void Start()
    {
        InitButtonCallback();//初始化menu的所有button事件和isDisable
        RoundEndEvent.AddListener(() =>
        {
            DoRoundEndBuffEffect();
            CheckDefault();
        });
    }

    public void StartBattle(StageData.StageDataEntry stageDataT)//封装好，暴露给facade
    {
        InitBattle(stageDataT);
    }

    private void InitBattle(StageData.StageDataEntry stageDataT)
    {
        stageData = stageDataT;
        SystemFacade.instance.PlayBGM("战斗a", 0f, 0.5f, 0f, () => { SystemFacade.instance.PlayBGM("战斗b", 0f, 0.5f, 0f); });
        currentRound = 1;
        InitEntity();//初始化角色和敌人属性
        InitUI();//初始化角色和敌人的ui，包括站位
        InitCommand();//清空指令列表
        SortEntityList();
        UpdateAllEntityUiInfo();
        UpdateMenuInfo();
        StartCoroutine(AllEntityPlayEnterAnimation());
    }

    private void InitEntity()//初始化角色和敌人属性
    {
        List<int> teamList = characterData.teamIndexList;
        //清空敌人和角色entity列表
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

        //设置角色
        if (characterEntityTransList.Count < teamList.Count)
        {
            Debug.LogError("队伍角色数量大于可用战斗位置数量");
        }
        for (int i = 0; i < teamList.Count; i++)
        {
            if (teamList[i] != -1)
            {
                CharacterData.CharacterDataEntry characterDataPiece = characterData.data[teamList[i]];
                //生成角色
                GameObject characterObj = Instantiate(characterDataPiece.battleObjPrefab);
                //设置角色站位
                characterObj.transform.position = characterEntityTransList[i].position;
                characterObj.transform.SetParent(entityObjContainer.transform);


                //设置entity属性
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

                //设置下方角色列表
                characterEntity.entityInfoUi.portrait.sprite = characterDataPiece.battlePortrait;
                characterEntity.entityInfoUi.readyText.SetActive(false);
                characterEntity.entityInfoUi.hpSlider.maxValue = characterDataPiece.maxHp;
                characterEntity.entityInfoUi.hpSlider.value = characterDataPiece.maxHp;
                characterEntity.entityInfoUi.mpSlider.maxValue = 100;
                characterEntity.entityInfoUi.mpSlider.value = 0;
                characterEntity.entityInfoUi.bpText.sprite = bpSpriteList[0 + 4];
                characterEntity.entityInfoUi.healthPoint.text = "0";
                characterEntity.entityInfoUi.magicPoint.text = "0";

                //组装ui
                FollowWorldObj followScript = characterEntity.gameObject.AddComponent<FollowWorldObj>();
                followScript.objFollowed = characterObj.transform.Find("FollowPoint");
                followScript.rectFollower = characterEntity.GetComponent<RectTransform>();
                followScript.offset = new Vector2(0f, 15f);
                characterEntity.EntityInit();

                //一些初始设置
                EntityOutline(characterEntity, false);

                //加进entityList
                characterNumber++;
                characterCount++;
                allEntities.Add(characterEntity);
            }
        }

        //生成敌人
        if (stageData.enemyTeam.Count > enemyEntityTransList.Count)
        {
            Debug.LogError("不够生成位置");
        }
        for (int i = 0; i < stageData.enemyTeam.Count; i++)
        {
            if (stageData.enemyTeam[i] == -1)
            {
                continue;
            }
            EnemyData.EnemyDataEntry enemyDataPiece = enemyData.data[stageData.enemyTeam[i]];
            //生成敌人
            GameObject enemyObj = Instantiate(enemyDataPiece.battleObjPrefab);
            //设置敌人站位
            enemyObj.transform.position = enemyEntityTransList[i].position;
            enemyObj.transform.SetParent(entityObjContainer.transform);

            //设置entity属性
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

            //设置左上角敌人列表
            //enemyEntity.entityInfoUi.portrait.sprite = enemyDataPiece.battlePortrait;
            //enemyEntity.entityInfoUi.readyText.SetActive(false);
            enemyEntity.entityInfoUi.hpSlider.maxValue = enemyDataPiece.maxHp;
            enemyEntity.entityInfoUi.hpSlider.value = enemyDataPiece.maxHp;
            enemyEntity.entityInfoUi.mpSlider.maxValue = 100;
            enemyEntity.entityInfoUi.mpSlider.value = 0;
            enemyEntity.entityInfoUi.healthPoint.text = "0";
            enemyEntity.entityInfoUi.magicPoint.text = "0";

            //组装ui
            FollowWorldObj followScript = enemyEntity.gameObject.AddComponent<FollowWorldObj>();
            followScript.objFollowed = enemyObj.transform.Find("FollowPoint");
            followScript.rectFollower = enemyEntity.GetComponent<RectTransform>();
            followScript.offset = new Vector2(0f, 15f);
            enemyEntity.EntityInit();

            //一些初始设置
            EntityOutline(enemyEntity, false);

            //加进entityList
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

    private void InitCommand()//初始化和每轮动画结算完毕后调用
    {
        inCommandExcuting = false;
        inBattleStartExcuting = false;

        for (int i = 0; i < allEntities.Count; i++)
        {
            if (!allEntities[i].isDie)
            {
                currentBattleId = i;//将第一个活着entity的设为currentBattleId
                break;
            }
        }

        for (int i = 0; i < allEntities.Count; i++)//清空所有entity的指令List
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
        for (int i = 1; i < menuPanelList.Count; i++)//关闭多余的界面
        {
            menuPanelList[i].SetActive(false);
            menuPanelList[i].GetComponent<RectTransform>().anchoredPosition = new Vector2(500f, 0f);
            menuPanelList[i].transform.Find("MenuMask").gameObject.SetActive(false);
        }

        List<int> teamList = characterData.teamIndexList;//设置下方角色列表
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
        //设置左上角敌人列表
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

        InitEntityToggleList();//初始化entity的复选框
    }

    private void InitButtonCallback()//初始化menu的所有button事件和isDisable
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

        //给skillMenuMask添加回调
        Utility.AddTriggersListener(skillMenuMask.GetComponent<EventTrigger>(), EventTriggerType.PointerClick, new UnityAction<BaseEventData>(CloseSkillMenu));
        for (int i = 0; i < skillObjs.Count; i++)
        {
            int index = i;
            skillObjs[index].GetComponent<Button>().onClick.AddListener(() => { SkillMenuButton(index); });
        }

        selectEntityPanel.transform.Find("SwitchCameraToggle").GetComponent<Toggle>().onValueChanged.AddListener(SwitchCameraToggle);
        SystemFacade.instance.SwitchObjCamera(CameraManager.ObjCameraState.IdleCommand, 0f);
    }

    private void InitEntityToggleList()//初始化entity的复选框
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

    private void SortEntityList()//依据entity的Speed对allEntities排序
    {
        sortEntities.Clear();
        for (int i = 0; i < allEntities.Count; i++)
        {
            sortEntities.Add(allEntities[i]);
        }

        sortEntities.Sort((x, y) => { return y.speed.CompareTo(x.speed); });
        //测试排序
        string log = "排序后的character顺序：";
        for (int i = 0; i < sortEntities.Count; i++)
        {
            if (!sortEntities[i].isEnemy)
            {
                log += sortEntities[i].characterId + 1 + "--";
            }
        }
        Debug.Log(log);
    }
    private void SortCommandInfoList()//依据entity的Speed对CommandInfoList排序
    {
        sortCommandInfoList.Clear();
        for (int i = 0; i < commandInfoList.Count; i++)
        {
            sortCommandInfoList.Add(commandInfoList[i]);
        }
        sortCommandInfoList.Sort((x, y) => { return allEntities[y.battleId].speed.CompareTo(allEntities[x.battleId].speed); });
        //测试排序
        string originalLog = "排序前的commandInfoList顺序：";
        for (int i = 0; i < commandInfoList.Count; i++)
        {
            originalLog += "\n" + commandInfoList[i].battleId + "--的指令--" + commandInfoList[i].commandType;
        }
        string newLog = "排序后的sortCommandInfoList顺序：";
        for (int i = 0; i < sortCommandInfoList.Count; i++)
        {
            newLog += "\n" + sortCommandInfoList[i].battleId + "--的指令--" + sortCommandInfoList[i].commandType;
        }
        Debug.Log(originalLog + "\n" + newLog);

        //输出所有entity的先制指令集
        originalLog = "";
        for (int i = 0; i < allEntities.Count; i++)
        {
            originalLog += "\n" + i + "的先制指令为\n";
            for (int j = 0; j < allEntities[i].entitybattleStartCommandList.Count; j++)
            {
                originalLog += j + ".->" + allEntities[i].entitybattleStartCommandList[j] + "\n";
            }
        }
        Debug.Log("全部entity的先制指令集" + originalLog);

        //输出所有entity的指令集
        originalLog = "";
        for (int i = 0; i < allEntities.Count; i++)
        {
            originalLog += "\n" + i + "的指令为\n";
            for (int j = 0; j < allEntities[i].entityCommandList.Count; j++)
            {
                originalLog += j + ".->" + allEntities[i].entityCommandList[j] + "\n";
            }
        }
        Debug.Log("全部entity的指令集" + originalLog);
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

        if (commandInfoList.Count != 0)//控制back摁钮
        {
            backButton.SetActive(true);
        }

        if (currentMenuLastIndex == 0)//控制menu关闭
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
            for (int i = currentBattleId; i < characterCount + 1; i++)//选出没死又有bp，并且没有眩晕buff，的最先的角色
            {
                if (!allEntities[currentBattleId].isDie && allEntities[currentBattleId].GetBp() >= 0)
                {
                    if (buffManager.CheckBuff(allEntities[currentBattleId].entityInfoUi, BuffName.眩晕).Count <= 0)
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
        else if (currentMenuLastIndex == -1)//刚开始游戏，演出结束时为-1
        {
            for (int i = currentBattleId; i < characterCount + 1; i++)//选出没死又有bp，并且没有眩晕buff，的最先的角色
            {
                if (!allEntities[currentBattleId].isDie && allEntities[currentBattleId].GetBp() >= 0)
                {
                    if (buffManager.CheckBuff(allEntities[currentBattleId].entityInfoUi, BuffName.眩晕).Count <= 0)
                    {
                        break;
                    }
                }
                currentBattleId++;
            }
        }


        if (currentBattleId >= characterCount)//此时currentBallteId是比allEntity的最大index多1
        {
            currentBattleId--;
            if (characterNumber > 0)//避免游戏输或赢后还显示
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

                continueButton.GetComponent<Animator>().SetTrigger("open");//显示ContinueButton
            }

            for (int i = 0; i < menuPanelList.Count; i++)
            {
                menuPanelList[i].transform.Find("Buttons").Find("Brave").GetComponent<BattleMenuButton>().SetEnable(() => BraveButton());
            }
            menuPanelList[3].transform.Find("Buttons").Find("Brave").GetComponent<BattleMenuButton>().SetDisable();
            menuPanelList[0].transform.Find("Buttons").Find("Default").GetComponent<BattleMenuButton>().SetEnable(() => DefaultButton());
        }
        else//当前还是角色
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

            if (currentMenuLastIndex == -1)//当前menu一个都没打开
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
            else//如果当前还有打开的menu，说明在brave阶段
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

    private void CharacterReadyEvent(int entityId)//角色输入完成指令后，的事件
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
    private void CharacterUnreadyEvent(int entityId)//角色取消已输入完成的指令后，的事件
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

    private void EnemySetCommandAI()//敌人输入指令的ai
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
            Debug.LogError("敌人找不到没死的角色");
        }
        //按hatred设定选取概率
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

        //四回合输入一次技能1
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
        Debug.Log("实体" + currentBattleId + "输入了指令" + allEntities[currentBattleId].entityCommandList[allEntities[currentBattleId].entityCommandList.Count - 1] + "，commandInfo的数量为" + commandInfoList.Count);

        currentBattleId++;

        if (currentBattleId == allEntities.Count)//此时currentBallteId是比allEntity的最大index多1
        {
            SetCommandReachEnd();
            return;
        }

        EnemySetCommandAI();
    }

    private void SetCommandReachEnd()//场上所有entity都输入完指令了
    {
        Debug.Log("场上所有entity都输入完指令了");
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

        //按速度排序，然后把排序后的entity的commandList加进allCommandList
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

        SortCommandInfoList();//给commandInfoList排序
        ExcuteBattleStartCommandList();
    }

    public void ExcuteBattleStartCommandList()//执行先制指令
    {
        //Debug.Log(battleStartAllCommandList.Count);
        if (!inBattleStartExcuting)
        {
            inBattleStartExcuting = true;
        }
        if (battleStartAllCommandList.Count == 0)//指令全部执行完毕
        {
            ExcuteBattleStartCommandReachEnd();//当前轮次的指令列表全部执行完毕
            return;
        }

        BattleEntity entity = allEntities[sortCommandInfoList[0].battleId];
        if (buffManager.CheckBuff(entity.entityInfoUi, BuffName.眩晕).Count > 0)
        {
            Debug.Log(sortCommandInfoList[0].battleId + "因为眩晕，放弃执行先制指令" + battleStartAllCommandList[0]);
            battleStartAllCommandList.RemoveAt(0);
            ExcuteBattleStartCommandList();
        }
        else
        {
            Debug.Log(sortCommandInfoList[0].battleId + "执行先制指令" + battleStartAllCommandList[0]);
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
        //sortCommandInfoList.RemoveAt(0);//不能删掉，里面那个是占位的，不是先制指令的info，先制指令没有info
    }
    private void ExcuteBattleStartCommandReachEnd()//当前回合的先制指令列表全部执行完毕
    {
        inBattleStartExcuting = false;
        Debug.Log("先制指令全部执行完毕");
        UpdateAllEntityUiInfo();
        Debug.Log("开始执行常规指令");
        ExcuteCommandList();//执行指令列表队列的队首
        inCommandExcuting = true;
    }
    public void ExcuteCommandList()//执行指令列表队列的队首
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
        if (allCommandList.Count == 0)//指令全部执行完毕
        {
            ExcuteCommandReachEnd();//当前轮次的指令列表全部执行完毕
            UpdateAllEntityUiInfo();
            yield break;
        }

        BattleEntity entity = allEntities[sortCommandInfoList[0].battleId];
        if (buffManager.CheckBuff(entity.entityInfoUi, BuffName.眩晕).Count > 0)
        {
            Debug.Log(sortCommandInfoList[0].battleId + "因为眩晕，放弃执行指令" + allCommandList[0]);
            SkipCommand();
            yield break;
        }
        else
        {
            //如果bp不够就跳过
            if (allEntities[sortCommandInfoList[0].battleId].GetBp() - sortCommandInfoList[0].bpNeed < -4)
            {
                Debug.Log(sortCommandInfoList[0].battleId + "因为bp不够，放弃执行指令" + allCommandList[0]);
                SkipCommand();
                yield break;
            }

            Debug.Log(sortCommandInfoList[0].battleId + "执行指令" + allCommandList[0]);

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
    private void ExcuteCommandReachEnd()//当前轮次的指令列表全部执行完毕
    {
        currentRound++;
        RoundEndEvent.Invoke();
        Debug.Log("指令全部执行完毕");

        InitCommand();
        for (int i = 0; i < menuPanelList.Count - 1; i++)//因为最后一个menu绝对不能有Brave，所以要-1
        {
            menuPanelList[i].transform.Find("Buttons").Find("Brave").GetComponent<BattleMenuButton>().SetEnable(() => BraveButton());
        }
        menuPanelList[0].transform.Find("Buttons").Find("Default").GetComponent<BattleMenuButton>().SetEnable(() => DefaultButton());

        SystemFacade.instance.SwitchObjCamera(CameraManager.ObjCameraState.IdleCommand);

        bool otherCondition = true;
        for (int i = 0; i < allEntities.Count; i++)//演出结束后切换动画
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
        //禁用摁键
        backButton.GetComponent<Button>().interactable = false;
        for (int i = 0; i < menuPanelList.Count; i++)
        {
            menuPanelList[i].transform.Find("MenuMask").gameObject.SetActive(true);
        }

        allEntities[currentBattleId].entitybattleStartCommandList.Add(CharacterDefend(currentBattleId));
        allEntities[currentBattleId].entityCommandList.Add(BattleStartCommandInCommandList(currentBattleId));//占位
        commandInfoList.Add(new BattleCommandInfo(false, BattleCommandType.Default, true, 0, null, currentBattleId));
        Debug.Log("实体" + currentBattleId + "输入了指令" + allEntities[currentBattleId].entityCommandList[allEntities[currentBattleId].entityCommandList.Count - 1] + "，commandInfo的数量为" + commandInfoList.Count);

        SetCommand();

        //启用摁键
        backButton.GetComponent<Button>().interactable = true;
        for (int i = 0; i < menuPanelList.Count; i++)
        {
            menuPanelList[i].transform.Find("MenuMask").gameObject.SetActive(false);
        }
    }
    private void CheckDefault()//每回合结束时解除default
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
    public void BraveButton()//能执行这个事件说明Brave检测时braveCount还没超过4
    {
        //禁用摁键
        backButton.GetComponent<Button>().interactable = false;
        for (int i = 0; i < menuPanelList.Count; i++)
        {
            menuPanelList[i].transform.Find("MenuMask").gameObject.SetActive(true);
        }

        StartCoroutine(Utility.PlayAnimation(allEntities[currentBattleId].buffVfxAnim, "Brave"));

        currentMenuLastIndex++;

        for (int i = 0; i < currentMenuLastIndex; i++)//把待输入指令的menu全部向左移动
        {
            float originalX = menuPanelList[i].GetComponent<RectTransform>().anchoredPosition.x;
            menuPanelList[i].GetComponent<RectTransform>().DOAnchorPosX(originalX - 20f, 0.3f);
        }

        menuPanelList[currentMenuLastIndex].GetComponent<RectTransform>().anchoredPosition = new Vector2(500f, 0f);
        menuPanelList[currentMenuLastIndex].SetActive(true);
        menuPanelList[currentMenuLastIndex].GetComponent<RectTransform>().DOAnchorPos(new Vector2(0f, 0f), 0.3f);

        allEntities[currentBattleId].entityCommandList.Add(CharacterBrave(currentBattleId));
        commandInfoList.Add(new BattleCommandInfo(false, BattleCommandType.Brave, false, 1, null, currentBattleId));
        Debug.Log("实体" + currentBattleId + "输入了指令" + allEntities[currentBattleId].entityCommandList[allEntities[currentBattleId].entityCommandList.Count - 1] + "，commandInfo的数量为" + commandInfoList.Count);

        if (allEntities[currentBattleId].braveCount > 0)
        {
            allEntities[currentBattleId].bpPreview--;
        }
        else if (allEntities[currentBattleId].braveCount == 0)//bp{review=-4就不能brave
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

        //启用摁键
        backButton.GetComponent<Button>().interactable = true;
        for (int i = 0; i < menuPanelList.Count; i++)
        {
            menuPanelList[i].transform.Find("MenuMask").gameObject.SetActive(false);
        }
    }

    public void BackButton()//撤销
    {
        StartCoroutine(IBackButton());
    }
    IEnumerator IBackButton()
    {
        //禁用摁键
        backButton.GetComponent<Button>().interactable = false;
        for (int i = 0; i < menuPanelList.Count; i++)
        {
            menuPanelList[i].transform.Find("MenuMask").gameObject.SetActive(true);
        }

        Tweener tweener;
        //判断当前continueButton是否在open状态
        AnimatorStateInfo stateinfo = continueButton.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0);
        if (stateinfo.IsName("ContinueButtonOpen"))
        {
            continueButton.GetComponent<Animator>().SetTrigger("close");
            CharacterUnreadyEvent(currentBattleId);
        }

        if (commandInfoList.Count > 1 && currentBattleId != commandInfoList[commandInfoList.Count - 1].battleId)//还有上一个指令，并且要切换角色
        {
            currentBattleId = commandInfoList[commandInfoList.Count - 1].battleId;//上一个输入指令的角色

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
        else if (commandInfoList.Count == 1 && currentBattleId != commandInfoList[0].battleId/*commandInfoList[0].commandType != BattleCommandType.Brave && characterNumber > 1*/)//这是最早的第一条指令，并且不是brave
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

        bool isBraveCommand = false;//这个指令是否是在brave界面输入的attack等指令
        //初始化
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
                    allEntities[currentBattleId].bpPreview += 1;//返还bp
                }
                else if (allEntities[currentBattleId].braveCount == 1)
                {
                    allEntities[currentBattleId].bpPreview += 2;//返还bp
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

                //函数要放里面，不然有时差，先减了后执行前面的回调,并且要在动画时关闭backButton不然出错
                menuPanelList[currentMenuLastIndex].SetActive(false);
                currentMenuLastIndex--;

                UpdateMenuInfo();
                break;

            case BattleCommandType.Default:
                break;
        }

        //如果撤销的是先制指令
        if (commandInfoList[commandInfoList.Count - 1].isBattleStartCommand)
        {
            allEntities[currentBattleId].entitybattleStartCommandList.RemoveAt(allEntities[currentBattleId].entitybattleStartCommandList.Count - 1);
        }
        Debug.Log("实体" + currentBattleId + "撤销了先制指令" + allEntities[currentBattleId].entityCommandList[allEntities[currentBattleId].entityCommandList.Count - 1] + "，commandInfo的数量+1为" + commandInfoList.Count);

        //统一撤销，包括先制指令有个占位的在commandList里
        Debug.Log("实体" + currentBattleId + "撤销了指令" + allEntities[currentBattleId].entityCommandList[allEntities[currentBattleId].entityCommandList.Count - 1] + "，commandInfo的数量+1为" + commandInfoList.Count);
        allEntities[currentBattleId].entityCommandList.RemoveAt(allEntities[currentBattleId].entityCommandList.Count - 1);
        commandInfoList.RemoveAt(commandInfoList.Count - 1);

        if (commandInfoList.Count == 0)
        {
            backButton.SetActive(false);
        }

        //启用摁键
        backButton.GetComponent<Button>().interactable = true;
        for (int i = 0; i < menuPanelList.Count; i++)
        {
            menuPanelList[i].transform.Find("MenuMask").gameObject.SetActive(false);
        }
    }

    IEnumerator InitDoneMenuForBack()
    {
        Tweener tweener1;

        int braveCount = allEntities[currentBattleId].braveCount;//比如角色设置了三层，就是2，撤回时，这个值就是2
        for (int i = 1; i < braveCount + 1; i++)//这个就是1,2
        {
            menuPanelList[i].GetComponent<RectTransform>().anchoredPosition = new Vector2(500f, 0f);
            menuPanelList[i].SetActive(true);
        }
        for (int i = braveCount + 1; i < menuPanelList.Count; i++)//这个是3
        {
            menuPanelList[i].GetComponent<RectTransform>().anchoredPosition = new Vector2(500f, 0f);
            menuPanelList[i].SetActive(false);
        }
        menuPanelList[0].GetComponent<RectTransform>().anchoredPosition = new Vector2(500f, 0f);
        menuPanelList[0].SetActive(true);//这个就是0
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

    IEnumerator CharacterAttack(int battleId, int bpNeed, List<BattleEntity> selectEntity)//这个enemyIndexList只有1个元素
    {
        float damagePoint = allEntities[battleId].damage;
        //Debug.Log(selectEntity.Count);
        for (int i = 0; i < selectEntity.Count; i++)
        {
            //Debug.Log(selectEntity[i]);

            if (selectEntity[i].isDie)
            {
                //无反应
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
    IEnumerator BattleStartCommandInCommandList(int battleId)//先制指令加进commandList中占位的
    {
        yield return null;
        ExcuteCommandList();
    }
    IEnumerator CharacterDefend(int battleId)//加进BattleStartCommandList中执行的
    {
        allEntities[battleId].defend += 500;//在RoundListener中注册了事件，检测是否输入default并减去defend
        yield return null;
        ExcuteBattleStartCommandList();
    }
    IEnumerator CharacterBrave(int battleId)//加进commandList中占位的
    {
        yield return null;
        ExcuteCommandList();
    }

    //entity
    public void EntityGetDamage(BattleEntity entityGet, BattleEntity entitySet, float damagePoint, float damageRateAddition = 0)
    {
        if (entityGet.isDie)//如果角色已经死了
        {
            return;
        }
        else//buff+Damage()
        {
            //Debug.Log(entityGet.characterInfoBtn != null);
            //Debug.Log(entityGet.characterInfoBtn != null && entityGet.characterInfoBtn.CheckBuff(BuffName.被掩护) > 0);
            //Debug.Log(entityGet.hurtToEntity != null);
            if (entityGet.entityInfoUi != null && buffManager.CheckBuff(entityGet.entityInfoUi, BuffName.被掩护).Count > 0 && entityGet.hurtToEntity != null)//有掩护方
            {
                Debug.Log(entityGet + "的受伤转移给" + entityGet.hurtToEntity);
                entityGet = entityGet.hurtToEntity;
            }

            EntityGetDamageNoReDamageBuffCheck(entityGet, entitySet, damagePoint, damageRateAddition);

            if (entityGet.entityInfoUi != null && buffManager.CheckBuff(entityGet.entityInfoUi, BuffName.反击架势).Count > 0)//有反击架势
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
        //    "伤害-防御=" + (int)(Mathf.Clamp((damagePoint - entityGet.defend), 0f, 1000000f)) + "\n" +
        //    "伤害rate*受伤rate=" + entityGet.hurtRate * entitySet.damageRate + "\n" +
        //    "总伤害=" + (int)(Mathf.Clamp((damagePoint - entityGet.defend), 0f, 1000000f) * entityGet.hurtRate * entitySet.damageRate));
        entityGet.hurtPointText.text = hurtPoint.ToString();
        Utility.TextFly(entityGet.hurtPointText, Vector3.zero);

        if (!entityGet.isEnemy)
        {
            //暂时设定成挨打会加10点MP==================================================================================
            entityGet.mp += 10;
            if (entityGet.mp > 100)
            {
                entityGet.mp = 100;
            }
        }
        DescreaseEntityHp(entityGet, entitySet, hurtPoint);
    }
    public void DescreaseEntityHp(BattleEntity entityGet, BattleEntity entitySet, int hpDescreaseCount)//强制减少hp
    {
        entityGet.UpdateHp(-hpDescreaseCount);

        if (entityGet.GetHp() <= 0)
        {
            if (buffManager.CheckBuffObj(entityGet.entityInfoUi, BuffName.返生).Count > 0)//有返生buff
            {
                BuffObject buffObj = buffManager.CheckBuffObj(entityGet.entityInfoUi, BuffName.返生)[0];
                buffManager.RemoveBuffEffectSwitch(buffObj);
                UpdateAllEntityUiInfo();
                return;
            }
            StartCoroutine(IEntityDie(entityGet));
        }

        UpdateAllEntityUiInfo();//更新下方角色信息列表
    }
    public void RecoverEntityHp(BattleEntity entity, int hpAddValue)//检测buff，来回复entity生命值，不检查buff就直接调用entity.UpdateHp()
    {
        if (buffManager.CheckBuff(entity.entityInfoUi, BuffName.燃烬).Count <= 0)
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
                    Debug.LogError("敌人攻击敌人了，待处理");
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
        //初始化角色的技能表=====================================================================
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
    private void CloseSkillMenu(BaseEventData baseEventData)//关闭SkillMenu面板和遮罩
    {
        for (int i = 0; i < menuPanelList.Count; i++)
        {
            Transform skillButtonTransform = menuPanelList[i].transform.Find("Buttons").Find("Skill");
            skillButtonTransform.GetComponent<Image>().color = new Color32(255, 255, 255, 255);
        }
        skillMenuMask.SetActive(false);
    }
    private void SkillMenuButton(int skillIndex)//点击输入了技能
    {
        //重置skillSelectInfo
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

            Debug.Log("实体" + currentBattleId + "输入了指令" + allEntities[currentBattleId].entityCommandList[allEntities[currentBattleId].entityCommandList.Count - 1] + "，commandInfo的数量为" + commandInfoList.Count);
            SystemFacade.instance.SwitchObjCamera(CameraManager.ObjCameraState.IdleCommand);
            backButton.SetActive(true);
        }

        CloseSkillMenu(null);
        skillDescribe.GetComponent<CanvasGroup>().alpha = 0f;
        //Debug.Log(skillIndex);
    }
    public void UniqueSkillButton()
    {
        //禁用摁键
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

            Debug.Log("实体" + currentBattleId + "输入了指令" + allEntities[currentBattleId].entityCommandList[allEntities[currentBattleId].entityCommandList.Count - 1] + "，commandInfo的数量为" + commandInfoList.Count);
            SystemFacade.instance.SwitchObjCamera(CameraManager.ObjCameraState.IdleCommand);
            backButton.SetActive(true);
        }

        //启用摁键
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
            Debug.LogError("选择了目标但目标列表为空");
        }

        List<BattleEntity> selectedEntity = new List<BattleEntity>();//list是引用类型，必须要深拷贝
        for (int i = 0; i < selectedEntityList.Count; i++)
        {
            selectedEntity.Add(selectedEntityList[i]);
        }

        switch (skillSelectInfo.battleEntity.characterId)
        {
            case 0://维多利亚
                switch (skillSelectInfo.skillIndex)
                {
                    case 0://反击架势，先制指令
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

            case 1://维克多
                switch (skillSelectInfo.skillIndex)
                {
                    case 0://反击架势，先制指令
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

        return skillSelectInfo;//引用变量，绝对会在上面的Switch中被修改然后传出
    }

    //Buff
    private void DoRoundEndBuffEffect()//每回合最后执行所有buff效果
    {
        for (int i = 0; i < allEntities.Count; i++)
        {
            if (!allEntities[i].isDie)
            {
                for (int j = 0; j < allEntities[i].entityInfoUi.buffs.Count; j++)//执行buff效果
                {
                    switch (allEntities[i].entityInfoUi.buffs[j].buffInfo.buffName)
                    {
                        case BuffName.苦旅:
                            for (int k = 0; k < commandInfoList.Count; k++)
                            {
                                if (commandInfoList[k].battleId == i && commandInfoList[k].commandType == BattleCommandType.Default)
                                {
                                    allEntities[i].UpdateBp(2);
                                    break;
                                }
                            }
                            break;
                        case BuffName.增生:
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
                        case BuffName.虚缪:
                            float damagePoint = allEntities[i].entityInfoUi.debuffs[j].buffInfo.entitySet.damage * 0.7f;
                            EntityGetDamage(allEntities[i], allEntities[i].entityInfoUi.debuffs[j].buffInfo.entitySet, damagePoint);
                            break;
                    }
                }
                buffManager.SubstractRoundDuring(allEntities[i]);//执行清除buff时的效果
                buffDescribe.SetActive(false);
            }
        }
        UpdateAllEntityUiInfo();
    }

    //选择目标模块
    private void SelectMenuOpen(BattleCommandType battleCommandType, SelectType selectType, bool cameraLookAt, int canSelectCountT)//初始化选择目标
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
    public void SelectOkButton()//从选择目标界面点击ok
    {
        //禁用摁键
        backButton.GetComponent<Button>().interactable = false;
        for (int i = 0; i < menuPanelList.Count; i++)
        {
            menuPanelList[i].transform.Find("MenuMask").gameObject.SetActive(true);
        }

        //Debug.Log(currentBallteId);
        //Debug.Log(selectedEntityList.Count);
        if (selectedEntityList.Count <= 0)//判断选择了目标没
        {
            return;//一个都没选
        }

        for (int i = 0; i < allEntities.Count; i++)//关闭entity描边
        {
            if (!allEntities[i].isDie)
            {
                EntityOutline(allEntities[i], false);
            }
        }

        List<BattleEntity> selectedEntity = new List<BattleEntity>();//list是引用类型，必须要深拷贝
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
                Debug.LogError("目标输入待添加新的CommandType");
                break;
        }

        Debug.Log("实体" + currentBattleId + "输入了指令" + allEntities[currentBattleId].entityCommandList[allEntities[currentBattleId].entityCommandList.Count - 1] + "，commandInfo的数量为" + commandInfoList.Count);

        SetCommand();
        selectEntityPanel.SetActive(false);
        SystemFacade.instance.SwitchObjCamera(CameraManager.ObjCameraState.IdleCommand);
        selectEntityMainMask.SetActive(true);
        backButton.SetActive(true);
        ResetSelectEntity();

        //启用摁键
        backButton.GetComponent<Button>().interactable = true;
        for (int i = 0; i < menuPanelList.Count; i++)
        {
            menuPanelList[i].transform.Find("MenuMask").gameObject.SetActive(false);
        }
    }
    public void SelectUndoButton()//从选择目标界面点击undo
    {
        //禁用摁键
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

        for (int i = 0; i < allEntities.Count; i++)//关闭entity描边
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

        //启用摁键
        backButton.GetComponent<Button>().interactable = true;
        for (int i = 0; i < menuPanelList.Count; i++)
        {
            menuPanelList[i].transform.Find("MenuMask").gameObject.SetActive(false);
        }
    }
    private void SelectEntity(bool isOn, BattleEntity entity)//给selectAttackButton调用
    {
        if (isOn)
        {
            selectedEntityList.Add(entity);
            canSelectCount--;
            if (canSelectCount <= 0 && activedToggleList[0].group == null)//activedToggleList[0].group != null来判断是不是只允许选择单体,因为选择单体要设定group
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
    private void ResetSelectEntity()//重置选择目标
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
        buffDescribe.SetActive(true);//强制更新contentSizeFilter需要开启gameObj

        //inspector窗口的string输入时会自动把\n转成\\n ，所以要转回来，不然不换行，艹
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
        //强制更新contentSizeFilter，不然更新大小有延迟
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
        //通过将skillDescribe的缩放xy改为-1，然后字体的缩放xy改为-1，实现向左下角拓展
        skillDescribe.transform.position = skillObj.transform.position + new Vector3(-17f, 32.4f, 0f);
        skillDescribe.SetActive(true);//强制更新contentSizeFilter需要开启gameObj

        //Debug.Log(skillObj.skillInfo.skillDescription);
        skillDescribe.transform.Find("InfoText").GetComponent<TextMeshProUGUI>().text = skillObj.skillInfo.skillDescription.Replace("\\n", "\n");
        //强制更新contentSizeFilter，不然更新大小有延迟
        Utility.ForceUpdateContentSizeFilter(skillDescribe.transform);

        skillDescribe.GetComponent<CanvasGroup>().alpha = 1;
    }
    public void CloseSkillDescribe()
    {
        skillDescribe.GetComponent<CanvasGroup>().alpha = 0;
        skillDescribe.SetActive(false);
    }

    public void UpdateAllEntityUiInfo()//更新下方角色信息列表
    {
        //设置信息
        for (int i = 0; i < allEntities.Count; i++)
        {
            if (allEntities[i].isDie)//角色死了
            {
                allEntities[i].entityInfoUi.hpSlider.value = 0;
                allEntities[i].entityInfoUi.mpSlider.value = 0;
                allEntities[i].entityInfoUi.healthPoint.text = "0";
                allEntities[i].entityInfoUi.magicPoint.text = "0";
                allEntities[i].hpSlider.value = 0f;

                if (!allEntities[i].isEnemy)//如果是角色
                {
                    allEntities[i].entityInfoUi.bpText.sprite = bpSpriteList[0 + 4];
                    allEntities[i].entityInfoUi.portrait.color = new Color(0.5f, 0.5f, 0.5f, 1f);
                }
            }
            else//角色没死
            {
                allEntities[i].entityInfoUi.hpSlider.value = allEntities[i].GetHp();
                allEntities[i].entityInfoUi.mpSlider.value = allEntities[i].mp;
                allEntities[i].entityInfoUi.healthPoint.text = allEntities[i].GetHp().ToString();
                allEntities[i].entityInfoUi.magicPoint.text = allEntities[i].mp.ToString();
                allEntities[i].hpSlider.value = allEntities[i].GetHp();

                if (!allEntities[i].isEnemy)//如果是角色
                {
                    allEntities[i].entityInfoUi.bpText.sprite = bpSpriteList[allEntities[i].GetBp() + 4];
                    allEntities[i].entityInfoUi.portrait.color = new Color(1f, 1f, 1f, 1f);
                }
            }
        }
    }
    private void UpdateMenuInfo()//更新角色指令面板信息
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
            Debug.Log("战斗胜利");
        }
        else
        {
            Debug.Log("战斗失败");
        }
        yield return new WaitForSeconds(2f);
        SystemFacade.instance.BattleFinish(isWin);
        yield return new WaitForSeconds(0.5f);
        battlePanel.GetComponent<UIPanel>().Close();
        battleScene.SetActive(false);
    }

}
