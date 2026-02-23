using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

public class ThrowBallInputHandler : MonoBehaviour
{

    // Initial configuration with pixels
    [Header("Swipe Configuration")]
    [SerializeField] private float maxSwipeDistance = 500f;
    [SerializeField] private float minSwipeDistance = 50f;

    // Events
    public event System.Action OnSwipeStarted; 
    public event System.Action<float> OnSwipeCancelled;
    public event System.Action<float> OnShootPowerChanged;
    public event System.Action<float> OnShootReleased;
    

    private Vector2 startPosition;
    private bool isTrackingSwipe = false;
    private float currentShootPower;

    // New Input System device references
    private Touchscreen touchscreen;
    private Mouse mouse;


    // Mouse input (PC / Unity Editor)
    private void HandleMouseInput()
    {
        // equivalent to old GetMouseButtonDown
        if (mouse.leftButton.wasPressedThisFrame)
        {
            StartTrackingSwipe(mouse.position.ReadValue());
        }

        // equivalent to old GetMouseButton (while held down)
        if (mouse.leftButton.isPressed && isTrackingSwipe)
        {
            TrackingSwipe(mouse.position.ReadValue());
        }

        // equivalent to old GetMouseButtonUp
        if (mouse.leftButton.wasReleasedThisFrame && isTrackingSwipe)
        {
            EndTrackingSwipe(mouse.position.ReadValue());
        }
    }

    // Touch input (Mobile)
    private void HandleTouchInput()
    {
        TouchControl primaryTouch = touchscreen.primaryTouch;

        if (primaryTouch.press.wasPressedThisFrame)
        {
            StartTrackingSwipe(primaryTouch.position.ReadValue());
        }

        if (primaryTouch.press.isPressed && isTrackingSwipe)
        {
            TrackingSwipe(primaryTouch.position.ReadValue());
        }

        if (primaryTouch.press.wasReleasedThisFrame && isTrackingSwipe)
        {
            EndTrackingSwipe(primaryTouch.position.ReadValue());
        }
    }

    // Called when the user click/taps the screen - saves the starting point
    private void StartTrackingSwipe(Vector2 screenPosition)
    {
        startPosition = screenPosition;
        isTrackingSwipe = true;
        currentShootPower = 0f;

        Debug.Log($"[ThrowBallInputHandler] Swipe started at: {startPosition}");
        OnSwipeStarted?.Invoke();
    }

    // Called every frame the user keeps clicking/tapping the screen - calculates power based on the vertical distance from the starting point
    private void TrackingSwipe(Vector2 currentScreenPosition)
    {
        // Only count positive Y axis
        float verticalDelta = currentScreenPosition.y - startPosition.y;
        float clampedDelta = Mathf.Max(0f, verticalDelta);

        currentShootPower = Mathf.Clamp01(clampedDelta / maxSwipeDistance);

        OnShootPowerChanged?.Invoke(currentShootPower);
    }

    // Called on release ? fires shoot event if swipe is valid
    private void EndTrackingSwipe(Vector2 releasePosition)
    {
        isTrackingSwipe = false;

        float verticalDelta = releasePosition.y - startPosition.y;

        if (verticalDelta >= minSwipeDistance)
        {
            Debug.Log($"[ThrowBallInputHandler] Shot released. Power: {currentShootPower:P0}");
            OnShootReleased?.Invoke(currentShootPower);
        }
        else
        {
            Debug.Log($"[ThrowBallInputHandler] Swipe cancelled - Not enough power. Power: {currentShootPower:P0}");
            OnSwipeCancelled?.Invoke(currentShootPower);
        }

        currentShootPower = 0f;
    }

    private void OnEnable()
    {
        // Subscribe to device connection/disconnection changes - In case the player plugs in/out a mouse or touchscreen device while the game is running
        InputSystem.onDeviceChange += OnDeviceChanged;
        RefreshDevices();
    }

    private void OnDisable()
    {
        InputSystem.onDeviceChange -= OnDeviceChanged;
    }

    private void OnDeviceChanged(InputDevice device, InputDeviceChange change)
    {
        RefreshDevices();
        DebugDeviceChanges(change);
    }

    private void DebugDeviceChanges(InputDeviceChange change)
    {
        switch (change)
        {
            case InputDeviceChange.Added:       
                Debug.Log("Device Added");
                break;
            case InputDeviceChange.Removed:      
                Debug.Log("Device Disconnected");
                break;
            case InputDeviceChange.Enabled:      
                Debug.Log("Device Enabled");
                break;
            case InputDeviceChange.Disabled:     
                Debug.Log("Device Disabled");
                break;
            case InputDeviceChange.Reconnected:  // dispositivo reconectado
                Debug.Log("Device Reconnected");
                break;
        }
    }

    // Saves references to the current touchscreen and mouse devices (if available)
    private void RefreshDevices()
    {
        touchscreen = Touchscreen.current;
        mouse = Mouse.current;
    }

    void Start()
    {
        currentShootPower = 0f;
    }

    // Update: detects mouse on Editor/PC and touch on mobile
    private void Update()
    {
        // Priority: use touchscreen if available (mobile), otherwise use mouse (PC/Editor)
        if (touchscreen != null)
        {
            HandleTouchInput();
        }
        else if (mouse != null)
        {
            HandleMouseInput();
        }
    }
}