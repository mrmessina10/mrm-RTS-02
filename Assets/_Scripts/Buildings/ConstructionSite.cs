using UnityEngine;

// Estado "en construcción" de un edificio recién colocado. Acumula el tiempo que le dedican los workers
// hasta llegar a BuildingDataSO.ConstructionTime; al completarse habilita la funcionalidad real del edificio
// y se elimina a sí mismo (el GameObject sigue siendo el edificio, ya sin este componente).
public class ConstructionSite : MonoBehaviour, IConstructable
{
    private BuildingDataSO buildingData;
    private float elapsedBuildTime;
    private DropOffBuilding dropOffBuilding;

    public InteractionType Type => InteractionType.Build;
    public Vector3 Position => transform.position;
    public bool IsComplete => elapsedBuildTime >= buildingData.ConstructionTime;
    public float BuildProgress => buildingData.ConstructionTime <= 0f ? 1f : Mathf.Clamp01(elapsedBuildTime / buildingData.ConstructionTime);

    public void Initialize(BuildingDataSO data)
    {
        buildingData = data;
        elapsedBuildTime = 0f;

        // Componentes funcionales que quedan inertes hasta terminar de construirse.
        // Al sumar tipos de edificio nuevos (producción, defensa) sus componentes se agregan acá.
        dropOffBuilding = GetComponent<DropOffBuilding>();
        if (dropOffBuilding != null) dropOffBuilding.enabled = false;

        if (IsComplete) Complete();
    }

    public void AddBuildProgress(float deltaTime)
    {
        if (IsComplete) return;

        elapsedBuildTime += deltaTime;
        if (IsComplete) Complete();
    }

    private void Complete()
    {
        if (dropOffBuilding != null) dropOffBuilding.enabled = true;

        Debug.Log($"[ConstructionSite] {buildingData.DisplayName} terminado en {name}.");
        Destroy(this);
    }

    public Transform GetTransform() => transform;
    public void Interact(UnitController unit) { }
}
