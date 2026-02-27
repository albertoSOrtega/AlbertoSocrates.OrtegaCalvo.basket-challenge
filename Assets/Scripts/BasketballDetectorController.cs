using UnityEngine;

public class BasketballDetectorController : MonoBehaviour
{

    public event System.Action<ShotType> OnBasketballScored;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Ball")) return;

        Rigidbody ballRb = other.GetComponent<Rigidbody>();

        // Only count if ball is moving downward (entered from above)
        if (ballRb.velocity.y >= 0f) return;

        ShotType ballShotType = other.GetComponent<BallController>().CurrentShotType;

        Debug.Log($"[BasketDetectorController] BASKET! ShotType: {ballShotType}");
        OnBasketballScored?.Invoke(ballShotType);
    }
}