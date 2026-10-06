using UnityEngine;
using System.Collections.Generic;

// Camino del mapa: lista ordenada de waypoints en espacio de mundo más un ancho. Lo comparten oleadas y caravanas.
// Expone la polilínea suavizada que usan las herramientas de editor para pintar/nivelar y el gameplay para recorrerlo.
public class MapPath : MonoBehaviour
{
    [SerializeField] private List<Vector3> waypoints = new List<Vector3>();

    [field: SerializeField] public float Width { get; private set; } = 4f;

    public IReadOnlyList<Vector3> Waypoints => waypoints;

    public List<Vector3> GetSmoothedPoints(float stepLength = 1f)
    {
        return GetSmoothedPoints(waypoints, stepLength);
    }

    public static List<Vector3> GetSmoothedPoints(IReadOnlyList<Vector3> waypoints, float stepLength)
    {
        List<Vector3> points = new List<Vector3>();
        if (waypoints.Count == 0) return points;

        for (int i = 0; i < waypoints.Count - 1; i++)
        {
            Vector3 previous = waypoints[Mathf.Max(i - 1, 0)];
            Vector3 start = waypoints[i];
            Vector3 end = waypoints[i + 1];
            Vector3 next = waypoints[Mathf.Min(i + 2, waypoints.Count - 1)];

            int steps = Mathf.Max(1, Mathf.CeilToInt(Vector3.Distance(start, end) / stepLength));
            for (int step = 0; step < steps; step++)
            {
                points.Add(EvaluateCatmullRom(previous, start, end, next, step / (float)steps));
            }
        }

        points.Add(waypoints[waypoints.Count - 1]);
        return points;
    }

    private static Vector3 EvaluateCatmullRom(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
    {
        float t2 = t * t;
        float t3 = t2 * t;

        return 0.5f * (2f * p1
                       + (p2 - p0) * t
                       + (2f * p0 - 5f * p1 + 4f * p2 - p3) * t2
                       + (3f * p1 - p0 - 3f * p2 + p3) * t3);
    }

    private void OnDrawGizmos()
    {
        if (waypoints.Count == 0) return;

        Gizmos.color = new Color(1f, 0.85f, 0.2f);

        List<Vector3> points = GetSmoothedPoints(1f);
        for (int i = 0; i < points.Count - 1; i++)
        {
            Gizmos.DrawLine(points[i] + Vector3.up * 0.2f, points[i + 1] + Vector3.up * 0.2f);
        }

        foreach (Vector3 waypoint in waypoints)
        {
            Gizmos.DrawWireSphere(waypoint, 0.4f);
        }
    }
}
