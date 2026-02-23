using UnityEngine;
using System.Collections;

public class HighlightEffect : MonoBehaviour, IHighlightable
{
    [Header("Hover Glitch Settings")]
    public Shader glitchShader;
    public float hoverGlitchAmount = 0.5f;
    public float hoverChromaOffset = 0.05f;
    public float lerpSpeed = 5f;

    [Header("Click Flash")]
    [ColorUsage(true, true)]
    public Color clickFlashColor = Color.white;
    public float flashDuration = 0.15f;

    private Material[] originalMaterials;
    private Material[] glitchMaterials;
    private Renderer objRenderer;
    private bool isHovering = false;
    private bool isFlashing = false;
    private float currentGlitchAmount = 0f;

    void Start()
    {
        objRenderer = GetComponent<Renderer>();
        if (objRenderer != null)
        {
            originalMaterials = objRenderer.sharedMaterials;
            glitchMaterials = new Material[originalMaterials.Length];
            
            if (glitchShader == null) glitchShader = Shader.Find("Custom/LocalGlitch");

            for (int i = 0; i < originalMaterials.Length; i++)
            {
                glitchMaterials[i] = new Material(glitchShader);
                glitchMaterials[i].CopyPropertiesFromMaterial(originalMaterials[i]);
                glitchMaterials[i].SetFloat("_ChromaOffset", hoverChromaOffset);
            }
        }
    }

    void Update()
    {
        if (objRenderer == null || isFlashing) return;

        // Smoothly transition between normal and glitch materials
        float targetGlitch = isHovering ? hoverGlitchAmount : 0f;
        currentGlitchAmount = Mathf.Lerp(currentGlitchAmount, targetGlitch, Time.deltaTime * lerpSpeed);

        if (currentGlitchAmount > 0.01f)
        {
            objRenderer.materials = glitchMaterials;
            foreach (var mat in glitchMaterials)
            {
                mat.SetFloat("_GlitchAmount", currentGlitchAmount);
            }
        }
        else
        {
            objRenderer.materials = originalMaterials;
        }
    }

    public void OnHoverStart()
    {
        isHovering = true;
    }

    public void OnHoverEnd()
    {
        isHovering = false;
    }

    public void OnClick()
    {
        if (!isFlashing) StartCoroutine(FlashAndGlitch());
    }

    private IEnumerator FlashAndGlitch()
    {
        isFlashing = true;
        // Maximum glitch on click
        foreach (var mat in glitchMaterials)
        {
            mat.SetFloat("_GlitchAmount", 1.0f);
            mat.SetColor("_Color", clickFlashColor);
        }
        
        yield return new WaitForSeconds(flashDuration);
        
        foreach (var mat in glitchMaterials)
        {
            mat.SetColor("_Color", Color.white);
        }
        isFlashing = false;
    }
}
