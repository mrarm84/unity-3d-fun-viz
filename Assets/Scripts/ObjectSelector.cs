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

    void Update()
    {
        // Toggle Selection Mode with 'M'
        if (Keyboard.current[toggleKey].wasPressedThisFrame)
        {
            isSelectionModeActive = !isSelectionModeActive;
            Debug.Log($"Selection Mode: {(isSelectionModeActive ? "ENABLED" : "DISABLED")}");
            UpdateCursorState();
        }

        // Handle Clicking in Selection Mode
        if (isSelectionModeActive && Mouse.current.leftButton.wasPressedThisFrame)
        {
            HandleSelection();
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
            // If your game is top-down, you might want to remove this line.
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
            Debug.Log($"[ObjectSelector] Clicked on: {clickedObject.name} at {hit.point}");

            // Optional: Trigger a script on the clicked object
            IInteractable interactable = clickedObject.GetComponent<IInteractable>();
            if (interactable != null)
            {
                interactable.OnInteract();
            }
        }
        else
        {
            Debug.Log("[ObjectSelector] Clicked on empty space.");
        }
    }
}

// Interface for objects you want to interact with specifically
public interface IInteractable
{
    void OnInteract();
}
