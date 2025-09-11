using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : MonoBehaviour
{
    #region Components
    public Animator anim { get; private set; }
    public ParticleSystem muzzleFlash { get; private set; }
    #endregion

    // 现有变量...
    public Transform firePoint;
    public GameObject bulletPrefab;
    public float fireRate = 0.2f;
    public int maxAmmo = 10;
    public int currentAmmo;
    public Hand hand;
    private float lastFireTime;

    [Header("弹匣设置")]
    public GameObject mag;
    public Transform ejectPoint;
    public GameObject magFallingPrefab;

    [Header("准星设置")]
    public RectTransform crosshairUI;
    public Camera cam;
    public float maxRayDistance;
    public LayerMask raycastMask;

    [Header("Grip Point")]
    public Transform gripPoint;

    [Header("辅助瞄准设置")]
    public bool aimAssistEnabled = true;
    public float aimAssistStrength = 0.3f;
    public float aimAssistAngle = 30f;
    public float maxAssistDistance = 2f;
    public float snapThreshold = 0.1f;
    public LayerMask targetLayerMask;
    public bool showAimAssistDebug = true;

    #region States
    public GunStateMachine StateMachine { get; private set; }
    public GunIdleState idleState { get; private set; }
    public GunFiringState firingState { get; private set; }
    public GunReloadState reloadState { get; private set; }
    #endregion

    // 辅助瞄准相关
    private List<GameObject> targets = new List<GameObject>();
    private Vector3 originalFireDirection;

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

        // 初始化目标列表
        FindAllTargets();
        StartCoroutine(UpdateTargetsRoutine());
    }

    void Update()
    {
        StateMachine.Update();
        UpdateCrosshair();

        if (Input.GetKeyDown(KeyCode.R))
        {
            StateMachine.ChangeState(reloadState);
        }

        // 调试快捷键
        if (Input.GetKeyDown(KeyCode.T))
        {
            aimAssistEnabled = !aimAssistEnabled;
            Debug.Log($"辅助瞄准: {(aimAssistEnabled ? "启用" : "禁用")}");
        }
    }

    void FixedUpdate()
    {
        StateMachine.FixedUpdate();
    }

    // 辅助瞄准目标更新
    IEnumerator UpdateTargetsRoutine()
    {
        while (true)
        {
            FindAllTargets();
            yield return new WaitForSeconds(1f); // 每秒更新一次目标列表
        }
    }

    void FindAllTargets()
    {
        targets.Clear();
        GameObject[] foundTargets = GameObject.FindGameObjectsWithTag("Target");
        targets.AddRange(foundTargets);
    }

    // 修改Fire方法以包含辅助瞄准
    public void Fire()
    {
        if (Time.time - lastFireTime < fireRate || currentAmmo <= 0) return;

        lastFireTime = Time.time;
        currentAmmo--;

        if (muzzleFlash != null)
        {
            muzzleFlash.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            muzzleFlash.Play();
        }

        // 应用辅助瞄准计算射击方向
        Vector3 fireDirection = GetAssistedFireDirection();
        Quaternion fireRotation = Quaternion.LookRotation(fireDirection);

        Instantiate(bulletPrefab, firePoint.position, fireRotation);
        Debug.Log("Bang!");
    }

    // 获取辅助瞄准后的射击方向
    private Vector3 GetAssistedFireDirection()
    {
        if (!aimAssistEnabled || targets.Count == 0)
            return firePoint.forward;

        GameObject nearestTarget = FindNearestTarget();
        if (nearestTarget == null)
            return firePoint.forward;

        Vector3 targetDirection = (nearestTarget.transform.position - firePoint.position).normalized;
        float angle = Vector3.Angle(firePoint.forward, targetDirection);

        // 如果角度太大，不应用辅助瞄准
        if (angle > aimAssistAngle)
            return firePoint.forward;

        // 应用辅助瞄准强度
        return Vector3.Slerp(firePoint.forward, targetDirection, aimAssistStrength);
    }

    private GameObject FindNearestTarget()
    {
        GameObject nearest = null;
        float minDistance = float.MaxValue;

        foreach (GameObject target in targets)
        {
            if (target == null || !target.activeInHierarchy) continue;

            // 检查目标是否在视野内
            Vector3 screenPos = cam.WorldToViewportPoint(target.transform.position);
            if (screenPos.z > 0 && screenPos.x > 0 && screenPos.x < 1 && screenPos.y > 0 && screenPos.y < 1)
            {
                float distance = Vector3.Distance(firePoint.position, target.transform.position);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    nearest = target;
                }
            }
        }

        return nearest;
    }

    // 修改准星更新以包含辅助瞄准可视化
    public void UpdateCrosshair()
    {
        Ray ray = new Ray(firePoint.position, GetAssistedFireDirection());
        Vector3 targetPoint;

        if (Physics.Raycast(ray, out RaycastHit hit, maxRayDistance, raycastMask))
        {
            targetPoint = hit.point;

            // 绘制辅助瞄准调试线
            if (showAimAssistDebug && aimAssistEnabled)
            {
                Debug.DrawLine(firePoint.position, targetPoint, Color.cyan, 0.1f);

                // 绘制原始瞄准线
                Ray originalRay = new Ray(firePoint.position, firePoint.forward);
                if (Physics.Raycast(originalRay, out RaycastHit originalHit, maxRayDistance, raycastMask))
                {
                    Debug.DrawLine(firePoint.position, originalHit.point, Color.yellow, 0.1f);
                }
            }
        }
        else
        {
            targetPoint = ray.origin + ray.direction * maxRayDistance;
        }

        Vector3 screenPos = cam.WorldToScreenPoint(targetPoint);
        crosshairUI.position = screenPos;
    }

    // 其余现有方法保持不变...
    public void AnimationTrigger() => StateMachine.CurrentState.AnimationFinishTrigger();

    public void Reload()
    {
        currentAmmo = maxAmmo;
        Debug.Log("Reloaded!");
    }

    public void OnEjectMag()
    {
        mag.SetActive(false);
        GameObject droppedMag = Instantiate(magFallingPrefab, ejectPoint.position, ejectPoint.rotation);
        Rigidbody rb = droppedMag.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.AddForce(-ejectPoint.forward * 2f, ForceMode.Impulse);
        }
        Destroy(droppedMag, 3f);
    }

    public void OnReloadInsertMag()
    {
        mag.SetActive(true);
    }

    public void OnGrab(Hand hand)
    {
        Debug.Log($"Gun grabbed by {hand.handSide}");
    }

    public void OnRelease(Hand hand)
    {
        Debug.Log($"Gun released by {hand.handSide}");
    }

    // 辅助瞄准调试信息
    void OnDrawGizmos()
    {
        if (showAimAssistDebug && Application.isPlaying)
        {
            // 绘制辅助瞄准范围
            Gizmos.color = new Color(0, 1, 1, 0.1f);
            Gizmos.DrawSphere(firePoint.position, maxAssistDistance);

            // 绘制检测到的目标
            Gizmos.color = Color.green;
            foreach (GameObject target in targets)
            {
                if (target != null && target.activeInHierarchy)
                {
                    Gizmos.DrawWireSphere(target.transform.position, 0.3f);
                    Gizmos.DrawLine(firePoint.position, target.transform.position);
                }
            }
        }
    }
}