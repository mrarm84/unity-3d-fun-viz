using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System.Collections;
using System.Collections.Generic;

public class DisintegrateEffect : MonoBehaviour, IInteractable
{
    [Header("Fragment Settings")]
    public int fragmentCount = 500;
    public float fragmentLifeTime = 3f;
    public float explosionForce = 5f;
    public float gravityMultiplier = 1f;

    [Header("Dissolve Settings")]
    public Shader dissolveShader;
    public Texture2D noiseTexture;
    public float dissolveDuration = 1.0f;
    [ColorUsage(true, true)]
    public Color edgeColor = Color.cyan;

    [Header("Post Processing Pulse")]
    public float chromaticIntensity = 1.5f;
    public float chromaticDuration = 0.4f;

    private Material[] materials;
    private bool isDisintegrating = false;
    private Volume globalVolume;
    private ChromaticAberration chromaticAberration;

    void Start()
    {
        globalVolume = Object.FindAnyObjectByType<Volume>();
        if (globalVolume != null && globalVolume.profile != null)
        {
            globalVolume.profile.TryGet(out chromaticAberration);
        }

        Renderer r = GetComponentInChildren<Renderer>();
        if (r != null) materials = r.materials;

        if (dissolveShader == null) dissolveShader = Shader.Find("Ultimate 10+ Shaders/Dissolve");
    }

    public void OnInteract()
    {
        Debug.Log("<color=red>[DisintegrateEffect] OnInteract called for: " + gameObject.name + "</color>");
        if (!isDisintegrating) StartCoroutine(ExplodeAndDissolve());
    }

    private IEnumerator ExplodeAndDissolve()
    {
        isDisintegrating = true;

        if (chromaticAberration != null) StartCoroutine(PulseChromatic());

        SpawnFragments();

        // Dissolve the original
        if (materials != null && dissolveShader != null)
        {
            foreach (var mat in materials)
            {
                mat.shader = dissolveShader;
                if (noiseTexture != null) mat.SetTexture("_NoiseTex", noiseTexture);
                mat.SetColor("_EdgeColor", edgeColor);
            }

            float elapsed = 0;
            while (elapsed < dissolveDuration)
            {
                elapsed += Time.deltaTime;
                float progress = elapsed / dissolveDuration;
                foreach (var mat in materials)
                {
                    if (mat.HasProperty("_Cutoff")) mat.SetFloat("_Cutoff", progress);
                }
                yield return null;
            }
        }

        Destroy(gameObject);
    }

    private void SpawnFragments()
    {
        Renderer r = GetComponentInChildren<Renderer>();
        if (r == null) {
            Debug.LogError("[DisintegrateEffect] No Renderer found to spawn fragments from!");
            return;
        }

        Bounds bounds = r.bounds;
        Vector3 center = bounds.center;
        Vector3 size = bounds.size;

        Debug.Log("[DisintegrateEffect] Spawning " + fragmentCount + " pieces at " + center);

        for (int i = 0; i < fragmentCount; i++)
        {
            Vector3 randomPos = new Vector3(
                Random.Range(-size.x / 2f, size.x / 2f),
                Random.Range(-size.y / 2f, size.y / 2f),
                Random.Range(-size.z / 2f, size.z / 2f)
            );
            
            GameObject frag = GameObject.CreatePrimitive(PrimitiveType.Cube);
            frag.transform.position = center + randomPos;
            frag.transform.localScale = Vector3.one * 0.1f;
            
            // Set Color
            if (materials != null && materials.Length > 0)
                frag.GetComponent<Renderer>().material.color = materials[0].color;

            // Physics
            Rigidbody rb = frag.AddComponent<Rigidbody>();
            rb.mass = 0.05f;
            rb.useGravity = true;
            
            // Explosion
            Vector3 diff = frag.transform.position - center;
            rb.AddForce(diff.normalized * explosionForce, ForceMode.Impulse);

            Destroy(frag, fragmentLifeTime + Random.Range(0f, 1f));
        }
    }

    private IEnumerator PulseChromatic()
    {
        chromaticAberration.active = true;
        var original = chromaticAberration.intensity.value;
        float elapsed = 0;
        while (elapsed < chromaticDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / chromaticDuration;
            chromaticAberration.intensity.Override(Mathf.Lerp(original, chromaticIntensity, Mathf.Sin(progress * Mathf.PI)));
            yield return null;
        }
        chromaticAberration.intensity.Override(original);
    }
}
