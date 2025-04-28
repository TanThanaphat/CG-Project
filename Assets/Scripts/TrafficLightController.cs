using UnityEngine;
using System.Collections;

public class TrafficLightController : MonoBehaviour
{
    public enum lightType { R, Y, G }

    private Material[] lightMaterialMap;
    private string[] materialsName = { "R", "RL", "Y", "YL", "G", "GL" };

    public lightType trafficLightStatus1;
    public lightType trafficLightStatus2;

    public float greenRedChangeInterval = 15f;
    public float yellowChangeInterval = 3f;

    private MeshRenderer trafficMeshRenderer;

    public BoxCollider trafficCollider1;
    public BoxCollider trafficCollider2;

    private void Start()
    {
        trafficMeshRenderer = GetComponent<MeshRenderer>();

        lightMaterialMap = new Material[6];
        for (int i = 0; i < 6; i++)
        {
            lightMaterialMap[i] = Resources.Load<Material>("TrafficLights/" + materialsName[i]);
        }

        // Start the traffic light cycle
        StartCoroutine(TrafficLightCycle());
    }

    private IEnumerator TrafficLightCycle()
    {
        while (true)
        {
            // Road 1 Green, Road 2 Red
            trafficLightStatus1 = lightType.G;
            trafficLightStatus2 = lightType.R;
            UpdateLight();
            yield return new WaitForSeconds(greenRedChangeInterval);

            // Road 1 Yellow
            trafficLightStatus1 = lightType.Y;
            trafficLightStatus2 = lightType.R;
            UpdateLight();
            yield return new WaitForSeconds(yellowChangeInterval);

            // Road 1 Red, Road 2 Green
            trafficLightStatus1 = lightType.R;
            trafficLightStatus2 = lightType.G;
            UpdateLight();
            yield return new WaitForSeconds(greenRedChangeInterval);

            // Road 2 Yellow
            trafficLightStatus1 = lightType.R;
            trafficLightStatus2 = lightType.Y;
            UpdateLight();
            yield return new WaitForSeconds(yellowChangeInterval);
        }
    }

    public void UpdateLight()
    {
        Material[] resultMaterials = trafficMeshRenderer.materials;

        // Handle Road 1 (elements 1-3)
        switch (trafficLightStatus1)
        {
            case lightType.R:
                resultMaterials[1] = lightMaterialMap[1];
                resultMaterials[2] = lightMaterialMap[2];
                resultMaterials[3] = lightMaterialMap[4];
                break;
            case lightType.Y:
                resultMaterials[1] = lightMaterialMap[0];
                resultMaterials[2] = lightMaterialMap[3];
                resultMaterials[3] = lightMaterialMap[4];
                break;
            case lightType.G:
                resultMaterials[1] = lightMaterialMap[0];
                resultMaterials[2] = lightMaterialMap[2];
                resultMaterials[3] = lightMaterialMap[5];
                break;
        }

        // Handle Road 2 (elements 4-6)
        switch (trafficLightStatus2)
        {
            case lightType.R:
                resultMaterials[4] = lightMaterialMap[1];
                resultMaterials[5] = lightMaterialMap[2];
                resultMaterials[6] = lightMaterialMap[4];
                break;
            case lightType.Y:
                resultMaterials[4] = lightMaterialMap[0];
                resultMaterials[5] = lightMaterialMap[3];
                resultMaterials[6] = lightMaterialMap[4];
                break;
            case lightType.G:
                resultMaterials[4] = lightMaterialMap[0];
                resultMaterials[5] = lightMaterialMap[2];
                resultMaterials[6] = lightMaterialMap[5];
                break;
        }

        trafficMeshRenderer.materials = resultMaterials;

        trafficCollider1.enabled = !(trafficLightStatus1 == lightType.G);
        trafficCollider2.enabled = !(trafficLightStatus2 == lightType.G);
    }
}
