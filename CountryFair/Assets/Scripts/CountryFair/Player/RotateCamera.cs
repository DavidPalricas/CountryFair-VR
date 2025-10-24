using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Handles camera rotation based on mouse movement with smooth interpolation and clamping.
/// </summary>
public class RotateCamera : MonoBehaviour
{
    /// <summary>Sensitivity multiplier for mouse movement.</summary>
    [Header("Mouse Settings")]
    [SerializeField]
    private readonly float mouseSensitivity = 5f;

    /// <summary>Smoothing factor for rotation interpolation.</summary>
    [SerializeField]
    private readonly float rotationSmoothness = 5f;

    /// <summary>Minimum rotation angle in degrees (downward limit).</summary>
    [SerializeField]
    private readonly float minYRotation = -90f;

    /// <summary>Maximum rotation angle in degrees (upward limit).</summary>
    [SerializeField]
    private readonly float maxYRotation = 90f;

    /// <summary>Input action reference for look input from the new input system.</summary>
    [Header("Input Reference")]
    [SerializeField]
    private InputActionReference lookAction;

    /// <summary>Current horizontal rotation angle in degrees.</summary>
    private float yRotation;

    /// <summary>Raw mouse delta this frame.</summary>
    private Vector2 currentMouseDelta;

    /// <summary>Smoothed mouse delta for interpolation.</summary>
    private Vector2 smoothMouseDelta;

    /// <summary>Mouse input values from the input action.</summary>
    private Vector2 mouseInput;

    /// <summary>
    /// Initializes the camera by locking the cursor and storing the current Y rotation.
    /// </summary>
    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        yRotation = transform.localEulerAngles.y;
    }

    /// <summary>
    /// Main update loop that reads input, processes it, and applies camera rotation.
    /// </summary>
    private void Update()
    {
        ReadMouseInput();
        HandleMouseInput();
        ApplyCameraRotation();
    }

    /// <summary>
    /// Reads the current mouse input from the input action.
    /// </summary>
    private void ReadMouseInput()
    {
        mouseInput = lookAction.action.ReadValue<Vector2>();
    }

    /// <summary>
    /// Processes raw mouse input and converts it to world-space camera rotation delta.
    /// </summary>
    private void HandleMouseInput()
    {
        float mouseX = mouseInput.x * mouseSensitivity * Time.deltaTime;
        currentMouseDelta = new Vector2(mouseX, 0f);
    }

    /// <summary>
    /// Applies the smoothed mouse delta to the camera rotation with clamping.
    /// Updates the camera's local rotation based on the calculated yRotation.
    /// </summary>
    private void ApplyCameraRotation()
    {
        smoothMouseDelta = Vector2.Lerp(smoothMouseDelta, currentMouseDelta, rotationSmoothness * Time.deltaTime);

        yRotation += smoothMouseDelta.x;
        yRotation = Mathf.Clamp(yRotation, minYRotation, maxYRotation);

        transform.localRotation = Quaternion.Euler(0f, yRotation, 0f);
    }
}