using UnityEngine;
using UnityEngine.InputSystem;

public class ObjectSelector : MonoBehaviour
{
    [Header("Input Keys")]
    public Key toggleKey = Key.M;

    [Header("Selection Mode")]
    public bool isSelectionModeActive = false;
    
    [Header("Raycast Settings")]
    public float maxDistance = 100f;
    public LayerMask layerMask = ~0; // Select everything by default

    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("ObjectSelector: No Main Camera found! Please tag your camera as 'MainCamera'.");
        }
        
        // Ensure cursor is locked/unlocked based on initial mode
        UpdateCursorState();
    }

    private GameObject currentHoveredObject;

    void Update()
    {
        // Toggle Selection Mode with 'M'
        if (Keyboard.current[toggleKey].wasPressedThisFrame)
        {
            isSelectionModeActive = !isSelectionModeActive;
            Debug.Log($"Selection Mode: {(isSelectionModeActive ? \"ENABLED\" : \"DISABLED\")}");
            UpdateCursorState();

            // Clear hover if we disable mode
            if (!isSelectionModeActive && currentHoveredObject != null)
            {
                ClearHover();
            }
        }

        if (isSelectionModeActive)
        {
            HandleHover();

            // Handle Clicking in Selection Mode
            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                HandleSelection();
            }
        }
    }

    void HandleHover()
    {
        Vector2 mousePos = Mouse.current.position.ReadValue();
        Ray ray = mainCamera.ScreenPointToRay(mousePos);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, maxDistance, layerMask))
        {
            GameObject hitObject = hit.collider.gameObject;

            if (hitObject != currentHoveredObject)
            {
                ClearHover();
                currentHoveredObject = hitObject;
                
                IHighlightable highlightable = currentHoveredObject.GetComponent<IHighlightable>();
                if (highlightable != null)
                {
                    highlightable.OnHoverStart();
                }
            }
        }
        else
        {
            ClearHover();
        }
    }

    void ClearHover()
    {
        if (currentHoveredObject != null)
        {
            IHighlightable highlightable = currentHoveredObject.GetComponent<IHighlightable>();
            if (highlightable != null)
            {
                highlightable.OnHoverEnd();
            }
            currentHoveredObject = null;
        }
    }

    void UpdateCursorState()
    {
        if (isSelectionModeActive)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            // Lock back if needed (typical for first-person games)
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    void HandleSelection()
    {
        Vector2 mousePos = Mouse.current.position.ReadValue();
        Ray ray = mainCamera.ScreenPointToRay(mousePos);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, maxDistance, layerMask))
        {
            GameObject clickedObject = hit.collider.gameObject;
            Debug.Log($"[ObjectSelector] Clicked on: {clickedObject.name}");

            // Trigger Highlight Click effect
            IHighlightable highlightable = clickedObject.GetComponent<IHighlightable>();
            if (highlightable != null)
            {
                highlightable.OnClick();
            }

            // Trigger interaction/disintegrate
            IInteractable interactable = clickedObject.GetComponent<IInteractable>();
            if (interactable != null)
            {
                interactable.OnInteract();
            }
        }
    }
}

public interface IHighlightable
{
    void OnHoverStart();
    void OnHoverEnd();
    void OnClick();
}

// Interface for objects you want to interact with specifically
public interface IInteractable
{
    void OnInteract();
}
