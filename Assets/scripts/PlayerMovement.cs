using System.Diagnostics;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement3D : MonoBehaviour
{
    [Header("Ustawienia ruchu")]
    public float moveSpeed = 5f;
    public float acceleration = 10f;
    public float deceleration = 8f;
    public bool rotateTowardsMoveDirection = true;

    public Animator animator;

    private Rigidbody rb;
    private Vector3 inputDir;
    private Vector3 velocity;

    private Transform spine;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        spine = animator.transform.Find("Armature/mixamorig:Hips/mixamorig:Spine/mixamorig:Spine2");
        var testSpine = animator.GetBoneTransform(HumanBodyBones.UpperChest);
        UnityEngine.Debug.Log($"UpperChest: {(testSpine ? testSpine.name : "NULL")}");
    }

    private void Update()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        inputDir = new Vector3(h, 0f, v).normalized;

        Vector3 localVel = transform.InverseTransformDirection(velocity);
        Vector3 target = localVel.normalized * Mathf.Clamp01(localVel.magnitude / moveSpeed);

        float currentX = animator.GetFloat("MoveX");
        float currentZ = animator.GetFloat("MoveZ");

        float smooth = 10f;
        float newX = Mathf.Lerp(currentX, target.x, Time.deltaTime * smooth);
        float newZ = Mathf.Lerp(currentZ, target.z, Time.deltaTime * smooth);

        if (velocity.magnitude < 0.1f)
        {
            newX = 0f;
            newZ = 0f;
        }

        animator.SetFloat("MoveX", newX);
        animator.SetFloat("MoveZ", newZ);
    }

    private void FixedUpdate()
    {

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

    void LateUpdate()
    {
        Transform spine = animator.GetBoneTransform(HumanBodyBones.UpperChest);
        if (spine == null) return;

        float moveX = animator.GetFloat("MoveX");

        float tiltY = Mathf.Lerp(0f, 40f, Mathf.Abs(moveX));
        Quaternion offset = Quaternion.Euler(0f, -tiltY, tiltY/3);

        Quaternion animatedRotation = spine.localRotation;

        spine.localRotation = animatedRotation * offset;
    }
}
