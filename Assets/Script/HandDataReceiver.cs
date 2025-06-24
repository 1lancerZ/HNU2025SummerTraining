using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Collections.Generic;
using Newtonsoft.Json;

public class HandDataReceiver : MonoBehaviour
{
    [Header("Socket Settings")]
    public string ipAddress = "127.0.0.1";
    public int port = 5052;

    private UdpClient _udpClient;
    private Thread _receiveThread;
    private bool _isRunning;

    [Header("Hand Data")]
    public int handCount = 0;
    public Vector3[] leftHandLandmarks = new Vector3[21];
    public Vector3[] rightHandLandmarks = new Vector3[21];

    [Header("Visualization")]
    public HandPointsController leftHandController;
    public HandPointsController rightHandController;

    void Start()
    {
        InitializeSocket();
    }

    void InitializeSocket()
    {
        _isRunning = true;
        _receiveThread = new Thread(ReceiveData);
        _receiveThread.IsBackground = true;
        _receiveThread.Start();
    }

    void ReceiveData()
    {
        _udpClient = new UdpClient(port);

        while (_isRunning)
        {
            try
            {
                IPEndPoint anyIP = new IPEndPoint(IPAddress.Any, port);
                byte[] data = _udpClient.Receive(ref anyIP);
                string jsonString = System.Text.Encoding.UTF8.GetString(data);

                ProcessHandData(jsonString);
            }
            catch (Exception e)
            {
                Debug.LogError(e.ToString());
            }
        }
    }

    void Update()
    {
        // 更新手部关键点位置
        if (leftHandController != null)
        {
            leftHandController.UpdatePoints(leftHandLandmarks);
        }

        if (rightHandController != null)
        {
            rightHandController.UpdatePoints(rightHandLandmarks);
        }
    }

    void ProcessHandData(string jsonString)
    {
        try
        {
            //Debug.Log("Received: " + jsonString);

            // 解析JSON数据
            var handData = JsonConvert.DeserializeObject<HandData>(jsonString);
            handCount = handData.handCount;
            //Debug.Log(handData.hands[0].landmarks.Count);

            // 清空现有数据
            //Array.Clear(leftHandLandmarks, 0, leftHandLandmarks.Length);
            //Array.Clear(rightHandLandmarks, 0, rightHandLandmarks.Length);

            // 处理第一只手
            if (handCount > 0 && handData.hands != null && handData.hands.Count > 0)
            {
                foreach (var hand in handData.hands)
                {
                    if (hand.type == "Left")
                    {
                        for (int i = 0; i < Mathf.Min(21, hand.landmarks.Count); i++)
                        {
                            leftHandLandmarks[i] = new Vector3(
                                hand.landmarks[i].x,
                                1 - hand.landmarks[i].y, // Unity Y轴反转
                                hand.landmarks[i].z
                            );
                            //Debug.Log($"Left Hand Landmark {i}: {leftHandLandmarks[i]}");
                        }
                    }
                    else if (hand.type == "Right")
                    {
                        for (int i = 0; i < Mathf.Min(21, hand.landmarks.Count); i++)
                        {
                            rightHandLandmarks[i] = new Vector3(
                                hand.landmarks[i].x,
                                1 - hand.landmarks[i].y,
                                hand.landmarks[i].z
                            );
                        }
                    }
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"处理手部数据错误: {e.Message}\n{e.StackTrace}");
        }
    }

    void OnDisable()
    {
        _isRunning = false;
        if (_receiveThread != null && _receiveThread.IsAlive)
            _receiveThread.Abort();

        if (_udpClient != null)
            _udpClient.Close();
    }

    // 定义与Python发送的数据结构匹配的类
    [System.Serializable]
    public class HandData
    {
        public int handCount;
        public List<SingleHandData> hands;
    }

    [System.Serializable]
    public class SingleHandData
    {
        public string type; // "Left" or "Right"
        public List<Landmark> landmarks;
    }

    [System.Serializable]
    public class Landmark
    {
        public float x;
        public float y;
        public float z;
        public int id;
    }
}