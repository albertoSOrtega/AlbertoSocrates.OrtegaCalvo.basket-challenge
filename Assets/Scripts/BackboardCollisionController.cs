using System.Collections;
using UnityEngine;

public class BackboardCollisionController : MonoBehaviour
{
    [Header("Rebound Configuration")]
    [Range(0f, 1f)]
    [SerializeField] private float physicsBias = 1f;
    [SerializeField] private float reboundFlightTime = 0.45f;
    [SerializeField] private float backboardEnableTime = 0.2f;
    [SerializeField] private float yOffset = 0.15f;

    private Rigidbody rb;
    private bool hasRebounded = false;
    private bool isPerfectBackboardShot = false;
    private Vector3 velocityBeforeImpact;
    private Transform rimTransform;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        rimTransform = GameObject.FindGameObjectWithTag("Rim").transform;
    }

    public void ResetRebound()
    {
        hasRebounded = false;
    }

    public void SetPerfectBackboardShot(bool value)
    {
        isPerfectBackboardShot = value;
    } 

    public void DisableBackboardCollider()
    {
        GameObject.FindGameObjectWithTag("Backboard").GetComponent<Collider>().enabled = false;
    }

    private void FixedUpdate()
    {
        if (!isPerfectBackboardShot || hasRebounded) return;
        // Capture the velocity just before the impact
        velocityBeforeImpact = rb.velocity;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("BackboardTrigger"))
        {
            return;
        }
        else
        {
            BackboardVisualFeedbackController.instance.TriggerHitFlash();
            Debug.Log("[Trigger] Collision detected with the backboard");
        }
        if (!isPerfectBackboardShot || hasRebounded) return;
        
        hasRebounded = true;

        // Get closest point on the backboard to the ball's position
        Vector3 impactPoint = other.ClosestPoint(transform.position);

        // calculate the normmal manually from the impact point to the center of the ball.
        Vector3 normal = (transform.position - impactPoint).normalized;

        ApplyGuidedRebound(impactPoint, normal);

        isPerfectBackboardShot = false;
        // enable the backboard collider after a short delay to allow the ball to rebound without interference
        StartCoroutine(EnableBackboardCollider(backboardEnableTime));

        Debug.Log("[Trigger] Impacto detectado en: " + impactPoint);
    }

    public IEnumerator EnableBackboardCollider(float delay)
    {
        yield return new WaitForSeconds(delay);
        GameObject.FindGameObjectWithTag("Backboard").GetComponent<Collider>().enabled = true;
    }

    private void ApplyGuidedRebound(Vector3 impactPoint, Vector3 normal)
    {
        // Get the actual physics of the impact, the actual reflection
        Vector3 reflection = Vector3.Reflect(velocityBeforeImpact, normal);

        // Get the trajectory from the impact to the rim (parabolic trajectory)
        Vector3 target = rimTransform.position + Vector3.up * yOffset;
        Vector3 ideal = CalculateParabolicVelocity(impactPoint, target, reboundFlightTime);

        // Mix of it if we need it
        Vector3 finalVel = Vector3.Lerp(reflection, ideal, physicsBias);

        // Apply the final velocity to the ball
        rb.velocity = finalVel;

        // Draw gizmos if needed
        DrawDebugRebound(impactPoint, reflection, ideal, target, 3f);
    }

    private Vector3 CalculateParabolicVelocity(Vector3 origin, Vector3 target, float time)
    {
        Vector3 displacement = target - origin;
        Vector3 gravityComp = 0.5f * Physics.gravity * time * time;
        return (displacement - gravityComp) / time;
    }

    private void DrawDebugRebound(Vector3 impact, Vector3 reflectVel, Vector3 idealVel, Vector3 target, float duration)
    {
        // Impact point (White cross)
        Debug.DrawRay(impact, Vector3.up * 0.2f, Color.white, duration);
        Debug.DrawRay(impact, Vector3.right * 0.2f, Color.white, duration);
        Debug.DrawRay(impact, Vector3.forward * 0.2f, Color.white, duration);

        // Prue physics trajectory (Red line)
        Debug.DrawRay(impact, reflectVel.normalized * 2f, Color.red, duration);

        // Ideal trajectory to the virtual rim (Green line)
        Debug.DrawRay(impact, idealVel.normalized * 2f, Color.green, duration);

        // Ideal trajectory to the actual rim (Cian transparente)
        Debug.DrawLine(impact, target, Color.cyan, duration);

        // Final target point (Green cross)
        Debug.DrawLine(target + Vector3.left * 0.1f, target + Vector3.right * 0.1f, Color.green, duration);
        Debug.DrawLine(target + Vector3.forward * 0.1f, target + Vector3.back * 0.1f, Color.green, duration);
    }
}