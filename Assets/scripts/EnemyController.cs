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

    private CheckForPlayer checkForPlayer;
    private Vector3 inputDir;
    private Vector3 velocity;
    private Rigidbody rb;
    private void Awake()
    {
        checkForPlayer = GetComponent<CheckForPlayer>();
        rb = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        if (checkForPlayer.isTargetVisible)
        {
            inputDir = new Vector3(
                checkForPlayer.targetGameObject.transform.position.x - transform.position.x, 
                0f,
                checkForPlayer.targetGameObject.transform.position.z - transform.position.z
                ).normalized;
            // Wyznaczenie docelowej prêdkoœci
            Vector3 targetVelocity = inputDir * moveSpeed;
            velocity = Vector3.Lerp(velocity, targetVelocity, Time.fixedDeltaTime * (inputDir.magnitude > 0 ? acceleration : deceleration));

            // Ruch
            rb.MovePosition(rb.position + velocity * Time.fixedDeltaTime);

            // Rotacja w kierunku ruchu
            if (rotateTowardsMoveDirection && velocity.sqrMagnitude > 0.01f)
            {
                Quaternion targetRot = Quaternion.LookRotation(velocity.normalized, Vector3.up);
                rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRot, Time.fixedDeltaTime * 10f));
            }
        }
    }
}
