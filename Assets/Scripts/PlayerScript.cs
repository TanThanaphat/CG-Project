using UnityEngine;

public class PlayerScript : MonoBehaviour
{
    public Rigidbody rb; // Reference to the Rigidbody component
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            // If the space key is pressed, log a message to the console
            Debug.Log("Space key was pressed");
            rb.AddForce(Vector3.up * 5, ForceMode.Impulse); // Add an upward force to the Rigidbody
        }
    }
}
