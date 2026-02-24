using JetBrains.Annotations;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform rimTransformReference;

    // Reference to components
    private ThrowBallInputHandler throwBallInputHandler;
    private BallShooterController ballShooterController;
    private PerfectZoneController perfectZoneController;
    private ShootingPositionController shootingPositionController;

    [Header("Ball Spawn Configuration")]
    [SerializeField] private float ballSpawnForwardDistance = 0.5f;
    [SerializeField] private float playerY = 1f;

    // State
    private GameObject currentBall;

    private void Awake()
    {
        throwBallInputHandler = GetComponent<ThrowBallInputHandler>();
        ballShooterController = GetComponent<BallShooterController>();
        perfectZoneController = GetComponent<PerfectZoneController>();
        shootingPositionController = GetComponent<ShootingPositionController>();
    }

    private void OnEnable()
    {
        throwBallInputHandler.OnShootReleased += HandleShootReleased;
        ballShooterController.OnShotCompleted += HandleShotCompleted;
        shootingPositionController.OnNewRoundGenerated += SetupPlayerInPosition;
    }

    private void OnDisable()
    {
        throwBallInputHandler.OnShootReleased -= HandleShootReleased;
        ballShooterController.OnShotCompleted -= HandleShotCompleted;
        shootingPositionController.OnNewRoundGenerated -= SetupPlayerInPosition;
    }

    private void HandleShootReleased(float shootPower)
    {
        if (perfectZoneController.IsInPerfectZone(shootPower))
        {
            ballShooterController.StartPerfectShot();
        }
        else
        {
            Debug.Log("Shot not in perfect zone. No action taken.");
        }
    }

    private void HandleShotCompleted()
    {
        BallPoolController.instance.ReturnBall(currentBall, 2f);
        currentBall = null;

        shootingPositionController.AdvancePosition();

        // checks that the player doesn't move to the next position on the first round, since it's already in position 0 when a new round is generated
        if (shootingPositionController.GetCurrentPositionIndex() != 0)
        {
            SetupPlayerInPosition();
        }
    }

    private void SetupPlayerInPosition()
    {
        MovePlayerToCurrentPosition();
        SpawnBall();
    }

    private void MovePlayerToCurrentPosition()
    {
        Vector3 nextShootingPosition = shootingPositionController.GetCurrentPosition().WorldPosition;
        nextShootingPosition.y = playerY;
        transform.position = nextShootingPosition;

        // Orient the player to face the rim
        OrientPlayerTowardsRim();

        Debug.Log($"Moved to position {shootingPositionController.GetCurrentPositionIndex()}: {nextShootingPosition}");
    }

    private void OrientPlayerTowardsRim()
    {
        Vector3 directionToRim = rimTransformReference.position - transform.position;
        // Ignore Y 
        directionToRim.y = 0f; 
        transform.rotation = Quaternion.LookRotation(directionToRim);
    }

    // Spawn the ball 0.5m in front of the player, in the direction of the rim
    private Vector3 GetBallSpawnPosition()
    {
        return transform.position + transform.forward * ballSpawnForwardDistance;
    }

    private void SpawnBall()
    {
        currentBall = BallPoolController.instance.GetBall(GetBallSpawnPosition(), true);
        ballShooterController.SetBall(currentBall.transform);

        Debug.Log($"Ball spawned at: {currentBall.transform.position}");
    }
}