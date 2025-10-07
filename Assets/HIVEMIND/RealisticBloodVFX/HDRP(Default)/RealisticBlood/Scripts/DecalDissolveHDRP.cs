using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

public class HDRPDecalDissolver : MonoBehaviour
{
    [Header("Dissolve Timing")]
    public float minDelay = 0.5f;           // Min time before starting dissolve
    public float maxDelay = 1.5f;           // Max time before starting dissolve
    public float dissolveDuration = 1.5f;   // Time it takes to fully dissolve

    private float timer = 0f;
    private float dissolveDelay;
    private bool startedDissolve = false;

    private List<Material> decalMaterials = new List<Material>();

    void Start()
    {
        // Pick a random dissolve delay between min and max
        dissolveDelay = Random.Range(minDelay, maxDelay);

        // Get all HDRP DecalProjectors in this object and its children
        DecalProjector[] projectors = GetComponentsInChildren<DecalProjector>();

        if (projectors == null || projectors.Length == 0)
        {
            Debug.LogError("No HDRP DecalProjector components found in prefab or children!");
            return;
        }

        foreach (DecalProjector projector in projectors)
        {
            Material originalMat = projector.material;

            if (originalMat == null)
            {
                Debug.LogWarning("Decal Projector has no material.");
                continue;
            }

            if (!originalMat.HasProperty("_Dissolve"))
            {
                Debug.LogWarning($"Material '{originalMat.name}' does not have a '_Dissolve' property.");
                continue;
            }

            // Clone the material to avoid modifying shared instances
            Material matInstance = Instantiate(originalMat);
            matInstance.SetFloat("_Dissolve", 0f);
            projector.material = matInstance;

            decalMaterials.Add(matInstance);
        }
    }

    void Update()
    {
        if (decalMaterials.Count == 0) return;

        timer += Time.deltaTime;

        if (!startedDissolve && timer >= dissolveDelay)
        {
            startedDissolve = true;
            timer = 0f;
        }

        if (startedDissolve)
        {
            float dissolveValue = Mathf.Clamp01(timer / dissolveDuration);

            foreach (Material mat in decalMaterials)
            {
                if (mat != null)
                    mat.SetFloat("_Dissolve", dissolveValue);
            }

            if (dissolveValue >= 1f)
            {
                Destroy(gameObject);
            }
        }
    }
}
