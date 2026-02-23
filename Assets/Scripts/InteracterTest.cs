using UnityEngine;

public class InteracterTest : MonoBehaviour, IInteractable
{
    private Renderer objRenderer;
    private Color originalColor;
    private bool isHighlighted = false;

    void Start()
    {
        objRenderer = GetComponent<Renderer>();
        if (objRenderer != null)
        {
            originalColor = objRenderer.material.color;
        }
    }

    public void OnInteract()
    {
        if (objRenderer == null) return;

        isHighlighted = !isHighlighted;
        objRenderer.material.color = isHighlighted ? Color.cyan : originalColor;

        Debug.Log($"[InteracterTest] {gameObject.name} interaction triggered! Highlight: {isHighlighted}");
    }
}
