using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static HandBinder;

namespace landmarktest
{
    // 简单的姿势数据结构
    [System.Serializable]
    public class HandPoseData
    {
        public Vector3[] positions;
        public Quaternion[] rotations;
    }

    public class HandRigController : MonoBehaviour
    {
        public List<GameObject> handPoints;
        public Camera targetCamera;  // 你手动指定的摄像机

        [Header("Setting")]
        [SerializeField] private float thumbModelLength = 0.03f;
        [SerializeField] private float distanceToCam = 1.4f;

        [Header("Pose Control")]
        public HandPoseData grabPose;  // 预设的抓握姿势
        public float poseLerpSpeed = 5f;
        [SerializeField] private bool useGrabPose = false; // 是否使用抓握姿势

        private float scale;
        private DepthCalibrator depthCalibrator = new DepthCalibrator(-0.0719f, 0.439f);
        private TransformLink[] transformLinkers;
        public bool isLeft;

        // 存储原始MediaPipe数据
        private Vector3[] mediaPipePositions;
        private Quaternion[] mediaPipeRotations;

        void Awake()
        {
            transformLinkers = this.transform.GetComponentsInChildren<TransformLink>();

            // 初始化数组
            if (handPoints != null && handPoints.Count > 0)
            {
                mediaPipePositions = new Vector3[handPoints.Count];
                mediaPipeRotations = new Quaternion[handPoints.Count];
            }
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

            if (useGrabPose)
            {
                // 使用抓握姿势时，先更新MediaPipe数据到缓存，然后应用抓握姿势
                UpdateMediaPipeToCache(landmarks, depthCM);
                ApplyGrabPose();
            }
            else
            {
                // 正常更新MediaPipe数据
                UpdateHandPointsNormal(landmarks, depthCM);
            }

            updateWristRotation();

            foreach (var linker in transformLinkers)
            {
                linker.UpdateTransform();
            }
        }

        /// <summary>
        /// 正常的MediaPipe更新逻辑
        /// </summary>
        private void UpdateHandPointsNormal(Vector3[] landmarks, float depthCM)
        {
            Vector3 wrist = new Vector3(landmarks[0].x * Screen.width, landmarks[0].y * Screen.height, distanceToCam - depthCM / 100f);
            handPoints[0].transform.position = targetCamera.ScreenToWorldPoint(wrist); // 手腕位置

            for (int i = 1; i < 21; i++)
            {
                Vector3 localDistance = new Vector3(landmarks[i].x - landmarks[0].x, landmarks[i].y - landmarks[0].y, (landmarks[i].z - landmarks[0].z) * 1.2f) * scale;
                handPoints[i].transform.position = localDistance + handPoints[0].transform.position;
            }

            // 保存当前MediaPipe数据
            for (int i = 0; i < handPoints.Count; i++)
            {
                mediaPipePositions[i] = handPoints[i].transform.position;
                mediaPipeRotations[i] = handPoints[i].transform.rotation;
            }
        }

        /// <summary>
        /// 更新MediaPipe数据到缓存（不直接应用到handPoints）
        /// </summary>
        private void UpdateMediaPipeToCache(Vector3[] landmarks, float depthCM)
        {
            Vector3 wrist = new Vector3(landmarks[0].x * Screen.width, landmarks[0].y * Screen.height, distanceToCam - depthCM / 100f);
            Vector3 wristWorldPos = targetCamera.ScreenToWorldPoint(wrist);

            // 更新缓存的MediaPipe数据
            mediaPipePositions[0] = wristWorldPos;

            for (int i = 1; i < 21; i++)
            {
                Vector3 localDistance = new Vector3(landmarks[i].x - landmarks[0].x, landmarks[i].y - landmarks[0].y, (landmarks[i].z - landmarks[0].z) * 1.2f) * scale;
                mediaPipePositions[i] = localDistance + wristWorldPos;
            }

            for (int i = 0; i < handPoints.Count; i++)
            {
                mediaPipeRotations[i] = handPoints[i].transform.rotation;
            }
        }

        /// <summary>
        /// 应用抓握姿势
        /// </summary>
        private void ApplyGrabPose()
        {
            if (grabPose == null || grabPose.positions == null) return;

            for (int i = 0; i < handPoints.Count && i < grabPose.positions.Length; i++)
            {
                // 平滑过渡到抓握姿势
                Vector3 targetPos = grabPose.positions[i];
                handPoints[i].transform.position = Vector3.Lerp(
                    handPoints[i].transform.position,
                    targetPos,
                    Time.deltaTime * poseLerpSpeed
                );

                if (grabPose.rotations != null && i < grabPose.rotations.Length)
                {
                    Quaternion targetRot = grabPose.rotations[i];
                    handPoints[i].transform.rotation = Quaternion.Lerp(
                        handPoints[i].transform.rotation,
                        targetRot,
                        Time.deltaTime * poseLerpSpeed
                    );
                }
            }
        }

        /// <summary>
        /// 开始使用抓握姿势
        /// </summary>
        public void StartGrabPose()
        {
            useGrabPose = true;
        }

        /// <summary>
        /// 停止使用抓握姿势，回到MediaPipe控制
        /// </summary>
        public void StopGrabPose()
        {
            useGrabPose = false;
        }

        /// <summary>
        /// 录制当前手部姿势为抓握姿势
        /// </summary>
        [ContextMenu("Record Grab Pose")]
        public void RecordCurrentAsGrabPose()
        {
            if (handPoints == null || handPoints.Count == 0) return;

            grabPose = new HandPoseData();
            grabPose.positions = new Vector3[handPoints.Count];
            grabPose.rotations = new Quaternion[handPoints.Count];

            for (int i = 0; i < handPoints.Count; i++)
            {
                grabPose.positions[i] = handPoints[i].transform.position;
                grabPose.rotations[i] = handPoints[i].transform.rotation;
            }

            Debug.Log("已录制抓握姿势!");
        }

        /// <summary>
        /// 平滑回到MediaPipe控制
        /// </summary>
        public void TransitionBackToMediaPipe()
        {
            if (!useGrabPose) return;

            StartCoroutine(TransitionBackCoroutine());
        }

        private IEnumerator TransitionBackCoroutine()
        {
            float duration = 1f / poseLerpSpeed; // 根据lerp速度计算过渡时间
            float elapsedTime = 0f;

            Vector3[] startPositions = new Vector3[handPoints.Count];
            Quaternion[] startRotations = new Quaternion[handPoints.Count];

            // 保存当前位置作为起点
            for (int i = 0; i < handPoints.Count; i++)
            {
                startPositions[i] = handPoints[i].transform.position;
                startRotations[i] = handPoints[i].transform.rotation;
            }

            while (elapsedTime < duration)
            {
                float progress = elapsedTime / duration;

                for (int i = 0; i < handPoints.Count; i++)
                {
                    handPoints[i].transform.position = Vector3.Lerp(
                        startPositions[i],
                        mediaPipePositions[i],
                        progress
                    );

                    handPoints[i].transform.rotation = Quaternion.Lerp(
                        startRotations[i],
                        mediaPipeRotations[i],
                        progress
                    );
                }

                elapsedTime += Time.deltaTime;
                yield return null;
            }

            // 确保最终位置精确
            for (int i = 0; i < handPoints.Count; i++)
            {
                handPoints[i].transform.position = mediaPipePositions[i];
                handPoints[i].transform.rotation = mediaPipeRotations[i];
            }

            useGrabPose = false;
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