using UnityEngine;
using System.Collections;

public class FollowWorldObj : MonoBehaviour
{
    public Transform objFollowed;//3D���壨���
    public RectTransform rectFollower;//UIԪ�أ��磺Ѫ���ȣ�
    public Vector2 offset;//ƫ����

    void Update()
    {
        Vector2 screenPos = Camera.main.WorldToScreenPoint(objFollowed.transform.position);
        rectFollower.position = screenPos + offset;
    }
}
