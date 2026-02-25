using DG.Tweening;
using System.Collections;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform playerTransform;
    [SerializeField] private Transform rimTransform;
    [SerializeField] private BallShooterController ballShooterController;

    [Header("Camera Offset and Follow Configuration")]
    [SerializeField] private Vector3 followOffset = new Vector3(0f, 2f, 4f);
    [SerializeField] private float smoothTime = 0.3f;
    [SerializeField] private float maxSpeed = 10f;

    [Header("Perfect Shot Shake Configuration")]
    [SerializeField] private Vector3 shakePunch = new Vector3(0f, 0.4f, 0f);
    [SerializeField] private float shakeDuration = 0.6f;
    [SerializeField] private int shakeVibrato = 6;
    [SerializeField] private float shakeElasticity = 0.5f;

    public event System.Action OnCameraBehindPlayer;

    // State
    private Transform currentTarget;
    private Transform ballTransform;
    private Vector3 currentVelocity = Vector3.zero;
    private bool isShaking = false;

    private void Awake()
    {
        currentTarget = playerTransform;
    }

    private void OnEnable()
    {
        ballShooterController.OnShotStarted += StartFollowingBall;
        ballShooterController.OnShotCompleted += CameraHandleShotCompleted;
    }

    private void OnDisable()
    {
        ballShooterController.OnShotStarted -= StartFollowingBall;
        ballShooterController.OnShotCompleted -= CameraHandleShotCompleted;
    }

    private void Start()
    {
        currentVelocity = Vector3.zero;
        isShaking = false;
    }

    // LateUpdate is used to ensure the camera updates after all other objects have moved in the frame, providing smoother following behavior
    private void LateUpdate()
    {
        if (currentTarget == null) return;

        UpdateCameraPosition();
        CameraLookAtRim();
    }

    private void UpdateCameraPosition()
    {
        if (isShaking) return; // Don't update position while shaking

        Vector3 newCameraPosition = GetCameraWorldPosition();

        // Time delta time internally calculated by SmoothDamp, so we don't need to multiply it here
        transform.position = Vector3.SmoothDamp(
            transform.position,
            newCameraPosition,
            ref currentVelocity,
            smoothTime,
            maxSpeed
        );
    }

    private void CameraLookAtRim()
    {
        if (rimTransform == null) return;

        // Tilt down to look at the rim
        transform.LookAt(rimTransform.position);
    }


    // Instantly snaps the camera behind the player
    public void SnapCameraToPlayer()
    {
        transform.position = GetCameraWorldPosition();
        CameraLookAtRim();
        currentVelocity = Vector3.zero;
        OnCameraBehindPlayer?.Invoke();
    }

    // Calculates desired camera world position based on the player's orientation + offset
    private Vector3 GetCameraWorldPosition()
    {
        // We use the player's rotation so the offset respects the direction the player is facing
        return currentTarget.position + playerTransform.rotation * followOffset;
    }

    // Calls the Coroutine
    private void CameraHandleShotCompleted(bool isPerfect)
    {
        // We do not follow the ball anymore, we wait until the shake is done to start following the player again
        ballTransform = null;
        currentTarget = null;

        if (isPerfect)
        {
            TriggerPerfectShotShake();
        }
        // todo: else
    }

    // Shakes the camera using DOTween
    private void TriggerPerfectShotShake()
    {
        isShaking = true;
        transform.DOKill();
        transform.DOPunchPosition(shakePunch, shakeDuration, shakeVibrato, shakeElasticity)
        .OnComplete(() =>
        {
            isShaking = false;
            currentTarget = playerTransform;
            SnapCameraToPlayer();
        });
    }

    private void StartFollowingBall()
    {
        ballTransform = ballShooterController.GetBallTransform();
        if (ballTransform != null)
        {
            currentTarget = ballTransform;
        }
    }

    public bool IsPlayerTarget()
    {
        return currentTarget == playerTransform;
    }

}