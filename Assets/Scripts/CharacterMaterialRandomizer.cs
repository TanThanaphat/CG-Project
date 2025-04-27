using UnityEngine;

public class CharacterMaterialRandomizer : MonoBehaviour
{
    private Material newMaterial;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        newMaterial = Resources.Load<Material>("CharacterMaterials/Character_" + Random.Range(1, 4)); // Load the new material from Resources folder
        Debug.Log(newMaterial.name);
        gameObject.GetComponent<SkinnedMeshRenderer>().sharedMaterial = newMaterial; // Apply the new material to the character
    }
}

