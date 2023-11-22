using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class BattleMenuButton : Button
{
    public void SetDisable()
    {
        image.color = new Color(1f, 1f, 1f, 0.3f);
        onClick.RemoveAllListeners();
        onClick.AddListener(() => ClickDisableButton());
    }
    public void SetEnable(UnityAction call)
    {
        image.color = new Color(1f, 1f, 1f, 1f);
        onClick.RemoveAllListeners();
        onClick.AddListener(call);
    }

    private static void ClickDisableButton()
    {
        SystemFacade.instance.PlaySe("error≤ªø…”√");
    }
}
