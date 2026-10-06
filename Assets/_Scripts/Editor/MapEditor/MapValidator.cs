using UnityEngine;
using UnityEngine.AI;
using UnityEditor;
using System.Collections.Generic;

public struct MapValidationMessage
{
    public MessageType Type;
    public string Text;
}

// Chequea que el mapa tenga lo mínimo para sostener el loop de juego: marcadores obligatorios, conexión por NavMesh
// entre cada spawn y el HQ / fin de oleada, camino trazado, recursos básicos y espacio edificable alrededor del inicio.
public static class MapValidator
{
    private const float NavMeshSampleDistance = 3f;
    private const int StartAreaRadius = 8;
    private const float MinStartBuildableRatio = 0.4f;

    public static List<MapValidationMessage> Validate(MapRoot mapRoot)
    {
        List<MapValidationMessage> messages = new List<MapValidationMessage>();

        List<MapMarker> spawns = new List<MapMarker>();
        List<MapMarker> playerStarts = new List<MapMarker>();
        List<MapMarker> waveEnds = new List<MapMarker>();

        foreach (MapMarker marker in mapRoot.GetMarkers())
        {
            if (marker.MarkerType == MapMarkerType.EnemySpawn) spawns.Add(marker);
            else if (marker.MarkerType == MapMarkerType.PlayerStart) playerStarts.Add(marker);
            else waveEnds.Add(marker);

            if (mapRoot.MapData != null && marker.Position.y < mapRoot.MapData.WaterLevel)
            {
                Add(messages, MessageType.Error, $"El marcador '{marker.name}' está bajo el agua.");
            }
        }

        if (spawns.Count == 0) Add(messages, MessageType.Error, "Falta al menos un Spawn de enemigos.");
        if (playerStarts.Count == 0) Add(messages, MessageType.Error, "Falta el Inicio del jugador (HQ).");
        if (playerStarts.Count > 1) Add(messages, MessageType.Error, "Hay más de un Inicio del jugador.");
        if (waveEnds.Count > 1) Add(messages, MessageType.Error, "Hay más de un Fin de oleada.");
        if (waveEnds.Count == 0) Add(messages, MessageType.Info, "Sin Fin de oleada: se asume que el camino termina en el HQ.");

        ValidateConnectivity(messages, spawns, playerStarts, "el Inicio del jugador");
        ValidateConnectivity(messages, spawns, waveEnds, "el Fin de oleada");
        ValidatePaths(messages, mapRoot);
        ValidateResources(messages, mapRoot);

        if (playerStarts.Count == 1) ValidateStartArea(messages, mapRoot, playerStarts[0].Position);

        if (messages.TrueForAll(message => message.Type != MessageType.Error))
        {
            Add(messages, MessageType.Info, "El mapa cumple los requisitos mínimos del loop de juego.");
        }

        return messages;
    }

    private static void ValidateConnectivity(List<MapValidationMessage> messages, List<MapMarker> spawns, List<MapMarker> targets, string targetLabel)
    {
        foreach (MapMarker target in targets)
        {
            if (!NavMesh.SamplePosition(target.Position, out NavMeshHit targetHit, NavMeshSampleDistance, NavMesh.AllAreas))
            {
                Add(messages, MessageType.Error, $"'{target.name}' no está sobre NavMesh. ¿Falta hacer el bake?");
                continue;
            }

            foreach (MapMarker spawn in spawns)
            {
                if (!NavMesh.SamplePosition(spawn.Position, out NavMeshHit spawnHit, NavMeshSampleDistance, NavMesh.AllAreas))
                {
                    Add(messages, MessageType.Error, $"'{spawn.name}' no está sobre NavMesh. ¿Falta hacer el bake?");
                    continue;
                }

                NavMeshPath path = new NavMeshPath();
                bool connected = NavMesh.CalculatePath(spawnHit.position, targetHit.position, NavMesh.AllAreas, path) && path.status == NavMeshPathStatus.PathComplete;
                if (!connected) Add(messages, MessageType.Error, $"No hay ruta caminable desde '{spawn.name}' hasta {targetLabel}.");
            }
        }
    }

    private static void ValidatePaths(List<MapValidationMessage> messages, MapRoot mapRoot)
    {
        MapPath[] paths = mapRoot.GetPaths();
        if (paths.Length == 0)
        {
            Add(messages, MessageType.Warning, "No hay ningún camino trazado.");
            return;
        }

        foreach (MapPath path in paths)
        {
            if (path.Waypoints.Count < 2) Add(messages, MessageType.Warning, $"El camino '{path.name}' tiene menos de 2 waypoints.");
        }
    }

    private static void ValidateResources(List<MapValidationMessage> messages, MapRoot mapRoot)
    {
        Dictionary<ResourceType, int> counts = new Dictionary<ResourceType, int>();
        if (mapRoot.ResourcesContainer != null)
        {
            foreach (ResourceNode node in mapRoot.ResourcesContainer.GetComponentsInChildren<ResourceNode>())
            {
                counts.TryGetValue(node.ResourceType, out int count);
                counts[node.ResourceType] = count + 1;
            }
        }

        foreach (ResourceType type in new[] { ResourceType.Wood, ResourceType.Food })
        {
            if (!counts.ContainsKey(type)) Add(messages, MessageType.Warning, $"No hay nodos de {type} en el mapa.");
        }
    }

    private static void ValidateStartArea(List<MapValidationMessage> messages, MapRoot mapRoot, Vector3 startPosition)
    {
        if (mapRoot.MapData == null) return;

        Vector2Int centerCell = mapRoot.MapData.WorldToCell(startPosition);
        int buildable = 0;
        int total = 0;

        for (int x = -StartAreaRadius; x <= StartAreaRadius; x++)
        {
            for (int z = -StartAreaRadius; z <= StartAreaRadius; z++)
            {
                total++;
                if (mapRoot.MapData.IsCellBuildable(centerCell + new Vector2Int(x, z))) buildable++;
            }
        }

        if (buildable < total * MinStartBuildableRatio)
        {
            Add(messages, MessageType.Warning, $"Solo {buildable} de {total} celdas alrededor del Inicio del jugador son edificables.");
        }
    }

    private static void Add(List<MapValidationMessage> messages, MessageType type, string text)
    {
        messages.Add(new MapValidationMessage { Type = type, Text = text });
    }
}
