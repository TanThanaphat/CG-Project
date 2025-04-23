using UnityEngine;

public class CarController : MonoBehaviour
{
    public float moveSpeed = 10f;          // ความเร็วเดินหน้า/ถอยหลัง
    public float turnSpeed = 100f;         // ความเร็วหมุนรถ
    public Transform steeringWheel;        // พวงมาลัย (หมุนเฉพาะโมเดล)
    public float steeringWheelMaxAngle = 45f; // องศาหมุนพวงมาลัย
    public Rigidbody rb;                   // Rigidbody ของรถ

    private float moveInput;
    private float steerInput;

    private Quaternion initialSteeringRotation;  // เก็บค่าเริ่มต้นของพวงมาลัย

    void Start()
    {
        if (steeringWheel != null)
        {
            // เก็บค่า rotation เริ่มต้นของพวงมาลัย
            initialSteeringRotation = steeringWheel.localRotation;
        }
    }

    void Update()
    {
        // รับอินพุตจากคีย์บอร์ด
        moveInput = Input.GetAxis("Vertical");   // W/S หรือ ↑/↓
        steerInput = Input.GetAxis("Horizontal"); // A/D หรือ ←/→

        // หมุนพวงมาลัย (โมเดลเท่านั้น)
        if (steeringWheel != null)
        {
            float steerAngle = steerInput * steeringWheelMaxAngle;

            // คูณการหมุนเพิ่มจากค่าเริ่มต้น
            steeringWheel.localRotation = initialSteeringRotation * Quaternion.Euler(0f, steerAngle, 0f);
        }
    }

    void FixedUpdate()
    {
        if (rb == null) return;

        // เคลื่อนที่ไปข้างหน้า/ถอยหลัง
        Vector3 forwardMovement = transform.forward * moveInput * moveSpeed;
        rb.linearVelocity = new Vector3(forwardMovement.x, rb.linearVelocity.y, forwardMovement.z); // รักษาแรงตก

        // หมุนตัวรถ (เฉพาะตอนมีการเคลื่อนที่)
        if (Mathf.Abs(moveInput) > 0.1f)
        {
            float turn = steerInput * turnSpeed * Time.fixedDeltaTime;
            Quaternion turnRotation = Quaternion.Euler(0f, turn, 0f);
            rb.MoveRotation(rb.rotation * turnRotation);
        }
    }
}
