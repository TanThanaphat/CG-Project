using UnityEngine;
using UnityEngine.AI;

public class NPCRagdoll : MonoBehaviour
{
    private Rigidbody[] ragdollBodies;
    private Animator animator;
    private NavMeshAgent agent;
    private CapsuleCollider mainCollider;
    private MonoBehaviour NPCController;

    private Vector3 carLinearVelocity;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        NPCController = transform.parent.GetComponent<MonoBehaviour>();

        ragdollBodies = transform.parent.GetComponentsInChildren<Rigidbody>();

        animator = GetComponentInParent<Animator>();
        agent = GetComponentInParent<NavMeshAgent>();

        mainCollider = GetComponent<CapsuleCollider>();

        SetUpRagdoll();
        SetRagdollState(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("NPC Ragdoll");
        //Debug.Log("Trigger entered with: " + other.gameObject.name);
        if (other.gameObject.CompareTag("Car"))
        {
            Debug.Log("NPC just got hit by a car!");

            carLinearVelocity = other.gameObject.transform.parent.parent.GetComponent<Rigidbody>().linearVelocity;

            Die();
        }
    }
    void SetUpRagdoll()
    {
        foreach (Rigidbody rb in ragdollBodies)
        {
            rb.mass = 10f; // Set mass for all ragdoll parts
        }
    }

    void SetRagdollState(bool state)
    {
        foreach (Rigidbody rb in ragdollBodies)
        {
            rb.isKinematic = !state;
            rb.detectCollisions = state;
            rb.AddForce((carLinearVelocity + Vector3.up * 30f) * 1.2f, ForceMode.Impulse);

            if (state)
            {
                rb.linearDamping = 2f;
                rb.angularDamping = 1.5f;
            }
        }
    }

    void Die()
    {
        NPCController.enabled = false;
        animator.enabled = false;
        agent.enabled = false;
        mainCollider.enabled = false;

        SetRagdollState(true);
    }

}
