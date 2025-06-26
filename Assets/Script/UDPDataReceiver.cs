using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Collections.Generic;
using Newtonsoft.Json;

public class UDPDataReceiver : MonoBehaviour
{
    [Header("Socket Settings")]
    public string ipAddress = "127.0.0.1";
    public int port = 5052;

    private UdpClient _udpClient;
    private Thread _receiveThread;
    private bool _isRunning;

    // 手部数据
    [Header("Hand Data")]
    public int handCount = 0;
    public Vector3[] leftHandLandmarks = new Vector3[21];
    public Vector3[] rightHandLandmarks = new Vector3[21];
    public Vector3[] leftHandLocalLandmarks = new Vector3[21];
    public Vector3[] rightHandLocalLandmarks = new Vector3[21];

    // 新增：身体骨骼关键点数据（33个点是MediaPipe Pose默认）
    [Header("Pose Data")]
    public Vector3[] poseLandmarks = new Vector3[33];
    public float[] poseLandmarksVisibility = new float[33]; // 可见度

    // 缩放因子
    [Header("Hand Setting")]
    public float scale = 2.0f;

    // 线程锁
    private readonly object dataLock = new object();

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
        // 线程安全复制一份数据供其他脚本使用，示例：
        Vector3[] leftLocalCopy = new Vector3[21];
        Vector3[] rightLocalCopy = new Vector3[21];
        Vector3[] poseCopy = new Vector3[33];
        float[] poseVisCopy = new float[33];

        lock (dataLock)
        {
            Array.Copy(leftHandLocalLandmarks, leftLocalCopy, 21);
            Array.Copy(rightHandLocalLandmarks, rightLocalCopy, 21);
            Array.Copy(poseLandmarks, poseCopy, 33);
            Array.Copy(poseLandmarksVisibility, poseVisCopy, 33);
        }
    }

    void ProcessHandData(string jsonString)
    {
        try
        {
            var handData = JsonConvert.DeserializeObject<UDPData>(jsonString);
            lock (dataLock)
            {
                handCount = handData.handCount;

                // 处理手部数据
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
                                    1 - hand.landmarks[i].y,
                                    (1 - hand.landmarks[i].z) * scale
                                );
                                leftHandLocalLandmarks[i] = leftHandLandmarks[i] - leftHandLandmarks[0];
                            }
                            leftHandLocalLandmarks[0] = leftHandLandmarks[0];
                        }
                        else if (hand.type == "Right")
                        {
                            for (int i = 0; i < Mathf.Min(21, hand.landmarks.Count); i++)
                            {
                                rightHandLandmarks[i] = new Vector3(
                                    hand.landmarks[i].x,
                                    1 - hand.landmarks[i].y,
                                    (1 - hand.landmarks[i].z) * scale
                                );
                                rightHandLocalLandmarks[i] = rightHandLandmarks[i] - rightHandLandmarks[0];
                            }
                            rightHandLocalLandmarks[0] = rightHandLandmarks[0];
                        }
                    }
                }

                // 新增：处理姿态数据
                if (handData.poseLandmarks != null && handData.poseLandmarks.Count >= 33)
                {
                    for (int i = 0; i < 33; i++)
                    {
                        poseLandmarks[i] = new Vector3(
                            handData.poseLandmarks[i].x,
                            1 - handData.poseLandmarks[i].y,
                            (1 - handData.poseLandmarks[i].z) * scale
                        );
                        poseLandmarksVisibility[i] = handData.poseLandmarks[i].visibility;
                    }
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"处理数据错误: {e.Message}\n{e.StackTrace}");
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
}
