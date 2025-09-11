using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 50f;
    public float lifeTime = 5f;
    public Rigidbody rb;

    private void Start()
    {
        Destroy(gameObject, lifeTime); // 防止子弹一直存在
    }

    void Update()
    {
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
    }

    void OnCollisionEnter(Collision collision)
    {
        // 如果击中非目标物体，也销毁子弹
        if (!collision.gameObject.CompareTag("Target"))
        {
            Debug.Log($"子弹碰撞: {collision.gameObject.name}");
            Destroy(gameObject);
        }
    }
}
