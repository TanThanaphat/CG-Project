using UnityEngine;

public class CarController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float maxSpeed = 30f;                 // ความเร็วสูงสุด
    public float acceleration = 5f;              // อัตราการเร่ง
    public float deceleration = 8f;              // อัตราการชะลอ
    public float brakeForce = 15f;               // แรงเบรค
    public float turnSpeed = 100f;               // ความเร็วหมุนรถ
    public float reverseSpeedMultiplier = 0.5f;  // ความเร็วเมื่อถอยหลัง

    [Header("Steering Wheel Settings")]
    public Transform steeringWheel;              // พวงมาลัย
    public float steeringWheelMaxAngle = 45f;    // องศาหมุนพวงมาลัยสูงสุด
    public float steeringReturnSpeed = 90f;      // ความเร็วที่พวงมาลัยคืนศูนย์

    [Header("References")]
    public Rigidbody rb;

    private float currentSpeed = 0f;
    private float moveInput;
    private float steerInput;
    private float currentSteerInput;
    private bool isBraking = false;

    private Quaternion initialSteeringRotation;  // ค่าเริ่มต้นของพวงมาลัย

    public enum GearMode { P, D, R }
    public GearMode currentGear = GearMode.P;
    public GearMode CurrentGear
    {
        get { return currentGear; }
    }

    public float CurrentSpeed => rb != null ? rb.linearVelocity.magnitude * 3.6f : 0f;

    void Start()
    {
        if (steeringWheel != null)
        {
            initialSteeringRotation = steeringWheel.localRotation;
        }

        if (rb == null)
        {
            rb = GetComponent<Rigidbody>();
        }
    }

    void Update()
    {
        HandleInput();
        UpdateSteeringWheel();
    }

    void HandleInput()
    {
        // ระบบเกียร์
        if (Input.GetKeyDown(KeyCode.P)) currentGear = GearMode.P;
        if (Input.GetKeyDown(KeyCode.D)) currentGear = GearMode.D;
        if (Input.GetKeyDown(KeyCode.R)) currentGear = GearMode.R;

        // ระบบเบรค
        isBraking = Input.GetKey(KeyCode.Space);

        steerInput = 0f;
        if (Input.GetKey(KeyCode.LeftArrow)) steerInput = -1f;
        if (Input.GetKey(KeyCode.RightArrow)) steerInput = 1f;

        moveInput = 0f;
        if (currentGear == GearMode.D && Input.GetKey(KeyCode.UpArrow)) moveInput = 1f;
        if (currentGear == GearMode.R && Input.GetKey(KeyCode.DownArrow)) moveInput = -1f;
    }

    void UpdateSteeringWheel()
    {
        // smoothly wheel
        currentSteerInput = Mathf.MoveTowards(currentSteerInput, steerInput,
            steeringReturnSpeed * Time.deltaTime / steeringWheelMaxAngle);

        if (steeringWheel != null)
        {
            float steerAngle = currentSteerInput * steeringWheelMaxAngle;
            steeringWheel.localRotation = initialSteeringRotation * Quaternion.Euler(0f, steerAngle, 0f);
        }
    }

    void FixedUpdate()
    {
        if (rb == null) return;
        if (currentGear == GearMode.P) return;

        HandleMovement();
        HandleSteering();
        ApplyBraking();
    }

    void HandleMovement()
    {
        float targetSpeed = 0f;

        // คำนวณความเร็วเป้าหมาย
        if (moveInput > 0.1f || moveInput < -0.1f)
        {
            targetSpeed = moveInput * maxSpeed;
            if (currentGear == GearMode.R)
            {
                targetSpeed *= reverseSpeedMultiplier;
            }
        }

        // Accelerate or decelerate
        if (!isBraking)
        {
            if (Mathf.Abs(targetSpeed) > Mathf.Abs(currentSpeed))
            {
                // เร่งความเร็ว
                currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, acceleration * Time.fixedDeltaTime);
            }
            else
            {
                // ชะลอความเร็วเมื่อปล่อยปุ่ม
                currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, deceleration * Time.fixedDeltaTime);
            }
        }

        Vector3 forwardMovement = transform.forward * currentSpeed;
        rb.linearVelocity = new Vector3(forwardMovement.x, rb.linearVelocity.y, forwardMovement.z);
    }

    void HandleSteering()
    {
        // หมุนรถเมื่อมีความเร็ว
        if (Mathf.Abs(currentSpeed) > 0.1f)
        {
            float direction = currentSpeed > 0 ? 1f : -1f;
            float turn = steerInput * turnSpeed * Time.fixedDeltaTime * direction;
            Quaternion turnRotation = Quaternion.Euler(0f, turn, 0f);
            rb.MoveRotation(rb.rotation * turnRotation);
        }
    }

    void ApplyBraking()
    {
        if (isBraking)
        {
            // ลดความเร็ว
            currentSpeed = Mathf.MoveTowards(currentSpeed, 0f, brakeForce * Time.fixedDeltaTime);

            // เพิ่มแรงต้านตอนเบรค 
            rb.AddForce(-rb.linearVelocity * brakeForce * 0.5f, ForceMode.Acceleration);
        }
    }
}