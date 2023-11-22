using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CharacterInfoBtn : MonoBehaviour
{
    public GameObject selectedBG;
    public Image portrait;
    public GameObject readyText;
    public Slider hpSlider;
    public Slider mpSlider;
    public Image bpText;
    public TextMeshProUGUI healthPoint;
    public TextMeshProUGUI magicPoint;

    public List<BuffObject> buffs;
    public List<BuffObject> debuffs;

    public Transform buffsContainer;

    public Animator animator;

    public void OpenSelectedBg()
    {
        animator.SetTrigger("selectedBgOpen");
    }
    public void CloseSelectedBg()
    {
        animator.SetTrigger("selectedBgClose");
    }
    public void IdleSelectedBg()
    {
        animator.Play("idle");
    }
}
