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


    public IEnumerator AddBuff(BuffName buffNameT, BattleEntity entityGetT, BattleEntity entitySetT, int roundDuringT, params object[] buffValueT)//���buffʱִ�е�Ч��
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
            Debug.LogError("û����buffData���ҵ�BuffName" + buffNameT);
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

        //���buff�ĵ����߼�
        List<BuffObject> sameBuffObjs = CheckBuffObj(characterInfoBtn, buffNameT);
        bool hasSameBuffObj = sameBuffObjs.Count > 0;

        if (hasSameBuffObj)
        {
            if (newBuffInfo.data.��ͬbuff�Ƿ�ɲ��д���)
            {
                AddBuffObj(newBuffInfo, characterInfoBtn);
                yield return StartCoroutine(AddBuffEffectSwitch(newBuffInfo));
            }
            else//��ͬbuff���ɲ��д��ڣ�������ͬ��BuffObj
            {
                BuffObject oldBuffObj = sameBuffObjs[0];

                if (newBuffInfo.data.�Ƿ�ɵ���)
                {
                    if (newBuffInfo.data.�ظ��ͷ��Ƿ�ˢ�»غ���)//buffObj����+1������during
                    {
                        oldBuffObj.UpdateLayers(1);
                        oldBuffObj.buffInfo.roundDuring = roundDuringT;
                        yield return StartCoroutine(AddBuffEffectSwitch(oldBuffObj.buffInfo));
                    }
                    else//buffObj����+1
                    {
                        oldBuffObj.UpdateLayers(1);
                        yield return StartCoroutine(AddBuffEffectSwitch(oldBuffObj.buffInfo));
                    }
                }
                else//�������ӣ����Բ�����ִ��һ��BuffEffect
                {
                    if (newBuffInfo.data.�ظ��ͷ��Ƿ�ˢ�»غ���)//����BuffObj��during
                    {
                        oldBuffObj.buffInfo.roundDuring = roundDuringT;
                    }
                    else
                    {
                        //������
                    }
                }
            }
        }
        else//û��ͬ��buffObj
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
            case BuffName.��:
                //���ͷ�����
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
            case BuffName.����:
                buffInfo.buffValues = new object[1] { "20%" };
                break;
            case BuffName.����������:
                entityGet.damage += int.Parse(buffInfo.buffValues[0].ToString());
                break;
            case BuffName.�������½�:
                entityGet.damage -= int.Parse(buffInfo.buffValues[0].ToString());
                break;
            case BuffName.���˼���:
                entityGet.hurtRate += float.Parse(buffInfo.buffValues[0].ToString());
                break;
            case BuffName.����:
                buffInfo.buffValues = new object[1] { "70%" };
                break;
            case BuffName.����:
                buffInfo.buffValues = new object[1] { "2" };
                break;
            case BuffName.����:
                entityGet.hurtRate -= float.Parse(buffInfo.buffValues[0].ToString());
                break;
            case BuffName.��������:
                buffInfo.buffValues = new object[1] { "70%" };
                break;
            case BuffName.���ڻ�:
                entityGet.hurtToEntity = buffInfo.buffValues[0] as BattleEntity;//�����˺�ת�Ƶ�Ŀ��
                break;
            case BuffName.����:
                buffInfo.buffValues = new object[1] { "30%" };
                break;
            case BuffName.����:
                buffInfo.buffValues = new object[1] { "1" };
                break;
            case BuffName.ѣ��:
                break;
            case BuffName.ȼ��:
                break;
            default:
                Debug.LogError("û��������buff��Ч��" + buffInfo.buffName);
                break;
        }
        yield return null;
    }

    //�Ƴ�buffʱִ�е�Ч��
    public void RemoveBuffEffectSwitch(BuffObject buffObj)
    {
        BuffInfo buffInfo = buffObj.buffInfo;
        BattleEntity entityGet = buffInfo.entityGet;
        BattleEntity entitySet = buffInfo.entitySet;

        for (int i = 0; i < buffObj.GetLayers(); i++)
        {
            switch (buffInfo.buffName)
            {
                case BuffName.��:
                    if (entityGet.isEnemy)//battleEntity�ָ�������
                    {
                        entityGet.defend += (int)(entityGet.enemyDataEntry.defend  *0.2f);
                    }
                    else
                    {
                        entityGet.defend += (int)(characterData.data[entityGet.characterId].defend * 0.2f);
                    }
                    break;
                case BuffName.����:
                    break;
                case BuffName.����������:
                    entityGet.damage -= int.Parse(buffInfo.buffValues[0].ToString());
                    break;
                case BuffName.�������½�:
                    entityGet.damage += int.Parse(buffInfo.buffValues[0].ToString());
                    break;
                case BuffName.���˼���:
                    entityGet.hurtRate -= float.Parse(buffInfo.buffValues[0].ToString());
                    break;
                case BuffName.����:
                    break;
                case BuffName.����:
                    break;
                case BuffName.����:
                    entityGet.hurtRate += float.Parse(buffInfo.buffValues[0].ToString());
                    break;
                case BuffName.��������:
                    break;
                case BuffName.���ڻ�:
                    entityGet.hurtToEntity = null;
                    break;
                case BuffName.����:
                    break;
                case BuffName.����:
                    entityGet.SetHp(1);
                    break;
                case BuffName.ѣ��:
                    break;
                case BuffName.ȼ��:
                    break;
                default:
                    Debug.LogError("û��������buff��Ч��" + buffInfo.buffName);
                    break;
            }
        }

    }

    public void CheckBpSubstract(BattleEntity characterEntity, int bpNeed)//ͨ��buffManager����bp
    {
        for (int j = 0; j < bpNeed; j++)
        {
            if (CheckBuff(characterEntity.entityInfoUi, BuffName.����).Count > 0)
            {
                int damageAdd = (int)(characterData.data[characterData.teamIndexList[characterEntity.characterId]].damage * 0.2f);

                UnityAction callback = new UnityAction(() =>
                {
                    StartCoroutine(AddBuff(BuffName.����������, characterEntity, characterEntity, 1, damageAdd));
                });
                characterEntity.UpdateBp(-1, callback);
            }
            else
            {
                characterEntity.UpdateBp(-1, null);
            }
        }
    }

    //ÿ�غϽ���ʱִ��ĳЩbuffEffect
    //��battleSystem�е�DoRoundEndBuffEffectʵ�֣���Ϊ���Ƿ������˷���ָ����Щ�¼�����ȥ����

    //�������buff
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

    //ÿ�غ�buff����ʱ���һ
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

        for (int j = 0; j < cleanBuffObjs.Count; j++)//ִ�����buffʱ��Ч��
        {
            RemoveBuffEffectSwitch(cleanBuffObjs[j]);
        }

        return cleanBuffObjs;
    }

    public void ForceRemoveBuffNoCallback(CharacterInfoBtn infoBtn, BuffName buffName)//ֱ���Ƴ�����һ�������ȵ�buff
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
            Debug.LogError("û��Ҫ�����buff" + buffName);
        }
    }

    public List<BuffInfo> CheckBuff(CharacterInfoBtn infoBtn, BuffName buffNameT)//����buffName����ȡbuff
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

    public List<BuffInfo> CheckBuff(CharacterInfoBtn infoBtn, bool isDebuff)//��ȡ���е�buff����debuff
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

    public List<BuffObject> CheckBuffObj(CharacterInfoBtn infoBtn, BuffName buffNameT)//��ȡBuffObject
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

    public List<BuffInfo> DisperseDebuff(CharacterInfoBtn characterInfoBtn)//��ɢdebuff
    {
        List<BuffInfo> debuffInfos = new List<BuffInfo>();

        for (int i = 0; i < characterInfoBtn.debuffs.Count; i++)
        {
            if (characterInfoBtn.debuffs[i].buffInfo.data.�Ƿ����ɢ)
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