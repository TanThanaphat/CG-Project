using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class NPCCarController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float maxSpeed = 7f;
    public float acceleration = 5f;
    public float deceleration = 8f;
    public float brakeForce = 15f;
    public float turnSpeed = 120f;

    [Header("Steering Settings")]
    public Transform steeringWheel;
    public float steeringWheelMaxAngle = 45f;
    public float steeringReturnSpeed = 90f;

    [Header("References")]
    public Rigidbody rb;
    public GameObject roadParents;

    private List<Waypoint> waypointList;
    private List<Transform> traversedWaypointList = new List<Transform>();
    private Transform currentTargetWaypoint;

    private NavMeshAgent agent;

    private float currentSpeed = 0f;
    private float moveInput;
    private float steerInput;
    private float currentSteerInput;
    private bool isBraking = false;
    private bool isReversing = false;
    private float reverseElapsed = 0;

    private Quaternion initialSteeringRotation;

    private NavMeshPath currentPath;
    private int currentPathIndex = 0;
    private float subwayPointThreshold = 10f;

    private float reachThreshold = 10f;
    private float maxAngleInFront;

    [Header("Obstacle Detection Settings")]
    public Transform colliderFront; // Reference to the front collider of the car
    public Transform colliderBody; // Reference to the car's collider body
    public int numberOfRays = 5; // How many rays you want
    public float raySpreadAngle = 30f; // Total spread (degrees)
    public float rayDistance = 7.5f; // How far to detect

    void Start()
    {
        maxAngleInFront = UnityEngine.Random.Range(90f, 120f);
        //Debug.Log("Max Angle in Front: " + maxAngleInFront);

        if (rb == null)
            rb = GetComponent<Rigidbody>();

        if (steeringWheel != null)
            initialSteeringRotation = steeringWheel.localRotation;

        waypointList = new List<Waypoint>(roadParents.GetComponentsInChildren<Waypoint>());
        //Debug.Log("Waypoints found: " + waypointList.Count);

        agent = GetComponent<NavMeshAgent>();
        if (agent != null)
        {
            agent.updatePosition = false;
            agent.updateRotation = false;
        }

        FindClosestWaypoint();
    }

    void Update()
    {
        if (currentTargetWaypoint == null)
            return;

        float distanceToWaypoint = Vector3.Distance(transform.position, currentTargetWaypoint.position);
        if (distanceToWaypoint <= reachThreshold)
        {
            FindClosestWaypoint(); // only find new waypoint when close
        }

        HandleInput();
        UpdateSteeringWheel();
    }

    void FixedUpdate()
    {
        DetectObstacle();
        HandleMovement();
        HandleSteering();
        ApplyBraking();
    }

    float GetTargetEdgeDistance(bool isRight)
    {
        Vector3 origin = transform.position;
        Vector3 edgeDirection = isRight ? transform.right : -transform.right;

        float maxCheckDistance = 30f; // how far to check to the right
        float step = 0.1f; // how small the steps are
        float traveled = 0f;

        for (float d = 0f; d < maxCheckDistance; d += step)
        {
            Vector3 samplePoint = origin + edgeDirection * d;

            if (!IsPointOnNavMesh(samplePoint))
            {
                // We went too far, return distance up to before the fall
                return traveled;
            }

            traveled = d;
        }

        return traveled;
    }

    bool IsPointOnNavMesh(Vector3 point)
    {
        NavMeshHit hit;
        // "1.0f" means look only close nearby vertically (no searching far above/below)
        return NavMesh.SamplePosition(point, out hit, 1.0f, NavMesh.AllAreas);
    }

    void FindClosestWaypoint()
    {
        Transform bestWaypoint = null;
        float bestPathLength = Mathf.Infinity;
        NavMeshPath bestPath = null;

        foreach (var waypoint in waypointList)
        {
            Vector3 localPath = transform.InverseTransformPoint(waypoint.transform.position).normalized;
            float angleToTarget = Mathf.Atan2(localPath.x, localPath.z) * Mathf.Rad2Deg;
            if (Mathf.Abs(angleToTarget) > maxAngleInFront)
                continue; // skip this waypoint if it's too far to the left or right)

            NavMeshPath path = new NavMeshPath();
            if (NavMesh.CalculatePath(transform.position, waypoint.transform.position, NavMesh.AllAreas, path))
            {
                float pathLength = GetPathLength(path);

                if (pathLength < bestPathLength && !traversedWaypointList.Exists(w => w == waypoint.transform))
                {
                    bestPathLength = pathLength;
                    bestWaypoint = waypoint.transform;
                    bestPath = path;
                }
            }
        }

        if (bestWaypoint != null)
        {
            traversedWaypointList.Add(bestWaypoint);
            //Debug.Log("Waypoint count: " + traversedWaypointList.Count);
            currentTargetWaypoint = bestWaypoint;
            currentPath = bestPath;
            currentPathIndex = 0;

            maxAngleInFront = UnityEngine.Random.Range(45f, 100f);
        }
    }

    void DetectObstacle()
    {
        bool resultBraking = false;

        float startAngle = -raySpreadAngle * 0.5f;

        for (int i = 0; i < numberOfRays; i++)
        {
            // Calculate angle for this ray
            float angle = startAngle + (raySpreadAngle / (numberOfRays - 1)) * i;

            // Create the direction by rotating the forward vector
            Vector3 direction = Quaternion.Euler(0, angle, 0) * transform.forward;

            // Ray from collider front
            Ray ray = new Ray(colliderFront.position, direction);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, rayDistance))
            {
                if (hit.collider.CompareTag("TrafficCollider") || hit.collider.CompareTag("CarTraffic"))
                {
                    float angleBetweenCollider = Vector3.Angle(transform.forward, hit.collider.transform.forward);
                    //Debug.Log(hit.collider.name + " " + angleBetweenCollider + " " + hit.collider.transform.forward);
                    if (Mathf.Abs(angleBetweenCollider) <= 75f)
                    {
                        Debug.DrawRay(colliderFront.position, direction * hit.distance, Color.red); // For visualization
                        resultBraking = true;

                        colliderBody.gameObject.tag = "CarTraffic";

                        break;
                    }

                    continue;
                } else
                {
                    colliderBody.gameObject.tag = "Car";
                }

                    Debug.DrawRay(colliderFront.position, direction * hit.distance, Color.red); // For visualization
                //Debug.Log("Detected obstacles in " + transform.name);

                resultBraking = true;

                reverseElapsed = 0;

                if (!DetectObstacleFromBehind())
                    resultBraking = false;

                break; // We can stop checking after first obstacle detected
            }
            else
            {
                Debug.DrawRay(colliderFront.position, direction * rayDistance, Color.green); // For visualization
                resultBraking = false;
            }
        }
        //Debug.Log("Elapsed time: " + reverseElapsed + " in " + transform.name);
        reverseElapsed += Time.fixedDeltaTime;
        if (!resultBraking && reverseElapsed >= 2)
            isReversing = false;

        isBraking = resultBraking;
    }

    bool DetectObstacleFromBehind()
    {
        bool resultReversing = false;

        float startAngle = -raySpreadAngle * 0.5f;

        for (int i = 0; i < numberOfRays; i++)
        {
            // Calculate angle for this ray
            float angle = startAngle + (raySpreadAngle / (numberOfRays - 1)) * i;

            // Create the direction by rotating the forward vector
            Vector3 direction = Quaternion.Euler(0, angle, 0) * -transform.forward;

            // Ray from collider front
            Ray ray = new Ray(colliderFront.position - transform.forward * 2f, direction);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, rayDistance))
            {
                Debug.DrawRay(colliderFront.position, direction * hit.distance, Color.red); // For visualization
                //Debug.Log("Detect obstacles from behind " + hit.collider.name);

                return true; // We can stop checking after first obstacle detected
            }
            else
            {
                Debug.DrawRay(colliderFront.position - transform.forward * 2f, direction * rayDistance, Color.green); // For visualization
                resultReversing = true;
            }
        }

        isReversing = resultReversing;

        return false;
    }

    void HandleInput()
    {
        if (currentTargetWaypoint == null)
        {
            moveInput = 0f;
            steerInput = 0f;
            return;
        }
        //Debug.Log("isReversing: " + isReversing + " in " + transform.name);
        if (!isReversing)
        {
            moveInput = 1f;
            turnSpeed = 105f;
        }
        else
        {
            moveInput = -0.5f;
            turnSpeed = 0;
        }
    }

    void HandleMovement()
    {
        float targetSpeed = moveInput * maxSpeed;

        if (!isBraking)
        {
            if (Mathf.Abs(targetSpeed) > Mathf.Abs(currentSpeed))
                currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, acceleration * Time.fixedDeltaTime);
            else
                currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, deceleration * Time.fixedDeltaTime);
        }

        Vector3 move = transform.forward * currentSpeed;
        rb.linearVelocity = new Vector3(move.x, rb.linearVelocity.y, move.z); // <-- Unity 6 style
    }

    void HandleSteering()
    {
        Vector3 targetSubwaypoint = currentPath.corners[currentPathIndex];
        if (Vector3.Distance(transform.position, targetSubwaypoint) < subwayPointThreshold)
        {
            currentPathIndex++;
            if (currentPathIndex >= currentPath.corners.Count())
            {
                currentPathIndex = currentPath.corners.Count() - 1; // Stay at final point
            }
        }

        if (Mathf.Abs(currentSpeed) > 0.1f)
        {
            Vector3 localTarget = transform.InverseTransformPoint(targetSubwaypoint);
            float angleToTarget = Mathf.Atan2(localTarget.x, localTarget.z) * Mathf.Rad2Deg;
            steerInput = angleToTarget / 45f;

            float distanceToLeftEdge = GetTargetEdgeDistance(false);
            float distanceToRightEdge = GetTargetEdgeDistance(true);
            float totalRoadWidth = distanceToLeftEdge + distanceToRightEdge;
            //Debug.Log("Distance to left edge: " + distanceToLeftEdge);
            //Debug.Log("Distance to right edge: " + distanceToRightEdge);
            //Debug.Log("Total road width: " + totalRoadWidth);
            //Debug.Log("Left Ratio: " + distanceToLeftEdge / totalRoadWidth);
            //Debug.Log("Right Ratio: " + distanceToRightEdge / totalRoadWidth);
            if (totalRoadWidth <= 35f)
            {
                if (distanceToRightEdge / totalRoadWidth > 0.355f)
                {
                    steerInput = distanceToRightEdge / totalRoadWidth;
                    //Debug.Log("Steering right to avoid falling off the road.");
                } else if (distanceToRightEdge / totalRoadWidth <= 0.205f)
                {
                    steerInput = -distanceToLeftEdge / totalRoadWidth;
                    //Debug.Log("Steering left to avoid falling off the road.");
                }
            }

            steerInput = Mathf.Clamp(steerInput, -1f, 1f);

            float direction = currentSpeed > 0 ? 1f : -1f;
            float turnAmount = steerInput * turnSpeed * Time.fixedDeltaTime * direction;
            Quaternion turnRotation = Quaternion.Euler(0f, turnAmount, 0f);
            rb.MoveRotation(rb.rotation * turnRotation);
        }
    }

    void ApplyBraking()
    {
        if (isBraking)
        {
            currentSpeed = Mathf.MoveTowards(currentSpeed, 0f, brakeForce * Time.fixedDeltaTime);
            rb.AddForce(-rb.linearVelocity * brakeForce * 0.5f, ForceMode.Acceleration);
        }
    }

    void UpdateSteeringWheel()
    {
        currentSteerInput = Mathf.MoveTowards(currentSteerInput, steerInput,
            steeringReturnSpeed * Time.deltaTime / steeringWheelMaxAngle);

        if (steeringWheel != null)
        {
            float wheelAngle = currentSteerInput * steeringWheelMaxAngle;
            steeringWheel.localRotation = initialSteeringRotation * Quaternion.Euler(0f, wheelAngle, 0f);
        }
    }

    float GetPathLength(NavMeshPath path)
    {
        float length = 0f;
        if (path.corners.Length < 2) return length;

        for (int i = 0; i < path.corners.Length - 1; i++)
            length += Vector3.Distance(path.corners[i], path.corners[i + 1]);

        return length;
    }

    private void OnDrawGizmos()
    {
        if (currentPath != null && currentPath.corners.Length > 1)
        {
            Gizmos.color = Color.green;
            for (int i = 0; i < currentPath.corners.Length - 1; i++)
            {
                Gizmos.DrawLine(currentPath.corners[i], currentPath.corners[i + 1]);
            }
        }
    }
}
