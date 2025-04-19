using UnityEngine;

public class IKDebug : MonoBehaviour
{
    Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
        Debug.Log("Animator on: " + gameObject.name);
    }

    void OnAnimatorIK(int layerIndex)
    {
        Debug.Log("IK running! Layer: " + layerIndex);
    }
}
