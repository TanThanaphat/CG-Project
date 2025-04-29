using UnityEngine;

public class CarController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float maxSpeed = 20f;
    public float acceleration = 2f;
    public float deceleration = 8f;
    public float brakeForce = 15f;
    public float turnSpeed = 100f;
    public float reverseSpeedMultiplier = 0.5f;

    [Header("Gear D'creep speed")]
    public float autoDriveSpeed = 1f;

    [Header("Steering Wheel Settings")]
    public Transform steeringWheel;
    public float steeringWheelMaxAngle = 45f;
    public float steeringReturnSpeed = 90f;

    [Header("Car Sound Effects")]
    public AudioSource engineSound;
    public AudioSource driveSound;
    public AudioSource brakeSound;

    [Header("References")]
    public Rigidbody rb;

    private float currentSpeed = 0f;
    private float moveInput;
    public float steerInput;
    private float currentSteerInput;
    private bool isBraking = false;

    private bool isEngineOn = false;
    private bool engineJustStarted = false; 

    private Quaternion initialSteeringRotation;

    public enum GearMode { P, R, N, D }
    public GearMode currentGear = GearMode.P;
    public GearMode CurrentGear => currentGear;

    public float CurrentSpeed => rb != null ? rb.linearVelocity.magnitude * 3.6f : 0f;

    private bool canShift = true;

    void Start()
    {
        if (steeringWheel != null)
            initialSteeringRotation = steeringWheel.localRotation;

        if (rb == null)
            rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        if (PauseMenu.GameIsPaused) {
            engineSound?.Stop();
            driveSound?.Stop();
            brakeSound?.Stop();
            return;
        }

        HandleInput();
        UpdateSteeringWheel();
        UpdateEngineAndDriveSounds();
    }

    // Handle input for gear shifting, movement, and braking    
    void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            isEngineOn = !isEngineOn;
            Debug.Log(isEngineOn ? "Engine Started!" : "Engine Stopped!");

            if (isEngineOn)
            {
                engineJustStarted = true; 
                Invoke(nameof(PlayEngineSound), 0.5f); 
            }
            else
            {
                engineSound?.Stop();
                driveSound?.Stop();
                brakeSound?.Stop();
            }
        }

        if (!isEngineOn) return;

        if (Mathf.Abs(CurrentSpeed) < 0.1f && canShift)
        {
            if (Input.GetKeyDown(KeyCode.DownArrow))
                ShiftGearUp();
            if (Input.GetKeyDown(KeyCode.UpArrow))
                ShiftGearDown();
        }

        isBraking = Input.GetKey(KeyCode.Space);

        steerInput = 0f;
        if (Input.GetKey(KeyCode.A)) steerInput = -1f;
        if (Input.GetKey(KeyCode.D)) steerInput = 1f;

        moveInput = 0f;
        if (currentGear == GearMode.D && Input.GetKey(KeyCode.W)) moveInput = 1f;
        if (currentGear == GearMode.R && Input.GetKey(KeyCode.W)) moveInput = -1f;
    }

    // Shift gears based on input       
    void ShiftGearUp()
    {
        canShift = false;
        if (currentGear == GearMode.P) currentGear = GearMode.R;
        else if (currentGear == GearMode.R) currentGear = GearMode.N;
        else if (currentGear == GearMode.N) currentGear = GearMode.D;
        Invoke(nameof(ResetShift), 0.2f);
    }

    void ShiftGearDown()
    {
        canShift = false;
        if (currentGear == GearMode.D) currentGear = GearMode.N;
        else if (currentGear == GearMode.N) currentGear = GearMode.R;
        else if (currentGear == GearMode.R) currentGear = GearMode.P;
        Invoke(nameof(ResetShift), 0.2f);
    }

    void ResetShift()
    {
        canShift = true;
    }
    // Update the steering wheel rotation based on input  
    void UpdateSteeringWheel()
    {
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
        if (rb == null || !isEngineOn) return;

        if (currentGear == GearMode.P || currentGear == GearMode.N)
        {
            rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);
            currentSpeed = 0f;
            return;
        }

        HandleMovement();
        HandleSteering();
        ApplyBraking();
    }

    // Handle car movement and physics    
    void HandleMovement()
    {
        float targetSpeed = 0f;

        if (moveInput > 0.1f || moveInput < -0.1f)
        {
            targetSpeed = moveInput * maxSpeed;
            if (currentGear == GearMode.R)
                targetSpeed *= reverseSpeedMultiplier;
        }
        else
        {
            if (currentGear == GearMode.D)
                targetSpeed = autoDriveSpeed;
        }

        if (!isBraking)
        {
            if (Mathf.Abs(targetSpeed) > Mathf.Abs(currentSpeed))
                currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, acceleration * Time.fixedDeltaTime);
            else
                currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, deceleration * Time.fixedDeltaTime);
        }

        Vector3 forwardMovement = transform.forward * currentSpeed;
        rb.linearVelocity = new Vector3(forwardMovement.x, rb.linearVelocity.y, forwardMovement.z);
    }

    // Handle steering based on input and speed
    void HandleSteering()
    {
        if (Mathf.Abs(currentSpeed) > 0.1f)
        {
            float direction = currentSpeed > 0 ? 1f : -1f;
            float turn = steerInput * turnSpeed * Time.fixedDeltaTime * direction;
            Quaternion turnRotation = Quaternion.Euler(0f, turn, 0f);
            rb.MoveRotation(rb.rotation * turnRotation);
        }
    }

    // Apply braking force to the car
    void ApplyBraking()
    {
        if (isBraking)
        {
            currentSpeed = Mathf.MoveTowards(currentSpeed, 0f, brakeForce * Time.fixedDeltaTime);
            rb.AddForce(-rb.linearVelocity * brakeForce * 0.5f, ForceMode.Acceleration);

            if (brakeSound != null && !brakeSound.isPlaying)
                brakeSound.Play();
        }
        else
        {
            if (brakeSound != null && brakeSound.isPlaying)
                brakeSound.Stop();
        }

        if (Mathf.Abs(CurrentSpeed) < 0.5f)
        {
            if (brakeSound != null && brakeSound.isPlaying)
                brakeSound.Stop();
        }
    }

    // Update engine and drive sounds based on car state
    void UpdateEngineAndDriveSounds()
    {
        if (!isEngineOn)
        {
            if (engineSound != null && engineSound.isPlaying) engineSound.Stop();
            if (driveSound != null && driveSound.isPlaying) driveSound.Stop();
            return;
        }

        bool isMoving = Mathf.Abs(CurrentSpeed) > 0.5f;

        // ถ้าอยู่ในโหมดถอยหลัง
        if (currentGear == GearMode.R)
        {
            if (!engineSound.isPlaying)
                engineSound.Play();
        }
        else if (isMoving)
        {
            if (engineSound != null && engineSound.isPlaying)
                engineSound.Stop();

            if (driveSound != null && !driveSound.isPlaying)
                driveSound.Play();
        }
        else
        {
            if (driveSound != null && driveSound.isPlaying)
                driveSound.Stop();

            if (engineSound != null && !engineSound.isPlaying && !engineJustStarted)
                engineSound.Play();
        }
    }

    // Play the engine sound after a delay when starting the engine
    void PlayEngineSound()
    {
        if (engineSound != null && !engineSound.isPlaying)
        {
            engineSound.Play();
        }
        engineJustStarted = false; 
    }
}
