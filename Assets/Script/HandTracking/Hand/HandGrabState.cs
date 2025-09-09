using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandGrabState : HandState
{
    private GameObject grabbedObject = null;
    private Rigidbody grabbedRigidbody = null;
    private float grabRadius = 0.3f;
    [SerializeField] private LayerMask grabbableLayer;

    public HandGrabState(Hand hand, HandStateMachine stateMachine) : base(hand, stateMachine)
    {
        grabbableLayer = LayerMask.GetMask("Default");
    }

    public override void Enter()
    {
        base.Enter();
        TryGrab();
    }

    public override void Exit()
    {
        base.Exit();
        Release();

        if (rig != null)
        {
            rig.StopGrabPose();
        }
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();

        // 抓取物体跟随手部
        if (grabbedObject != null)
        {
            grabbedObject.transform.position = hand.grabAnchor.position;
            grabbedObject.transform.rotation = hand.grabAnchor.rotation;
        }
    }

    private void TryGrab()
    {
        Collider[] hits = Physics.OverlapSphere(hand.grabAnchor.position, grabRadius, grabbableLayer);
        if (hits.Length > 0)
        {
            grabbedObject = hits[0].gameObject;
            grabbedRigidbody = grabbedObject.GetComponent<Rigidbody>();

            // 这里专门处理 Gun
            Gun gun = grabbedObject.GetComponent<Gun>();
            if (gun != null)
            {
                GrabGun(gun);
                return;
            }

            // 泛型物体处理
            if (grabbedRigidbody != null)
            {
                grabbedRigidbody.isKinematic = true;
            }
            grabbedObject.transform.SetParent(hand.grabAnchor);
            grabbedObject.transform.localPosition = Vector3.zero;
            grabbedObject.transform.localRotation = Quaternion.identity;
        }
    }

    private void Release()
    {
        if (grabbedRigidbody != null)
        {
            grabbedRigidbody.isKinematic = false;
        }

        if (grabbedObject != null)
        {
            grabbedObject.transform.SetParent(null);
        }

        grabbedObject = null;
        grabbedRigidbody = null;
        hand.currentHeldObject = null;
    }

    private void GrabGun(Gun target)
    {
        hand.currentHeldObject = target;

        if (target.gripPoint != null)
        {
            // 先解除父级，避免旋转被手部影响
            target.transform.SetParent(null);

            // 1. 先旋转枪，使 gripPoint 对齐 grabAnchor
            Quaternion gripRotationOffset = hand.grabAnchor.rotation * Quaternion.Inverse(target.gripPoint.rotation);
            target.transform.rotation = gripRotationOffset * target.transform.rotation;

            // 2. 位置对齐 gripPoint
            Vector3 positionOffset = target.transform.position - target.gripPoint.position;
            target.transform.position = hand.grabAnchor.position + positionOffset;

            // 3. 修正枪口方向，使其朝向手掌 forward
            Vector3 desiredForward = hand.grabAnchor.forward;
            Vector3 currentForward = (target.firePoint.position - target.transform.position).normalized;
            Quaternion alignGunForward = Quaternion.FromToRotation(currentForward, desiredForward);
            target.transform.rotation = alignGunForward * target.transform.rotation;

            // 4. 挂回手部抓握点
            target.transform.SetParent(hand.grabAnchor);
        }
        else
        {
            target.transform.SetParent(hand.grabAnchor);
            target.transform.localPosition = Vector3.zero;
            target.transform.localRotation = Quaternion.identity;
        }

        var rb = target.GetComponent<Rigidbody>();
        if (rb) rb.isKinematic = true;

        rig.StartGrabPose();
        target.OnGrab(hand);
    }
}
