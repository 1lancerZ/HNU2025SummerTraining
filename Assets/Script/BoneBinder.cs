using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoneBinder : MonoBehaviour
{
    public bool isLeft = true;
    [Tooltip("顺序应与 Mediapipe 21 关键点一致")]
    public Transform[] handBones = new Transform[21]; // 0-20
    private Vector3[] lastLandmarks = new Vector3[21];
    private Quaternion midHand;

    private void Start()
    {
        //对齐矩阵
        midHand = Quaternion.Inverse(handBones[0].rotation) * Quaternion.LookRotation(
                    handBones[4].position - handBones[9].position,
                    TriangleNormal(handBones[0].position, handBones[2].position, handBones[9].position)
                    );
    }

    public void UpdateBones(Vector3[] landmarks)
    {
        if (landmarks == null || landmarks.Length != 21) return;

        lastLandmarks = landmarks;

        // 控制指骨旋转（使用局部旋转）
        RotateBone(0, 1); RotateBone(1, 2); RotateBone(2, 3); RotateBone(3, 4);

        RotateBone(0, 5); RotateBone(5, 6); RotateBone(6, 7); RotateBone(7, 8);

        RotateBone(0, 9); RotateBone(9, 10); RotateBone(10, 11); RotateBone(11, 12);

        RotateBone(0, 13);RotateBone(13, 14); RotateBone(14, 15); RotateBone(15, 16);

        RotateBone(0, 17); RotateBone(17, 18); RotateBone(18, 19); RotateBone(19, 20);
    }

    private void RotateBone(int fromIdx, int toIdx)
    {
        if (handBones[fromIdx] == null) return;

        Vector3 dir = (lastLandmarks[toIdx] - lastLandmarks[fromIdx]).normalized;

        if (dir.sqrMagnitude < 0.001f) return;


        handBones[fromIdx].rotation = Quaternion.LookRotation(dir,
            TriangleNormal(
                lastLandmarks[0], lastLandmarks[4], lastLandmarks[9]
            )) * midHand;
    }

    private Vector3 TriangleNormal(Vector3 a, Vector3 b, Vector3 c)
    {
        Vector3 d1 = a - b;
        Vector3 d2 = a - c;

        Vector3 dd = Vector3.Cross(d1, d2);
        dd.Normalize();

        return dd;
    }
}
