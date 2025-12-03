using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public enum BossState
{
    Sleep,
    Running,
    Aiming
}

public class BossController : MonoBehaviour
{
    BossState state = BossState.Sleep;
    private EnemyLife enemyLife;
    private CheckForPlayer checkForPlayer;

    [Header("Ustawienia ruchu")]
    public float moveSpeed = 30f;
    public float acceleration = 5f;
    public float deceleration = 8f;
    private Rigidbody rb;
    public Animator animator;
    private Vector3 velocity;

    public AudioSource audioSource;

    [Header("DŸwiêki animacji")]
    public AudioClip runSound;
    public AudioClip attackSound;
    public AudioClip idleSound;
    public AudioClip tauntSound;
    public AudioClip deathSound;

    private float aimingTimer = 0f;

    private float idleTimer = 0f;
    private float nextIdleChangeTime = 0f;

    void Start()
    {
        checkForPlayer = GetComponent<CheckForPlayer>();
        rb = GetComponent<Rigidbody>();
        enemyLife = GetComponent<EnemyLife>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (!enemyLife.isAlive())
        {
            return;
        }

        if (!checkForPlayer.isTargetVisible)
        {
            state = BossState.Sleep;
            animator.SetBool("Idle", true);
            animator.SetBool("Running", false);
            animator.SetBool("Attack", false);

            velocity = Vector3.Lerp(velocity, Vector3.zero, Time.fixedDeltaTime * deceleration);
            rb.MovePosition(rb.position + velocity * Time.fixedDeltaTime);

            idleTimer += Time.deltaTime;
            if (idleTimer >= nextIdleChangeTime)
            {
                animator.SetTrigger("Taunt");

                idleTimer = 0f;
                nextIdleChangeTime = UnityEngine.Random.Range(15f, 30f);
            }

            return;
        }

        switch (state)
        {
            case BossState.Sleep:
                animator.SetBool("Idle", true);
                animator.SetBool("Running", false);
                animator.SetBool("Attack", false);

                if (checkForPlayer.isTargetVisible)
                {
                    state = BossState.Aiming;
                }
                break;

            case BossState.Running:
                animator.SetBool("Idle", false);
                animator.SetBool("Running", true);
                animator.SetBool("Attack", false);
                Vector3 targetVelocity = transform.forward * moveSpeed;
                velocity = Vector3.Lerp(velocity, targetVelocity, Time.fixedDeltaTime * (transform.forward.magnitude > 0 ? acceleration : deceleration));
                rb.MovePosition(rb.position + velocity * Time.fixedDeltaTime);
                break;

            case BossState.Aiming:
                Quaternion targetRot = Quaternion.LookRotation(checkForPlayer.targetGameObject.transform.position - transform.position, Vector3.up);
                rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRot, Time.fixedDeltaTime * 10f));


            aimingTimer += Time.deltaTime;
            if (aimingTimer >= 5f)
            {
                state = BossState.Running;
                aimingTimer = 0f;
            }
            break;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if((collision.gameObject.layer == LayerMask.NameToLayer("Wall") || collision.gameObject.layer == LayerMask.NameToLayer("Player")) && state == BossState.Running )
        {
            velocity = Vector3.zero;
            state = BossState.Sleep;
        }
    }

    public void PlayRunSound()
    {
        if (runSound) audioSource.PlayOneShot(runSound);
    }

    public void PlayAttackSound()
    {
        if (attackSound) audioSource.PlayOneShot(attackSound);
    }

    public void PlayIdleSound()
    {
        if (idleSound) audioSource.PlayOneShot(idleSound);
    }

    public void PlayTauntSound()
    {
        if (tauntSound) audioSource.PlayOneShot(tauntSound);
    }

    public void PlayDeathSound()
    {
        if (deathSound) audioSource.PlayOneShot(deathSound);
    }
}
