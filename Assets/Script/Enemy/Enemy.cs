using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Enemy : MonoBehaviour
{
    #region Components
    public Animator anim { get; private set; }

    public AudioSource audioSource { get; private set; }
    #endregion

    public EnemyStateMachine stateMachine { get; private set; }

    [Header("基础属性")]
    public int maxHealth = 100;
    protected int currentHealth;
    public bool isDead { get; private set; }

    protected virtual void Awake()
    {
        stateMachine = new EnemyStateMachine();
    }

    protected virtual void Start()
    {
        anim = GetComponentInChildren<Animator>();

        currentHealth = maxHealth;
    }

    protected virtual void Update()
    {
        stateMachine.Update();
    }

    public virtual void TakeDamage(int damage)
    {
        if (isDead) return;

        currentHealth -= damage;
        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            OnHit();
        }
    }

    protected virtual void OnHit()
    {
        // 可在子类重写为：播放受击动画、进入受击状态
        //stateMachine.ChangeState(new EnemyTakeHitState(this));
    }

    protected virtual void Die()
    {
        isDead = true;
        //stateMachine.ChangeState(new EnemyDeadState(this));
    }

    public void DestroySelf(float delay = 2f)
    {
        Destroy(gameObject, delay);
    }
}

