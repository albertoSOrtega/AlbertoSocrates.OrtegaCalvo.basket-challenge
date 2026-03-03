using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.EnhancedTouch;

public class ThrowBallInputHandler : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CameraController cameraController;

    [Header("PC Mouse Swipe Configuration (screen height %)")]
    [SerializeField][Range(0.1f, 0.6f)] private float maxSwipePcPct = 0.25f; // 25% screen height
    [SerializeField][Range(0.01f, 0.1f)] private float minSwipePcPct = 0.04f; // 4% screen height

    [Header("Mobile Swipe Configuration (DPI + cm approach)")]
    [SerializeField] private float maxSwipeDistanceCm = 6f;
    [SerializeField] private float minSwipeDistanceCm = 0.8f;

    [Header("Mobile Swipe Configuration 2 (screen height %) - Mobile Fallback if DPI unavailable")]
    [SerializeField][Range(0.1f, 0.6f)] private float maxSwipeMobilePct = 0.30f;
    [SerializeField][Range(0.01f, 0.1f)] private float minSwipeMobilePct = 0.04f;

    [Header("Swipe Time Limit")]
    [SerializeField] private float maxSwipeTime = 0.5f;


    #if UNITY_EDITOR
    [Header("Debug - Editor only, not added to builds")]
    [SerializeField] private bool forceMobileInEditor = false;
    [SerializeField] private bool forceNoDPI = false;
    [SerializeField] private float editorSimulatedDPI = 420f;
    #endif


    // Events
    public event System.Action OnSwipeStarted;
    public event System.Action<float> OnSwipeCancelled;
    public event System.Action<float> OnShootPowerChanged;
    public event System.Action<float> OnShootReleased;
    public event System.Action OnInputEnabledNextFrame; // called when the camera is ready and enables input

    // State
    private float maxSwipeDistancePx;
    private float minSwipeDistancePx;
    private Vector2 startPosition;
    private float currentShootPower;
    private float swipeTimer;
    private float lastSwipeY;  // only goes up
    private bool isTrackingSwipe = false;
    private bool isInputEnabled = false; // Blocked until the camera is ready
    private bool isWaitingForRelease = false; // for preventing ghost swipes

    // New Input System device references
    private Touchscreen touchscreen;
    private Mouse mouse;

    private void Awake()
    {

        // Lock orientation per platform
        #if UNITY_ANDROID || UNITY_IOS
        Screen.orientation = ScreenOrientation.Portrait;
        #else
        Screen.orientation = ScreenOrientation.LandscapeLeft;
        #endif

        isInputEnabled = false;
        CalculateSwipeDistances();
    }

    private void Start()
    {
        currentShootPower = 0f;
        swipeTimer = 0f;
    }

    private void OnEnable()
    {
        // Subscribe to device connection/disconnection changes - In case the player plugs in/out a mouse or touchscreen device while the game is running
        InputSystem.onDeviceChange += OnDeviceChanged;
        cameraController.OnCameraBehindPlayer += EnableInput;
        RefreshDevices();
    }

    private void OnDisable()
    {
        InputSystem.onDeviceChange -= OnDeviceChanged;
        cameraController.OnCameraBehindPlayer -= EnableInput;
    }

    // Extra cover for application focus - if the player alt-tabs or switches apps and comes back
    // we refresh devices to ensure input still works
    private void OnApplicationFocus(bool hasFocus)
    {
        if (hasFocus) RefreshDevices();
    }

    // Update: detects mouse on Editor/PC and touch on mobile
    private void Update()
    {
        if (touchscreen == null && Touchscreen.current != null)
        {
            RefreshDevices();
        }

        // Swipe time limit
        if (isTrackingSwipe)
        {
            swipeTimer += Time.deltaTime;
            if (swipeTimer >= maxSwipeTime)
            {
                Debug.Log($"[ThrowBallInputHandler] Swipe time limit reached. Power: {currentShootPower:P0}");
                isWaitingForRelease = true;
                EndTrackingSwipe(touchscreen != null
                    ? touchscreen.primaryTouch.position.ReadValue()
                    : mouse.position.ReadValue());
            }
        }

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

    public float GetMaxSwipeDistancePx()
    {
        return maxSwipeDistancePx;
    }

    public float GetMinSwipeDistancePx()
    {
        return minSwipeDistancePx;
    }

    // Mouse input (PC / Unity Editor)
    private void HandleMouseInput()
    {
        if (isWaitingForRelease)
        {
            if (mouse.leftButton.wasReleasedThisFrame)
                isWaitingForRelease = false;
            return;
        }

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

        // Wait for the finger to physically lift before accepting a new swipe
        if (isWaitingForRelease)
        {
            if (primaryTouch.press.wasReleasedThisFrame)
                isWaitingForRelease = false;
            return;
        }

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

    // Called by CameraController once it has repositioned behind the player.
    // Uses a one-frame delay to guarantee it runs after EndTrackingSwipe
    // has fully completed in the same frame (avoids a race condition where
    // EnableInput fires synchronously inside the OnShootReleased event chain
    // and isInputEnabled gets immediately overwritten to false again).
    private void EnableInput()
    {
        StartCoroutine(EnableInputNextFrame());
    }

    private IEnumerator EnableInputNextFrame()
    {
        OnInputEnabledNextFrame?.Invoke();
        yield return null;
        isInputEnabled = true;
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
        currentShootPower = Mathf.Clamp01(verticalDelta / maxSwipeDistancePx);

        OnShootPowerChanged?.Invoke(currentShootPower);
    }

    // Called on release ? fires shoot event if swipe is valid
    private void EndTrackingSwipe(Vector2 releasePosition)
    {
        isTrackingSwipe = false;
        swipeTimer = 0f;

        float verticalDelta = releasePosition.y - startPosition.y;

        if (verticalDelta >= minSwipeDistancePx)
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

    private void CalculateSwipeDistances()
    {
        // PC with mouse -> percentage of screen height, more predictable than DPI on desktop
        if (IsPCWithMouse())
        {
            maxSwipeDistancePx = Screen.height * maxSwipePcPct;
            minSwipeDistancePx = Screen.height * minSwipePcPct;

            Debug.Log($"[ThrowBallInputHandler] PC mode. " +
                      $"MaxSwipe: {maxSwipePcPct * 100f}% = {maxSwipeDistancePx:F0}px. " +
                      $"MinSwipe: {minSwipePcPct * 100f}% = {minSwipeDistancePx:F0}px.");
            return;
        }

        // Mobile -> DPI approach (physical cm)
        #if UNITY_EDITOR
        float dpi = forceNoDPI ? 0f : editorSimulatedDPI;
        #else
        float dpi = Screen.dpi;
        #endif

        if (dpi > 0f)
        {
            float pixelsPerCm = dpi / 2.54f;
            maxSwipeDistancePx = maxSwipeDistanceCm * pixelsPerCm;
            minSwipeDistancePx = minSwipeDistanceCm * pixelsPerCm;

            Debug.Log($"[ThrowBallInputHandler] Mobile mode. DPI: {dpi:F0}. " +
                      $"MaxSwipe: {maxSwipeDistanceCm}cm = {maxSwipeDistancePx:F0}px. " +
                      $"MinSwipe: {minSwipeDistanceCm}cm = {minSwipeDistancePx:F0}px.");
        }
        else
        {
            // same as pc if dpi not available
            maxSwipeDistancePx = Screen.height * maxSwipeMobilePct;
            minSwipeDistancePx = Screen.height * minSwipeMobilePct;

            Debug.LogWarning($"[ThrowBallInputHandler] Mobile DPI unavailable — fallback. " +
                             $"MaxSwipe: {maxSwipeDistancePx:F0}px. MinSwipe: {minSwipeDistancePx:F0}px.");
        }
    }

    // True if running on a desktop platform
    // Used to select the correct swipe distance calculation.
    private bool IsPCWithMouse()
    {

        #if UNITY_EDITOR
        if (forceMobileInEditor) return false;
        #endif

        return Application.platform == RuntimePlatform.WindowsPlayer
            || Application.platform == RuntimePlatform.WindowsEditor
            || Application.platform == RuntimePlatform.OSXPlayer
            || Application.platform == RuntimePlatform.OSXEditor
            || Application.platform == RuntimePlatform.LinuxPlayer
            || Application.platform == RuntimePlatform.LinuxEditor;
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
    
}