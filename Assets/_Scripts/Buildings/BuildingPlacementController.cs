using UnityEngine;
using System.Collections.Generic;

// Modo de colocación de edificios: recibe el pedido de InputReader.BuildRequestEvent, sigue al mouse con un
// ghost visual-only (sin lógica ni colliders reales), valida contra BuildingPlacement.IsAreaBuildable en vivo
// y confirma (gasta recursos + instancia el prefab real) o cancela con click.
public class BuildingPlacementController : MonoBehaviour
{
    public static BuildingPlacementController Instance { get; private set; }

    [Header("Architecture")]
    [SerializeField] private InputReader inputReader;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private LayerMask groundMask;

    [Header("Edificios disponibles para construir")]
    [SerializeField] private List<BuildingDataSO> availableBuildings;

    [Header("Ghost")]
    [SerializeField] private Color validColor = new Color(0f, 1f, 0f, 0.5f);
    [SerializeField] private Color invalidColor = new Color(1f, 0f, 0f, 0.5f);

    private BuildingDataSO pendingBuilding;
    private GameObject ghost;
    private bool isValidPlacement;
    private Vector2 pointerScreenPosition;

    public bool IsPlacing => pendingBuilding != null;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void OnEnable()
    {
        if (inputReader == null) return;

        inputReader.BuildRequestEvent += HandleBuildRequest;
        inputReader.PointerPositionEvent += HandlePointerPosition;
        inputReader.SelectEvent += HandleConfirm;
        inputReader.CommandEvent += HandleCancel;
    }

    private void OnDisable()
    {
        if (inputReader == null) return;

        inputReader.BuildRequestEvent -= HandleBuildRequest;
        inputReader.PointerPositionEvent -= HandlePointerPosition;
        inputReader.SelectEvent -= HandleConfirm;
        inputReader.CommandEvent -= HandleCancel;
    }

    private void Update()
    {
        if (!IsPlacing) return;

        UpdateGhostPosition();
    }

    private void HandlePointerPosition(Vector2 screenPosition)
    {
        pointerScreenPosition = screenPosition;
    }

    private void HandleBuildRequest(BuildingType type)
    {
        if (PlacementModeState.IsActive && !IsPlacing)
        {
            Debug.Log("[BuildingPlacementController] Ya hay otro modo de colocación activo, cancelalo primero (click derecho).");
            return;
        }

        BuildingDataSO data = FindBuildingData(type);
        if (data == null)
        {
            Debug.LogWarning($"[BuildingPlacementController] No hay BuildingDataSO cargado para {type}.");
            return;
        }

        if (data.BuildingPrefab == null)
        {
            Debug.LogWarning($"[BuildingPlacementController] {data.DisplayName} no tiene BuildingPrefab asignado.");
            return;
        }

        if (IsPlacing) EndPlacement(); // cambiar de edificio a mitad de colocación cancela el anterior

        StartPlacement(data);
    }

    private BuildingDataSO FindBuildingData(BuildingType type)
    {
        foreach (var data in availableBuildings)
        {
            if (data != null && data.BuildingType == type) return data;
        }
        return null;
    }

    private void StartPlacement(BuildingDataSO data)
    {
        pendingBuilding = data;
        ghost = Instantiate(data.BuildingPrefab);
        BuildingGhostUtility.StripFunctionalComponents(ghost);
        PlacementModeState.IsActive = true;
    }

    private void UpdateGhostPosition()
    {
        Ray ray = mainCamera.ScreenPointToRay(pointerScreenPosition);
        if (!Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, groundMask)) return;

        Vector3 origin = BuildingPlacement.GetFootprintOrigin(hit.point, pendingBuilding.Footprint);
        Vector3 center = origin + new Vector3(pendingBuilding.Footprint.x / 2f, 0f, pendingBuilding.Footprint.y / 2f);
        ghost.transform.position = center;

        isValidPlacement = BuildingPlacement.IsAreaBuildable(hit.point, pendingBuilding.Footprint);
        BuildingGhostUtility.Tint(ghost, isValidPlacement ? validColor : invalidColor);
    }

    private void HandleConfirm()
    {
        if (!IsPlacing) return;
        if (inputReader.IsLeftClickHeld) return; // SelectEvent dispara en press Y release; solo confirmamos en el release

        if (!isValidPlacement)
        {
            Debug.Log($"[BuildingPlacementController] Posición inválida para {pendingBuilding.DisplayName}.");
            return;
        }

        if (ResourceManager.Instance == null || !ResourceManager.Instance.HasEnoughResources(pendingBuilding.ConstructionCost))
        {
            Debug.Log($"[BuildingPlacementController] Recursos insuficientes para {pendingBuilding.DisplayName}.");
            return;
        }

        foreach (var cost in pendingBuilding.ConstructionCost)
        {
            ResourceManager.Instance.TrySpend(cost.Type, cost.Amount);
        }

        // No se instancia terminado: se coloca como cimiento (ConstructionSite) a la espera de que un worker lo construya
        GameObject building = Instantiate(pendingBuilding.BuildingPrefab, ghost.transform.position, Quaternion.identity);
        building.AddComponent<ConstructionSite>().Initialize(pendingBuilding);
        Debug.Log($"[BuildingPlacementController] Cimiento de {pendingBuilding.DisplayName} colocado en {ghost.transform.position}. Falta que un worker lo construya.");

        EndPlacement();
    }

    private void HandleCancel()
    {
        if (!IsPlacing) return;

        Debug.Log($"[BuildingPlacementController] Colocación de {pendingBuilding.DisplayName} cancelada.");
        EndPlacement();
    }

    private void EndPlacement()
    {
        if (ghost != null) Destroy(ghost);
        ghost = null;
        pendingBuilding = null;
        PlacementModeState.IsActive = false;
    }
}
