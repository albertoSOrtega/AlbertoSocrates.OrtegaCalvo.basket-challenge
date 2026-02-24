using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallPoolController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject ballPrefab;
    [SerializeField] private Transform ballsPoolContainer;

    [Header("Ball Pool Parameters Adjustment")]
    [SerializeField] private int poolSize = 3;

    // Pool
    private List<GameObject> pool = new List<GameObject>();

    //Singleton Pattern
    public static BallPoolController instance;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        InitializePool();
    }

    // Initializes the pool by instantiating the specified number of ball prefabs and deactivating them
    private void InitializePool()
    {
        for (int i = 0; i < poolSize; i++)
        {
            GameObject ball = Instantiate(ballPrefab, ballsPoolContainer);
            ball.SetActive(false);
            pool.Add(ball);
        }
    }

    // Returns an available ball from the pool, and activates and positions it at the given position
    // If randomBallRotation is true, the ball will be given a random rotation around the all axis,
    // otherwise it will be set to the default rotation (Quaternion.identity)
    public GameObject GetBall(Vector3 position, bool randomBallRotation)
    {
        foreach (GameObject ball in pool)
        {
            if (!ball.activeInHierarchy)
            {
                ball.transform.position = position;
                ball.transform.rotation = randomBallRotation ? Random.rotation : Quaternion.identity;
                ball.SetActive(true);
                return ball;
            }
        }

        Debug.LogWarning("No available balls in the pool.");
        return null;
    }

    // Returns a ball to the pool after a delay, resetting its Rigidbody component
    public void ReturnBall(GameObject ball, float delay)
    {
        StartCoroutine(ReturnBallCoroutine(ball, delay));
    }

    private IEnumerator ReturnBallCoroutine(GameObject ball, float delay)
    {
        yield return new WaitForSeconds(delay);

        Rigidbody rb = ball.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = false;
            rb.useGravity = false;
        }

        ball.SetActive(false);
    }
}