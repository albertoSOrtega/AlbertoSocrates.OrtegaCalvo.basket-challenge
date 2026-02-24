using UnityEngine;

public class PerfectZoneController : MonoBehaviour
{

    [Header("Perfect Shooting configuration parameters")]
    [Range(0.1f, 0.3f)]
    public float perfectZoneSize = 0.1f;
    [Range(0.15f, 0.3f)]
    public float perfectZoneStartMin = 0.15f;

    [HideInInspector]
    public float perfectZoneStart;
    [HideInInspector]
    public float perfectZoneEnd;

    // Event for drawinf the perfect zone once initialized
    public event System.Action OnPerfectZoneRandomized;

    private void Start()
    {
        RandomizePerfectZone();
    }

    // Generates a new random position for the perfect zone
    public void RandomizePerfectZone()
    {
        perfectZoneStart = Random.Range(perfectZoneStartMin, 1f - perfectZoneSize);
        perfectZoneEnd = perfectZoneStart + perfectZoneSize;

        OnPerfectZoneRandomized?.Invoke();
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
}