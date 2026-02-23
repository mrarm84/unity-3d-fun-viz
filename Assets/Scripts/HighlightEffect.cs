using UnityEngine;
using System.Collections;

public class HighlightEffect : MonoBehaviour, IHighlightable
{
    [Header("Hover Settings")]
    [ColorUsage(true, true)]
    public Color hoverEmissionColor = new Color(0.2f, 0.2f, 0.2f, 1f);
    public float lerpSpeed = 10f;

    [Header("Click Settings")]
    [ColorUsage(true, true)]
    public Color clickFlashColor = new Color(1f, 1f, 1f, 1f);
    public float flashDuration = 0.1f;

    private Material[] materials;
    private Color[] originalEmissionColors;
    private bool isHovering = false;
    private bool isFlashing = false;

    void Start()
    {
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            materials = renderer.materials;
            originalEmissionColors = new Color[materials.Length];
            
            for (int i = 0; i < materials.Length; i++)
            {
                // Enable emission keyword if not already enabled
                materials[i].EnableKeyword("_EMISSION");
                
                // Try to get existing emission color
                if (materials[i].HasProperty("_EmissionColor"))
                {
                    originalEmissionColors[i] = materials[i].GetColor("_EmissionColor");
                }
                else
                {
                    originalEmissionColors[i] = Color.black;
                }
            }
        }
    }

    void Update()
    {
        if (isFlashing || materials == null) return;

        // Smoothly lerp emission color for hover effect
        Color targetColor = isHovering ? hoverEmissionColor : Color.black; // Using black as base if no original was set
        
        for (int i = 0; i < materials.Length; i++)
        {
            Color currentEmission = materials[i].GetColor("_EmissionColor");
            Color baseColor = isHovering ? hoverEmissionColor : originalEmissionColors[i];
            materials[i].SetColor("_EmissionColor", Color.Lerp(currentEmission, baseColor, Time.deltaTime * lerpSpeed));
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
        if (!isFlashing)
        {
            StartCoroutine(FlashRoutine());
        }
    }

    private IEnumerator FlashRoutine()
    {
        isFlashing = true;
        
        // Instant flash to bright color
        for (int i = 0; i < materials.Length; i++)
        {
            materials[i].SetColor("_EmissionColor", clickFlashColor);
        }

        yield return new WaitForSeconds(flashDuration);
        
        isFlashing = false;
        // The Update loop will naturally lerp back to original or hover color
    }
}
