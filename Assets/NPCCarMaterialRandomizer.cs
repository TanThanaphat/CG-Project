using UnityEngine;

public class NPCCarMaterialRandomizer : MonoBehaviour
{
    private string targetMaterialName;
    private Material newMaterial;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        newMaterial = Resources.Load<Material>("CarMaterials/body_SC " + Random.Range(1, 9)); // Load the new material from Resources folder
        targetMaterialName = "body_SC";

        MeshRenderer[] allRenderers = GetComponentsInChildren<MeshRenderer>();

        foreach (MeshRenderer renderer in allRenderers)
        {
            Material[] materials = renderer.materials; // Notice: .materials (not sharedMaterials) so we get an instance we can edit!

            for (int i = 0; i < materials.Length; i++)
            {
                if (materials[i] != null && materials[i].name.Contains(targetMaterialName))
                {
                    materials[i] = newMaterial; // Replace with new material
                }
            }

            renderer.materials = materials; // Apply modified array back
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
