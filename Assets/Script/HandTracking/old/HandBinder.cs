using landmarktest;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using static PoseBinder;

public class HandBinder : MonoBehaviour
{
    public class AvatarTree
    {
        public Transform transf;
        public AvatarTree[] childs;
        public AvatarTree parent;
        public int idx;
        public Quaternion quaternion;//起始旋转
        public AvatarTree(Transform tf, int count, int idx, Quaternion quaternion, AvatarTree parent = null)
        {
            this.transf = tf;
            this.parent = parent;
            this.idx = idx;
            this.quaternion = quaternion;
            if (count > 0)
            {
                childs = new AvatarTree[count];
            }
        }
        public Vector3 GetDir()
        {
            if (parent != null)
            {
                return transf.position - parent.transf.position;
            }
            return Vector3.up;
        }
    }



    public Animator anim;//角色动画控制器
    [Header("Setting")]
    [SerializeField]private float smoothAlpha = 0.7f; // 越小越稳定
    public DepthCalibrator calibrator = new DepthCalibrator(-0.0719f, 0.439f);
    private Vector3[] smoothLeft = new Vector3[21];
    private Vector3[] smoothRight = new Vector3[21];

    [Header("Left Hand")]
    public Transform lWristRoot;  // = 0
    public Transform lThumb1, lThumb2, lThumb3, lThumbTip; // = 1-4
    public Transform lIndex1, lIndex2, lIndex3, lIndexTip; // = 5-8
    public Transform lMiddle1, lMiddle2, lMiddle3, lMiddleTip; // = 9-12
    public Transform lRing1, lRing2, lRing3, lRingTip; // = 13-16
    public Transform lPinky1, lPinky2, lPinky3, lPinkyTip; // = 17-20
    private AvatarTree LWristRoot;
    private AvatarTree LThumb1, LThumb2, LThumb3, LThumbTip; // = 1-4
    private AvatarTree LIndex1, LIndex2, LIndex3, LIndexTip; // = 5-8
    private AvatarTree LMiddle1, LMiddle2, LMiddle3, LMiddleTip; // = 9-12
    private AvatarTree LRing1, LRing2, LRing3, LRingTip; // = 13-16
    private AvatarTree LPinky1, LPinky2, LPinky3, LPinkyTip; // = 17-20
    private Vector3[] leftHandPoints = new Vector3[21];

    [Header("Right Hand")]
    public Transform rWristRoot;  // = 0
    public Transform rThumb1, rThumb2, rThumb3, rThumbTip; // = 1-4
    public Transform rIndex1, rIndex2, rIndex3, rIndexTip; // = 5-8
    public Transform rMiddle1, rMiddle2, rMiddle3, rMiddleTip; // = 9-12
    public Transform rRing1, rRing2, rRing3, rRingTip; // = 13-16
    public Transform rPinky1, rPinky2, rPinky3, rPinkyTip; // = 17-20
    private AvatarTree RWristRoot;
    private AvatarTree RThumb1, RThumb2, RThumb3, RThumbTip; // = 1-4
    private AvatarTree RIndex1, RIndex2, RIndex3, RIndexTip; // = 5-8
    private AvatarTree RMiddle1, RMiddle2, RMiddle3, RMiddleTip; // = 9-12
    private AvatarTree RRing1, RRing2, RRing3, RRingTip; // = 13-16
    private AvatarTree RPinky1, RPinky2, RPinky3, RPinkyTip; // = 17-20
    private Vector3[] rightHandPoints = new Vector3[21];
    public float lerp;

    public bool isGrabbingLeft = false;
    public bool isGrabbingRight = false;

    private Quaternion[] grabRotations = new Quaternion[]
    {
        Quaternion.Euler(0, 0, 50), // 第一节
        Quaternion.Euler(0, 0, 90), // 第二节
        Quaternion.Euler(0, 0, 90)  // 第三节
    };

