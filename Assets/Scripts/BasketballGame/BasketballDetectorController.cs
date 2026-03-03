using UnityEngine;

public class BasketballDetectorController : MonoBehaviour
{

    public event System.Action<ShotType, GameEntity> OnBasketballScored;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Ball")) return;

        Rigidbody ballRb = other.GetComponent<Rigidbody>();

        // Only count if ball is moving downward (entered from above)
        if (ballRb.velocity.y >= 0f) return;

        BallController ballController = other.GetComponent<BallController>();

        Debug.Log($"[BasketDetectorController] BASKET! ShotType: {ballController.CurrentShotType}");
        OnBasketballScored?.Invoke(ballController.CurrentShotType, ballController.Owner);
    }
}