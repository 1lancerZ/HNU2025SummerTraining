using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModelController : MonoBehaviour
{
    // 将21个小球拖拽到这个数组中
    public GameObject[] handPoints = new GameObject[21];
    public GameObject[] posePoints = new GameObject[33]; // 用于显示身体骨骼关键点

    //public LineRenderer connectionLines;

    //private readonly (int start, int end)[] _connections = {
    //// 拇指
    //(0, 1), (1, 2), (2, 3), (3, 4), (4, 3), (3, 2), (2, 1), (1, 0),
    //// 食指
    //(0, 5), (5, 6), (6, 7), (7, 8), (8, 7), (7, 6), (6, 5), (5, 0),
    //// 中指
    //(0, 9), (9, 10), (10, 11), (11, 12), (12, 11), (11, 10), (10, 9), (9, 0),
    //// 无名指
    //(0, 13), (13, 14), (14, 15), (15, 16), (16, 15), (15, 14), (14, 13), (13, 0),
    //// 小指
    //(0, 17), (17, 18), (18, 19), (19, 20),(20, 19), (19, 18), (18, 17), (17, 0),
    //// 手掌基部连线
    //(5, 9), (9, 13), (13, 17)
    //};

    // 更新关键点位置
    public void UpdatePoints(Vector3[] landmarks)
    {
        if (landmarks == null || landmarks.Length != 21) return;

        for (int i = 0; i < 21; i++)
        {
            if (handPoints[i] != null)
            {
                // 调整坐标比例和位置
                handPoints[i].transform.localPosition = landmarks[i] * 25f; // 放大25倍
            }
        }
        //DrawConnections();
    }

    public void UpdateHandPoints(Vector3[] landmarks)
    {
        if (landmarks == null || landmarks.Length != 33) return;

        for(int i =0; i < 33; i++)
        {
            if (posePoints[i] != null)
            {
                // 调整坐标比例和位置
                posePoints[i].transform.localPosition = landmarks[i] * 25f; // 放大25倍
            }
        }
    }

    //private void DrawConnections()
    //{
    //    if (connectionLines == null || handPoints == null) return;

    //    List<Vector3> linePoints = new List<Vector3>();

    //    foreach (var (start, end) in _connections)
    //    {
    //        if (handPoints[start] && handPoints[end])
    //        {
    //            linePoints.Add(handPoints[start].transform.localPosition);
    //            linePoints.Add(handPoints[end].transform.localPosition);
    //        }
    //    }

    //    connectionLines.positionCount = linePoints.Count;
    //    connectionLines.SetPositions(linePoints.ToArray());
    //}
}
