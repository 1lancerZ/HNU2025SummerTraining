using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataManager : MonoBehaviour
{
    public UDPDataReceiver receiver;
    public ModelController leftController;
    public ModelController rightController;
    public ModelController poseController;
    public HandBinder HandModel;
    public PoseBinder poseModel;

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
            poseController.UpdateHandPoints(receiver.poseLandmarks);
        }

        if (HandModel != null)
        {
            HandModel.UpdateHandPoints(receiver.leftHandLandmarks, true);
            HandModel.UpdateHandPoints(receiver.rightHandLandmarks, false);
        }
        if (poseModel != null)
        {
            poseModel.UpdatePosePoints(receiver.poseLandmarks);
        }
    }
}
