using UnityEngine;
using static ShootingPositionController;

public enum ShotType { Perfect, Imperfect, Short, PerfectBackboard, LowerBackboard, UpperBackboard }

public class ShootingBarZoneController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ShootingPositionController shootingPositionController;
    [SerializeField] private ThrowBallInputHandler throwBallInputHandler;

    [Header("Perfect Shooting Zone")]
    [Range(0.1f, 0.2f)]
    public float perfectZoneSize = 0.1f;

    [Header("Imperfect Shooting Zone")]
    [Range(0.05f, 0.1f)]
    public float imperfectZoneSize = 0.05f;

    [Header("Backboard Zones")]
    [Range(0.05f, 0.2f)]
    public float backboardZoneSize = 0.1f;
    [Range(0.05f, 0.1f)]
    public float lowerBackboardSize = 0.05f; // Fixed size, does not depend on distance

    // --- Computed zone boundaries (public for UI use) ---
    [HideInInspector] public float perfectZoneStart;
    [HideInInspector] public float perfectZoneEnd;
    [HideInInspector] public float lowerBackboardStart;
    [HideInInspector] public float lowerBackboardEnd;
    [HideInInspector] public float backboardStart;
    [HideInInspector] public float backboardEnd;
    [HideInInspector] public float upperBackboardStart;
    // upperBackboardEnd is always 1f

    private const float UpperBackboardMinSize = 0.1f;

    // Event for drawing all zones once initialized
    public event System.Action OnShootingZonesInitialized;

    private void OnEnable()
    {
        shootingPositionController.OnNewRoundGenerated += InitializeZones;
        shootingPositionController.OnPositionChanged += UpdateZones;
    }

    private void OnDisable()
    {
        shootingPositionController.OnNewRoundGenerated -= InitializeZones;
        shootingPositionController.OnPositionChanged -= UpdateZones;
    }

    public void InitializeZones()
    {
        UpdateZones(shootingPositionController.GetCurrentPosition());
    }

    public void UpdateZones(ShootingPositionData shootingPositionData)
    {
        float maxDistance = shootingPositionController.GetFurthestDistance();
        float currentDistance = shootingPositionData.Distance;

        // Fixed sizes that never change
        float fixedSizes = imperfectZoneSize * 2f
                         + perfectZoneSize
                         + lowerBackboardSize
                         + backboardZoneSize
                         + UpperBackboardMinSize;

        // Space available for the ShortZone (before the first imperfectZone) + upperBackboard overflow
        float movableSpace = 1f - fixedSizes;

        if (movableSpace < 0f)
        {
            Debug.LogWarning("[ShootingBarZoneController] Zone sizes exceed 1.0! Reduce zone sizes.");
            movableSpace = 0f;
        }

        // t=0 -> closest (perfectZoneStart pushed down, upperBackboard is largest)
        // t=1 -> furthest (perfectZoneStart pushed up, upperBackboard is smallest = UpperBackboardMinSize)
        float t = Mathf.InverseLerp(0f, maxDistance, currentDistance);

        // How much of movableSpace goes to ShortZone vs upperBackboard overflow
        // At t=0 (close): ShortZone is small, upperBackboard gets the extra space
        // At t=1 (far):   ShortZone is large, upperBackboard shrinks to its minimum
        float shortZoneSize = Mathf.Lerp(0f, movableSpace, t);
        // upperBackboard gets the remaining movable space
        float upperBackboardSize = Mathf.Max(UpperBackboardMinSize, UpperBackboardMinSize + (movableSpace - shortZoneSize));

        // --- Build boundaries bottom to top ---

        // ShortZone: [0, shortZoneSize)
        // ImperfectZone1: [shortZoneSize, shortZoneSize + imperfectZoneSize)
        perfectZoneStart = shortZoneSize + imperfectZoneSize;
        perfectZoneEnd = perfectZoneStart + perfectZoneSize;
        // ImperfectZone2: [perfectZoneEnd, perfectZoneEnd + imperfectZoneSize)
        lowerBackboardStart = perfectZoneEnd + imperfectZoneSize;
        lowerBackboardEnd = lowerBackboardStart + lowerBackboardSize;
        backboardStart = lowerBackboardEnd;
        backboardEnd = backboardStart + backboardZoneSize;
        upperBackboardStart = backboardEnd;
        // upperBackboardEnd  = 1f (always)

        ValidateBoundaries();
        OnShootingZonesInitialized?.Invoke();
    }

    private void ValidateBoundaries()
    {
        if (upperBackboardStart > 1f)
        {
            Debug.LogWarning($"[ShootingBarZoneController] upperBackboardStart ({upperBackboardStart}) exceeds 1.0! Reduce zone sizes.");
        }
    }

    public bool IsInPerfectZone(float shootPower)
    {
        return shootPower >= perfectZoneStart && shootPower <= perfectZoneEnd;
    }

    public bool IsInImperfectZone(float shootPower)
    {
        float imperfectZone1Start = perfectZoneStart - imperfectZoneSize;
        float imperfectZone2End = perfectZoneEnd + imperfectZoneSize;

        return shootPower >= imperfectZone1Start
            && shootPower <= imperfectZone2End
            && !IsInPerfectZone(shootPower);
    }

    public bool IsInShortShotZone(float shootPower)
    {
        float minRegisteredPower = throwBallInputHandler.GetMinSwipeDistance()
                                 / throwBallInputHandler.GetMaxSwipeDistance();
        float imperfectZone1Start = perfectZoneStart - imperfectZoneSize;

        return shootPower >= minRegisteredPower && shootPower < imperfectZone1Start;
    }

    public bool IsInBackboardZone(float shootPower)
    {
        return shootPower >= backboardStart && shootPower <= backboardEnd;
    }

    public bool IsInLowerBackboardZone(float shootPower)
    {
        return shootPower >= lowerBackboardStart && shootPower < lowerBackboardEnd;
    }

    public bool IsInUpperBackboardZone(float shootPower)
    {
        return shootPower > backboardEnd && shootPower <= 1f;
    }

    public ShotType GetShotType(float shootPower)
    {
        switch (shootPower)
        {
            case float p when IsInPerfectZone(p):
                return ShotType.Perfect;

            case float p when IsInImperfectZone(p):
                return ShotType.Imperfect;

            case float p when IsInShortShotZone(p):
                return ShotType.Short;

            case float p when IsInBackboardZone(p):
                return ShotType.PerfectBackboard;

            case float p when IsInLowerBackboardZone(p):
                return ShotType.LowerBackboard;

            case float p when IsInUpperBackboardZone(p):
                return ShotType.UpperBackboard;

            // in case of bad functioning, perfect shot
            default:
                return ShotType.Perfect;
        }
    }
}