using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


public class OnceClickButton : Button
{
    protected override void Awake()
    {
        onClick.AddListener(ButtonOnClick);
    }

    private void ButtonOnClick()
    {
        transform.SetAsLastSibling();
        interactable = false;
    }

    public void EnableInteractable()
    {
        interactable = true;
    }
}