    private void Start()
    {
        BulidTree(); // 必须构建手部骨骼树
    }

    private void BulidTree()
    {
        // 左手 Wrist 根节点
        LWristRoot = new AvatarTree(lWristRoot, 5, 0, lWristRoot.rotation);
        LThumb1 = LWristRoot.childs[0] = new AvatarTree(lThumb1, 1, 1, lThumb1.rotation, LWristRoot);
        LThumb2 = LThumb1.childs[0] = new AvatarTree(lThumb2, 1, 2, lThumb2.rotation, LThumb1);
        LThumb3 = LThumb2.childs[0] = new AvatarTree(lThumb3, 1, 3, lThumb3.rotation, LThumb2);
        LThumbTip = LThumb3.childs[0] = new AvatarTree(lThumbTip, 0, 4, lThumbTip.rotation, LThumb3);

        LIndex1 = LWristRoot.childs[1] = new AvatarTree(lIndex1, 1, 5, lIndex1.rotation, LWristRoot);
        LIndex2 = LIndex1.childs[0] = new AvatarTree(lIndex2, 1, 6, lIndex2.rotation, LIndex1);
        LIndex3 = LIndex2.childs[0] = new AvatarTree(lIndex3, 1, 7, lIndex3.rotation, LIndex2);
        LIndexTip = LIndex3.childs[0] = new AvatarTree(lIndexTip, 0, 8, lIndexTip.rotation, LIndex3);

        LMiddle1 = LWristRoot.childs[2] = new AvatarTree(lMiddle1, 1, 9, lMiddle1.rotation, LWristRoot);
        LMiddle2 = LMiddle1.childs[0] = new AvatarTree(lMiddle2, 1, 10, lMiddle2.rotation, LMiddle1);
        LMiddle3 = LMiddle2.childs[0] = new AvatarTree(lMiddle3, 1, 11, lMiddle3.rotation, LMiddle2);
        LMiddleTip = LMiddle3.childs[0] = new AvatarTree(lMiddleTip, 0, 12, lMiddleTip.rotation, LMiddle3);

        LRing1 = LWristRoot.childs[3] = new AvatarTree(lRing1, 1, 13, lRing1.rotation, LWristRoot);
        LRing2 = LRing1.childs[0] = new AvatarTree(lRing2, 1, 14, lRing2.rotation, LRing1);
        LRing3 = LRing2.childs[0] = new AvatarTree(lRing3, 1, 15, lRing3.rotation, LRing2);
        LRingTip = LRing3.childs[0] = new AvatarTree(lRingTip, 0, 16, lRingTip.rotation, LRing3);

        LPinky1 = LWristRoot.childs[4] = new AvatarTree(lPinky1, 1, 17, lPinky1.rotation, LWristRoot);
        LPinky2 = LPinky1.childs[0] = new AvatarTree(lPinky2, 1, 18, lPinky2.rotation, LPinky1);
        LPinky3 = LPinky2.childs[0] = new AvatarTree(lPinky3, 1, 19, lPinky3.rotation, LPinky2);
        LPinkyTip = LPinky3.childs[0] = new AvatarTree(lPinkyTip, 0, 20, lPinkyTip.rotation, LPinky3);

        //右手 Wrist 根节点
        RWristRoot = new AvatarTree(rWristRoot, 5, 0, rWristRoot.rotation);
        RThumb1 = RWristRoot.childs[0] = new AvatarTree(rThumb1, 1, 1, rThumb1.rotation, RWristRoot);
        RThumb2 = RThumb1.childs[0] = new AvatarTree(rThumb2, 1, 2, rThumb2.rotation, RThumb1);
        RThumb3 = RThumb2.childs[0] = new AvatarTree(rThumb3, 1, 3, rThumb3.rotation, RThumb2);
        RThumbTip = RThumb3.childs[0] = new AvatarTree(rThumbTip, 0, 4, rThumbTip.rotation, RThumb3);

        RIndex1 = RWristRoot.childs[1] = new AvatarTree(rIndex1, 1, 5, rIndex1.rotation, RWristRoot);
        RIndex2 = RIndex1.childs[0] = new AvatarTree(rIndex2, 1, 6, rIndex2.rotation, RIndex1);
        RIndex3 = RIndex2.childs[0] = new AvatarTree(rIndex3, 1, 7, rIndex3.rotation, RIndex2);
        RIndexTip = RIndex3.childs[0] = new AvatarTree(rIndexTip, 0, 8, rIndexTip.rotation, RIndex3);

        RMiddle1 = RWristRoot.childs[2] = new AvatarTree(rMiddle1, 1, 9, rMiddle1.rotation, RWristRoot);
        RMiddle2 = RMiddle1.childs[0] = new AvatarTree(rMiddle2, 1, 10, rMiddle2.rotation, RMiddle1);
        RMiddle3 = RMiddle2.childs[0] = new AvatarTree(rMiddle3, 1, 11, rMiddle3.rotation, RMiddle2);
        RMiddleTip = RMiddle3.childs[0] = new AvatarTree(rMiddleTip, 0, 12, rMiddleTip.rotation, RMiddle3);

        RRing1 = RWristRoot.childs[3] = new AvatarTree(rRing1, 1, 13, rRing1.rotation, RWristRoot);
        RRing2 = RRing1.childs[0] = new AvatarTree(rRing2, 1, 14, rRing2.rotation, RRing1);
        RRing3 = RRing2.childs[0] = new AvatarTree(rRing3, 1, 15, rRing3.rotation, RRing2);
        RRingTip = RRing3.childs[0] = new AvatarTree(rRingTip, 0, 16, rRingTip.rotation, RRing3);

        RPinky1 = RWristRoot.childs[4] = new AvatarTree(rPinky1, 1, 17, rPinky1.rotation, RWristRoot);
        RPinky2 = RPinky1.childs[0] = new AvatarTree(rPinky2, 1, 18, rPinky2.rotation, RPinky1);
        RPinky3 = RPinky2.childs[0] = new AvatarTree(rPinky3, 1, 19, rPinky3.rotation, RPinky2);
        RPinkyTip = RPinky3.childs[0] = new AvatarTree(rPinkyTip, 0, 20, rPinkyTip.rotation, RPinky3);
    }//匹配关节父子节点

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.G))
            isGrabbingLeft = !isGrabbingLeft;
        //lerp += Time.deltaTime;
        //if (lerp >= 1.0f)
        //{
        //    lerp = 0;
        //}
        ////左手
        //UpdateTree(LWristRoot, lerp, true);
        ////右手
        //UpdateTree(RWristRoot, lerp, false);
        // 左手
        if (leftHandPoints != null && leftHandPoints.Length == 21)
        {
            Quaternion palmRot = EstimatePalmRotation(leftHandPoints);
            LWristRoot.transf.rotation = Quaternion.Slerp(LWristRoot.transf.rotation, palmRot, lerp);
            UpdateTree(LWristRoot, lerp, true);
        }

        // 右手
        if (rightHandPoints != null && rightHandPoints.Length == 21)
        {
            Quaternion palmRot = EstimatePalmRotation(rightHandPoints);
            RWristRoot.transf.rotation = Quaternion.Slerp(RWristRoot.transf.rotation, palmRot, lerp);
            UpdateTree(RWristRoot, lerp, false);
        }
    }

    public void UpdateHandPoints(Vector3[] landmarks, bool isLeft)
    {
        if (landmarks == null || landmarks.Length != 21)
            return;

        Vector3[] worldPoints = ConvertHandLandmarksToWorld(landmarks, Camera.main);

        if (isLeft)
        {
            for (int i = 0; i < 21; i++)
                smoothLeft[i] = Vector3.Lerp(smoothLeft[i], worldPoints[i], 1f - smoothAlpha);
            leftHandPoints = smoothLeft;
        }
        else
        {
            for (int i = 0; i < 21; i++)
                smoothRight[i] = Vector3.Lerp(smoothRight[i], worldPoints[i], 1f - smoothAlpha);
            rightHandPoints = smoothRight;
        }
    }

    public Vector3[] ConvertHandLandmarksToWorld(Vector3[] landmarks, Camera cam, float zScale = 2f)
    {
        Vector3[] result = new Vector3[landmarks.Length];

        // 用大拇指长度估算 Wrist 的深度
        //float refDepth = calibrator.GetDepthFromThumbLength(Vector3.Distance(landmarks[0], landmarks[4]));

        for (int i = 0; i < landmarks.Length; i++)
        {
            float x = (1 - landmarks[i].x) * cam.pixelWidth;
            float y = -landmarks[i].y * cam.pixelHeight;
            //float zOffset = landmarks[i].z * zScale; // landmark.z 是相对值
            float z = landmarks[i].z; // 摄像机前是负Z

            result[i] = cam.ScreenToWorldPoint(new Vector3(x, y, z));
        }

        return result;
    }

    private void UpdateTree(AvatarTree tree, float lerp, bool isLeft)
    {
        bool isGrabbing = isLeft ? isGrabbingLeft : isGrabbingRight;

        if (tree.parent != null)
        {
            if (isGrabbing && tree.idx >= 1 && tree.idx <= 19) // 手指关节
            {
                ApplyGrabbingRotation(tree);
            }
            else
            {
                UpdateHandBone(tree, lerp, isLeft);
            }
        }
        if (tree.childs != null)
        {
            for (int i = 0; i < tree.childs.Length; i++)
            {
                UpdateTree(tree.childs[i], lerp, isLeft);
            }
        }
    }//遍历关节 

    private void UpdateHandBone(AvatarTree tree, float lerp, bool isLeft)
    {
        if (tree.parent == null) return;

        Vector3 dir1 = tree.GetDir();
        Vector3 dir2;
        if (isLeft)
        {
            var child_dir = leftHandPoints[tree.idx];
            var parent_dir = leftHandPoints[tree.parent.idx];
            dir2 = parent_dir - child_dir;
        }
        else
        {
            var child_dir = rightHandPoints[tree.idx];
            var parent_dir = rightHandPoints[tree.parent.idx];
            dir2 = parent_dir - child_dir;
        }
        dir2.z = -dir2.z;
        Quaternion rot1 = tree.parent.transf.rotation;
        Quaternion rot = Quaternion.FromToRotation(dir1, dir2);
        tree.parent.transf.rotation = rot * rot1;
    }

    private Quaternion EstimatePalmRotation(Vector3[] landmarks)
    {
        if (landmarks == null || landmarks.Length < 18)
            return Quaternion.identity;

        Vector3 wrist = landmarks[0];
        Vector3 indexBase = landmarks[5];
        Vector3 pinkyBase = landmarks[17];

        Vector3 v1 = indexBase - wrist;
        Vector3 v2 = pinkyBase - wrist;

        Vector3 palmNormal = Vector3.Cross(v1, v2).normalized;      // 掌心方向
        Vector3 palmForward = v1.normalized;                        // 手指朝前方向

        if (Vector3.Dot(palmNormal, Vector3.up) < 0)
            palmNormal = -palmNormal;


        return Quaternion.LookRotation(palmNormal, palmForward);
    }

    private void ApplyGrabbingRotation(AvatarTree joint)
    {
        int relativeIndex = (joint.idx - 1) % 4; // 每个手指3节（1,2,3），tip除外
        int jointIndex = Mathf.Clamp(relativeIndex, 0, 2); // 只对三节有效
        joint.transf.localRotation = Quaternion.Slerp(
            joint.transf.localRotation,
            grabRotations[jointIndex],
            lerp
        );
    }
}
