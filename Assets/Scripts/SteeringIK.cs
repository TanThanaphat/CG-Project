using UnityEngine;

public class SteeringIK : MonoBehaviour
{
    public Transform leftHandTarget;
    public Transform rightHandTarget;

    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    void OnAnimatorIK(int layerIndex)
    {
        Debug.Log("IK running!");
        if (animator)
        {
            // Lean the body forward
            animator.bodyPosition += transform.forward * 0.1f; // slouch forward slightly
            animator.bodyRotation = Quaternion.Euler(7f, 0, 0) * animator.bodyRotation; // tilt forward

            // Set IK weights
            animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 1);
            animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, 1);
            animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 1);
            animator.SetIKRotationWeight(AvatarIKGoal.RightHand, 1);

            // Apply target transforms
            animator.SetIKPosition(AvatarIKGoal.LeftHand, leftHandTarget.position);
            animator.SetIKRotation(AvatarIKGoal.LeftHand, leftHandTarget.rotation);
            animator.SetIKPosition(AvatarIKGoal.RightHand, rightHandTarget.position);
            animator.SetIKRotation(AvatarIKGoal.RightHand, rightHandTarget.rotation);
        }
    }
}
