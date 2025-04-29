using UnityEngine;
using UnityEngine.Rendering.Universal;

public class ChangeMaterialsToLit : MonoBehaviour
{
    void Start()
    {
        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer == null)
        {
            Debug.LogWarning("No MeshRenderer found on this GameObject.");
            return;
        }

        Material[] materials = meshRenderer.materials;

        Shader litShader = Shader.Find("Universal Render Pipeline/Lit");
        if (litShader == null)
        {
            Debug.LogError("Could not find URP Lit shader. Make sure URP is installed and set up.");
            return;
        }

        for (int i = 0; i < materials.Length; i++)
        {
            Debug.Log(materials[i].name + " " + materials[i].shader.name);
        }

        meshRenderer.materials = materials; // Reassign the updated array
    }
}