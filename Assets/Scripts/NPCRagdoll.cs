using UnityEngine;
using UnityEngine.AI;

public class NPCRagdoll : MonoBehaviour
{
    private Rigidbody[] ragdollBodies;
    private Animator animator;
    private NavMeshAgent agent;
    private CapsuleCollider mainCollider;
    private MonoBehaviour NPCController;

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
        Debug.Log("Trigger entered with: " + other.gameObject.name);
        if (other.gameObject.CompareTag("Car"))
        {
            Debug.Log("NPC just got hit by a car!");
            Die();
        }
    }
    void SetUpRagdoll()
    {
        foreach (Rigidbody rb in ragdollBodies)
        {
            rb.mass = 4f; // Set mass for all ragdoll parts
        }
    }

    void SetRagdollState(bool state)
    {
        foreach (Rigidbody rb in ragdollBodies)
        {
            rb.isKinematic = !state;
            rb.detectCollisions = state;
            rb.AddForce(Vector3.up * 50f, ForceMode.Impulse);

            if (state)
            {
                rb.linearDamping = 2f;
                rb.angularDamping = 0.75f;
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
