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
    [SerializeField] public Transform grabAnchor;
    [SerializeField] private float requiredPalmDuration = 0.3f; // 松开需要保持的时间
    public Gun currentHeldObject;
    private float palmHoldTime = 0f;


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
        // 进入抓取
        if (handpose == HandPose.Fist && stateMachine.CurrentState != grabState)
        {
            stateMachine.ChangeState(grabState);
            palmHoldTime = 0f; // 重置 Palm 计时
        }
        // 在抓取状态下才处理 Palm 释放
        else if (stateMachine.CurrentState == grabState)
        {
            if (handpose == HandPose.Palm)
            {
                palmHoldTime += Time.deltaTime;
                if (palmHoldTime >= requiredPalmDuration)
                {
                    stateMachine.ChangeState(idleState);
                }
            }
            else
            {
                palmHoldTime = 0f; // 如果手势变回去，计时清零
            }
        }

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
