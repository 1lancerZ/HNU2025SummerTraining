using System.Collections;
using System.Collections.Generic;
using System.Xml;
using Unity.IO.LowLevel.Unsafe;
using UnityEngine;


public class Gun : MonoBehaviour
{
    #region Components
    public Animator anim { get; private set; }
    public ParticleSystem muzzleFlash { get; private set; } // 枪口火焰特效
    #endregion

    public Transform firePoint;
    public GameObject bulletPrefab;


    public float fireRate = 0.2f;
    public int maxAmmo = 10;
    public int currentAmmo;

    public Hand hand;

    private float lastFireTime;

    [Header("弹匣设置")]
    public GameObject mag; // 弹匣对象
    public Transform ejectPoint; // 弹出时的位置
    public GameObject magFallingPrefab; // 掉落的弹匣模型预制体

    [Header("准星设置")]
    public RectTransform crosshairUI;
    public Camera cam;
    public float maxRayDistance;
    public LayerMask raycastMask; // 不包括 Bullet 层

    [Header("Grip Point (设置手握住的空物体)")]
    public Transform gripPoint;

    #region States
    public GunStateMachine StateMachine { get; private set; }

    public GunIdleState idleState { get; private set; }
    public GunFiringState firingState { get; private set; }
    public GunReloadState reloadState { get; private set; }
    #endregion

    void Awake()
    {
        StateMachine = new GunStateMachine();
        idleState = new GunIdleState(this, StateMachine, "isIdle");
        firingState = new GunFiringState(this, StateMachine, "isFire");
        reloadState = new GunReloadState(this, StateMachine, "isReload");
    }



    void Start()
    {
        anim = GetComponentInChildren<Animator>();
        muzzleFlash = GetComponentInChildren<ParticleSystem>();

        currentAmmo = maxAmmo;
        StateMachine.Initialize(idleState);
    }

    void Update()
    {
        StateMachine.Update();
        UpdateCrosshair();

        // 示例：按 R 键手动换弹（模拟）
        if (Input.GetKeyDown(KeyCode.R))
        {
            StateMachine.ChangeState(reloadState);
        }
    }

    void FixedUpdate()
    {
        StateMachine.FixedUpdate();
    }

    public void AnimationTrigger() => StateMachine.CurrentState.AnimationFinishTrigger();

    public void Fire()
    {
        if (Time.time - lastFireTime < fireRate || currentAmmo <= 0) return;

        lastFireTime = Time.time;
        currentAmmo--;
        if (muzzleFlash != null) {
            muzzleFlash.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            muzzleFlash.Play();
        }

        Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
        Debug.Log("Bang!");
    }

    public void Reload()
    {
        currentAmmo = maxAmmo;
        Debug.Log("Reloaded!");
    }

    // 动画事件中会调用这个函数
    public void OnEjectMag()
    {
        mag.SetActive(false); // 隐藏原本的弹匣（在枪上）

        // 生成一个掉落的弹匣实例
        GameObject droppedMag = Instantiate(magFallingPrefab, ejectPoint.position, ejectPoint.rotation);
        Rigidbody rb = droppedMag.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.AddForce(-ejectPoint.forward * 2f, ForceMode.Impulse); // 给个弹出力
        }

        Destroy(droppedMag, 3f); // 3秒后销毁掉落弹匣   
    }

    // 动画后续再调用这个函数装入新弹匣
    public void OnReloadInsertMag()
    {
        mag.SetActive(true);
    }

    public void UpdateCrosshair()
    {
        Ray ray = new Ray(firePoint.position, firePoint.forward);
        Vector3 targetPoint;

        if (Physics.Raycast(ray, out RaycastHit hit, maxRayDistance, raycastMask))
        {
            targetPoint = hit.point;
        }
        else
        {
            targetPoint = ray.origin + ray.direction * maxRayDistance;
        }

        Vector3 screenPos = cam.WorldToScreenPoint(targetPoint);
        crosshairUI.position = screenPos;
    }


    public void OnGrab(Hand hand)
    {
        Debug.Log($"Gun grabbed by {hand.handSide}");

        //if (anim)
        //    anim.SetBool("Held", true);

        // 可以禁用开火、重载控制器，或连接状态机
    }

    public void OnRelease(Hand hand)
    {
        Debug.Log($"Gun released by {hand.handSide}");

        //if (anim)
        //    anim.SetBool("Held", false);
    }
}
