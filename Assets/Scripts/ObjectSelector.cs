using UnityEngine;
using UnityEngine.InputSystem;

public class ObjectSelector : MonoBehaviour
{
    [Header("Input Keys")]
    public Key toggleKey = Key.M;

    [Header("Selection Mode")]
    public bool isSelectionModeActive = false;
    
    [Header("Raycast Settings")]
    public float maxDistance = 1000f; // Increased distance
    public LayerMask layerMask = ~0; 

    private Camera mainCamera;
    private GameObject currentHoveredObject;

    void Start()
    {
        mainCamera = GetComponent<Camera>();
        if (mainCamera == null) mainCamera = Camera.main;
        
        if (mainCamera == null)
        {
            Debug.LogError("[ObjectSelector] NO CAMERA FOUND! Attach this script to your Camera or tag your camera as 'MainCamera'.");
        }
        
        UpdateCursorState();
    }

    void Update()
    {
        if (Keyboard.current[toggleKey].wasPressedThisFrame)
        {
            isSelectionModeActive = !isSelectionModeActive;
            Debug.Log("<color=cyan>[ObjectSelector] Selection Mode: " + (isSelectionModeActive ? "ENABLED" : "DISABLED") + "</color>");
            UpdateCursorState();

            if (!isSelectionModeActive) ClearHover();
        }

        if (isSelectionModeActive)
        {
            HandleHover();

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

        // Visual debug line in Scene View
        Debug.DrawRay(ray.origin, ray.direction * 10f, Color.red);

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
            if (highlightable != null) highlightable.OnHoverEnd();
            currentHoveredObject = null;
        }
    }

    void UpdateCursorState()
    {
        Cursor.lockState = isSelectionModeActive ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = isSelectionModeActive;
    }

    void HandleSelection()
    {
        Vector2 mousePos = Mouse.current.position.ReadValue();
        Ray ray = mainCamera.ScreenPointToRay(mousePos);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, maxDistance, layerMask))
        {
            GameObject clickedObject = hit.collider.gameObject;
            Debug.Log("<color=yellow>[ObjectSelector] HIT: " + clickedObject.name + "</color>");

            IHighlightable h = clickedObject.GetComponent<IHighlightable>();
            if (h != null) h.OnClick();

            IInteractable i = clickedObject.GetComponent<IInteractable>();
            if (i != null) i.OnInteract();
            else Debug.LogWarning("[ObjectSelector] " + clickedObject.name + " has no DisintegrateEffect script!");
        }
        else
        {
            Debug.Log("[ObjectSelector] Missed. Clicked empty space.");
        }
    }
}

public interface IHighlightable { void OnHoverStart(); void OnHoverEnd(); void OnClick(); }
public interface IInteractable { void OnInteract(); }
