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
    public Vector3 fragmentScale = new Vector3(0.1f, 0.1f, 0.1f);

    [Header("Dissolve Settings")]
    public Shader dissolveShader;
    public Texture2D noiseTexture;
    public float dissolveDuration = 1.5f;
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
        // Find Global Volume
        globalVolume = Object.FindAnyObjectByType<Volume>();
        if (globalVolume != null && globalVolume.profile != null)
        {
            globalVolume.profile.TryGet(out chromaticAberration);
        }

        Renderer r = GetComponent<Renderer>();
        if (r != null) materials = r.materials;

        if (dissolveShader == null) dissolveShader = Shader.Find("Ultimate 10+ Shaders/Dissolve");
    }

    public void OnInteract()
    {
        if (!isDisintegrating)
        {
            StartCoroutine(ExplodeAndDissolve());
        }
    }

    private IEnumerator ExplodeAndDissolve()
    {
        isDisintegrating = true;

        // 1. Post Process Pulse
        if (chromaticAberration != null) StartCoroutine(PulseChromatic());

        // 2. Spawn Fragments
        SpawnFragments();

        // 3. Hide original object or dissolve it
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

        // 4. Finally destroy the original object
        Destroy(gameObject);
    }

    private void SpawnFragments()
    {
        Mesh mesh = null;
        if (GetComponent<MeshFilter>() != null) mesh = GetComponent<MeshFilter>().sharedMesh;
        else if (GetComponent<SkinnedMeshRenderer>() != null) mesh = GetComponent<SkinnedMeshRenderer>().sharedMesh;

        if (mesh == null) return;

        Bounds bounds = mesh.bounds;
        Vector3 size = bounds.size;
        Vector3 center = transform.TransformPoint(bounds.center);

        // Spawn fragment pieces
        for (int i = 0; i < fragmentCount; i++)
        {
            // Random point within bounds
            Vector3 randomPos = new Vector3(
                Random.Range(-size.x / 2f, size.x / 2f),
                Random.Range(-size.y / 2f, size.y / 2f),
                Random.Range(-size.z / 2f, size.z / 2f)
            );
            
            // World position relative to current transform
            Vector3 spawnPos = transform.TransformPoint(bounds.center + randomPos);

            GameObject fragment = GameObject.CreatePrimitive(PrimitiveType.Cube);
            fragment.transform.position = spawnPos;
            fragment.transform.localScale = Vector3.Scale(transform.lossyScale, fragmentScale);
            fragment.transform.rotation = Random.rotation;

            // Inherit Color if possible
            Renderer fragRen = fragment.GetComponent<Renderer>();
            if (fragRen != null && materials != null && materials.Length > 0)
            {
                fragRen.material.color = materials[0].color;
            }

            // Physics
            Rigidbody rb = fragment.AddComponent<Rigidbody>();
            rb.mass = 0.1f;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            
            // Explosion force away from center
            Vector3 forceDir = (spawnPos - center).normalized;
            rb.AddForce(forceDir * explosionForce, ForceMode.Impulse);

            // Cleanup
            Destroy(fragment, fragmentLifeTime + Random.Range(0f, 1f));
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
            float curve = Mathf.Sin(progress * Mathf.PI); 
            chromaticAberration.intensity.Override(Mathf.Lerp(original, chromaticIntensity, curve));
            yield return null;
        }
        chromaticAberration.intensity.Override(original);
    }
}
