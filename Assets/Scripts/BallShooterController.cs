using DG.Tweening;
using System.Collections;
using UnityEngine;

public class BallShooterController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform rimTransform;
    //public ShootingPositionController shootingPositionController;
    

    [Header("Shot Configuration")]
    [SerializeField] private float shotDuration = 2f;

    [Header("Bezier Curve Parameters Adjustment")]
    [SerializeField] private Vector3 inCPOffset = new Vector3(0f, 4f, 1f);
    [SerializeField] private Vector3 outCPOffset = new Vector3(0f, 3f, -1f);

    [Header("Gizmos")]
    public bool showGizmos = true;
    public Color gizmoColor = Color.yellow;
    public int gizmoResolution = 20;
    public float gizmoSphereRadius = 0.05f;
    private Vector3 shotOrigin; 

    // Events
    public event System.Action OnShotStarted;
    public event System.Action OnShotCompleted;

    // State
    private bool isShooting = false;
    private Vector3 inCP;
    private Vector3 outCP;

    // Balls that the controller will shoot - Gotten from the pool
    private Rigidbody ballRb;
    private Transform ballTransform;

    private void Awake()
    {
        isShooting = false;
    }

    public void SetBall(Transform ballTransformParam)
    {
        ballTransform = ballTransformParam;
        ballRb = ballTransform.GetComponent<Rigidbody>();
    }

    private void CalculateControlPoints()
    {
        Vector3 flatDirection = (rimTransform.position - ballTransform.position);
        flatDirection.y = 0f;
        flatDirection = flatDirection.normalized;

        inCP = ballTransform.position + Vector3.up * inCPOffset.y + flatDirection * inCPOffset.z;
        outCP = rimTransform.position + Vector3.up * outCPOffset.y + flatDirection * outCPOffset.z;
    }

    public void StartPerfectShot()
    {
        if (isShooting)
        {
            Debug.LogWarning("Already shooting. Cannot start a new shot until the current one is complete.");
            return;
        }

        CalculateControlPoints();

        shotOrigin = ballTransform.position; 

        isShooting = true;

        // Disable physics during the shot
        ballRb.isKinematic = true;

        OnShotStarted?.Invoke();

        Debug.Log("Starting Perfect Shot. RB Physics are disabled.");

        // First approach using DOTween's DOPath with CubicBezier, but it doesn't give the expected results.
        //ballTransform.DOPath( new Vector3[]{inCP, outCP, rimTransform.position}, shotDuration, PathType.CubicBezier)
        //.SetEase(Ease.InOutSine)
        //.OnComplete(OnShotComplete);

        // Second approach using DoVirtual.Float and the CubicBezier formula
        Vector3 origin = ballTransform.position;
        DOVirtual.Float(0f, 1f, shotDuration, t =>
        {
            ballTransform.position = CalculateCubicBezierPoint(
                t,
                origin,
                inCP,
                outCP,
                rimTransform.position
            );
        })
        .SetEase(Ease.InOutSine)
        .OnComplete(OnShotComplete);
    }

    private void OnShotComplete()
    {
        // Re-enable physics so the ball falls naturally
        ballRb.isKinematic = false;
        ballRb.useGravity = true;

        isShooting = false;

        OnShotCompleted?.Invoke();

        Debug.Log("Perfect shot completed. RB Physics are enabled.");
    }

    //public IEnumerator ShootingTest(float shootingDelay)
    //{
    //    foreach (var shootingPosition in shootingPositionController.GetCurrentRoundSemicirclePositions())
    //    {
    //        ballTransform.position = shootingPosition.WorldPosition + new Vector3(0f, 2f, 0f); // Reset ball position above the parent
    //        StartPerfectShot();
    //        yield return new WaitForSeconds(shootingDelay);
    //    }
    //}

    private void OnDrawGizmos()
    {
        if (!showGizmos || ballTransform == null || rimTransform == null) return;
        if (Application.isPlaying && !isShooting && shotOrigin == Vector3.zero) return;

        Vector3 origin = Application.isPlaying ? shotOrigin : ballTransform.position;
        Vector3 destination = rimTransform.position;

        // Same calculation as CalculateControlPoints()
        Vector3 flatDirection = (destination - origin);
        flatDirection.y = 0f;
        flatDirection = flatDirection.normalized;

        Vector3 cp1 = origin + Vector3.up * inCPOffset.y + flatDirection * inCPOffset.z;
        Vector3 cp2 = destination + Vector3.up * outCPOffset.y + flatDirection * outCPOffset.z;

        Gizmos.color = gizmoColor;

        Vector3 previousPoint = origin;

        for (int i = 1; i <= gizmoResolution; i++)
        {
            float t = i / (float)gizmoResolution;
            Vector3 point = CalculateCubicBezierPoint(t, origin, cp1, cp2, destination);

            Gizmos.DrawLine(previousPoint, point);
            Gizmos.DrawSphere(point, gizmoSphereRadius);

            previousPoint = point;
        }

        // Draw control points
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(cp1, gizmoSphereRadius * 2f);
        Gizmos.DrawSphere(cp2, gizmoSphereRadius * 2f);

        Gizmos.DrawLine(origin, cp1);
        Gizmos.DrawLine(destination, cp2);
    }

    // From wikipedia: https://en.wikipedia.org/wiki/B%C3%A9zier_curve#Cubic_B%C3%A9zier_curves
    // B(t) = (1-t)³P0 + 3(1-t)²tP1 + 3(1-t)t²P2 + t³P3, t in [0, 1]
    private Vector3 CalculateCubicBezierPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
    {
        float alpha = 1f - t;
        return (alpha * alpha * alpha * p0)
             + (3f * alpha * alpha * t * p1)
             + (3f * alpha * t * t * p2)
             + (t * t * t * p3);
    }

    //IEnumerator Start()
    //{
    //    ballRb = ballTransform.GetComponent<Rigidbody>();
    //    isShooting = false;
    //    yield return new WaitForSeconds(1f);
    //    StartCoroutine(ShootingTest(4f));
    //}
}