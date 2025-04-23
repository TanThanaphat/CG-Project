using UnityEngine;

public class CarController : MonoBehaviour
{
    public float acceleration = 5f;              // ความเร่ง
    public float maxSpeed = 17f;                  // ความเร็วสูงสุด (m/s)
    public float turnSpeed = 100f;                // ความเร็วหมุนรถ
    public float brakeStrength = 50f;             // ความแรงเบรก
    public Transform steeringWheel;               // พวงมาลัย
    public float steeringWheelMaxAngle = 45f;     // องศาหมุนพวงมาลัยสูงสุด
    public float steeringReturnSpeed = 90f;       // ความเร็วที่พวงมาลัยคืนศูนย์
    public Rigidbody rb;                          // Rigidbody

    private float moveInput;
    private float steerInput;
    private float currentSteerInput;
    private float currentSpeed;

    private Quaternion initialSteeringRotation;

    public enum GearMode { P, D, R }
    public GearMode currentGear = GearMode.D;
    public GearMode CurrentGear => currentGear;

    public float CurrentSpeed => rb != null ? rb.linearVelocity.magnitude * 3.6f : 0f; // แสดง km/h

    void Start()
    {
        if (steeringWheel != null)
        {
            initialSteeringRotation = steeringWheel.localRotation;
        }

    }

    void Update()
    {
        moveInput = 0f;
        steerInput = 0f;

        // ระบบเกียร์
        if (Input.GetKeyDown(KeyCode.P)) currentGear = GearMode.P;
        if (Input.GetKeyDown(KeyCode.D)) currentGear = GearMode.D;
        if (Input.GetKeyDown(KeyCode.R)) currentGear = GearMode.R;

        // พวงมาลัยซ้ายขวา
        if (Input.GetKey(KeyCode.LeftArrow)) steerInput = -1f;
        if (Input.GetKey(KeyCode.RightArrow)) steerInput = 1f;

        // อินพุตการเคลื่อนที่
        if (currentGear == GearMode.D && Input.GetKey(KeyCode.UpArrow)) moveInput = 1f;
        if (currentGear == GearMode.R && Input.GetKey(KeyCode.DownArrow)) moveInput = -1f;

        // พวงมาลัยสมูท
        currentSteerInput = Mathf.MoveTowards(currentSteerInput, steerInput, steeringReturnSpeed * Time.deltaTime / steeringWheelMaxAngle);

        // หมุนพวงมาลัยโมเดล
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

        // เบรก
        if (Input.GetKey(KeyCode.Space))
        {
            rb.linearVelocity = Vector3.MoveTowards(rb.linearVelocity, Vector3.zero, brakeStrength * Time.fixedDeltaTime);
            return;
        }

        // เร่งความเร็ว
        Vector3 desiredVelocity = transform.forward * moveInput * maxSpeed;
        Vector3 velocityChange = desiredVelocity - rb.linearVelocity;
        Vector3 accelerationForce = Vector3.ClampMagnitude(velocityChange, acceleration) * rb.mass;

        rb.AddForce(accelerationForce, ForceMode.Force);

        // หมุนรถ (ถ้ามีการเคลื่อนที่)
        if (Mathf.Abs(rb.linearVelocity.magnitude) > 0.1f)
        {
            float direction = Vector3.Dot(rb.linearVelocity, transform.forward) >= 0 ? 1f : -1f;
            float turn = steerInput * turnSpeed * Time.fixedDeltaTime * direction;
            Quaternion turnRotation = Quaternion.Euler(0f, turn, 0f);
            rb.MoveRotation(rb.rotation * turnRotation);
        }
    }
}
