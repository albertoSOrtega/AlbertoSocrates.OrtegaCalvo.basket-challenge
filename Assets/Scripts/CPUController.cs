using DG.Tweening;
using System.Collections;
using UnityEngine;

public class CPUController : MonoBehaviour
{
    [System.Serializable]
    private struct ShotTypeWeight
    {
        public ShotType shotType;
        [Min(0f)] public float weight;
    }

    [Header("References")]
    [SerializeField] private Transform rimTransformReference;
    [SerializeField] private GameController gameController;

    [Header("Shooting Timing")]
    [SerializeField] private float shootingRateMin = 2f;
    [SerializeField] private float shootingRateMax = 4f;

    [Header("Shot Weights - (normalized)")]
    [SerializeField]
    private ShotTypeWeight[] shotWeights = new ShotTypeWeight[]
    {
        new ShotTypeWeight { shotType = ShotType.Perfect, weight = 0.5f },
        new ShotTypeWeight { shotType = ShotType.Imperfect, weight = 0.2f },
        new ShotTypeWeight { shotType = ShotType.Short, weight = 0.1f },
        new ShotTypeWeight { shotType = ShotType.PerfectBackboard, weight = 0.1f },
        new ShotTypeWeight { shotType = ShotType.LowerBackboard, weight = 0.05f },
        new ShotTypeWeight { shotType = ShotType.UpperBackboard, weight = 0.05f },
    };

    [Header("Backboard bonus - do a perfect shot probability")]
    [SerializeField][Range(0f, 1f)] private float perfectBackboardBonusProbability = 0.5f;

    [Header("Ball Spawn Configuration")]
    [SerializeField] private float ballSpawnForwardDistance = 0.5f;
    [SerializeField] private float cpuY = 1f;

    // Components on the same GameObject
    private BallShooterController ballShooterController;
    private ShootingPositionController shootingPositionController;

    // State
    private GameObject currentBall;
    private bool isShooting = false;
    private bool isBackboardBonusActive = false;
    private float totalWeight = 0f; // used to normalize in case the sum of weights is not 1

    private void Awake()
    {
        ballShooterController = GetComponent<BallShooterController>();
        shootingPositionController = GetComponent<ShootingPositionController>();

        CalculateTotalWeight();
    }

    private void OnEnable()
    {
        // same subscription logic as PlayerController, but without the input handler since the CPU doesn't receive input events
        shootingPositionController.OnNewRoundGenerated += HandleNewRound;
        ballShooterController.OnShotCompleted += HandleShotCompleted;
        gameController.OnBackboardBonusActivated += HandleBackboardBonusActivated; 
        gameController.OnBackboardBonusReset += HandleBackboardBonusReset;         
    }

    private void OnDisable()
    {
        shootingPositionController.OnNewRoundGenerated -= HandleNewRound;
        ballShooterController.OnShotCompleted -= HandleShotCompleted;
        gameController.OnBackboardBonusActivated -= HandleBackboardBonusActivated; 
        gameController.OnBackboardBonusReset -= HandleBackboardBonusReset;         
    }

    private void HandleBackboardBonusActivated()
    {
        isBackboardBonusActive = true;
    }

    // NEW
    private void HandleBackboardBonusReset()
    {
        isBackboardBonusActive = false;
    }

    // Called by GameController via ShootingPositionController.OnNewRoundGenerated
    private void HandleNewRound()
    {
        MoveToCurrentPosition();
        SpawnBall();

        if (!isShooting)
            StartCoroutine(ShootingLoop());
    }

    private void HandleShotCompleted(ShotType shotType)
    {
        BallPoolController.instance.ReturnBall(currentBall, 2f);
        currentBall = null;
        isShooting = false;

        shootingPositionController.AdvancePosition();

        // AdvancePosition fires OnNewRoundGenerated if a new round starts,
        // or OnPositionChanged if still within the same round
        if (shootingPositionController.GetCurrentPositionIndex() != 0)
        {
            MoveToCurrentPosition();
            SpawnBall();
            StartCoroutine(ShootingLoop());
        }
        // If index == 0, a new round was generated -> HandleNewRound will be called via event
    }

