using PixelCrushers.DialogueSystem.UnityGUI;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class BuffManager : MonoBehaviour
{
    public BattleSystem battleSystem;
    public GameObject buffObjPrefab;

    public BuffData buffData;
    public CharacterData characterData;


    public IEnumerator AddBuff(BuffName buffNameT, BattleEntity entityGetT, BattleEntity entitySetT, int roundDuringT, params object[] buffValueT)//添加buff时执行的效果
    {
        BuffData.BuffDataEntry buffEntryT = null;
        for (int i = 0; i < buffData.data.Count; i++)
        {
            if (buffData.data[i].buffName == buffNameT)
            {
                buffEntryT = buffData.data[i];
                break;
            }
        }
        if (buffEntryT == null)
        {
            Debug.LogError("没有在buffData中找到BuffName" + buffNameT);
        }

        BuffInfo newBuffInfo = new BuffInfo()
        {
            buffName = buffNameT,
            entityGet = entityGetT,
            entitySet = entitySetT,
            roundDuring = roundDuringT,
            buffValues = buffValueT,
            data = buffEntryT
        };

        BattleEntity entityGet = newBuffInfo.entityGet;
        BattleEntity entitySet = newBuffInfo.entitySet;
        CharacterInfoBtn characterInfoBtn = entityGet.entityInfoUi;

        //添加buff的叠层逻辑
        List<BuffObject> sameBuffObjs = CheckBuffObj(characterInfoBtn, buffNameT);
        bool hasSameBuffObj = sameBuffObjs.Count > 0;

        if (hasSameBuffObj)
        {
            if (newBuffInfo.data.相同buff是否可并行存在)
            {
                AddBuffObj(newBuffInfo, characterInfoBtn);
                yield return StartCoroutine(AddBuffEffectSwitch(newBuffInfo));
            }
            else//相同buff不可并行存在，并且有同名BuffObj
            {
                BuffObject oldBuffObj = sameBuffObjs[0];

                if (newBuffInfo.data.是否可叠层)
                {
                    if (newBuffInfo.data.重复释放是否刷新回合数)//buffObj层数+1，更新during
                    {
                        oldBuffObj.UpdateLayers(1);
                        oldBuffObj.buffInfo.roundDuring = roundDuringT;
                        yield return StartCoroutine(AddBuffEffectSwitch(oldBuffObj.buffInfo));
                    }
                    else//buffObj层数+1
                    {
                        oldBuffObj.UpdateLayers(1);
                        yield return StartCoroutine(AddBuffEffectSwitch(oldBuffObj.buffInfo));
                    }
                }
                else//层数不加，所以不会再执行一次BuffEffect
                {
                    if (newBuffInfo.data.重复释放是否刷新回合数)//更新BuffObj的during
                    {
                        oldBuffObj.buffInfo.roundDuring = roundDuringT;
                    }
                    else
                    {
                        //不更新
                    }
                }
            }
        }
        else//没有同名buffObj
        {
            AddBuffObj(newBuffInfo, characterInfoBtn);
            yield return StartCoroutine(AddBuffEffectSwitch(newBuffInfo));
        }
    }
    private void AddBuffObj(BuffInfo buffInfo, CharacterInfoBtn characterInfoBtn)
    {
        BuffObject buffObjTemp = Instantiate(buffObjPrefab).GetComponent<BuffObject>();
        buffObjTemp.SetLayers(1);
        buffObjTemp.buffInfo = buffInfo;

        if (buffObjTemp.buffInfo.data.isDebuff)
        {
            buffObjTemp.transform.SetParent(characterInfoBtn.buffsContainer.Find("Debuffs"));
            characterInfoBtn.debuffs.Add(buffObjTemp);
        }
        else
        {
            buffObjTemp.transform.SetParent(characterInfoBtn.buffsContainer.Find("Buffs"));
            characterInfoBtn.buffs.Add(buffObjTemp);
        }
    }

    private IEnumerator AddBuffEffectSwitch(BuffInfo buffInfo)
    {
        BattleEntity entityGet = buffInfo.entityGet;

        switch (buffInfo.buffName)
        {
            case BuffName.星:
                //降低防御力
                int defandDecrease = 0;
                if (entityGet.isEnemy)
                {
                    defandDecrease = (int)(entityGet.enemyDataEntry.defend * 0.2f);
                }
                else
                {
                    defandDecrease = (int)(characterData.data[entityGet.characterId].defend * 0.2f);
                }
                entityGet.defend -= defandDecrease;
                buffInfo.buffValues = new object[1] { "20%" };
                break;
            case BuffName.激情:
                buffInfo.buffValues = new object[1] { "20%" };
                break;
            case BuffName.攻击力提升:
                entityGet.damage += int.Parse(buffInfo.buffValues[0].ToString());
                break;
            case BuffName.攻击力下降:
                entityGet.damage -= int.Parse(buffInfo.buffValues[0].ToString());
                break;
            case BuffName.受伤加重:
                entityGet.hurtRate += float.Parse(buffInfo.buffValues[0].ToString());
                break;
            case BuffName.虚缪:
                buffInfo.buffValues = new object[1] { "70%" };
                break;
            case BuffName.苦旅:
                buffInfo.buffValues = new object[1] { "2" };
                break;
            case BuffName.忍耐:
                entityGet.hurtRate -= float.Parse(buffInfo.buffValues[0].ToString());
                break;
            case BuffName.反击架势:
                buffInfo.buffValues = new object[1] { "70%" };
                break;
            case BuffName.被掩护:
                entityGet.hurtToEntity = buffInfo.buffValues[0] as BattleEntity;//设置伤害转移的目标
                break;
            case BuffName.增生:
                buffInfo.buffValues = new object[1] { "30%" };
                break;
            case BuffName.返生:
                buffInfo.buffValues = new object[1] { "1" };
                break;
            case BuffName.眩晕:
                break;
            case BuffName.燃烬:
                break;
            default:
                Debug.LogError("没有添加这个buff的效果" + buffInfo.buffName);
                break;
        }
        yield return null;
    }

    //移除buff时执行的效果
    public void RemoveBuffEffectSwitch(BuffObject buffObj)
    {
        BuffInfo buffInfo = buffObj.buffInfo;
        BattleEntity entityGet = buffInfo.entityGet;
        BattleEntity entitySet = buffInfo.entitySet;

        for (int i = 0; i < buffObj.GetLayers(); i++)
        {
            switch (buffInfo.buffName)
            {
                case BuffName.星:
                    if (entityGet.isEnemy)//battleEntity恢复防御力
                    {
                        entityGet.defend += (int)(entityGet.enemyDataEntry.defend  *0.2f);
                    }
                    else
                    {
                        entityGet.defend += (int)(characterData.data[entityGet.characterId].defend * 0.2f);
                    }
                    break;
                case BuffName.激情:
                    break;
                case BuffName.攻击力提升:
                    entityGet.damage -= int.Parse(buffInfo.buffValues[0].ToString());
                    break;
                case BuffName.攻击力下降:
                    entityGet.damage += int.Parse(buffInfo.buffValues[0].ToString());
                    break;
                case BuffName.受伤加重:
                    entityGet.hurtRate -= float.Parse(buffInfo.buffValues[0].ToString());
                    break;
                case BuffName.虚缪:
                    break;
                case BuffName.苦旅:
                    break;
                case BuffName.忍耐:
                    entityGet.hurtRate += float.Parse(buffInfo.buffValues[0].ToString());
                    break;
                case BuffName.反击架势:
                    break;
                case BuffName.被掩护:
                    entityGet.hurtToEntity = null;
                    break;
                case BuffName.增生:
                    break;
                case BuffName.返生:
                    entityGet.SetHp(1);
                    break;
                case BuffName.眩晕:
                    break;
                case BuffName.燃烬:
                    break;
                default:
                    Debug.LogError("没有添加这个buff的效果" + buffInfo.buffName);
                    break;
            }
        }

    }

    public void CheckBpSubstract(BattleEntity characterEntity, int bpNeed)//通过buffManager减少bp
    {
        for (int j = 0; j < bpNeed; j++)
        {
            if (CheckBuff(characterEntity.entityInfoUi, BuffName.激情).Count > 0)
            {
                int damageAdd = (int)(characterData.data[characterData.teamIndexList[characterEntity.characterId]].damage * 0.2f);

                UnityAction callback = new UnityAction(() =>
                {
                    StartCoroutine(AddBuff(BuffName.攻击力提升, characterEntity, characterEntity, 1, damageAdd));
                });
                characterEntity.UpdateBp(-1, callback);
            }
            else
            {
                characterEntity.UpdateBp(-1, null);
            }
        }
    }

    //每回合结束时执行某些buffEffect
    //在battleSystem中的DoRoundEndBuffEffect实现，因为像是否输入了防御指令这些事件懒得去处理

    //清除所有buff
    public void ClearBuff(CharacterInfoBtn characterInfoBtn)
    {
        characterInfoBtn.buffs.Clear();
        characterInfoBtn.debuffs.Clear();
        for (int i = 0; i < characterInfoBtn.buffsContainer.Find("Buffs").childCount; i++)
        {
            Destroy(characterInfoBtn.buffsContainer.Find("Buffs").GetChild(i).gameObject);
        }
        for (int i = 0; i < characterInfoBtn.buffsContainer.Find("Debuffs").childCount; i++)
        {
            Destroy(characterInfoBtn.buffsContainer.Find("Debuffs").GetChild(i).gameObject);
        }
    }

    //每回合buff持续时间减一
    public List<BuffObject> SubstractRoundDuring(BattleEntity entityGet)
    {
        CharacterInfoBtn characterInfoBtn = entityGet.entityInfoUi;

        List<BuffObject> cleanBuffObjs = new List<BuffObject>();
        for (int i = 0; i < characterInfoBtn.buffs.Count; i++)
        {
            characterInfoBtn.buffs[i].buffInfo.roundDuring--;
            if (characterInfoBtn.buffs[i].buffInfo.roundDuring <= 0)
            {
                GameObject waitDestoryObj = characterInfoBtn.buffs[i].gameObject;
                cleanBuffObjs.Add(characterInfoBtn.buffs[i]);

                characterInfoBtn.buffs.Remove(characterInfoBtn.buffs[i]);
                Destroy(waitDestoryObj);
            }
        }
        for (int i = 0; i < characterInfoBtn.debuffs.Count; i++)
        {
            characterInfoBtn.debuffs[i].buffInfo.roundDuring--;
            if (characterInfoBtn.debuffs[i].buffInfo.roundDuring <= 0)
            {
                GameObject waitDestoryObj = characterInfoBtn.debuffs[i].gameObject;
                cleanBuffObjs.Add(characterInfoBtn.debuffs[i]);

                characterInfoBtn.debuffs.Remove(characterInfoBtn.debuffs[i]);
                Destroy(waitDestoryObj);
            }
        }

        for (int j = 0; j < cleanBuffObjs.Count; j++)//执行清除buff时的效果
        {
            RemoveBuffEffectSwitch(cleanBuffObjs[j]);
        }

        return cleanBuffObjs;
    }

    public void ForceRemoveBuffNoCallback(CharacterInfoBtn infoBtn, BuffName buffName)//直接移除名字一样的最先的buff
    {
        BuffObject buffObj = null;
        for (int i = 0; i < infoBtn.buffs.Count; i++)
        {
            if (infoBtn.buffs[i].buffInfo.buffName == buffName)
            {
                buffObj = infoBtn.buffs[i];
                break;
            }
        }
        for (int i = 0; i < infoBtn.debuffs.Count; i++)
        {
            if (infoBtn.debuffs[i].buffInfo.buffName == buffName)
            {
                buffObj = infoBtn.debuffs[i];
                break;
            }
        }
        if (buffObj != null)
        {
            if (buffObj.buffInfo.data.isDebuff)
            {
                GameObject waitDestoryObj = buffObj.gameObject;
                infoBtn.debuffs.Remove(buffObj);
                Destroy(waitDestoryObj);
            }
            else
            {
                GameObject waitDestoryObj = buffObj.gameObject;
                infoBtn.buffs.Remove(buffObj);
                Destroy(waitDestoryObj);
            }
        }
        else
        {
            Debug.LogError("没有要清除的buff" + buffName);
        }
    }

    public List<BuffInfo> CheckBuff(CharacterInfoBtn infoBtn, BuffName buffNameT)//根据buffName来获取buff
    {
        List<BuffInfo> tempBuffs = new List<BuffInfo>();

        for (int i = 0; i < infoBtn.buffs.Count; i++)
        {
            if (infoBtn.buffs[i].buffInfo.buffName == buffNameT)
            {
                tempBuffs.Add(infoBtn.buffs[i].buffInfo);
            }
        }
        for (int i = 0; i < infoBtn.debuffs.Count; i++)
        {
            if (infoBtn.debuffs[i].buffInfo.buffName == buffNameT)
            {
                tempBuffs.Add(infoBtn.debuffs[i].buffInfo);
            }
        }
        return tempBuffs;
    }

    public List<BuffInfo> CheckBuff(CharacterInfoBtn infoBtn, bool isDebuff)//获取所有的buff或者debuff
    {
        List<BuffInfo> tempBuffs = new List<BuffInfo>();

        if (isDebuff)
        {
            for (int i = 0; i < infoBtn.debuffs.Count; i++)
            {
                tempBuffs.Add(infoBtn.debuffs[i].buffInfo);
            }
        }
        else
        {
            for (int i = 0; i < infoBtn.buffs.Count; i++)
            {
                tempBuffs.Add(infoBtn.buffs[i].buffInfo);
            }
        }

        return tempBuffs;
    }

    public List<BuffObject> CheckBuffObj(CharacterInfoBtn infoBtn, BuffName buffNameT)//获取BuffObject
    {
        List<BuffObject> tempBuffObjs = new List<BuffObject>();

        for (int i = 0; i < infoBtn.buffs.Count; i++)
        {
            if (infoBtn.buffs[i].buffInfo.buffName == buffNameT)
            {
                tempBuffObjs.Add(infoBtn.buffs[i]);
            }
        }
        for (int i = 0; i < infoBtn.debuffs.Count; i++)
        {
            if (infoBtn.debuffs[i].buffInfo.buffName == buffNameT)
            {
                tempBuffObjs.Add(infoBtn.debuffs[i]);
            }
        }
        return tempBuffObjs;
    }

    public List<BuffInfo> DisperseDebuff(CharacterInfoBtn characterInfoBtn)//驱散debuff
    {
        List<BuffInfo> debuffInfos = new List<BuffInfo>();

        for (int i = 0; i < characterInfoBtn.debuffs.Count; i++)
        {
            if (characterInfoBtn.debuffs[i].buffInfo.data.是否可驱散)
            {
                GameObject waitDestoryObj = characterInfoBtn.debuffs[i].gameObject;
                debuffInfos.Add(characterInfoBtn.debuffs[i].buffInfo);

                characterInfoBtn.debuffs.Remove(characterInfoBtn.debuffs[i]);
                Destroy(waitDestoryObj);
            }
        }
        return debuffInfos;
    }
}