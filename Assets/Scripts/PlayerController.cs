using DG.Tweening;
using System.Collections;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform rimTransformReference;
    [SerializeField] private CameraController cameraController;

    // Reference to components
    private ThrowBallInputHandler throwBallInputHandler;
    private BallShooterController ballShooterController;
    private ShootingBarZoneController shootingBarZoneController;
    private ShootingPositionController shootingPositionController;

    [Header("Ball Spawn Configuration")]
    [SerializeField] private float ballSpawnForwardDistance = 0.5f;
    [SerializeField] private float playerY = 1f;

    // State
    private GameObject currentBall;
    private bool isFirstRound = true;

    private void Awake()
    {
        throwBallInputHandler = GetComponent<ThrowBallInputHandler>();
        ballShooterController = GetComponent<BallShooterController>();
        shootingBarZoneController = GetComponent<ShootingBarZoneController>();
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

    // Wrapper to pass the information to the PlayerJumpAndShoot coroutine, since the event doesn't receive the shot type directly
    private void HandleShootReleased(float shootPower)
    {
        StartCoroutine(PlayerJumpAndShoot(shootPower));
    }

    private void HandleShotCompleted(ShotType shotType)
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

        // if it's the first round, we snap the camera to the player
        if (isFirstRound)
        {
            cameraController.SnapCameraToPlayer();
            isFirstRound = false;
        }
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
        currentBall = BallPoolController.instance.GetBall(GetBallSpawnPosition(), true, transform);
        ballShooterController.SetBall(currentBall.transform);

        Debug.Log($"Ball spawned at: {currentBall.transform.position}");
    }

    private IEnumerator PlayerJumpAndShoot(float shootPower)
    {
        // get the shot type and set up the shotType in the ball
        ShotType shotType = shootingBarZoneController.GetShotType(shootPower);
        currentBall.GetComponent<BallController>().CurrentShotType = shotType;

        transform.DOLocalJump(transform.position, 1f, 1, 1f);
        yield return new WaitForSeconds(0.5f); // Wait for the jump to reach its peak

        switch (shotType)
        {
            case ShotType.Perfect:
                ballShooterController.StartPerfectShot(shootPower);
                break;
            case ShotType.Imperfect:
                ballShooterController.StartImperfectShot();
                break;
            case ShotType.Short:    
                ballShooterController.StartShortShot(shootPower);
                break;
            case ShotType.PerfectBackboard:
                ballShooterController.StartPerfectBackboardShot();
                break;
            case ShotType.LowerBackboard:
                ballShooterController.StartLowerBackboardShot();
                break;
            case ShotType.UpperBackboard:
                ballShooterController.StartUpperBackboardShot();
                break;
        }
    }

    private void Start()
    {
        shootingPositionController.GenerateNewRound();
    }
}