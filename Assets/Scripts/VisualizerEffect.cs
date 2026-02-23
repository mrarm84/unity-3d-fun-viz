using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class VisualizerEffect : MonoBehaviour, IHighlightable, IInteractable
{
    [Header("Hover Settings")]
    public float hoverGlitch = 0.3f;
    public float hoverBlur = 0.05f;
    public float hoverRGBOffset = 0.1f;
    public float pulseSpeed = 5.0f;
    public float pulseIntensity = 0.2f;

    [Header("Splash Settings")]
    public int fragmentCount = 500;
    public float explosionForce = 2.0f; // Slower splash
    public float dragOnFragments = 1.0f; // More air resistance

    private Material visualizerMaterial;
    private Material originalMaterial;
    private Renderer objRenderer;
    private bool isHovered, isRightClicked, isNPressed, isBPressed;
    private Vector3 originalScale;

    void Start()
    {
        objRenderer = GetComponentInChildren<Renderer>();
        originalScale = transform.localScale;
        
        if (objRenderer != null)
        {
            originalMaterial = objRenderer.sharedMaterial;
            visualizerMaterial = new Material(Shader.Find("Custom/CyberVisualizer"));
            visualizerMaterial.CopyPropertiesFromMaterial(originalMaterial);
        }
    }

    void Update()
    {
        if (objRenderer == null) return;

        // Effect 1: Hover Glitch + RGB
        float targetGlitch = isHovered ? hoverGlitch : 0;
        float targetRGB = isHovered ? hoverRGBOffset : 0;
        
        // Effect 2: Breathing Pulse on Hover
        if (isHovered) {
            float pulse = 1.0f + Mathf.Sin(Time.time * pulseSpeed) * pulseIntensity;
            transform.localScale = originalScale * pulse;
        } else {
            transform.localScale = Vector3.Lerp(transform.localScale, originalScale, Time.deltaTime * 5f);
        }

        // Effect 3: N Key - Blur Intensity
        float targetBlur = isNPressed ? 0.02f : 0;
        
        // Effect 4: B Key - Inversion
        float targetInvert = isBPressed ? 1.0f : 0;

        // Effect 5: Right Click - Hue Shift (Color Change)
        if (isRightClicked) {
            visualizerMaterial.SetColor("_Color", Color.Lerp(Color.red, Color.blue, Mathf.PingPong(Time.time, 1)));
        } else {
            visualizerMaterial.SetColor("_Color", Color.white);
        }

        // Apply shader values
        visualizerMaterial.SetFloat("_GlitchAmount", targetGlitch);
        visualizerMaterial.SetFloat("_RGBOffset", targetRGB);
        visualizerMaterial.SetFloat("_BlurAmount", targetBlur);
        visualizerMaterial.SetFloat("_Invert", targetInvert);

        if (isHovered || isRightClicked || isNPressed || isBPressed) {
            objRenderer.material = visualizerMaterial;
        } else {
            objRenderer.material = originalMaterial;
        }
    }

    public void OnHoverStart() { isHovered = true; }
    public void OnHoverEnd() { isHovered = false; }
    
    // Toggle Right Click state
    public void OnRightClick() { isRightClicked = !isRightClicked; }
    
    // Key-triggered actions
    public void OnKeyN() { isNPressed = !isNPressed; }
    public void OnKeyB() { isBPressed = !isBPressed; }

    public void OnInteract() // Left Click
    {
        Debug.Log("<color=green>[Visualizer] Slow Splash: " + gameObject.name + "</color>");
        SpawnSlowFragments();
    }

    private void SpawnSlowFragments()
    {
        Bounds b = objRenderer.bounds;
        for (int i = 0; i < fragmentCount; i++)
        {
            GameObject frag = GameObject.CreatePrimitive(PrimitiveType.Cube);
            frag.transform.position = b.center + Random.insideUnitSphere * 0.5f;
            frag.transform.localScale = Vector3.one * 0.05f;
            
            frag.GetComponent<Renderer>().material.color = Random.ColorHSV();

            Rigidbody rb = frag.AddComponent<Rigidbody>();
            rb.drag = dragOnFragments;
            rb.mass = 0.01f;
            rb.AddForce(Random.onUnitSphere * explosionForce, ForceMode.Impulse);

            Destroy(frag, 5f);
        }
    }

    public void OnClick() {} // Keep for interface
}
