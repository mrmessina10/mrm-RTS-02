using UnityEngine;
using UnityEngine.AI;

// Convierte al edificio en obstáculo estático de NavMesh una vez construido (carving sobre su footprint)
// y expone la validación de si un área de la grilla (1 unidad de Unity = 1 celda) está sobre NavMesh navegable antes de poder construirse ahí
[RequireComponent(typeof(NavMeshObstacle))]
public class BuildingPlacement : MonoBehaviour
{
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

    // Sampleando el centro de cada celda del footprint, confirma que toda el área cae sobre NavMesh navegable
    public static bool IsAreaBuildable(Vector3 desiredCenter, Vector2Int footprint, float sampleTolerance = 0.1f)
    {
        Vector3 origin = GetFootprintOrigin(desiredCenter, footprint);

        for (int x = 0; x < footprint.x; x++)
        {
            for (int z = 0; z < footprint.y; z++)
            {
                Vector3 cellCenter = origin + new Vector3(x + 0.5f, 0f, z + 0.5f);

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
}
