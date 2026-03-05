using UnityEngine;

public class SwipeTrailController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ThrowBallInputHandler throwBallInputHandler;
    [SerializeField] private TrailRenderer trailRenderer;
    [SerializeField] private Camera trailCamera;

    private void Awake()
    {
        trailCamera.orthographicSize = Screen.height / 2f;
        trailRenderer.emitting = false;
    }

    private void OnEnable()
    {
        throwBallInputHandler.OnSwipeStarted += HandleSwipeStarted;
        throwBallInputHandler.OnShootPowerChanged += HandleSwipeUpdate;
        throwBallInputHandler.OnShootReleased += HandleSwipeEnded;
        throwBallInputHandler.OnSwipeCancelled += HandleSwipeCancelled;
    }

    private void OnDisable()
    {
        throwBallInputHandler.OnSwipeStarted -= HandleSwipeStarted;
        throwBallInputHandler.OnShootPowerChanged -= HandleSwipeUpdate;
        throwBallInputHandler.OnShootReleased -= HandleSwipeEnded;
        throwBallInputHandler.OnSwipeCancelled -= HandleSwipeCancelled;
    }

    private void HandleSwipeStarted()
    {
        trailRenderer.emitting = false;
        MoveToInputPosition();
        trailRenderer.Clear();
        trailRenderer.emitting = true;
    }

    private void HandleSwipeUpdate(float power)
    {
        MoveToInputPosition();
    }

    private void HandleSwipeEnded(float power)
    {
        trailRenderer.emitting = false;
    }

    private void HandleSwipeCancelled(float power)
    {
        trailRenderer.emitting = false;
    }

    private void MoveToInputPosition()
    {
        Vector2 screenPos = GetCurrentInputPosition();
        float distanceToPlane = Mathf.Abs(trailCamera.transform.position.z);
        Vector3 worldPos = trailCamera.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, distanceToPlane));
        transform.position = worldPos;
    }

    private Vector2 GetCurrentInputPosition()
    {
        if (UnityEngine.InputSystem.Touchscreen.current != null)
            return UnityEngine.InputSystem.Touchscreen.current.primaryTouch.position.ReadValue();

        if (UnityEngine.InputSystem.Mouse.current != null)
            return UnityEngine.InputSystem.Mouse.current.position.ReadValue();

        return Vector2.zero;
    }
}