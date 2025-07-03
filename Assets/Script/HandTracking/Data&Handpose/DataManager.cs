using landmarktest;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataManager : MonoBehaviour
{
    public UDPDataReceiver receiver;
    //public ModelController leftController;
    //public ModelController rightController;
    //public ModelController poseController;
    //public HandBinder HandModel;
    //public PoseBinder poseModel;
    public HandRigController handRigControllerL;
    public HandRigController handRigControllerR;
    public Hand leftHand;
    public Hand rightHand;

    void Update()
    {
        //if (leftController != null)
        //{
        //    leftController.UpdatePoints(receiver.leftHandLandmarks);
        //}
        //if(rightController != null)
        //{
        //    rightController.UpdatePoints(receiver.rightHandLandmarks);
        //}
        //if(poseController != null)
        //{
        //    poseController.UpdateHandPoints(receiver.poseLandmarks);
        //}

        //if (HandModel != null)
        //{
        //    HandModel.UpdateHandPoints(receiver.leftHandLandmarks, true);
        //    HandModel.UpdateHandPoints(receiver.rightHandLandmarks, false);
        //}
        //if (poseModel != null)
        //{
        //    poseModel.UpdatePosePoints(receiver.poseLandmarks);
        //}

        if(handRigControllerL != null)
        {
            handRigControllerL.UpdateHandPointsFromLandmarks(receiver.leftHandLandmarks, true, receiver.leftHandDepthCM[0]);
        }
        if (handRigControllerR != null)
        {
            handRigControllerR.UpdateHandPointsFromLandmarks(receiver.rightHandLandmarks, false, receiver.rightHandDepthCM[0]);
        }

        if (leftHand != null)
        {
            //leftHand.UpdateHandPose();
        }
        if (rightHand != null)
        {
            //rightHand.UpdateHandPose();
        }
    }
}
