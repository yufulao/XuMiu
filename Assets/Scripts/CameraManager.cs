using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class CameraManager : MonoBehaviour
{
    public enum ObjCameraState
    {
        IdleCommand,
        IdleAnimation,
        Enemy,
        Character
    }

    public Transform cameraContainer;
    private static Camera objCamera;
    private static Camera uiCamera;

    public static ObjCameraState objCameraState;
    private static Tweener objMoveTweener;
    private static Tweener objRotateTweener;

    private void Start()
    {
        objCamera = cameraContainer.Find("ObjCamera").GetComponent<Camera>();
        uiCamera = cameraContainer.Find("UiCamera").GetComponent<Camera>();
    }

    public static IEnumerator SwitchCamera(BattleEntity entityGet, float during = 0f)
    {
        if (entityGet.isEnemy)
        {
            SwitchObjCamera(CameraManager.ObjCameraState.Enemy, during);
        }
        else
        {
            SwitchObjCamera(CameraManager.ObjCameraState.Character, during);
        }
        yield return new WaitForSeconds(during);
    }

    public static void SwitchObjCamera(ObjCameraState nextState, float during=0.3f)
    {
        //idleCommand:-854f,-490f,-1f|82.5f; idleAnimation:-850.5f,-490f,-1f|82.5f; enemy:-861.3f,-490f,-1f|60f; character:-846.7f,-490f,-1f|60f
        switch (nextState)
        {
            case ObjCameraState.IdleCommand:
                objMoveTweener = objCamera.transform.DOLocalMove(new Vector3(-849f, -490f, -1f), 0.3f);
                objMoveTweener = objCamera.DOFieldOfView(82.5f, 0.3f);
                break;

            case ObjCameraState.IdleAnimation:
                    objMoveTweener = objCamera.transform.DOLocalMove(new Vector3(-850.5f, -490f, -1f), during);
                    objMoveTweener = objCamera.DOFieldOfView(82.5f, during);
                break;

            case ObjCameraState.Enemy:
                    objMoveTweener = objCamera.transform.DOLocalMove(new Vector3(-854f, -490f, -1f), during);
                    objMoveTweener = objCamera.DOFieldOfView(60f, during);
                break;

            case ObjCameraState.Character:
                    objMoveTweener = objCamera.transform.DOLocalMove(new Vector3(-846.7f, -490f, -1f), during);
                    objMoveTweener = objCamera.DOFieldOfView(60f, during);
                break;
        }

        objCameraState = nextState;
    }

    public void TestCameraAnimation(int nextStateInde)
    {
        ObjCameraState nextState = (ObjCameraState)nextStateInde;
        SwitchObjCamera(nextState) ;
    }
}
