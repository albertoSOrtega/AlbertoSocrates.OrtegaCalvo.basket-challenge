using DG.Tweening;
using System.Collections;
using UnityEditor;
using UnityEngine;

public class BallShooterController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform rimTransform;
    [SerializeField] private Transform backboardTransform;
    [SerializeField] private ThrowBallInputHandler throwBallInputHandler;
    [SerializeField] private ShootingBarZoneController shootingBarZoneController;

    [Header("Shot Configuration")]
    [SerializeField] private float shotDuration = 2f;
    [SerializeField] private float backspinSpeedDegreesPerSecond = 720f;
    [SerializeField] private float perfectShortShotFinalDownImpulse = 2.5f;

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

    [Header("Short Shot Configuration")]
    [SerializeField] private Vector3 inCPOffsetShort= new Vector3(0f, 3f, 1f);
    [SerializeField] private Vector3 outCPOffsetShort = new Vector3(0f, 2f, -1f);
    [SerializeField] private float shortShotMinDistanceFromRim = 1f;
    [SerializeField] private float shortShotMaxDistanceFromRim = 2f;
    [SerializeField] private float shortYOffset = -0.5f;

    [Header("Lower Backboard Shot Configuration")]
    [SerializeField] private float lowerBackboardXMin = 0.7f;
    [SerializeField] private float lowerBackboardXMax = 0.8f;
    [SerializeField] private float lowerBackboardYOffset = 0.33f;
    [SerializeField] private float lowerBackboardZOffset = -0.44f;


    [Header("Gizmos")]
    public bool showGizmos = true;
    public int gizmoResolution = 20;
    public float gizmoSphereRadius = 0.05f;
    private bool hasImperfectShotData = false; // Control variable for the Gizmos
    private bool hasShortShotData = false; // Control variable for the Gizmos

    // Events
    public event System.Action OnShotStarted;
    public event System.Action<ShotType> OnShotCompleted;

    // State
    private bool isShooting = false;
    private ShotType currentShotType;
    private Vector3 shotOrigin;
    private Vector3 inCP;
    private Vector3 outCP;
    private Vector3 inCPImperfect;
    private Vector3 outCPImperfect;
    private Vector3 currentRimEdgePoint;
    private Vector3 currentRimEdgeDirection;
    private Vector3 spinAxis;
    private Vector3 currentShortShotTarget; // for gizmos

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

    private void CalculateControlPoints(Vector3 inOffset, Vector3 outOffset, out Vector3 inCP, out Vector3 outCP)
    {
        Vector3 flatDirection = (rimTransform.position - ballTransform.position);
        flatDirection.y = 0f;
        flatDirection = flatDirection.normalized;

        inCP = ballTransform.position + Vector3.up * inOffset.y + flatDirection * inOffset.z;
        outCP = rimTransform.position + Vector3.up * outOffset.y + flatDirection * outOffset.z;
    }

    // Common logic for both shots
    private Vector3 BeginShot(ShotType shotType)
    {
        // Detach from player so any ongoing DOTween on the player doesn't affect the ball
        ballTransform.SetParent(null);

        shotOrigin = ballTransform.position;
        isShooting = true;
        currentShotType = shotType;
        ballRb.isKinematic = true;

        // Backspin axis (horizontal, perpendicular to shot)
        Vector3 shotDir = (rimTransform.position - shotOrigin).normalized;
        spinAxis = Vector3.Cross(shotDir, Vector3.up).normalized;

        OnShotStarted?.Invoke();
        return ballTransform.position;
    }

    private void OnShotComplete()
    {
        // Re-enable physics
        EnableBallPhysics();

        isShooting = false;

        OnShotCompleted?.Invoke(currentShotType);
        Debug.Log("Shot completed. RB Physics are enabled.");
    }

    private void EnableBallPhysics()
    {
        ballRb.isKinematic = false;
        ballRb.useGravity = true;
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

    // Starts the perfect shot by calculating control points, then animating the ball along the Bezier curve to the rim position.
    // On completion, it calls OnShotComplete to re-enable physics and notify listeners.
    public void StartPerfectShot(float shootPower)
    {
        PerformPerfectShortShotLogic(ShotType.Perfect, shootPower);
    }

    public void StartShortShot(float shootPower)
    {
        PerformPerfectShortShotLogic(ShotType.Short, shootPower);
    }

    // Since Perfect and Short shots share the same logic except for the control points and target position,
    // this function handles both by taking the shot type as a parameter.
    public void PerformPerfectShortShotLogic(ShotType shotType, float shootPower)
    {
        // Reset gizmo Data for Imperfect Shot since we are starting a new shot that is not imperfect. This way, the
        hasImperfectShotData = false;

        if (isShooting) { Debug.LogWarning("Already shooting."); return; }

        Vector3 cp1;
        Vector3 cp2;
        Vector3 targetPosition;

        if (shotType == ShotType.Perfect)
        {
            CalculateControlPoints(inCPOffset, outCPOffset, out cp1, out cp2);
            targetPosition = rimTransform.position;
            hasShortShotData = false;
        }
        else
        {
            CalculateControlPoints(inCPOffsetShort, outCPOffsetShort, out cp1, out cp2);
            targetPosition = CalculateShortTarget(shootPower);
            currentShortShotTarget = targetPosition; // Store for Gizmos
            hasShortShotData = true;
        }

        Vector3 origin = BeginShot(shotType);
        ballTransform.GetComponent<BallSpinController>().StartSpin(backspinSpeedDegreesPerSecond, spinAxis);

        DOVirtual.Float(0f, 1f, shotDuration, t =>
        {
            ballTransform.position = CalculateCubicBezierPoint(t, origin, cp1, cp2, targetPosition);

            // Ball backspin
            //ballTransform.Rotate(spinAxis, backspinSpeedDegreesPerSecond * Time.deltaTime, Space.World);
        })
        .SetEase(Ease.Linear)
        .OnComplete(() => { EnableBallPhysics(); ballRb.AddForce((Vector3.zero - ballTransform.position).normalized * perfectShortShotFinalDownImpulse, ForceMode.Impulse); OnShotComplete();  });
    }

    // Calculates the distance from the rim at which a short shot should target based on the shoot power.
    public Vector3 CalculateShortTarget(float shootPower)
    {
        float distanceShort = CalculateShortShotDistance(shootPower);

        // Direction from the rim to the player
        Vector3 directionToPlayer = transform.position - rimTransform.position;
        directionToPlayer.y = 0f;

        // 3. Norimalize direction and multiply by distance
        Vector3 targetPosition = rimTransform.position + directionToPlayer.normalized * distanceShort + new Vector3(0, shortYOffset, 0);

        return targetPosition;
    }

    // Calculates the distance from the rim at which a short shot should target based on the shoot power.
    public float CalculateShortShotDistance(float shootPower)
    {
        // Power low limit (minSwipe / maxSwipe)
        float minPower = throwBallInputHandler.GetMinSwipeDistance() / throwBallInputHandler.GetMaxSwipeDistance();
        float maxPower = shootingBarZoneController.perfectZoneStart - shootingBarZoneController.imperfectZoneSize;

        // Normalize shootPower 0 y 1
        // If shootPower is minPower, t will be 0.If its 1, t will be 1.
        float t = Mathf.InverseLerp(minPower, maxPower, shootPower);

        // Invert t so that max power means closer to the rim
        t = 1 - t;

        // Interpolate in the Distance Range
        float finalDistance = Mathf.Lerp(shortShotMinDistanceFromRim, shortShotMaxDistanceFromRim, t);

        return finalDistance;
    }

    // Starts the imperfect shot by calculating control points, then animating the ball along a Bezier curve to a random point on the rim edge.
    // On completion, it applies an impulse to the ball away from the rim edge point and notify listeners in the function
    public void StartImperfectShot()
    {
        hasShortShotData = false;

        if (isShooting) { Debug.LogWarning("Already shooting."); return; }

        CalculateControlPoints(inCPOffsetImperfect, outCPOffsetImperfect, out Vector3 cp1, out Vector3 cp2);

        hasImperfectShotData = true;
        Vector3 origin = BeginShot(ShotType.Imperfect);

        currentRimEdgePoint = CalculateRandomRimEdgePoint();
        currentRimEdgeDirection = (currentRimEdgePoint - rimTransform.position);
        currentRimEdgeDirection.y = 0f;
        currentRimEdgeDirection = currentRimEdgeDirection.normalized;

        DOVirtual.Float(0f, 1f, shotDuration, t =>
        {
            ballTransform.position = CalculateCubicBezierPoint(t, origin, cp1, cp2, currentRimEdgePoint);

            // Ball backspin
            ballTransform.Rotate(spinAxis, backspinSpeedDegreesPerSecond * Time.deltaTime, Space.World);
        })
        .SetEase(Ease.Linear)
        .OnComplete(() => ApplyRimImpulse(currentRimEdgeDirection));
    }

    private Vector3 CalculateRandomRimEdgePoint()
    {
        float randomAngle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        Vector3 randomDirection = new Vector3(Mathf.Cos(randomAngle), 0f, Mathf.Sin(randomAngle));
        Vector3 rimEdgePoint = rimTransform.position + randomDirection * rimRadius;
        rimEdgePoint.y = rimTransform.position.y + rimEdgeHeightOffset;
        return rimEdgePoint;
    }

    // reactivate physics and apply impulse away from the rim edge point to the center of it, with a downward tilt - used by Imp. Shot
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

    public void StartLowerBackboardShot()
    {
        if (isShooting) { Debug.LogWarning("Already shooting."); return; }

        // Detach from player so the DOTween jump doesn't affect the ball
        ballTransform.SetParent(null);

        shotOrigin = ballTransform.position;
        isShooting = true;
        currentShotType = ShotType.LowerBackboard;

        Vector3 target = CalculateLowerBackboardTarget();
        Vector3 velocity = CalculateParabolicVelocity(shotOrigin, target, shotDuration);

        Vector3 shotDir = (target - shotOrigin).normalized;
        spinAxis = Vector3.Cross(shotDir, Vector3.up).normalized;
        ballRb.GetComponent<BallSpinController>().StartSpin(backspinSpeedDegreesPerSecond, spinAxis);

        // Reset Rigidbody manually completely before applying the new velocity
        EnableBallPhysics();
        // Now apply our calculated velocity
        ballRb.velocity = velocity;       

        OnShotStarted?.Invoke();

        StartCoroutine(BackboardShotComplete(shotDuration));

        DebugDrawParabola(shotOrigin, velocity, shotDuration, Color.cyan);
        Debug.Log($"[LowerBackboard] Origin: {shotOrigin}, Target: {target}, Velocity: {velocity}, T: {shotDuration}");
    }

    // Given an origin, a target and a flight time, calculates the initial velocity needed
    // to reach the target in that time under Unity's gravity using projectile motion equations:
    // target = origin + v0*t + 0.5*g*t^2  →  v0 = (target - origin - 0.5*g*t^2) / t
    private Vector3 CalculateParabolicVelocity(Vector3 origin, Vector3 target, float flightTime)
    {
        Vector3 displacement = target - origin;
        Vector3 gravityComponent = 0.5f * Physics.gravity * flightTime * flightTime;
        return (displacement - gravityComponent) / flightTime;
    }

    private Vector3 CalculateLowerBackboardTarget()
    {
        // Depending on which side of the backboard the player is
        float side = ballTransform.position.x > backboardTransform.position.x ? 1f : -1f;
        float xOffset = side * Random.Range(lowerBackboardXMin, lowerBackboardXMax);
        float yOffset = lowerBackboardYOffset;

        return new Vector3(
            backboardTransform.position.x + xOffset,               // X: lateral offset from center
            backboardTransform.position.y + lowerBackboardYOffset,  // Y: vertical offset
            backboardTransform.position.z + lowerBackboardZOffset   // Z: push towards the field to hit the front face
        );
    }

    private IEnumerator BackboardShotComplete(float delay)
    {
        yield return new WaitForSeconds(delay);
        isShooting = false;
        OnShotCompleted?.Invoke(currentShotType);
        Debug.Log($"[BallShooterController] Backboard shot completed: {currentShotType}");
    }

    // Draws the parabolic trajectory of a shot in the Scene view for debugging purposes. 
    private void DebugDrawParabola(Vector3 origin, Vector3 v0, float duration, Color color, int steps = 30)
    {
        Vector3 prev = origin;
        for (int i = 1; i <= steps; i++)
        {
            float t = duration * i / steps;
            Vector3 pos = origin + v0 * t + 0.5f * Physics.gravity * t * t;
            Debug.DrawLine(prev, pos, color, duration + 2f);
            prev = pos;
        }
    }


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

        Gizmos.color = Color.yellow;

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
        if (hasImperfectShotData && Application.isPlaying)
        {
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

        if (hasShortShotData && Application.isPlaying)
        {
            // Calculamos el destino específico para el tiro corto (usando un valor de ejemplo o el último procesado)
            // Nota: En Gizmos, si no estamos en Play Mode, usamos un power de 0.5f para previsualizar
            Vector3 shortTarget = currentShortShotTarget;

            // Recalculamos flatDirection hacia el nuevo target
            Vector3 shortFlatDir = (shortTarget - origin);
            shortFlatDir.y = 0f;
            shortFlatDir = shortFlatDir.normalized;

            Vector3 cp1Short = origin + Vector3.up * inCPOffsetShort.y + shortFlatDir * inCPOffsetShort.z;
            Vector3 cp2Short = shortTarget + Vector3.up * outCPOffsetShort.y + shortFlatDir * outCPOffsetShort.z;

            Gizmos.color = Color.cyan;
            previousPoint = origin;
            for (int i = 1; i <= gizmoResolution; i++)
            {
                float t = i / (float)gizmoResolution;
                Vector3 point = CalculateCubicBezierPoint(t, origin, cp1Short, cp2Short, shortTarget);
                Gizmos.DrawLine(previousPoint, point);
                previousPoint = point;
            }

            // Dibujar Target y Control Points del Short Shot
            Gizmos.DrawWireSphere(shortTarget, gizmoSphereRadius * 2f);
            Gizmos.color = new Color(0, 1, 1, 0.5f); // Cyan semitransparente para líneas de control
            Gizmos.DrawLine(origin, cp1Short);
            Gizmos.DrawLine(shortTarget, cp2Short);
        }
    }
}