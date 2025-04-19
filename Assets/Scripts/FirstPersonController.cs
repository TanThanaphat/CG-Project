using UnityEngine;

public class FirstPersonController : MonoBehaviour
{
    public float mouseSensitivity = 3f;
    public float maxLookAngleX = 60f;
    public float maxLookAngleY = 160f;

    private float xRotation = 0f;
    private float yRotation = 0f;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        yRotation += mouseX;
        xRotation -= mouseY;

        xRotation = Mathf.Clamp(xRotation, -maxLookAngleX, maxLookAngleX);
        yRotation = Mathf.Clamp(yRotation, -maxLookAngleY, maxLookAngleY);

        transform.localRotation = Quaternion.Euler(xRotation, yRotation, 0f);
    }
}
