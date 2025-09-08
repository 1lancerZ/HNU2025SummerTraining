using landmarktest;
using System;
using UnityEngine;

public enum HandSide
{
    Left,
    Right
}

public enum HandPose
{
    Fire,
    Fist,
    Palm
}

public class Hand : MonoBehaviour
{
    [Header("Hand Data")]
    public HandSide handSide;
    public HandPose handpose;

    [Header("Grabbing")]
    [SerializeField]public Transform grabAnchor;
    public Gun currentHeldObject;

    #region Components
    public HandRigController rigController { get; private set; }
    #endregion

    #region States
    public HandStateMachine stateMachine { get; private set; }
    public HandIdleState idleState { get; private set; }
    public HandGrabState grabState { get; private set; }

    #endregion

    void Awake()
    {
        stateMachine = new HandStateMachine();
        idleState = new HandIdleState(this, stateMachine);
        grabState = new HandGrabState(this, stateMachine);
    }

    void Start()
    {
        rigController = GetComponentInParent<HandRigController>();
        stateMachine.Initialize(idleState);
    }

    void Update()
    {
        stateMachine.Update();
    }

    void FixedUpdate()
    {
        stateMachine.FixedUpdate();
    }

    public void UpdateHandPose(string _handpose)
    {
        handpose = (HandPose)Enum.Parse(typeof(HandPose), _handpose);
    }
}
