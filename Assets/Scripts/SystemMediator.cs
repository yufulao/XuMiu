using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PixelCrushers.DialogueSystem;
using UnityEngine.EventSystems;

public class SystemMediator : MonoBehaviour
{
    //DialogueSystem�޷�����Ѿ�������һ��dialogueManager������
    //public static DialogueManager dialogueManager;

    public StageFlowSystem stageFlowSystem;
    public BattleSystem battleSystem;
    public BgmManager bgmManager;
    public SfxManager seManager;
    public SfxManager voiceManager;
    public FadeCanvasManager fadeCanvasManager;
    public CameraManager cameraManager;
}