    // Waits a random interval then jumps and shoots
    private IEnumerator ShootingLoop()
    {
        isShooting = true;

        float waitTime = Random.Range(shootingRateMin, shootingRateMax);
        yield return new WaitForSeconds(waitTime);

        ShotType selectedShot = SelectShotType();
        currentBall.GetComponent<BallController>().CurrentShotType = selectedShot;

        // CPU jumps like the player
        transform.DOLocalJump(transform.position, 1f, 1, 1f);
        yield return new WaitForSeconds(0.5f); // Wait for jump peak

        ExecuteShot(selectedShot);
    }

    // If bonus is active, first roll for PerfectBackboard with its dedicated probability.
    // If the roll fails, fall back to the normal weighted random selection.
    private ShotType SelectShotType()
    {
        if (isBackboardBonusActive && Random.value <= perfectBackboardBonusProbability)
        {
            Debug.Log("[CPUController] Bonus roll succeeded — forcing PerfectBackboard.");
            return ShotType.PerfectBackboard;
        }

        return SelectWeightedShotType();
    }

    private void ExecuteShot(ShotType shotType)
    {
        Debug.Log($"[CPUController] Shooting: {shotType}");

        switch (shotType)
        {
            case ShotType.Perfect: ballShooterController.StartPerfectShot(1f); break;
            case ShotType.Imperfect: ballShooterController.StartImperfectShot(); break;
            case ShotType.Short:
                ballShooterController.GetShortShotPowerRange(out float shortMin, out float shortMax);
                float shortShotPower = Random.Range(shortMin, shortMax);
                ballShooterController.StartShortShot(shortShotPower); 
                break;
            case ShotType.PerfectBackboard: ballShooterController.StartPerfectBackboardShot(); break;
            case ShotType.LowerBackboard: ballShooterController.StartLowerBackboardShot(); break;
            case ShotType.UpperBackboard: ballShooterController.StartUpperBackboardShot(); break;
        }
    }

    private void MoveToCurrentPosition()
    {
        Vector3 position = shootingPositionController.GetCurrentPosition().WorldPosition;
        position.y = cpuY;
        transform.position = position;
        OrientTowardsRim();

        Debug.Log($"[CPUController] Moved to position {shootingPositionController.GetCurrentPositionIndex()}: {position}");
    }

    private void OrientTowardsRim()
    {
        Vector3 dir = rimTransformReference.position - transform.position;
        dir.y = 0f;
        if (dir != Vector3.zero)
            transform.rotation = Quaternion.LookRotation(dir);
    }

    private Vector3 GetBallSpawnPosition()
    {
        return transform.position + transform.forward * ballSpawnForwardDistance;
    }

    private void SpawnBall()
    {
        currentBall = BallPoolController.instance.GetBall(GetBallSpawnPosition(), true, transform);

        BallController ballController = currentBall.GetComponent<BallController>();
        ballController.Owner = GameEntity.CPU;

        ballShooterController.SetBall(currentBall.transform, GameEntity.CPU);

        Debug.Log($"[CPUController] Ball spawned at: {currentBall.transform.position}");
    }

    // Calculate total weight once on Awake to avoid recalculating every shot
    private void CalculateTotalWeight()
    {
        totalWeight = 0f;
        foreach (ShotTypeWeight entry in shotWeights)
            totalWeight += entry.weight;
    }

    // Weighted random selection - O(n), fine for n<=6
    // creates a division for each shot type proportional to its weight, then rolls a random number to select the shot type
    // based on those divisions    
    private ShotType SelectWeightedShotType()
    {
        float roll = Random.Range(0f, totalWeight);
        float cumulative = 0f;

        foreach (ShotTypeWeight entry in shotWeights)
        {
            cumulative += entry.weight;
            if (roll <= cumulative)
                return entry.shotType;
        }

        // Security return — should never reach here
        return ShotType.Perfect;
    }
}