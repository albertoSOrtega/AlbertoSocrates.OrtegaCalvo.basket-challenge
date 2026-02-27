using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteAlways]
public class CourtLinesGeneratorController : MonoBehaviour
{
    public enum CourtMode { HalfCourt, FullCourt }

    [Header("Mode")]
    public CourtMode mode = CourtMode.HalfCourt;

    [Header("Court dimensions")]
    public float courtWidth = 15f;
    public float courtLength = 28f;

    [Header("Rim reference")]
    public Transform rimTransform;
    public Vector2 rimXZ = Vector2.zero;
    public float rimToBaseline = 1.575f;

    [Header("FIBA old key - Trapezoid")]
    public float keyWidthAtBaseline = 6.0f;
    public float keyWidthAtFTLine = 3.6f;
    public float baselineToFTLine = 5.8f;

    [Header("Circles & arcs")]
    public float freeThrowCircleRadius = 1.8f;
    public float centerCircleRadius = 1.8f;
    public float threePointRadius = 6.25f;
    [Range(12, 256)] public int arcSegments = 96;

    [Header("Line style")]
    public float lineWidth = 0.05f;
    public float yOffset = 0.01f;
    public Material lineMaterial;
    public Color lineColor = Color.white;

    [Header("Naming")]
    public string childRootName = "CourtLines";

    [Header("Root GameObject")]
    public GameObject gameObjectRoot;

    private bool _isRebuilding = false;

    void OnValidate()
    {
        if (_isRebuilding) return;
#if UNITY_EDITOR
        EditorApplication.delayCall -= SafeRebuild;
        EditorApplication.delayCall += SafeRebuild;
#endif
    }

    void OnEnable() => SafeRebuild();

    // Rebuilds the lines if the're not rebuilding
    private void SafeRebuild()
    {
        if (this == null) return;
        _isRebuilding = true;
        Rebuild();
        _isRebuilding = false;
    }

    // Builds the court lines based on the current settings. It clears existing lines and redraws everything.
    public void Rebuild()
    {
        var root = GetOrCreateRoot();
        ClearAllLines(root);

        Vector2 rimNear = GetRimXZ();
        float halfW = courtWidth * 0.5f;
        float zBaselineNear = rimNear.y - rimToBaseline;
        float zMidcourt = zBaselineNear + (courtLength * 0.5f);

        float zTop;
        if (mode == CourtMode.FullCourt)
        {
            zTop = zBaselineNear + courtLength;
        }
        else
        {
            zTop = zMidcourt;
        }

        // Perimeter
        DrawLine(root, "Baseline_Near", new Vector3(-halfW, yOffset, zBaselineNear), new Vector3(halfW, yOffset, zBaselineNear));
        DrawLine(root, "Sideline_Left", new Vector3(-halfW, yOffset, zBaselineNear), new Vector3(-halfW, yOffset, zTop));
        DrawLine(root, "Sideline_Right", new Vector3(halfW, yOffset, zBaselineNear), new Vector3(halfW, yOffset, zTop));
        DrawLine(root, "Midcourt_Line", new Vector3(-halfW, yOffset, zMidcourt), new Vector3(halfW, yOffset, zMidcourt));

        if (mode == CourtMode.FullCourt)
        {
            float zBaselineFar = zBaselineNear + courtLength;
            DrawLine(root, "Baseline_Far", new Vector3(-halfW, yOffset, zBaselineFar), new Vector3(halfW, yOffset, zBaselineFar));

            // Full center circle
            DrawCircle(root, "CenterCircle", new Vector3(0, yOffset, zMidcourt), centerCircleRadius, arcSegments);

            // Near basket
            DrawBasketSide(root, "Near", rimNear, zBaselineNear, 1f);

            // Far basket
            Vector2 rimFar = new Vector2(rimNear.x, zBaselineFar - rimToBaseline);
            DrawBasketSide(root, "Far", rimFar, zBaselineFar, -1f);
        }
        else
        {
            // Half center circle for half court
            DrawArc(root, "CenterCircleHalf", new Vector3(0, yOffset, zMidcourt), centerCircleRadius, 180, 360, arcSegments);
            DrawBasketSide(root, "Near", rimNear, zBaselineNear, 1f);
        }
    }

