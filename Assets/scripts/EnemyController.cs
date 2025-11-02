using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [Header("Ustawienia ruchu")]
    public float moveSpeed = 3f;
    public float acceleration = 5f;
    public float deceleration = 8f;
    public bool rotateTowardsMoveDirection = true;

    [Header("Ustawienia ataku")]
    public float attackRange = 2f;
    public float attackCooldown = 1.5f;

    [Header("Komponenty")]
    public Animator animator;

    private CheckForPlayer checkForPlayer;
    private Vector3 inputDir;
    private Vector3 velocity;
    private Rigidbody rb;
    private float attackTimer = 0f;

    private void Awake()
    {
        checkForPlayer = GetComponent<CheckForPlayer>();
        rb = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        if (!checkForPlayer.isTargetVisible)
        {
            velocity = Vector3.Lerp(velocity, Vector3.zero, Time.fixedDeltaTime * deceleration);
            rb.MovePosition(rb.position + velocity * Time.fixedDeltaTime);
            animator.SetFloat("Velocity", velocity.magnitude, 0.1f, Time.deltaTime);
            return;
        }

        Transform target = checkForPlayer.targetGameObject.transform;
        float distanceToTarget = Vector3.Distance(transform.position, target.position);

        Vector3 lookDir = (target.position - transform.position);
        lookDir.y = 0f;

        if (rotateTowardsMoveDirection && velocity.sqrMagnitude > 0.01f)
        {
            Quaternion targetRot = Quaternion.LookRotation(velocity.normalized, Vector3.up);
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRot, Time.fixedDeltaTime * 10f));
        }

        if (attackTimer > 0f)
            attackTimer -= Time.fixedDeltaTime;

        if (distanceToTarget > attackRange)
        {
            inputDir = lookDir.normalized;
            // Wyznaczenie docelowej prędkości
            Vector3 targetVelocity = inputDir * moveSpeed;
            velocity = Vector3.Lerp(velocity, targetVelocity, Time.fixedDeltaTime * (inputDir.magnitude > 0 ? acceleration : deceleration));

            // Ruch
            rb.MovePosition(rb.position + velocity * Time.fixedDeltaTime);

            // Rotacja w kierunku ruchu
            animator.SetFloat("Velocity", velocity.magnitude, 0.1f, Time.deltaTime);
            animator.ResetTrigger("Melee1");
            animator.ResetTrigger("Melee2");
        }
        else
        {
            velocity = Vector3.Lerp(velocity, Vector3.zero, Time.fixedDeltaTime * deceleration);
            rb.MovePosition(rb.position + velocity * Time.fixedDeltaTime);
            animator.SetFloat("Velocity", 0f);

            if (attackTimer <= 0f)
            {
                Attack();
                attackTimer = attackCooldown;
            }
        }
    }

    private void Attack()
    {
        int attackIndex = UnityEngine.Random.Range(0, 2);
        string triggerName = attackIndex == 0 ? "Melee1" : "Melee2";

        animator.ResetTrigger("Melee1");
        animator.ResetTrigger("Melee2");

        animator.SetTrigger(triggerName);

        // TODO logika napierdalania playera po ryju
    }
}
