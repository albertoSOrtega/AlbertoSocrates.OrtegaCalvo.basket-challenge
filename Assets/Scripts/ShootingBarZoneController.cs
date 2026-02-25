using UnityEngine;
using static ShootingPositionController;

public class ShootingBarZoneController : MonoBehaviour
{

    [Header("References")]
    [SerializeField] private ShootingPositionController shootingPositionController;

    [Header("Perfect Shooting Zone configuration parameters")]
    [Range(0.1f, 0.3f)]
    public float perfectZoneSize = 0.1f;

    [Header("Imperfect Shooting Zone configuration parameters")]
    [Range(0.1f, 0.3f)]
    public float imperfectZoneSize = 0.1f;

    [HideInInspector]
    public float perfectZoneStart;
    [HideInInspector]
    public float perfectZoneEnd;

    // Event for drawinf the perfect zone once initialized
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

    // Generate the position for the perfect zone and the imperfect zone based in the distance
    public void InitializeZones()
    {
        UpdateZones(shootingPositionController.GetCurrentPosition());
    }

    public void UpdateZones(ShootingPositionData shootingPositionData)
    {
        float maxDistance = shootingPositionController.GetFurthestDistance();
        float currentDistance = shootingPositionData.Distance;

        float totalZoneSize = perfectZoneSize + imperfectZoneSize * 2f;
        float barSize = 1f - totalZoneSize;

        float t = Mathf.InverseLerp(0f, maxDistance, currentDistance);
        perfectZoneStart = Mathf.Clamp(
            Mathf.Lerp(0f, barSize, t) + imperfectZoneSize,
            imperfectZoneSize,
            imperfectZoneSize + barSize
        );
        perfectZoneEnd = perfectZoneStart + perfectZoneSize;

        OnShootingZonesInitialized?.Invoke();
    }

    public bool IsInPerfectZone(float shootPower)
    {
        if (shootPower >= perfectZoneStart && shootPower <= perfectZoneEnd)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public bool isInImperfectZone(float shootPower)
    {
        float imperfectZoneStart = perfectZoneStart - imperfectZoneSize;
        float imperfectZoneEnd = perfectZoneEnd + imperfectZoneSize;

        if (shootPower >= imperfectZoneStart && shootPower <= imperfectZoneEnd)
        {
            // its in perfect zone, not imperfect
            if (IsInPerfectZone(shootPower))
            {
                return false; 
            }
            else
            {
                return true;
            }
        }
        else
        {
            return false; 
        }
    }
}