using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

public class ThrowBallInputHandler : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CameraController cameraController;

    // Initial configuration with pixels
    [Header("Swipe Configuration")]
    [SerializeField] private float maxSwipeDistance = 500f;
    [SerializeField] private float minSwipeDistance = 50f;
    [SerializeField] private float maxSwipeTime = 1.5f;

    // Events
    public event System.Action OnSwipeStarted;
    public event System.Action<float> OnSwipeCancelled;
    public event System.Action<float> OnShootPowerChanged;
    public event System.Action<float> OnShootReleased;

    private Vector2 startPosition;
    private bool isTrackingSwipe = false;
    private float currentShootPower;
    private float swipeTimer;
    private float lastSwipeY;  // only goes up
    private bool isInputEnabled = false; // Bloqueado hasta que la cámara esté lista

    // New Input System device references
    private Touchscreen touchscreen;
    private Mouse mouse;

    private void Awake()
    {
        isInputEnabled = false;
    }

    private void OnEnable()
    {
        // Subscribe to device connection/disconnection changes - In case the player plugs in/out a mouse or touchscreen device while the game is running
        InputSystem.onDeviceChange += OnDeviceChanged;
        RefreshDevices();
        cameraController.OnCameraBehindPlayer += EnableInput;
    }

    private void OnDisable()
    {
        InputSystem.onDeviceChange -= OnDeviceChanged;
        cameraController.OnCameraBehindPlayer -= EnableInput;
    }

    public float GetMaxSwipeDistance()
    {
        return maxSwipeDistance;
    }

    public float GetMinSwipeDistance()
    {
        return minSwipeDistance;
    }

    //todo: private
    public void EnableInput()
    {
        isInputEnabled = true;
    }

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
        if (!isInputEnabled) return;

        startPosition = screenPosition;
        lastSwipeY = screenPosition.y;
        isTrackingSwipe = true;
        currentShootPower = 0f;
        swipeTimer = 0f;

        Debug.Log($"[ThrowBallInputHandler] Swipe started at: {startPosition}");
        OnSwipeStarted?.Invoke();
    }

    // Called every frame the user keeps clicking/tapping the screen - calculates power based on the vertical distance from the starting point
    private void TrackingSwipe(Vector2 currentScreenPosition)
    {
        // Ignore downward movement
        if (currentScreenPosition.y <= lastSwipeY) return;

        lastSwipeY = currentScreenPosition.y;

        // Only count positive Y axis
        float verticalDelta = lastSwipeY - startPosition.y;
        currentShootPower = Mathf.Clamp01(verticalDelta / maxSwipeDistance);

        OnShootPowerChanged?.Invoke(currentShootPower);
    }

    // Called on release ? fires shoot event if swipe is valid
    private void EndTrackingSwipe(Vector2 releasePosition)
    {
        isTrackingSwipe = false;
        swipeTimer = 0f;

        float verticalDelta = releasePosition.y - startPosition.y;

        if (verticalDelta >= minSwipeDistance)
        {
            isInputEnabled = false; // Disable input until the camera is behind the player again for the next shot
            Debug.Log($"[ThrowBallInputHandler] Shot released. Power: {currentShootPower}");
            OnShootReleased?.Invoke(currentShootPower);
        }
        else
        {
            Debug.Log($"[ThrowBallInputHandler] Swipe cancelled - Not enough power. Power: {currentShootPower}");
            OnSwipeCancelled?.Invoke(currentShootPower);
        }

        currentShootPower = 0f;
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
            case InputDeviceChange.Reconnected:
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

    private void Start()
    {
        currentShootPower = 0f;
        swipeTimer = 0f;
    }

    // Update: detects mouse on Editor/PC and touch on mobile
    private void Update()
    {
        // Priority: use touchscreen if available (mobile), otherwise use mouse (PC/Editor)
        // Swipe time limit
        if (isTrackingSwipe)
        {
            swipeTimer += Time.deltaTime;
            if (swipeTimer >= maxSwipeTime)
            {
                Debug.Log($"[ThrowBallInputHandler] Swipe time limit reached. Power: {currentShootPower:P0}");
                EndTrackingSwipe(touchscreen != null
                    ? touchscreen.primaryTouch.position.ReadValue()
                    : mouse.position.ReadValue());
                isTrackingSwipe = false;
            }
        }

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