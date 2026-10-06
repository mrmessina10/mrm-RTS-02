using UnityEngine;
using UnityEngine.AI;

// Convierte al edificio en obstáculo estático de NavMesh una vez construido (carving sobre su footprint)
// y expone la validación de si un área de la grilla (1 unidad de Unity = 1 celda) está sobre NavMesh navegable antes de poder construirse ahí
[RequireComponent(typeof(NavMeshObstacle))]
public class BuildingPlacement : MonoBehaviour
{
    private const float OccupancyCheckHeight = 2f;
    private const float OccupancyCheckInset = 0.05f; // evita falsos positivos por colliders que solo rozan el borde de la celda
    private static int occupancyMask = -1;

    [SerializeField] private BuildingDataSO buildingData;

    private NavMeshObstacle obstacle;

    private void Awake()
    {
        obstacle = GetComponent<NavMeshObstacle>();
        ConfigureObstacle();
    }

    private void ConfigureObstacle()
    {
        Vector2Int footprint = buildingData.Footprint;

        obstacle.shape = NavMeshObstacleShape.Box;
        obstacle.size = new Vector3(footprint.x, obstacle.size.y, footprint.y);
        obstacle.center = Vector3.zero;
        obstacle.carving = true;
        obstacle.carveOnlyStationary = true; // el edificio nunca se mueve, evita recalcular el carve cada frame
    }

    // Esquina mínima (grid-aligned) del footprint dado un centro deseado, para que el área ocupe celdas enteras
    public static Vector3 GetFootprintOrigin(Vector3 desiredCenter, Vector2Int footprint)
    {
        float originX = Mathf.Round(desiredCenter.x - footprint.x / 2f);
        float originZ = Mathf.Round(desiredCenter.z - footprint.y / 2f);
        return new Vector3(originX, desiredCenter.y, originZ);
    }

    // Cada celda del footprint debe estar habilitada en la máscara edificable del mapa (si hay MapRoot), caer sobre
    // NavMesh navegable y no estar ocupada físicamente por un recurso u otro edificio
    public static bool IsAreaBuildable(Vector3 desiredCenter, Vector2Int footprint, float sampleTolerance = 0.1f)
    {
        Vector3 origin = GetFootprintOrigin(desiredCenter, footprint);

        for (int x = 0; x < footprint.x; x++)
        {
            for (int z = 0; z < footprint.y; z++)
            {
                Vector3 cellCenter = origin + new Vector3(x + 0.5f, 0f, z + 0.5f);

                if (MapRoot.Instance != null && !MapRoot.Instance.IsBuildable(cellCenter))
                    return false;

                if (IsCellOccupied(cellCenter))
                    return false;

                if (!NavMesh.SamplePosition(cellCenter, out NavMeshHit hit, sampleTolerance, NavMesh.AllAreas))
                    return false;

                Vector2 hitXZ = new Vector2(hit.position.x, hit.position.z);
                Vector2 cellXZ = new Vector2(cellCenter.x, cellCenter.z);
                if (Vector2.Distance(hitXZ, cellXZ) > sampleTolerance)
                    return false;
            }
        }

        return true;
    }

    private static bool IsCellOccupied(Vector3 cellCenter)
    {
        if (occupancyMask < 0) occupancyMask = LayerMask.GetMask("Resources", "Buildings");

        Vector3 boxCenter = cellCenter + Vector3.up * OccupancyCheckHeight / 2f;
        Vector3 halfExtents = new Vector3(0.5f - OccupancyCheckInset, OccupancyCheckHeight / 2f, 0.5f - OccupancyCheckInset);
        return Physics.CheckBox(boxCenter, halfExtents, Quaternion.identity, occupancyMask, QueryTriggerInteraction.Ignore);
    }
}
