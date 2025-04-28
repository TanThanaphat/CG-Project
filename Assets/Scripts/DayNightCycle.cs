using UnityEngine;

public class DayNightCycle : MonoBehaviour
{
    private Vector3 rotation = Vector3.zero;
    private static float degreeRate = 3;

    // Update is called once per frame
    void Update()
    {
        rotation.x = degreeRate*Time.deltaTime;
        transform.Rotate(rotation, Space.World);
    }
}
