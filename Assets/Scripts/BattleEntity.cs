using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;
using Spine.Unity;

public class BattleEntity : MonoBehaviour
{
    public bool isDie;
    private int m_hp;
    public int mp;
    private int m_bp;
    public int bpPreview;
    public int braveCount;
    public bool hadUniqueSkill;
    public int hatred;

    public int damage;
    public int defend;
    public int maxHp;
    public float damageRate;
    public float hurtRate;
    public int speed;

    public bool isEnemy;
    public int characterId;
    public Slider hpSlider;
    public Slider mpSlider;
    public Toggle selectToggle;
    public Animator animator;
    public Animator skillVfxAnim;
    public Animator buffVfxAnim;
    public CharacterInfoBtn entityInfoUi;
    public GameObject entityObj;
    public MeshRenderer meshRenderer;

    public EnemyData.EnemyDataEntry enemyDataEntry;
    public TextMeshProUGUI hurtPointText;
    public SkeletonRendererCustomMaterials outlineComponent;

    //buff
    public BattleEntity hurtToEntity;//被掩护，转移伤害给的目标

    public List<IEnumerator> entityCommandList = new List<IEnumerator>();
    public List<IEnumerator> entitybattleStartCommandList = new List<IEnumerator>();

    public void EntityInit()
    {
        outlineComponent = entityObj.GetComponent<SkeletonRendererCustomMaterials>();

        if (isEnemy)
        {
            selectToggle.transform.GetComponent<UIButton>().RegisterUiButton(entityInfoUi.transform.Find("SelectSpineBg").GetComponent<Animator>());
        }
    }

    public int GetBp() { return m_bp; }
    public void SetBp(int bp, UnityAction callback = null)
    {
        m_bp = bp;
        callback?.Invoke();
    }
    public void UpdateBp(int bpAddValue, UnityAction callback = null)
    {
        m_bp += bpAddValue;
        callback?.Invoke();
    }

    public int GetHp() { return m_hp; }
    public void SetHp(int hp, UnityAction callback = null)
    {
        m_hp = hp;
        callback?.Invoke();
    }
    public int UpdateHp(int hpAddValue, UnityAction callback = null)
    {
        m_hp += hpAddValue;
        callback?.Invoke();
        if (m_hp > maxHp)
        {
            int surplusHp = m_hp - maxHp;
            m_hp = maxHp;
            return surplusHp;
        }
        return 0;
    }
}
