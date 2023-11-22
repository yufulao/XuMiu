using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIButton : MonoBehaviour
{
    private Animator animator;

    private void Awake()
    {
        try
        {
            animator = GetComponent<Animator>();
        }
        catch (System.Exception)
        {

            throw;
        }
    }

    public void RegisterUiButton(Animator animatorT)
    {
        animator = animatorT;
    }

    public virtual void MousePointerEnter()
    {
        if (animator != null)
        {
            animator.SetBool("selected", true);
        }
    }

    public virtual void MousePointerExit()
    {
        if (animator != null)
        {
            animator.SetBool("selected", false);
        }

    }
}