using UnityEngine;
using System.Collections.Generic;

// Modo de colocación de la Empalizada: traza tiras rectas de segmentos de muro (celdas de 1x1) entre clicks
// sucesivos. El primer click fija el punto inicial; cada click siguiente confirma el tramo hasta ahí (paga costo
// por celda válida, saltea las inválidas) y ese mismo punto pasa a ser el nuevo inicio, encadenando tramos con
// cambios de dirección en una sola sesión. Click derecho cierra la sesión.
public class WallPlacementController : MonoBehaviour
{
    [Header("Architecture")]
    [SerializeField] private InputReader inputReader;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private LayerMask groundMask;

    [Header("Segmento de muro")]
    [SerializeField] private BuildingDataSO wallSegmentData;

    [Header("Ghost")]
    [SerializeField] private Color validColor = new Color(0f, 1f, 0f, 0.5f);
    [SerializeField] private Color invalidColor = new Color(1f, 0f, 0f, 0.5f);
    [SerializeField] private float groundRaycastHeight = 100f;

    private bool wallModeActive;
    private bool hasStartPoint;
    private Vector2Int startCell;
    private Vector2 pointerScreenPosition;
    private readonly List<GameObject> ghosts = new List<GameObject>();

    private void OnEnable()
    {
        if (inputReader == null) return;

        inputReader.WallBuildRequestEvent += HandleWallBuildRequest;
        inputReader.PointerPositionEvent += HandlePointerPosition;
        inputReader.SelectEvent += HandleConfirm;
        inputReader.CommandEvent += HandleCancel;
    }

    private void OnDisable()
    {
        if (inputReader == null) return;

        inputReader.WallBuildRequestEvent -= HandleWallBuildRequest;
        inputReader.PointerPositionEvent -= HandlePointerPosition;
        inputReader.SelectEvent -= HandleConfirm;
        inputReader.CommandEvent -= HandleCancel;
    }

    private void Update()
    {
        if (!wallModeActive) return;

        UpdateGhosts();
    }

    private void HandlePointerPosition(Vector2 screenPosition)
    {
        pointerScreenPosition = screenPosition;
    }

    private void HandleWallBuildRequest()
    {
        if (PlacementModeState.IsActive && !wallModeActive)
        {
            Debug.Log("[WallPlacementController] Ya hay otro modo de colocación activo, cancelalo primero (click derecho).");
            return;
        }

        if (wallSegmentData == null || wallSegmentData.BuildingPrefab == null)
        {
            Debug.LogWarning("[WallPlacementController] No hay wallSegmentData/BuildingPrefab asignado.");
            return;
        }

        wallModeActive = true;
        hasStartPoint = false;
        PlacementModeState.IsActive = true;
    }

    private void HandleConfirm()
    {
        if (!wallModeActive) return;
        if (inputReader.IsLeftClickHeld) return; // SelectEvent dispara en press Y release; solo confirmamos en el release

        if (!TryGetPointerCell(out Vector2Int clickedCell)) return;

        if (!hasStartPoint)
        {
            startCell = clickedCell;
            hasStartPoint = true;
            return;
        }

        BuildRun(startCell, clickedCell);
        startCell = clickedCell; // encadena: el final de este tramo es el inicio del próximo
    }

    private void HandleCancel()
    {
        if (!wallModeActive) return;

        Debug.Log("[WallPlacementController] Modo de trazado de muro cerrado.");
        EndWallMode();
    }

    private void EndWallMode()
    {
        ClearGhosts();
        wallModeActive = false;
        hasStartPoint = false;
        PlacementModeState.IsActive = false;
    }

    private void UpdateGhosts()
    {
        List<Vector2Int> cells;

        if (hasStartPoint && TryGetPointerCell(out Vector2Int endCell))
        {
            cells = RasterizeLine(startCell, endCell);
        }
        else if (!hasStartPoint && TryGetPointerCell(out Vector2Int hoverCell))
        {
            cells = new List<Vector2Int> { hoverCell }; // todavía sin punto inicial: preview de una sola celda
        }
        else
        {
            cells = new List<Vector2Int>();
        }

        while (ghosts.Count < cells.Count)
        {
            GameObject ghost = Instantiate(wallSegmentData.BuildingPrefab);
            BuildingGhostUtility.StripFunctionalComponents(ghost);
            ghosts.Add(ghost);
        }
        while (ghosts.Count > cells.Count)
        {
            Destroy(ghosts[ghosts.Count - 1]);
            ghosts.RemoveAt(ghosts.Count - 1);
        }

        for (int i = 0; i < cells.Count; i++)
        {
            if (!TryGetCellGroundPosition(cells[i], out Vector3 position)) continue;

            ghosts[i].transform.position = position;
            bool valid = BuildingPlacement.IsAreaBuildable(position, Vector2Int.one);
            BuildingGhostUtility.Tint(ghosts[i], valid ? validColor : invalidColor);
        }
    }

