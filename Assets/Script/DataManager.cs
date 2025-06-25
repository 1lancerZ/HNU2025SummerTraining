using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataManager : MonoBehaviour
{
    public UDPDataReceiver receiver;
    public ModelController leftController;
    public ModelController rightController;
    public ModelController poseController;
    public BoneBinder leftHandModel;
    public BoneBinder rightHandModel;

    void Update()
    {
        if (leftController != null)
        {
            leftController.UpdatePoints(receiver.leftHandLandmarks);
        }
        if(rightController != null)
        {
            rightController.UpdatePoints(receiver.rightHandLandmarks);
        }
        if(poseController != null)
        {
            poseController.UpdatePosePoints(receiver.poseLandmarks);
        }

        if (leftHandModel != null)
        {
            leftHandModel.UpdateBones(receiver.leftHandLandmarks);
        }
        if (rightHandModel != null)
        {
            rightHandModel.UpdateBones(receiver.rightHandLandmarks);
        }
    }
}
