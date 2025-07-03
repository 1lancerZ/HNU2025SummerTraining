using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 数据结构定义，新增 poseLandmarks
[Serializable]
public class UDPData
{
    public int handCount;
    public List<SingleHandData> hands;
    public List<PoseLandmark> poseLandmarks; // 新增
}

[Serializable]
public class SingleHandData
{
    public string type;
    public List<Landmark> landmarks;
}

[Serializable]
public class Landmark
{
    public float x;     // 归一化或屏幕坐标
    public float y;
    public float z;     // 相对深度
    public int id;      // 节点索引
    public float depthCM; // 可选：如果你在 Python 端估算了 depthCM
}

[Serializable]
public class PoseLandmark
{
    public float x;
    public float y;
    public float z;
    public float visibility;
    public int id;
}
