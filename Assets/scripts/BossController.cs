using System.Collections;
using System.Collections.Generic;
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

    private float aimingTimer = 0f;


    void Start()
    {
        checkForPlayer = GetComponent<CheckForPlayer>();
        rb = GetComponent<Rigidbody>();
        enemyLife = GetComponent<EnemyLife>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        Debug.Log(state.ToString());
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

        switch (state)
        {
            case BossState.Sleep:
                if (checkForPlayer.isTargetVisible)
                {
                    state = BossState.Aiming;
                }
                break;

            case BossState.Running:
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
}
