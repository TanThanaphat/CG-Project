using UnityEngine;
using UnityEngine.AI;

public class NPCController : MonoBehaviour
{
    private NavMeshAgent agent;
    private Animator animator;

    public float randomMoveRadius = 5f;
    public float moveInterval = 10f;
    private float moveTimer;

    public float turnAngleThreshold = 75f;
    public float turnDuration = 2f;

    private bool isTurning = false;
    private float turnTimer = 0f;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        moveTimer = moveInterval;
        MoveToNewDestination();
    }

    void Update()
    {
        if (isTurning)
        {
            turnTimer -= Time.deltaTime;
            if (turnTimer <= 0f)
            {
                ResumeMovement();
            }

            // Freeze speed animation during turn
            animator.SetFloat("Speed", 0f);
            return;
        }

        // Calculate direction to next point
        Vector3 desiredDir = agent.desiredVelocity.normalized;
        float angle = Vector3.Angle(transform.forward, desiredDir);

        // If angle is sharp and we're not turning already
        if (angle > turnAngleThreshold && agent.velocity.magnitude > 0.1f)
        {
            Debug.Log("Turning: " + angle);
            Debug.Log(Mathf.Min(1, angle / 90));
            turnTimer = turnDuration * Mathf.Min(1, angle / 90);
            BeginTurn(desiredDir);
            return;
        }

        // Move animation based on speed
        animator.SetFloat("Speed", agent.velocity.magnitude);

        // Move to target waypoint
        if (agent.remainingDistance < 0.5f && !agent.pathPending)
        {
            moveTimer -= Time.deltaTime;
            if (moveTimer <= 0f)
            {
                MoveToNewDestination();
                moveTimer = moveInterval;
            }
        }
    }

    private void MoveToNewDestination()
    {
        Vector3 randomDirection = Random.insideUnitSphere * randomMoveRadius;
        randomDirection += transform.position;

        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomDirection, out hit, randomMoveRadius, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }
    }

    void BeginTurn(Vector3 desiredDir)
    {
        isTurning = true;
        agent.isStopped = true;

        animator.SetFloat("Speed", 0f); // force stop walk animation

        // Choose turn direction based on cross product
        Vector3 cross = Vector3.Cross(transform.forward, desiredDir);
        if (cross.y < 0)
        {
            animator.ResetTrigger("TurnRight");
            animator.SetTrigger("TurnLeft");
        }
        else
        {
            animator.ResetTrigger("TurnLeft");
            animator.SetTrigger("TurnRight");
        }
    }

    void ResumeMovement()
    {
        isTurning = false;
        agent.isStopped = false;
    }
}
