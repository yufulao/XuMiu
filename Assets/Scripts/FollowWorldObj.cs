using UnityEngine;
using System.Collections;

public class FollowWorldObj : MonoBehaviour
{
    public Transform objFollowed;//3D物体（人物）
    public RectTransform rectFollower;//UI元素（如：血条等）
    public Vector2 offset;//偏移量

    void Update()
    {
        Vector2 screenPos = Camera.main.WorldToScreenPoint(objFollowed.transform.position);
        rectFollower.position = screenPos + offset;
    }
}
