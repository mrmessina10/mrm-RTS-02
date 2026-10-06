using UnityEngine;

// Raíz del mapa en la escena: referencia el MapDataSO activo, el Terrain, el agua y los contenedores donde viven
// los objetos colocados, los marcadores del loop de juego y los caminos. Es el punto de consulta del gameplay
// para todo lo que dependa del mapa (celdas edificables, spawns, punto de inicio del jugador, caminos).
[DefaultExecutionOrder(-100)]
public class MapRoot : MonoBehaviour
{
    public static MapRoot Instance { get; private set; }

    [field: SerializeField] public MapDataSO MapData { get; private set; }
    [field: SerializeField] public Terrain Terrain { get; private set; }
    [field: SerializeField] public Transform Water { get; private set; }
    [field: SerializeField] public Transform ResourcesContainer { get; private set; }
    [field: SerializeField] public Transform NatureContainer { get; private set; }
    [field: SerializeField] public Transform ManMadeContainer { get; private set; }
    [field: SerializeField] public Transform MarkersContainer { get; private set; }
    [field: SerializeField] public Transform PathsContainer { get; private set; }

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Debug.LogWarning($"[MapRoot] Hay más de un MapRoot en la escena; se ignora '{name}'.");
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    public Transform GetContainer(MapObjectCategory category)
    {
        switch (category)
        {
            case MapObjectCategory.Resource: return ResourcesContainer;
            case MapObjectCategory.Nature: return NatureContainer;
            default: return ManMadeContainer;
        }
    }

    public bool IsBuildable(Vector3 worldPosition)
    {
        return MapData == null || MapData.IsBuildable(worldPosition);
    }

    public MapMarker[] GetMarkers()
    {
        return MarkersContainer != null ? MarkersContainer.GetComponentsInChildren<MapMarker>() : new MapMarker[0];
    }

    public MapMarker GetMarker(MapMarkerType type)
    {
        foreach (MapMarker marker in GetMarkers())
        {
            if (marker.MarkerType == type) return marker;
        }
        return null;
    }

    public MapPath[] GetPaths()
    {
        return PathsContainer != null ? PathsContainer.GetComponentsInChildren<MapPath>() : new MapPath[0];
    }
}
