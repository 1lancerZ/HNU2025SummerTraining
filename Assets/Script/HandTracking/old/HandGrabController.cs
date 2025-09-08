using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandGrabController : MonoBehaviour
{
    public Transform handTransform;        // 手部 Transform
    public KeyCode grabKey = KeyCode.G;    // 模拟抓取键
    public float grabRadius = 0.3f;        // 抓取半径
    public LayerMask grabbableLayer;       // 可抓取物体的层
    public Hand hand;

    private GameObject grabbedObject = null;
    private Rigidbody grabbedRigidbody = null;

    void Update()
    {
        if (Input.GetKeyDown(grabKey) || hand.handpose == HandPose.Fist)
        {
            if (grabbedObject == null)
            {
                TryGrab();
            }
        }

        if (grabbedObject != null)
        {
            grabbedObject.transform.position = handTransform.position;
            grabbedObject.transform.rotation = handTransform.rotation;
        }
    }

    void TryGrab()
    {
        Collider[] hits = Physics.OverlapSphere(handTransform.position, grabRadius, grabbableLayer);
        if (hits.Length > 0)
        {
            grabbedObject = hits[0].gameObject;
            grabbedRigidbody = grabbedObject.GetComponent<Rigidbody>();
            if (grabbedRigidbody != null)
            {
                grabbedRigidbody.isKinematic = true;
            }
        }
    }

    void Release()
    {
        if (grabbedRigidbody != null)
        {
            grabbedRigidbody.isKinematic = false;
        }

        grabbedObject = null;
        grabbedRigidbody = null;
    }
}
