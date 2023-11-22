using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToolForPlotState : MonoBehaviour
{
    public void PlotAFinish()
    {
        GameObject.Find("SystemFacade").GetComponent<SystemFacade>().PlotAFinish();
    }
    public void PlotBFinish()
    {
        GameObject.Find("SystemFacade").GetComponent<SystemFacade>().PlotBFinish();
    }
}
