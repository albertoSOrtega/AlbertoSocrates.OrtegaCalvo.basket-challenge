using UnityEngine;

public class BallSpinController : MonoBehaviour
{
    private bool isSpinning = false;
    private float backspinSpeed;
    private Vector3 spinAxis;
    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void StartSpin(float spinSpeed, Vector3 spinAxis)
    {
        this.backspinSpeed = spinSpeed;
        this.spinAxis = spinAxis;
        isSpinning = true;
    }

    public void StopSpin()
    {
        isSpinning = false;
        rb.freezeRotation = false;

        // convert the current manual rotation to angular velocity so that the ball continues
        // spinning naturally after the tween ends
        rb.angularVelocity = spinAxis * (backspinSpeed * Mathf.Deg2Rad);
    }

    private void OnCollisionEnter(Collision collision)
    {
        StopSpin();
    }

    private void Update()
    {
        if (!isSpinning) return;

        // If rigidbody is active (post-tween), we freeze its rotation
        // so it doesn't interfere with our manual rotation
        if (rb != null && !rb.isKinematic)
        {
            rb.freezeRotation = true;
        }

        transform.Rotate(spinAxis, backspinSpeed * Time.deltaTime, Space.World);
    }
}