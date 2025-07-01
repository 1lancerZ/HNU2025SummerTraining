using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static HandBinder;

namespace landmarktest
{
    public class HandRigController : MonoBehaviour
    {
        public List<GameObject> handPoints;
        public Camera targetCamera;  // 你手动指定的摄像机

        [Header("Setting")]
        [SerializeField] private float thumbModelLength = 0.03f;
        [SerializeField] private float distanceToCam = 1.4f;


        private float scale;
        private DepthCalibrator depthCalibrator = new DepthCalibrator(-0.0719f, 0.439f);
        private TransformLink[] transformLinkers;
        public bool isLeft;

        void Awake()
        {
            transformLinkers = this.transform.GetComponentsInChildren<TransformLink>();
        }

        /// <summary>
        /// 根据归一化 landmark 数据更新 handPoints 中的 GameObject 坐标（带深度估算）
        /// </summary>
        /// <param name="landmarks">21个 Vector3 归一化坐标</param>
        /// <param name="isLeft">是否左手</param>
        /// <param name="depthCM">估算深度（单位 cm），可为 null</param>
        public void UpdateHandPointsFromLandmarks(Vector3[] landmarks, bool isLeft, float depthCM)
        {
            if (landmarks == null || landmarks.Length != 21 || handPoints.Count < 21)
            {
                Debug.LogWarning("无效的 landmark 或 handPoints 数据");
                return;
            }

            if (targetCamera == null)
            {
                Debug.LogError("未指定目标摄像机 targetCamera");
                return;
            }

            if (isLeft != this.isLeft) return;

            var thumbDetectedLength = Vector3.Distance(landmarks[0], landmarks[1]);
            if (thumbDetectedLength <= 0) return;
            scale = thumbModelLength / thumbDetectedLength;

            Vector3 wrist = new Vector3(landmarks[0].x * Screen.width, landmarks[0].y * Screen.height, distanceToCam - depthCM/100f);
            handPoints[0].transform.position = targetCamera.ScreenToWorldPoint(wrist); // 手腕位置


            for (int i = 1; i < 21; i++)
            {
                Vector3 localDistance = new Vector3(landmarks[i].x - landmarks[0].x, landmarks[i].y - landmarks[0].y, (landmarks[i].z - landmarks[0].z) * 1.2f) * scale;
                handPoints[i].transform.position = localDistance + handPoints[0].transform.position;
            }

            updateWristRotation();
            foreach (var linker in transformLinkers)
            {
                linker.UpdateTransform();
            }
        }

        private void updateWristRotation()
        {
            var wristTransform = handPoints[0].transform;
            var indexFinger = handPoints[5].transform.position;
            var middleFinger = handPoints[9].transform.position;

            var vectorToMiddle = middleFinger - wristTransform.position;
            var vectorToIndex = indexFinger - wristTransform.position;

            Vector3.OrthoNormalize(ref vectorToMiddle, ref vectorToIndex);
            Vector3 normalVector = Vector3.Cross(vectorToIndex, vectorToMiddle);

            wristTransform.rotation = Quaternion.LookRotation(normalVector, vectorToIndex);
        }
    }

    // 拇指长度估算距离的模型类
    public class DepthCalibrator
    {
        private float m;
        private float c;

        public DepthCalibrator(float m, float c)
        {
            this.m = m;
            this.c = c;
        }

        public float GetDepthFromThumbLength(float length)
        {
            if (length == 0) return 0;
            return m / length + c;
        }
    }
}
