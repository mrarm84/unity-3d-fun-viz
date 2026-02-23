using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System.Collections;

public class DisintegrateEffect : MonoBehaviour, IInteractable
{
    [Header("Shader Settings")]
    public Shader dissolveShader;
    public Texture2D noiseTexture;
    public float dissolveDuration = 2.0f;
    [ColorUsage(true, true)]
    public Color edgeColor = new Color(0, 1, 1, 1); // Cyan glow
    public float edgeWidth = 0.1f;

    [Header("Post Processing")]
    public float chromaticIntensity = 1.0f;
    public float chromaticDuration = 0.5f;

    private Material[] materials;
    private bool isDisintegrating = false;
    private Volume globalVolume;
    private ChromaticAberration chromaticAberration;

    void Start()
    {
        // Try to find the Global Volume for Chromatic Aberration
        globalVolume = FindObjectOfType<Volume>();
        if (globalVolume != null && globalVolume.profile != null)
        {
            globalVolume.profile.TryGet(out chromaticAberration);
        }

        // Pre-load materials
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            materials = renderer.materials;
        }

        // Default shader if not set (Assuming you have the Ultimate 10+ Shaders)
        if (dissolveShader == null)
        {
            dissolveShader = Shader.Find("Ultimate 10+ Shaders/Dissolve");
        }
    }

    public void OnInteract()
    {
        if (!isDisintegrating)
        {
            StartCoroutine(DisintegrateSequence());
        }
    }

    private IEnumerator DisintegrateSequence()
    {
        isDisintegrating = true;

        // 1. Prepare Materials
        foreach (var mat in materials)
        {
            mat.shader = dissolveShader;
            if (noiseTexture != null) mat.SetTexture("_NoiseTex", noiseTexture);
            mat.SetColor("_EdgeColor", edgeColor);
            mat.SetFloat("_EdgeWidth", edgeWidth);
        }

        // 2. Start Chromatic Aberration Pulse
        StartCoroutine(PulseChromatic());

        // 3. Animate Dissolve (_Cutoff goes from 0 to 1)
        float elapsed = 0;
        while (elapsed < dissolveDuration)
        {
            elapsed += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsed / dissolveDuration);
            
            foreach (var mat in materials)
            {
                mat.SetFloat("_Cutoff", progress);
            }
            yield return null;
        }

        // 4. Destroy object when finished
        Destroy(gameObject);
    }

    private IEnumerator PulseChromatic()
    {
        if (chromaticAberration == null) yield break;

        // Set Override
        chromaticAberration.active = true;
        var originalIntensity = chromaticAberration.intensity.value;

        float elapsed = 0;
        while (elapsed < chromaticDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / chromaticDuration;
            // Smooth peak and fall
            float curve = Mathf.Sin(progress * Mathf.PI); 
            chromaticAberration.intensity.Override(Mathf.Lerp(originalIntensity, chromaticIntensity, curve));
            yield return null;
        }

        chromaticAberration.intensity.Override(originalIntensity);
    }
}
