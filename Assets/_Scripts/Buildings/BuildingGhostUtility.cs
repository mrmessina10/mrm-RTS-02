using UnityEngine;
using UnityEngine.AI;

// Deja un GameObject instanciado de un prefab de edificio como ghost puramente visual: sin obstáculo de NavMesh,
// sin collider, sin ninguna lógica de gameplay real. Compartido por todos los controllers de colocación.
public static class BuildingGhostUtility
{
    public static void StripFunctionalComponents(GameObject instance)
    {
        foreach (var obstacle in instance.GetComponentsInChildren<NavMeshObstacle>()) Object.Destroy(obstacle);
        foreach (var collider in instance.GetComponentsInChildren<Collider>()) Object.Destroy(collider);
        foreach (var placement in instance.GetComponentsInChildren<BuildingPlacement>()) Object.Destroy(placement);
        foreach (var dropOff in instance.GetComponentsInChildren<DropOffBuilding>()) Object.Destroy(dropOff);
        foreach (var health in instance.GetComponentsInChildren<Health>()) Object.Destroy(health);
        foreach (var selection in instance.GetComponentsInChildren<UnitSelectionHandler>()) Object.Destroy(selection);
    }

    public static void Tint(GameObject instance, Color color)
    {
        foreach (var renderer in instance.GetComponentsInChildren<MeshRenderer>())
        {
            renderer.material.color = color;
        }
    }
}
