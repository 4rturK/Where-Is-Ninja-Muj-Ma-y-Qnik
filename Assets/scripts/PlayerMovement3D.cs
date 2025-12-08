using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement3D : MonoBehaviour
{
    public enum ControlMode { Normal, Voice }

    [Header("Tryb sterowania")]
    [SerializeField] private ControlMode controlMode = ControlMode.Normal;
    public ControlMode Mode => controlMode;

    [Header("Ustawienia ruchu (Normal)")]
    public float moveSpeed = 5f;
    public float acceleration = 10f;
    public float deceleration = 8f;
    public bool rotateTowardsMoveDirection = true;

    [Header("Ustawienia ruchu (Voice - krok)")]
    public float stepSpeed = 6f;           // prêdkoœæ wykonywania kroku
    public float maxBufferedStep = 5f;     // maks ile metrów mo¿e siê "zbuforowaæ" z kolejnych wyzwaleñ

    [Header("Animator")]
    public Animator animator;

    private Rigidbody rb;
    private Vector3 velocity;
    public Vector3 inputDir;

    private Transform spineCached;

    private float stepRemaining = 0f;
    private Vector3 stepDir = Vector3.forward;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        if (animator != null)
        {
            spineCached = animator.transform.Find("Armature/mixamorig:Hips/mixamorig:Spine/mixamorig:Spine2");
            var testSpine = animator.GetBoneTransform(HumanBodyBones.UpperChest);
            Debug.Log($"UpperChest: {(testSpine ? testSpine.name : "NULL")}");
        }
        else
        {
            Debug.LogWarning("PlayerMovement3D: Brak przypiêtego Animatora.");
        }
    }

    public void SetControlMode(ControlMode mode)
    {
        controlMode = mode;

        // wyzeruj ruch przy prze³¹czaniu
        velocity = Vector3.zero;
        stepRemaining = 0f;
    }

    /// <summary>Wyzwala krok o zadany dystans (Voice).</summary>
    public void Step(float distance)
    {
        Step(distance, Vector3.zero);
    }

    /// <summary>Wyzwala krok o zadany dystans w zadanym kierunku (Voice).</summary>
    public void Step(float distance, Vector3 preferredDir)
    {
        if (controlMode != ControlMode.Voice) return;

        Vector3 dir = (inputDir.sqrMagnitude > 0.0001f) ? inputDir : transform.forward;
        stepDir = dir.normalized;

        stepRemaining = Mathf.Clamp(stepRemaining + Mathf.Max(0f, distance), 0f, maxBufferedStep);
    }

    private void Update()
    {
        // Input czytamy zawsze — w Voice mo¿e s³u¿yæ jako kierunek kroku
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        inputDir = new Vector3(h, 0f, v).normalized;

        // --- ANIMATOR: zostawiam dok³adnie logikê jak mia³eœ ---
        if (animator == null) return;

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

            if (Input.GetKeyDown(KeyCode.G))
                animator.SetBool("Taunt1", true);
        }
        else
        {
            animator.SetBool("Taunt1", false);
        }

        animator.SetFloat("MoveX", newX);
        animator.SetFloat("MoveZ", newZ);
    }

    private void FixedUpdate()
    {
        if (controlMode == ControlMode.Normal)
        {
            // --- Normal: 1:1 jak wczeœniej ---
            Vector3 targetVelocity = inputDir * moveSpeed;
            velocity = Vector3.Lerp(
                velocity,
                targetVelocity,
                Time.fixedDeltaTime * (inputDir.magnitude > 0 ? acceleration : deceleration)
            );

            rb.MovePosition(rb.position + velocity * Time.fixedDeltaTime);

            if (rotateTowardsMoveDirection && velocity.sqrMagnitude > 0.01f)
            {
                Quaternion targetRot = Quaternion.LookRotation(velocity.normalized, Vector3.up);
                rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRot, Time.fixedDeltaTime * 10f));
            }
        }
        else // Voice
        {
            if (stepRemaining <= 0f)
            {
                // doci¹gnij velocity do zera dla animatora
                velocity = Vector3.Lerp(velocity, Vector3.zero, Time.fixedDeltaTime * deceleration);
                return;
            }

            float moveThisFrame = Mathf.Min(stepSpeed * Time.fixedDeltaTime, stepRemaining);
            Vector3 delta = stepDir * moveThisFrame;

            rb.MovePosition(rb.position + delta);
            stepRemaining -= moveThisFrame;

            // velocity u¿ywane przez animator w Update()
            velocity = delta / Time.fixedDeltaTime;

            if (rotateTowardsMoveDirection && delta.sqrMagnitude > 0.000001f)
            {
                Quaternion targetRot = Quaternion.LookRotation(stepDir, Vector3.up);
                rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRot, Time.fixedDeltaTime * 10f));
            }
        }
    }

    private void LateUpdate()
    {
        if (animator == null) return;

        Transform spine = animator.GetBoneTransform(HumanBodyBones.UpperChest);
        if (spine == null) return;

        float moveX = animator.GetFloat("MoveX");

        float tiltY = Mathf.Lerp(0f, 40f, Mathf.Abs(moveX));
        Quaternion offset = Quaternion.Euler(0f, -tiltY, tiltY / 3f);

        Quaternion animatedRotation = spine.localRotation;
        spine.localRotation = animatedRotation * offset;
    }
}
