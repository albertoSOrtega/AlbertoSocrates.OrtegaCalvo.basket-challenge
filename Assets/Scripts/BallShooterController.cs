using DG.Tweening;
using System.Collections;
using UnityEngine;

public class BallShooterController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform rimTransform;    

    [Header("Shot Configuration")]
    [SerializeField] private float shotDuration = 2f;

    [Header("Bezier Curve Parameters Adjustment")]
    [SerializeField] private Vector3 inCPOffset = new Vector3(0f, 4f, 1f);
    [SerializeField] private Vector3 outCPOffset = new Vector3(0f, 3f, -1f);

    [Header("Imperfect Shot Configuration")]
    [SerializeField] private Vector3 inCPOffsetImperfect = new Vector3(0f, 4f, 1f);
    [SerializeField] private Vector3 outCPOffsetImperfect = new Vector3(0f, 2f, -0.5f);
    [SerializeField] private float rimRadius = 0.25f;
    [SerializeField] private float rimImpulseForce = 2f;
    [SerializeField] private float rimImpulseDownwardAngle = 25f;
    [SerializeField] private float rimEdgeHeightOffset = 0.4f;

    [Header("Gizmos")]
    public bool showGizmos = true;
    public Color gizmoColor = Color.yellow;
    public int gizmoResolution = 20;
    public float gizmoSphereRadius = 0.05f;
    private Vector3 shotOrigin; 

    // Events
    public event System.Action OnShotStarted;
    public event System.Action<bool> OnShotCompleted;

    // State
    private bool isShooting = false;
    private bool isCurrentShotPerfect = false;
    private Vector3 inCP;
    private Vector3 outCP;
    private Vector3 inCPImperfect;
    private Vector3 outCPImperfect;

    // Imperfect shot state - stored so gizmos can draw the same point used in the shot
    private Vector3 currentRimEdgePoint;
    private Vector3 currentImperfectDirectionToPlayer;
    private bool hasImperfectShotData = false;

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

    public Transform GetBallTransform()
    {
        return ballTransform;
    }

    // -- Perfect Shot Logic --

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
        isCurrentShotPerfect = true;
        hasImperfectShotData = false;

        // Disable physics during the shot
        ballRb.isKinematic = true;

        OnShotStarted?.Invoke();

        Debug.Log("Starting Perfect Shot. RB Physics are disabled.");

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
        .SetEase(Ease.Linear)
        .OnComplete(OnShotComplete);
    }

    // -- Imperfect Shot Logic --
    private void CalculateImperfectControlPoints()
    {
        Vector3 flatDirection = (rimTransform.position - ballTransform.position);
        flatDirection.y = 0f;
        flatDirection = flatDirection.normalized;

        inCPImperfect = ballTransform.position + Vector3.up * inCPOffsetImperfect.y + flatDirection * inCPOffsetImperfect.z;
        outCPImperfect = rimTransform.position + Vector3.up * outCPOffsetImperfect.y + flatDirection * outCPOffsetImperfect.z;
    }

    public void StartImperfectShot()
    {
        if (isShooting)
        {
            Debug.LogWarning("Already shooting. Cannot start a new shot until the current one is complete.");
            return;
        }

        CalculateImperfectControlPoints();
        shotOrigin = ballTransform.position;
        isShooting = true;
        isCurrentShotPerfect = false;

        ballRb.isKinematic = true;

        OnShotStarted?.Invoke();

        Vector3 origin = ballTransform.position;

        // Random point on the rim circumference
        currentRimEdgePoint = CalculateRandomRimEdgePoint();

        // Direction from rim edge point back to rim center, for the impulse
        currentImperfectDirectionToPlayer = (currentRimEdgePoint - rimTransform.position);
        currentImperfectDirectionToPlayer.y = 0f;
        currentImperfectDirectionToPlayer = currentImperfectDirectionToPlayer.normalized;

        hasImperfectShotData = true;

        DOVirtual.Float(0f, 1f, shotDuration, t =>
        {
            ballTransform.position = CalculateCubicBezierPoint(t, origin, inCPImperfect, outCPImperfect, currentRimEdgePoint);
        })
        .SetEase(Ease.Linear)
        .OnComplete(() => ApplyRimImpulse(currentImperfectDirectionToPlayer));
    }

    private Vector3 CalculateRandomRimEdgePoint()
    {
        float randomAngle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        Vector3 randomDirection = new Vector3(Mathf.Cos(randomAngle), 0f, Mathf.Sin(randomAngle));
        Vector3 rimEdgePoint = rimTransform.position + randomDirection * rimRadius;
        rimEdgePoint.y = rimTransform.position.y + rimEdgeHeightOffset;
        return rimEdgePoint;
    }

    private void ApplyRimImpulse(Vector3 directionToPlayer)
    {
        EnableBallPhysics();

        // Rotate directionToPlayer towards center, then tilt downward by rimImpulseDownwardAngle
        Vector3 toCenter = -directionToPlayer;
        Vector3 impulseDirection = Quaternion.AngleAxis(-rimImpulseDownwardAngle, Vector3.Cross(toCenter, Vector3.up)) * toCenter;

        ballRb.AddForce(impulseDirection * rimImpulseForce, ForceMode.Impulse);

        Debug.Log($"Imperfect shot: impulse applied. Direction: {impulseDirection}, Force: {rimImpulseForce}");

        OnShotComplete();
    }

    // -- Shared Logic --

    private void OnShotComplete()
    {
        // Re-enable physics
        EnableBallPhysics();

        isShooting = false;

        OnShotCompleted?.Invoke(isCurrentShotPerfect);

        Debug.Log("Perfect shot completed. RB Physics are enabled.");
    }

    // Re-enable physics 
    private void EnableBallPhysics()
    {
        ballRb.isKinematic = false;
        ballRb.useGravity = true;
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

        // --- Imperfect Shot Gizmo (red) - only drawn when shot data is available ---
        if (!hasImperfectShotData) return;

        Vector3 cp1Imperfect = origin + Vector3.up * inCPOffsetImperfect.y + flatDirection * inCPOffsetImperfect.z;
        Vector3 cp2Imperfect = destination + Vector3.up * outCPOffsetImperfect.y + flatDirection * outCPOffsetImperfect.z;

        Gizmos.color = Color.red;
        previousPoint = origin;
        for (int i = 1; i <= gizmoResolution; i++)
        {
            float t = i / (float)gizmoResolution;
            Vector3 point = CalculateCubicBezierPoint(t, origin, cp1Imperfect, cp2Imperfect, currentRimEdgePoint);
            Gizmos.DrawLine(previousPoint, point);
            Gizmos.DrawSphere(point, gizmoSphereRadius);
            previousPoint = point;
        }

        Gizmos.color = Color.magenta;
        Gizmos.DrawSphere(cp1Imperfect, gizmoSphereRadius * 2f);
        Gizmos.DrawSphere(cp2Imperfect, gizmoSphereRadius * 2f);
        Gizmos.DrawLine(origin, cp1Imperfect);
        Gizmos.DrawLine(currentRimEdgePoint, cp2Imperfect);

        Gizmos.color = Color.red;
        Gizmos.DrawSphere(currentRimEdgePoint, gizmoSphereRadius * 3f);
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
}