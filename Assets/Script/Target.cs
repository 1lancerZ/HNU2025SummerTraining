using System.Collections;
using UnityEngine;

public class Target : MonoBehaviour
{
    private TargetController controller;
    private float lifetime;
    private Coroutine lifetimeCoroutine;

    public void Initialize(TargetController targetController, float lifeTime)
    {
        controller = targetController;
        lifetime = lifeTime;

        // 开始生命周期计时
        lifetimeCoroutine = StartCoroutine(LifetimeCountdown());
    }

    IEnumerator LifetimeCountdown()
    {
        yield return new WaitForSeconds(lifetime);

        // 超时消失
        if (controller != null)
        {
            controller.OnTargetTimeout(gameObject);
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        // 检查是否是子弹击中
        if (collision.gameObject.CompareTag("Bullet"))
        {
            HandleBulletHit(collision.gameObject);
        }
    }

    void HandleBulletHit(GameObject bullet)
    {
        // 停止生命周期计时
        if (lifetimeCoroutine != null)
        {
            StopCoroutine(lifetimeCoroutine);
        }

        // 通知控制器
        if (controller != null)
        {
            controller.OnTargetHit(gameObject);
        }

        // 销毁子弹
        Destroy(bullet);

        // 这里可以添加更多的命中效果，如粒子、声音等
    }

    // 手动销毁目标（用于测试或其他情况）
    public void DestroyTarget()
    {
        if (controller != null)
        {
            controller.OnTargetHit(gameObject, false);
        }
    }
}