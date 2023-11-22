using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;

public class UiPenetrate : MonoBehaviour, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler
{
    private bool hasPassedEvent;

    public void OnPointerDown(PointerEventData eventData)
    {
        PassEvent(eventData, ExecuteEvents.pointerDownHandler);
    }
    public void OnPointerUp(PointerEventData eventData)
    {
        PassEvent(eventData, ExecuteEvents.pointerUpHandler);
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        PassEvent(eventData, ExecuteEvents.submitHandler);
        PassEvent(eventData, ExecuteEvents.pointerClickHandler);
    }

    public void PassEvent<T>(PointerEventData data, ExecuteEvents.EventFunction<T> function) where T : IEventSystemHandler
    {
        if (hasPassedEvent)
        {
            return;
        }
        hasPassedEvent = true;

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(data, results);
        GameObject current = gameObject;
        for (int i = 0; i < results.Count; i++)
        {
            if (current != results[i].gameObject)
            {
                if (ExecuteEvents.Execute(results[i].gameObject, data, function))
                {
                    break;
                }
                //RaycastAll��ugui���Լ����������ֻ����Ӧ͸��ȥ�������һ����Ӧ������ExecuteEvents.Execute��ֱ��break���С�
            }
        }
        results.Clear();

        hasPassedEvent = false;
    }
}
