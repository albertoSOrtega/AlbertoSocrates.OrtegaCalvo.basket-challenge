using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShootingPositionController : MonoBehaviour
{
    [Header("Rim Reference")]
    [SerializeField] private Transform rimTransformReference;

    [Header("Semicircle Distance")]
    [SerializeField] private float nearMinDistance = 3f;
    [SerializeField] private float nearMaxDistance = 4f;
    [SerializeField] private float midMinDistance = 5f;
    [SerializeField] private float midMaxDistance = 6f;
    [SerializeField] private float farMinDistance = 7f;
    [SerializeField] private float farMaxDistance = 8f;

    [Header("Angle Configuration")]
    [Range(5, 10)]
    [SerializeField] private float firstShotMinAngle = 10f;
    [Range(15, 20)]
    [SerializeField] private float firstShotMaxAngle = 20f;
    [Range(65, 70)]
    [SerializeField] private float lastShotMinAngle = 70f;
    [Range(75, 80)]
    [SerializeField] private float lastShotMaxAngle = 80f;

    [Header("Gizmos")]
    [SerializeField] private bool showGizmos = true;
    [SerializeField] private float gizmoSphereRadius = 0.15f;

    public struct ShootingPositionData
    {
        public int SemicircleDistanceIndex { get; private set; } // 0: Near, 1: Mid, 2: Far
        public int ShotIndex { get; private set; } // 0 a 3
        public float Angle { get; private set; } // negativo=izquierda, positivo=derecha
        public float Distance { get; private set; } // metros al aro
        public bool IsZoneRight { get; private set; }
        public Vector3 WorldPosition { get; private set; }

        public ShootingPositionData(int semicircleDistanceIndex, int shotIndex, float angle, float distance, bool isZoneRight, Vector3 worldPosition)
        {
            SemicircleDistanceIndex = semicircleDistanceIndex;
            ShotIndex = shotIndex;
            Angle = angle;
            Distance = distance;
            IsZoneRight = isZoneRight;
            WorldPosition = worldPosition;
        }
    }

    // Events
    public event System.Action<ShootingPositionData> OnPositionChanged;

    // State
    private List<ShootingPositionData> currentRoundSemicirclePositions = new List<ShootingPositionData>();
    private int currentPositionIndex = 0;
    private bool isCurrentSemicircleRight = true;
    private int currentSemicircleDistanceIndex = 0;

    public void GenerateAllRoundSemicircles(bool isInitialOrientationRight)
    {
        currentRoundSemicirclePositions.Clear();
        currentSemicircleDistanceIndex = 0;

        bool isZoneRight = isInitialOrientationRight;
        List<float> generatedDistances = new List<float>();

        for (int semicircleIndex = 0; semicircleIndex < 3; semicircleIndex++)
        {
            // Gets the distance to the rim for the current semicircle based on the index
            float distance = GetRandomDistance(semicircleIndex);
            generatedDistances.Add(distance);

            // Generates the 4 shooting positions for the current semicircle and adds them to the list
            List<ShootingPositionData> positions = GenerateSemicirclePositions(semicircleIndex, distance, isZoneRight);
            currentRoundSemicirclePositions.AddRange(positions);

            // alternate zone every semicircle
            isZoneRight = !isZoneRight; 
        }

        Debug.Log($"New 12 Position Round Generated. \nStarting Orientation: {(isInitialOrientationRight ? "Right" : "Left")}" +
            $"\n Distance of the first Circle: {generatedDistances[0]}" + $"\n Distance of the second Circle: {generatedDistances[1]}"
            + $"\n Distance of the third Circle: {generatedDistances[2]}");

        DebugAllRoundPositions();
    }

    // Generates the 4 positions based on the distance. Angles are generated randomly based in the configuration and the orientation of
    // the semicircle's zone (right or left)
    private List<ShootingPositionData> GenerateSemicirclePositions(int semicircleIndex, float distance, bool isZoneRight)
    {
        List<ShootingPositionData> positions = new List<ShootingPositionData>();

        float orientationSign = isZoneRight ? 1f : -1f;
        float firstAngle = orientationSign * Random.Range(firstShotMinAngle, firstShotMaxAngle);
        float lastAngle = orientationSign * Random.Range(lastShotMinAngle, lastShotMaxAngle);
        float separationBetweenPositions = (lastAngle - firstAngle) / 3f;

        for (int i = 0; i < 4; i++)
        {
            float angle = firstAngle + separationBetweenPositions * i;
            Vector3 worldPosition = CalculateWorldPosition(angle, distance);

            ShootingPositionData data = new ShootingPositionData(
                semicircleDistanceIndex: semicircleIndex,
                shotIndex: i,
                angle: angle,
                distance: distance,
                isZoneRight: isZoneRight,
                worldPosition: worldPosition
            );

            positions.Add(data);
        }

        return positions;
    }

    // Tools

    private float GetRandomDistance(int semicircleIndex)
    {
        float distance;

        switch (semicircleIndex)
        {
            case 0:
                distance = Random.Range(nearMinDistance, nearMaxDistance);
                break;
            case 1:
                distance = Random.Range(midMinDistance, midMaxDistance);
                break;
            case 2:
                distance = Random.Range(farMinDistance, farMaxDistance);
                break;
            default:
                distance = Random.Range(nearMinDistance, nearMaxDistance);
                break;
        }

        return distance;
    }

    // Given an angle in the semicircle and the distance to the rim, Calculates the shooting ball world position relative to the rim transform (0,0).
    private Vector3 CalculateWorldPosition(float angleDegrees, float distance)
    {
        float angleRad = angleDegrees * Mathf.Deg2Rad;

        // 0° is directly in front of hoop (negative Z axis), right is positive in the X axis, left is negative in the X axis.
        // We calculate the position using basic trigonometry.
        float x = Mathf.Sin(angleRad) * distance;
        float z = Mathf.Cos(angleRad) * distance;

        return rimTransformReference.position + new Vector3(x, -rimTransformReference.position.y, z);
    }

    public void DebugAllRoundPositions()
    {
        // Show information in Debug Log
        foreach (ShootingPositionData positionData in currentRoundSemicirclePositions)
        {
            Debug.Log($"Semicircle Index: {positionData.SemicircleDistanceIndex}, Shot Index: {positionData.ShotIndex}," +
                $" Angle: {positionData.Angle}, Distance: {positionData.Distance}, Is Right Zone: {positionData.IsZoneRight}," +
                $" World Position: {positionData.WorldPosition}");
        }
    }

    // For visualization purposes in the editor, we draw gizmos for each shooting position with different colors based
    // on the semicircle distance and labels indicating the shot index.
    private void OnDrawGizmos()
    {
        if (!showGizmos || currentRoundSemicirclePositions.Count == 0) return;

        // One color per semicircle: Near/0 = Green, Mid/1 = Yellow, Far/2 = Red
        Color[] semicircleColors = { Color.green, Color.yellow, Color.red };

        for (int i = 0; i < currentRoundSemicirclePositions.Count; i++)
        {
            ShootingPositionData data = currentRoundSemicirclePositions[i];

            Gizmos.color = semicircleColors[data.SemicircleDistanceIndex];
            Gizmos.DrawSphere(data.WorldPosition, gizmoSphereRadius);

            // Draw shot index label
#if UNITY_EDITOR
            GUIStyle style = new GUIStyle();
            style.normal.textColor = semicircleColors[data.SemicircleDistanceIndex];
            style.fontStyle = FontStyle.Bold;
            style.fontSize = 14;
            UnityEditor.Handles.Label(data.WorldPosition + Vector3.up * 0.3f,
                $"S{data.SemicircleDistanceIndex}-P{data.ShotIndex}", style);
#endif

            // Draw line connecting consecutive positions within the same semicircle
            if (i > 0 && currentRoundSemicirclePositions[i - 1].SemicircleDistanceIndex == data.SemicircleDistanceIndex)
            {
                Gizmos.DrawLine(currentRoundSemicirclePositions[i - 1].WorldPosition, data.WorldPosition);
            }
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        bool randomOrientation = Random.value > 0.5f;
        isCurrentSemicircleRight = randomOrientation;
        GenerateAllRoundSemicircles(randomOrientation);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
