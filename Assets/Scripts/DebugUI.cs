using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DebugUI : MonoBehaviour
{
    public Text debugText;
    public GameObject testObj;
     
    public void CheckGameObjActiveSelf()
    {
        debugText.text += "\n"+ testObj.activeInHierarchy.ToString();
    }
}
