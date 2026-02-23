using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class VisualizerEffect : MonoBehaviour, IHighlightable, IInteractable
{
    [Header("Visual Settings")]
    public float hoverGlitch = 0.5f;
    public float hoverRGBOffset = 0.2f;
    public float pulseSpeed = 8.0f;
    public float pulseIntensity = 0.25f;

    private Material visualizerMaterial;
    private Material originalMaterial;
    private Renderer objRenderer;
    private bool isHovered, isNPressed, isBPressed, isRightClicked;
    private Vector3 originalScale;

    void Start()
    {
        objRenderer = GetComponentInChildren<Renderer>();
        originalScale = transform.localScale;
        
        if (objRenderer != null)
        {
            originalMaterial = objRenderer.sharedMaterial;
            Shader s = Shader.Find("Custom/CyberVisualizer");
            if (s != null)
            {
                visualizerMaterial = new Material(s);
                visualizerMaterial.CopyPropertiesFromMaterial(originalMaterial);
            }
        }
        else
        {
            Debug.LogWarning("[Visualizer] No Renderer found on " + gameObject.name + " or children.");
        }
    }

    void Update()
    {
        if (objRenderer == null || visualizerMaterial == null) return;

        // Effect Logic
        float targetGlitch = isHovered ? hoverGlitch : 0;
        float targetRGB = isHovered ? hoverRGBOffset : 0;
        float targetBlur = isNPressed ? 0.8f : 0; // Intense Blur!
        float targetNeon = isBPressed ? 1.0f : 0; // Neon Wireframe!
        float targetInvert = isRightClicked ? 1.0f : 0;

        // Pulsing
        if (isHovered) {
            float p = 1.0f + Mathf.Sin(Time.time * pulseSpeed) * pulseIntensity;
            transform.localScale = originalScale * p;
        } else {
            transform.localScale = Vector3.Lerp(transform.localScale, originalScale, Time.deltaTime * 5f);
        }

        // Apply Shader Values
        visualizerMaterial.SetFloat("_GlitchAmount", targetGlitch);
        visualizerMaterial.SetFloat("_RGBOffset", targetRGB);
        visualizerMaterial.SetFloat("_BlurAmount", targetBlur);
        visualizerMaterial.SetFloat("_Neon", targetNeon);
        visualizerMaterial.SetFloat("_Invert", targetInvert);

        if (isHovered || isNPressed || isBPressed || isRightClicked) {
            objRenderer.material = visualizerMaterial;
        } else {
            objRenderer.material = originalMaterial;
        }
    }

    public void OnHoverStart() { isHovered = true; }
    public void OnHoverEnd() { isHovered = false; }
    public void OnKeyN() { isNPressed = !isNPressed; }
    public void OnKeyB() { isBPressed = !isBPressed; }
    public void OnRightClick() { isRightClicked = !isRightClicked; }
    public void OnInteract() { SpawnSlowSplash(); }

    private void SpawnSlowSplash()
    {
        Bounds b = objRenderer.bounds;
        for (int i = 0; i < 500; i++)
        {
            GameObject frag = GameObject.CreatePrimitive(PrimitiveType.Cube);
            frag.transform.position = b.center + Random.insideUnitSphere * 0.3f;
            frag.transform.localScale = Vector3.one * 0.05f;
            frag.GetComponent<Renderer>().material.color = Random.ColorHSV(0, 1, 1, 1, 1, 1);
            
            Rigidbody rb = frag.AddComponent<Rigidbody>();
            rb.drag = 2f; // High air resistance for "slow" look
            rb.AddForce(Random.onUnitSphere * 3f, ForceMode.Impulse);
            Destroy(frag, 3f);
        }
    }

    public void OnClick() {} // Required for interface
}
