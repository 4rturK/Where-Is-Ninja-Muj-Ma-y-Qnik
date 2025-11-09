using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class EnemyRangeController : MonoBehaviour
{
    [Header("Ustawienia ruchu")]
    public float moveSpeed = 3f;
    public float acceleration = 5f;
    public float deceleration = 8f;
    public bool rotateTowardsMoveDirection = true;

    [Header("Ustawienia ataku")]
    public float attackRange = 2f;  
    //public float attackCooldown = 1.5f;

    [Header("Crossbow Settings")]
    public GameObject crossbowArrowPrefab;
    public Transform firePoint;
    public float reloadTime = 3f;
    public float arrowForce = 100f;

    [Header("Komponenty")]
    public Animator animator;

    private CheckForPlayer checkForPlayer;
    private Vector3 inputDir;
    private Vector3 velocity;
    private Rigidbody rb;
    private float attackTimer = 0f;
    private EnemyLife enemyLife;

    private bool isMovementLocked = false;
    private float movementLockTimer = 0f;


    private void Awake()
    {
        checkForPlayer = GetComponent<CheckForPlayer>();
        rb = GetComponent<Rigidbody>();
        enemyLife = GetComponent<EnemyLife>();
    }

    private void FixedUpdate()
    {
        if (isMovementLocked)
        {
            movementLockTimer -= Time.fixedDeltaTime;

            //velocity = Vector3.zero;
            rb.MovePosition(rb.position);
            animator.SetFloat("Velocity", 0f);

            if (movementLockTimer <= 0f)
                isMovementLocked = false;

            return;
        }
        else
        {
            if (!enemyLife.isAlive())
            {
                return;
            }
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
                // Wyznaczenie docelowej prêdkoœci
                Vector3 targetVelocity = inputDir * moveSpeed;
                velocity = Vector3.Lerp(velocity, targetVelocity, Time.fixedDeltaTime * (inputDir.magnitude > 0 ? acceleration : deceleration));

                // Ruch
                rb.MovePosition(rb.position + velocity * Time.fixedDeltaTime);

                // Rotacja w kierunku ruchu
                //animator.SetFloat("Velocity", velocity.magnitude, 0.1f, Time.deltaTime);
                animator.SetBool("IsAiming", false);
            }
            else
            {
                velocity = Vector3.Lerp(velocity, Vector3.zero, Time.fixedDeltaTime * deceleration);
                rb.MovePosition(rb.position + velocity * Time.fixedDeltaTime);
                animator.SetBool("IsAiming", true);

                //Vector3 forward = transform.forward;
                Vector3 toTarget = (target.position - transform.position).normalized;

                Quaternion lookRot = Quaternion.LookRotation(toTarget, Vector3.up);

                Quaternion offset = Quaternion.Euler(0f, 36f, 0f);

                Quaternion finalRot = lookRot * offset;
                rb.MoveRotation(Quaternion.Slerp(rb.rotation, finalRot, Time.fixedDeltaTime * 10000000f));


                Vector3 newForward = rb.rotation * Vector3.forward;
                float dot = Vector3.Dot(newForward, toTarget);

                bool isFacingTarget = dot > 0.8f;

                if (isFacingTarget && attackTimer <= 0f)
                {
                    UnityEngine.Debug.Log($"attack");
                    Attack();
                    attackTimer = reloadTime;
                }
                UnityEngine.Debug.Log($"Dist={distanceToTarget:F2} isFacingTarget={isFacingTarget} attacktimer={attackTimer}");
            }

            animator.SetFloat("Velocity", velocity.magnitude, 0.1f, Time.deltaTime);

        }
    }

    private void Attack()
    {
        isMovementLocked = true;
        movementLockTimer = 2f;

        animator.ResetTrigger("Reload");
        animator.SetBool("IsAiming", false);

        animator.SetTrigger("Reload");

        if (crossbowArrowPrefab != null && firePoint != null)
        {
            GameObject arrow = UnityEngine.Object.Instantiate(crossbowArrowPrefab, firePoint.position, firePoint.rotation);
            Rigidbody rb = arrow.GetComponent<Rigidbody>();
            if (rb != null)
                rb.AddRelativeForce(Vector3.forward * arrowForce, ForceMode.Impulse);
        }
    }
}