    private void BuildRun(Vector2Int from, Vector2Int to)
    {
        List<Vector2Int> cells = RasterizeLine(from, to);
        List<Vector3> validPositions = new List<Vector3>();

        foreach (var cell in cells)
        {
            if (!TryGetCellGroundPosition(cell, out Vector3 position)) continue;
            if (BuildingPlacement.IsAreaBuildable(position, Vector2Int.one)) validPositions.Add(position);
        }

        if (validPositions.Count == 0) return;

        List<ResourceCost> totalCost = new List<ResourceCost>();
        foreach (var cost in wallSegmentData.ConstructionCost)
        {
            totalCost.Add(new ResourceCost { Type = cost.Type, Amount = cost.Amount * validPositions.Count });
        }

        if (ResourceManager.Instance == null || !ResourceManager.Instance.HasEnoughResources(totalCost))
        {
            // Todo o nada por tramo: si no alcanza para el tramo completo, no se construye nada de este tramo
            Debug.Log($"[WallPlacementController] Recursos insuficientes para {validPositions.Count} segmentos de {wallSegmentData.DisplayName}. No se construye nada de este tramo.");
            return;
        }

        foreach (var cost in totalCost)
        {
            ResourceManager.Instance.TrySpend(cost.Type, cost.Amount);
        }

        foreach (var position in validPositions)
        {
            GameObject segment = Instantiate(wallSegmentData.BuildingPrefab, position, Quaternion.identity);
            segment.AddComponent<ConstructionSite>().Initialize(wallSegmentData);
        }

        Debug.Log($"[WallPlacementController] {validPositions.Count} cimientos de {wallSegmentData.DisplayName} colocados ({cells.Count - validPositions.Count} celdas salteadas por terreno inválido).");
    }

    private bool TryGetPointerCell(out Vector2Int cell)
    {
        Ray ray = mainCamera.ScreenPointToRay(pointerScreenPosition);
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, groundMask))
        {
            cell = WorldToCell(hit.point);
            return true;
        }
        cell = default;
        return false;
    }

    private Vector2Int WorldToCell(Vector3 worldPoint)
    {
        Vector3 origin = BuildingPlacement.GetFootprintOrigin(worldPoint, Vector2Int.one);
        return new Vector2Int(Mathf.RoundToInt(origin.x), Mathf.RoundToInt(origin.z));
    }

    // Cada celda samplea su propia altura de terreno con un raycast individual hacia abajo, así el muro acompaña
    // el relieve en vez de asumir una única altura para toda la tira.
    private bool TryGetCellGroundPosition(Vector2Int cell, out Vector3 position)
    {
        Vector3 origin = new Vector3(cell.x + 0.5f, groundRaycastHeight, cell.y + 0.5f);
        if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, groundRaycastHeight * 2f, groundMask))
        {
            position = hit.point;
            return true;
        }
        position = default;
        return false;
    }

    // Algoritmo de línea de Bresenham adaptado a grilla 2D: conecta dos celdas con una tira continua sin huecos,
    // en cualquier ángulo. Referencia: http://members.chello.at/easyfilter/bresenham.html
    private static List<Vector2Int> RasterizeLine(Vector2Int start, Vector2Int end)
    {
        List<Vector2Int> cells = new List<Vector2Int>();

        int x0 = start.x, y0 = start.y, x1 = end.x, y1 = end.y;
        int dx = Mathf.Abs(x1 - x0), sx = x0 < x1 ? 1 : -1;
        int dy = -Mathf.Abs(y1 - y0), sy = y0 < y1 ? 1 : -1;
        int err = dx + dy;

        while (true)
        {
            cells.Add(new Vector2Int(x0, y0));
            if (x0 == x1 && y0 == y1) break;

            int e2 = 2 * err;
            if (e2 >= dy) { err += dy; x0 += sx; }
            if (e2 <= dx) { err += dx; y0 += sy; }
        }

        return cells;
    }

    private void ClearGhosts()
    {
        foreach (var ghost in ghosts)
        {
            if (ghost != null) Destroy(ghost);
        }
        ghosts.Clear();
    }
}
