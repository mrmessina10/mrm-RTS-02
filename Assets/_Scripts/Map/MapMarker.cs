using UnityEngine;

// Punto del mapa con significado para el loop de juego: spawn de enemigos, fin de oleada o inicio del jugador (HQ)
public class MapMarker : MonoBehaviour
{
    private const float PoleHeight = 4f;
    private const float HeadRadius = 0.6f;
    private const float BaseRadius = 1.5f;

    [field: SerializeField] public MapMarkerType MarkerType { get; private set; }

    public Vector3 Position => transform.position;

    public static Color GetColor(MapMarkerType type)
    {
        switch (type)
        {
            case MapMarkerType.EnemySpawn: return new Color(0.9f, 0.15f, 0.15f);
            case MapMarkerType.WaveEnd: return new Color(0.95f, 0.6f, 0.1f);
            default: return new Color(0.2f, 0.5f, 1f);
        }
    }

    private void OnDrawGizmos()
    {
        Vector3 head = transform.position + Vector3.up * PoleHeight;

        Gizmos.color = GetColor(MarkerType);
        Gizmos.DrawLine(transform.position, head);
        Gizmos.DrawSphere(head, HeadRadius);
        Gizmos.DrawWireSphere(transform.position, BaseRadius);

#if UNITY_EDITOR
        UnityEditor.Handles.Label(head + Vector3.up, name);
#endif
    }
}
