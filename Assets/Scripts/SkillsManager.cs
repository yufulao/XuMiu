using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class SkillsManager : MonoBehaviour
{
    public BattleSystem battleSystem;
    public CharacterData characterData;
    public EnemyData enemyData;

    private void FilterDieSelectEntity(ref List<BattleEntity> selectEntity)
    {
        for (int i = 0; i < selectEntity.Count; i++)
        {
            if (selectEntity[i].isDie)
            {
                selectEntity.Remove(selectEntity[i]);
            }
        }
    }

    //Vectoria
    public IEnumerator VectoriaSkill1(BattleEntity entitySet, int bpNeed, List<BattleEntity> selectEntity)
    {
        SystemFacade.instance.SwitchObjCamera(CameraManager.ObjCameraState.Character, 0f);
        FilterDieSelectEntity(ref selectEntity);

        if (selectEntity.Count != 0)
        {
            float damagePoint = entitySet.damage * 0.5f;

            for (int i = 0; i < selectEntity.Count; i++)
            {
                yield return StartCoroutine(CameraManager.SwitchCamera(selectEntity[i]));
                yield return StartCoroutine(battleSystem.buffManager.AddBuff(BuffName.星, selectEntity[i], entitySet, 2));
                battleSystem.EntityGetDamage(selectEntity[i], entitySet, damagePoint);
            }

            battleSystem.CheckBpSubstract(entitySet, bpNeed);
            battleSystem.UpdateAllEntityUiInfo();
        }

        battleSystem.ExcuteCommandList();
        yield return null;
    }
    public IEnumerator VectoriaSkill2(BattleEntity entitySet, int bpNeed, List<BattleEntity> selectEntity)
    {
        SystemFacade.instance.SwitchObjCamera(CameraManager.ObjCameraState.Character, 0f);
        FilterDieSelectEntity(ref selectEntity);

        if (selectEntity.Count != 0)
        {
            for (int i = 0; i < selectEntity.Count; i++)
            {
                yield return StartCoroutine(CameraManager.SwitchCamera(selectEntity[i]));
                yield return StartCoroutine(battleSystem.buffManager.AddBuff(BuffName.激情, selectEntity[i], entitySet, 3));
            }

            battleSystem.CheckBpSubstract(entitySet, bpNeed);
            battleSystem.UpdateAllEntityUiInfo();
        }

        battleSystem.ExcuteCommandList();
        yield return null;
    }
    public IEnumerator VectoriaSkill3(BattleEntity entitySet, int bpNeed, List<BattleEntity> selectEntity)
    {
        SystemFacade.instance.SwitchObjCamera(CameraManager.ObjCameraState.Character, 0f);
        FilterDieSelectEntity(ref selectEntity);

        if (selectEntity.Count != 0)
        {
            float damagePointNormal = entitySet.damage * 0.7f;
            float damagePointWithBuff = entitySet.damage * 2.5f;
            float damagePointFinal = -1f;

            for (int i = 0; i < selectEntity.Count; i++)
            {
                if (selectEntity[i].isEnemy)
                {
                    if (battleSystem.buffManager.CheckBuff(selectEntity[i].entityInfoUi, BuffName.星).Count > 0)
                    {
                        damagePointFinal = damagePointWithBuff;
                    }
                    else
                    {
                        damagePointFinal = damagePointNormal;
                    }

                    yield return StartCoroutine(CameraManager.SwitchCamera(selectEntity[i]));

                    if (selectEntity[i].enemyDataEntry.enemyType == EnemyType.大型敌人)
                    {
                        battleSystem.EntityGetDamage(selectEntity[i], entitySet, damagePointFinal, 0.4f);
                    }
                    else
                    {
                        battleSystem.EntityGetDamage(selectEntity[i], entitySet, damagePointFinal);
                    }
                }
            }

            battleSystem.CheckBpSubstract(entitySet, bpNeed);
            battleSystem.UpdateAllEntityUiInfo();
        }

        battleSystem.ExcuteCommandList();
        yield return null;
    }
    public IEnumerator VectoriaSkill4(BattleEntity entitySet, int bpNeed, List<BattleEntity> selectEntity)
    {
        SystemFacade.instance.SwitchObjCamera(CameraManager.ObjCameraState.Character, 0f);
        FilterDieSelectEntity(ref selectEntity);

        if (selectEntity.Count != 0)
        {
            for (int i = 0; i < selectEntity.Count; i++)
            {
                if (!selectEntity[i].isDie && selectEntity[i].isEnemy)
                {
                    yield return StartCoroutine(CameraManager.SwitchCamera(selectEntity[i]));
                    yield return StartCoroutine(battleSystem.buffManager.AddBuff(BuffName.攻击力下降, selectEntity[i], entitySet, 3, selectEntity[i].enemyDataEntry.damage * 0.2f    ));
                    yield return StartCoroutine(battleSystem.buffManager.AddBuff(BuffName.受伤加重, selectEntity[i], entitySet, 3, 0.2f));
                }
            }

            battleSystem.CheckBpSubstract(entitySet, bpNeed);
            battleSystem.UpdateAllEntityUiInfo();
        }

        battleSystem.ExcuteCommandList();
        yield return null;
    }
    public IEnumerator VectoriaSkill5(BattleEntity entitySet, int bpNeed, List<BattleEntity> selectEntity)
    {
        SystemFacade.instance.SwitchObjCamera(CameraManager.ObjCameraState.Character, 0f);
        FilterDieSelectEntity(ref selectEntity);

        if (selectEntity.Count != 0)
        {
            for (int i = 0; i < selectEntity.Count; i++)
            {
                yield return StartCoroutine(CameraManager.SwitchCamera(selectEntity[i]));

                List<BuffInfo> disperseDebuffs = battleSystem.buffManager.DisperseDebuff(selectEntity[i].entityInfoUi);
                for (int j = 0; j < disperseDebuffs.Count; j++)
                {
                    yield return StartCoroutine(battleSystem.buffManager.AddBuff(BuffName.虚缪, selectEntity[i], entitySet, 5, 0f));
                }
            }

            battleSystem.CheckBpSubstract(entitySet, bpNeed);
            battleSystem.UpdateAllEntityUiInfo();
        }

        battleSystem.ExcuteCommandList();
        yield return null;
    }
    public IEnumerator VectoriaSkill6(BattleEntity entitySet, int bpNeed)
    {
        SystemFacade.instance.SwitchObjCamera(CameraManager.ObjCameraState.Character, 0f);

        yield return StartCoroutine(battleSystem.buffManager.AddBuff(BuffName.苦旅, entitySet, entitySet, 5));
        yield return StartCoroutine(battleSystem.buffManager.AddBuff(BuffName.忍耐, entitySet, entitySet, 5, 0.1f));

        battleSystem.CheckBpSubstract(entitySet, bpNeed);
        battleSystem.ExcuteCommandList();
        yield return null;
    }
    public IEnumerator VectoriaUniqueSkill(BattleEntity entitySet, int bpNeed, List<BattleEntity> selectEntity)
    {
        SystemFacade.instance.SwitchObjCamera(CameraManager.ObjCameraState.Character, 0f);
        FilterDieSelectEntity(ref selectEntity);

        if (selectEntity.Count != 0)
        {
            float damagePoint = entitySet.damage * 1.9f;
            float damageOtherPoint = entitySet.damage * 0.7f;
            float damageHalve = 1;//有星buff的伤害减半

            for (int i = 0; i < selectEntity.Count; i++)
            {
                //allEntities[battleId].animator.Play("skill1", 0, 0f);
                //allEntities[battleId].skillObjAnim.Play("dog_skill_3", 0, 0f);
                //SystemFacade.instance.PlaySFX("攻击123");
                //yield return new WaitForSeconds(Utility.GetAnimatorLength(allEntities[battleId].animator, "skill1"));

                int entityGetBuffCount = battleSystem.buffManager.CheckBuff(selectEntity[i].entityInfoUi, false).Count;
                if (entityGetBuffCount != 0)
                {
                    damageHalve = 0.5f;
                }
                else
                {
                    damageHalve = 1f;
                }

                yield return StartCoroutine(CameraManager.SwitchCamera(selectEntity[i]));
                battleSystem.EntityGetDamage(selectEntity[i], entitySet, damagePoint * damageHalve);

                int entityGetDebuffCount = battleSystem.buffManager.CheckBuff(selectEntity[i].entityInfoUi, true).Count;
                for (int j = 0; j < entityGetDebuffCount; j++)
                {
                    battleSystem.EntityGetDamage(selectEntity[i], entitySet, damageOtherPoint * damageHalve);
                }

                yield return StartCoroutine(battleSystem.buffManager.AddBuff(BuffName.星, selectEntity[i], entitySet, 2));

                battleSystem.CheckBpSubstract(entitySet, bpNeed);
                entitySet.mp -= 100;
                battleSystem.UpdateAllEntityUiInfo();
            }
        }

        battleSystem.ExcuteCommandList();
        yield return null;
    }

    //Vector
    public IEnumerator VectorSkill1(BattleEntity entitySet, int bpNeed)//先制指令
    {
        SystemFacade.instance.PlayVoice("攻击123");
        yield return StartCoroutine(Utility.PlayAnimation(entitySet.animator, "skill2"));
        yield return StartCoroutine(Utility.PlayAnimation(entitySet.buffVfxAnim, "def_buff"));

        battleSystem.CheckBpSubstract(entitySet, bpNeed);
        entitySet.UpdateBp(1);
        yield return StartCoroutine(battleSystem.buffManager.AddBuff(BuffName.反击架势, entitySet, entitySet, 1));
        yield return new WaitForSeconds(0.2f);

        battleSystem.ExcuteBattleStartCommandList();
    }
    public IEnumerator VectorSkill2(BattleEntity entitySet, int bpNeed, List<BattleEntity> selectEntity)
    {
        SystemFacade.instance.SwitchObjCamera(CameraManager.ObjCameraState.Character, 0f);
        FilterDieSelectEntity(ref selectEntity);

        if (selectEntity.Count != 0)
        {
            for (int i = 0; i < selectEntity.Count; i++)
            {
                SystemFacade.instance.PlayVoice("技能12");
                yield return StartCoroutine(Utility.PlayAnimation(entitySet.animator, "skill2"));
                yield return StartCoroutine(CameraManager.SwitchCamera(selectEntity[i]));
                yield return StartCoroutine(Utility.PlayAnimation(selectEntity[i].buffVfxAnim, "def_buff"));
                yield return StartCoroutine(battleSystem.buffManager.AddBuff(BuffName.被掩护, selectEntity[i], entitySet, 1, entitySet));
                yield return new WaitForSeconds(0.2f);
            }

            battleSystem.CheckBpSubstract(entitySet, bpNeed);
        }

        battleSystem.ExcuteCommandList();
    }
    public IEnumerator VectorSkill3(BattleEntity entitySet, int bpNeed, List<BattleEntity> selectEntity)
    {
        SystemFacade.instance.SwitchObjCamera(CameraManager.ObjCameraState.Character, 0f);
        FilterDieSelectEntity(ref selectEntity);

        if (selectEntity.Count != 0)
        {
            float damagePoint = entitySet.damage * 1.5f;

            for (int i = 0; i < selectEntity.Count; i++)
            {
                SystemFacade.instance.PlayVoice("攻击123");
                yield return StartCoroutine(Utility.PlayAnimation(entitySet.animator, "skill1"));

                yield return StartCoroutine(CameraManager.SwitchCamera(selectEntity[i]));
                yield return StartCoroutine(Utility.PlayAnimation(selectEntity[i].skillVfxAnim, "dog_skill3"));

                battleSystem.EntityGetDamage(selectEntity[i], entitySet, damagePoint); ;
            }

            entitySet.hatred++;
            battleSystem.DescreaseEntityHp(entitySet, entitySet, (int)(entitySet.maxHp * 0.1f));

            battleSystem.CheckBpSubstract(entitySet, bpNeed);
        }

        battleSystem.ExcuteCommandList();
    }
    public IEnumerator VectorSkill4(BattleEntity entitySet, int bpNeed)
    {
        SystemFacade.instance.SwitchObjCamera(CameraManager.ObjCameraState.Character, 0f);

        SystemFacade.instance.PlayVoice("技能12");
        yield return StartCoroutine(Utility.PlayAnimation(entitySet.animator, "skill2"));

        yield return StartCoroutine(Utility.PlayAnimation(entitySet.skillVfxAnim, "dog_skill4&6"));
        yield return StartCoroutine(battleSystem.buffManager.AddBuff(BuffName.增生, entitySet, entitySet, 3));
        yield return new WaitForSeconds(0.2f);
        yield return StartCoroutine(Utility.PlayAnimation(entitySet.buffVfxAnim, "heal"));
        yield return StartCoroutine(battleSystem.buffManager.AddBuff(BuffName.返生, entitySet, entitySet, 5));
        yield return new WaitForSeconds(0.2f);

        battleSystem.CheckBpSubstract(entitySet, bpNeed);
        battleSystem.ExcuteCommandList();
    }
    public IEnumerator VectorSkill5(BattleEntity entitySet, int bpNeed, List<BattleEntity> selectEntity)
    {
        SystemFacade.instance.SwitchObjCamera(CameraManager.ObjCameraState.Character, 0f);
        FilterDieSelectEntity(ref selectEntity);

        if (selectEntity.Count != 0)
        {
            float hpRate = entitySet.GetHp() / entitySet.maxHp;
            float damagePoint = entitySet.damage * (-2.5f * hpRate + 3f);
            //Debug.Log(hpRate);
            //Debug.Log(damagePoint);

            for (int i = 0; i < selectEntity.Count; i++)
            {
                SystemFacade.instance.PlayVoice("攻击123");
                yield return StartCoroutine(Utility.PlayAnimation(entitySet.animator, "skill1"));

                yield return StartCoroutine(CameraManager.SwitchCamera(selectEntity[i]));
                yield return StartCoroutine(Utility.PlayAnimation(selectEntity[i].skillVfxAnim, "dog_skill5"));
                yield return new WaitForSeconds(0.2f);

                battleSystem.EntityGetDamage(selectEntity[i], entitySet, damagePoint);
            }

            battleSystem.DescreaseEntityHp(entitySet, entitySet, (int)(entitySet.maxHp * 0.1f));
            battleSystem.CheckBpSubstract(entitySet, bpNeed);
        }

        battleSystem.ExcuteCommandList();
    }
    public IEnumerator VectorSkill6(BattleEntity entitySet, int bpNeed, List<BattleEntity> selectEntity)
    {
        SystemFacade.instance.SwitchObjCamera(CameraManager.ObjCameraState.Character, 0f);
        FilterDieSelectEntity(ref selectEntity);

        SystemFacade.instance.PlayVoice("技能12");
        yield return StartCoroutine(Utility.PlayAnimation(entitySet.animator, "skill2"));

        for (int i = 0; i < selectEntity.Count; i++)
        {
            StartCoroutine(Utility.PlayAnimation(selectEntity[i].buffVfxAnim, "heal"));
            battleSystem.RecoverEntityHp(selectEntity[i], (int)(entitySet.maxHp * 0.7f));
        }
        yield return new WaitForSeconds(Utility.GetAnimatorLength(entitySet.buffVfxAnim, "heal"));
        yield return new WaitForSeconds(0.2f);

        for (int i = 0; i < selectEntity.Count; i++)
        {
            StartCoroutine(Utility.PlayAnimation(selectEntity[i].skillVfxAnim, "dog_skill4&6"));
            StartCoroutine(battleSystem.buffManager.AddBuff(BuffName.增生, selectEntity[i], entitySet, 3));
        }
        yield return new WaitForSeconds(Utility.GetAnimatorLength(entitySet.skillVfxAnim, "dog_skill4&6"));
        yield return new WaitForSeconds(0.2f);

        battleSystem.CheckBpSubstract(entitySet, bpNeed);
        battleSystem.ExcuteCommandList();
    }
    public IEnumerator VectorUniqueSkill(BattleEntity entitySet, int bpNeed, List<BattleEntity> selectEntity)
    {
        SystemFacade.instance.SwitchObjCamera(CameraManager.ObjCameraState.Character, 0f);
        FilterDieSelectEntity(ref selectEntity);

        if (selectEntity.Count != 0)
        {
            float damagePoint = entitySet.damage * 5.0f;
            //Debug.Log(damagePoint);

            for (int i = 0; i < selectEntity.Count; i++)
            {
                yield return StartCoroutine(CameraManager.SwitchCamera(selectEntity[i]));
                yield return new WaitForSeconds(0.2f);
                battleSystem.EntityGetDamage(selectEntity[i], entitySet, damagePoint);
                yield return StartCoroutine(battleSystem.buffManager.AddBuff(BuffName.燃烬, selectEntity[i], entitySet, 5));
            }

            battleSystem.CheckBpSubstract(entitySet, bpNeed);
            entitySet.mp -= 100;
        }

        battleSystem.ExcuteCommandList();
    }

    //Enemy1
    public IEnumerator EnemyASkill1(BattleEntity entitySet, BattleEntity entityGet)
    {
        SystemFacade.instance.SwitchObjCamera(CameraManager.ObjCameraState.Enemy, 0f);

        yield return StartCoroutine(Utility.PlayAnimation(entitySet.animator, "skill1"));
        yield return StartCoroutine(CameraManager.SwitchCamera(entityGet));
        battleSystem.EntityGetDamage(entityGet, entitySet, entitySet.damage);
        yield return StartCoroutine(Utility.PlayAnimation(entityGet.skillVfxAnim, "attack_A"));
        yield return StartCoroutine(battleSystem.buffManager.AddBuff(BuffName.眩晕, entityGet, entitySet, 1));
        yield return new WaitForSeconds(0.2f);

        battleSystem.UpdateAllEntityUiInfo();
        battleSystem.ExcuteCommandList();
    }
}
