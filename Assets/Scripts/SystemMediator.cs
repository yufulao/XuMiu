using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PixelCrushers.DialogueSystem;
using UnityEngine.EventSystems;

public class SystemMediator : MonoBehaviour
{
    //DialogueSystem无法解耦，已经生成了一个dialogueManager单例了
    //public static DialogueManager dialogueManager;

    public StageFlowSystem stageFlowSystem;
    public BattleSystem battleSystem;
    public BgmManager bgmManager;
    public SfxManager seManager;
    public SfxManager voiceManager;
    public FadeCanvasManager fadeCanvasManager;
    public CameraManager cameraManager;
}
