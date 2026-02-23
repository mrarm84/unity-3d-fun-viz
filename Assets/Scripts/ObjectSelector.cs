using UnityEngine;
using UnityEngine.InputSystem;

public class ObjectSelector : MonoBehaviour
{
    [Header("Input Keys")]
    public Key toggleKey = Key.M;
    public Key nKey = Key.N;
    public Key bKey = Key.B;

    [Header("Selection Mode")]
    public bool isSelectionModeActive = false;
    
    [Header("Raycast Settings")]
    public float maxDistance = 1000f;
    public LayerMask layerMask = ~0; 

    private Camera mainCamera;
    private VisualizerEffect currentVisualizer;

    void Start()
    {
        mainCamera = GetComponent<Camera>();
        if (mainCamera == null) mainCamera = Camera.main;
        UpdateCursorState();
    }

    void Update()
    {
        if (Keyboard.current[toggleKey].wasPressedThisFrame)
        {
            isSelectionModeActive = !isSelectionModeActive;
            Debug.Log("<color=cyan>[Visualizer] Mode: " + (isSelectionModeActive ? "ENABLED" : "DISABLED") + "</color>");
            UpdateCursorState();
        }

        if (isSelectionModeActive)
        {
            HandleInteraction();
        }
    }

    void HandleInteraction()
    {
        // Use center of screen (Crosshair mode)
        Ray ray = mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;

        Debug.DrawRay(ray.origin, ray.direction * 10f, Color.green);

        if (Physics.Raycast(ray, out hit, maxDistance, layerMask))
        {
            GameObject hitObject = hit.collider.gameObject;
            VisualizerEffect hitVis = hitObject.GetComponentInParent<VisualizerEffect>();

            // AUTO-INJECTION: Add the script if missing
            if (hitVis == null) {
                Debug.Log("<color=orange>[Visualizer] Auto-Injecting Effect into: " + hitObject.name + "</color>");
                hitVis = hitObject.AddComponent<VisualizerEffect>();
            }

            if (hitVis != currentVisualizer)
            {
                if (currentVisualizer != null) currentVisualizer.OnHoverEnd();
                currentVisualizer = hitVis;
                if (currentVisualizer != null) currentVisualizer.OnHoverStart();
            }

            if (currentVisualizer != null)
            {
                // Left Click: Slow Splash
                if (Mouse.current.leftButton.wasPressedThisFrame) currentVisualizer.OnInteract();
                
                // Right Click: Color Toggle
                if (Mouse.current.rightButton.wasPressedThisFrame) currentVisualizer.OnRightClick();
                
                // N Key: Blur Toggle
                if (Keyboard.current[nKey].wasPressedThisFrame) currentVisualizer.OnKeyN();
                
                // B Key: Invert Toggle
                if (Keyboard.current[bKey].wasPressedThisFrame) currentVisualizer.OnKeyB();
            }
        }
        else
        {
            if (currentVisualizer != null)
            {
                currentVisualizer.OnHoverEnd();
                currentVisualizer = null;
            }
        }
    }

    void UpdateCursorState()
    {
        Cursor.lockState = isSelectionModeActive ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !isSelectionModeActive;
    }

    void OnGUI()
    {
        if (isSelectionModeActive)
        {
            float size = 20;
            float xMin = (Screen.width / 2) - (size / 2);
            float yMin = (Screen.height / 2) - (size / 2);
            GUI.Box(new Rect(xMin, yMin, size, size), "+");
        }
    }
}

public interface IHighlightable
{
    void OnHoverStart();
    void OnHoverEnd();
    void OnClick();
}

public interface IInteractable
{
    void OnInteract();
}