    // Draws a whole side of the court
    void DrawBasketSide(Transform root, string prefix, Vector2 rim, float zBaseline, float direction)
    {
        float halfBottom = keyWidthAtBaseline * 0.5f;
        float halfTop = keyWidthAtFTLine * 0.5f;
        float zFTLine = zBaseline + (baselineToFTLine * direction);

        Vector3 p1 = new Vector3(rim.x - halfBottom, yOffset, zBaseline);
        Vector3 p2 = new Vector3(rim.x + halfBottom, yOffset, zBaseline);
        Vector3 p3 = new Vector3(rim.x + halfTop, yOffset, zFTLine);
        Vector3 p4 = new Vector3(rim.x - halfTop, yOffset, zFTLine);
        DrawPolyline(root, $"{prefix}_Key", new[] { p1, p2, p3, p4, p1 });
        DrawLine(root, $"{prefix}_FT_Line", p4, p3);

        // Free throw circle
        DrawCircle(root, $"{prefix}_FT_Circle", new Vector3(rim.x, yOffset, zFTLine), freeThrowCircleRadius, arcSegments);

        // Three point line (arc + straight lines) 
        float radius = threePointRadius;
        float zArcCenter = rim.y; // The arc starts at the rim's Z height

        // Straight lines that connect to the baseline
        // Left side
        Vector3 straightL_Start = new Vector3(rim.x - radius, yOffset, zBaseline);
        Vector3 straightL_End = new Vector3(rim.x - radius, yOffset, zArcCenter);
        DrawLine(root, $"{prefix}_TripleLine_L", straightL_Start, straightL_End);

        // Right side
        Vector3 straightR_Start = new Vector3(rim.x + radius, yOffset, zBaseline);
        Vector3 straightR_End = new Vector3(rim.x + radius, yOffset, zArcCenter);
        DrawLine(root, $"{prefix}_TripleLine_R", straightR_Start, straightR_End);

        // Semicircle that connects both straight lines
        float startAngle, endAngle;
        if (direction > 0)
        {
            // Near basket (+Z): arc from 0 to 180 degrees
            startAngle = 0f;
            endAngle = 180f;
        }
        else
        {
            // Far basket (-Z): arc from 180 to 360 degrees
            startAngle = 180f;
            endAngle = 360f;
        }

        DrawArc(root, $"{prefix}_TripleArc", new Vector3(rim.x, yOffset, zArcCenter), radius, startAngle, endAngle, arcSegments);
    }

    // --- UTILITIES --- 
    void ClearAllLines(Transform root)
    {
        for (int i = root.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(root.GetChild(i).gameObject);
        }        
    }

    Transform GetOrCreateRoot()
    {
        Transform parent = (gameObjectRoot != null) ? gameObjectRoot.transform : this.transform;
        Transform t = parent.Find(childRootName);

        if (t != null)
        {
            // Already exists, clear it so it gets rebuilt fresh
            ClearAllLines(t);
        }
        else
        {
            GameObject go = new GameObject(childRootName);
            go.transform.SetParent(parent, false);
            t = go.transform;
        }

        return t;
    }

    void DrawLine(Transform root, string name, Vector3 a, Vector3 b)
    {
        var lr = CreateLR(root, name);
        lr.positionCount = 2;
        lr.SetPosition(0, a);
        lr.SetPosition(1, b);
    }

    void DrawPolyline(Transform root, string name, Vector3[] points)
    {
        var lr = CreateLR(root, name);
        lr.positionCount = points.Length;
        lr.SetPositions(points);
    }

    void DrawCircle(Transform root, string name, Vector3 center, float radius, int segments)
    {
        var lr = CreateLR(root, name);
        lr.loop = true;
        lr.positionCount = segments;
        for (int i = 0; i < segments; i++)
        {
            float ang = (i * 360f / segments) * Mathf.Deg2Rad;
            lr.SetPosition(i, new Vector3(center.x + Mathf.Cos(ang) * radius, yOffset, center.z + Mathf.Sin(ang) * radius));
        }
    }

    void DrawArc(Transform root, string name, Vector3 center, float radius, float start, float end, int segments)
    {
        var lr = CreateLR(root, name);
        lr.positionCount = segments + 1;
        for (int i = 0; i <= segments; i++)
        {
            float ang = Mathf.Lerp(start, end, i / (float)segments) * Mathf.Deg2Rad;
            lr.SetPosition(i, new Vector3(center.x + Mathf.Cos(ang) * radius, yOffset, center.z + Mathf.Sin(ang) * radius));
        }
    }

    LineRenderer CreateLR(Transform root, string name)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(root, false);
        var lr = go.AddComponent<LineRenderer>();
        lr.startWidth = lr.endWidth = lineWidth;
        lr.sharedMaterial = lineMaterial;
        lr.useWorldSpace = true;
        lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        lr.receiveShadows = false;
        return lr;
    }

    Vector2 GetRimXZ() => rimTransform != null ? new Vector2(rimTransform.position.x, rimTransform.position.z) : rimXZ;
}