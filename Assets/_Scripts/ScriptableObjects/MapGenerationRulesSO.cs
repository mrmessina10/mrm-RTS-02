using UnityEngine;
using System.Collections.Generic;

public enum MapLayoutType
{
    PathToStart,
    PassThrough
}

public enum MapWaterMode
{
    None,
    Lakes,
    River,
    Coast
}

[System.Serializable]
public class MapLayoutRules
{
    [Tooltip("Peso relativo del layout Spawn → HQ en la punta del camino")]
    public float PathToStartWeight = 2f;

    [Tooltip("Peso relativo del layout A → B con el HQ al costado del camino")]
    public float PassThroughWeight = 1f;

    [Tooltip("Cantidad de spawns de enemigos (mín, máx)")]
    public Vector2Int SpawnCount = new Vector2Int(1, 2);

    [Tooltip("Celdas del borde del mapa que quedan sin gameplay; los spawns se apoyan sobre esa línea")]
    public int EdgeMargin = 6;

    [Tooltip("Profundidad del HQ medida desde el borde de entrada, como fracción del mapa (layout Spawn → HQ)")]
    public Vector2 StartDepth = new Vector2(0.62f, 0.8f);

    [Tooltip("Posición lateral del HQ como fracción del mapa")]
    public Vector2 StartLateral = new Vector2(0.3f, 0.7f);

    [Tooltip("Distancia del HQ al camino principal en celdas (layout A → B)")]
    public Vector2 StartPathOffset = new Vector2(16f, 24f);

    [Tooltip("Distancia mínima en línea recta de cada spawn al HQ, como fracción del lado menor del mapa")]
    public float MinSpawnDistance = 0.4f;

    [Tooltip("Separación mínima entre spawns, como fracción del lado menor del mapa")]
    public float MinSpawnSeparation = 0.25f;

    [Tooltip("Largo mínimo de la ruta de cada spawn hasta el punto que defiende el jugador, como fracción del lado menor")]
    public float MinRouteLength = 0.4f;

    [Tooltip("Largo mínimo del tramo final que comparten todas las rutas, como fracción del lado menor")]
    public float MinSharedStretch = 0.12f;
}

[System.Serializable]
public class MapStartRules
{
    [Tooltip("Radio en celdas de la zona de inicio: plana, sin relieve, agua ni decoración")]
    public int ClearRadius = 10;

    [Tooltip("Fracción mínima de celdas edificables dentro de la zona de inicio")]
    [Range(0f, 1f)] public float MinBuildableRatio = 0.6f;

    [Tooltip("Distancia en celdas del HQ a la que termina el camino")]
    public float PathStopDistance = 5f;

    [Tooltip("Radio en celdas despejado y no edificable alrededor de cada spawn y del fin de oleada")]
    public int SpawnClearRadius = 6;
}

[System.Serializable]
public class MapTerrainRules
{
    [Tooltip("Amplitud en unidades del relieve suave de base")]
    public Vector2 ReliefAmplitude = new Vector2(0.2f, 0.6f);

    [Tooltip("Longitud de onda en celdas del relieve de base")]
    public Vector2 ReliefWavelength = new Vector2(30f, 60f);

    [Tooltip("Cantidad de mesetas (mín, máx)")]
    public Vector2Int PlateauCount = new Vector2Int(1, 4);

    [Tooltip("Radio de cada meseta como fracción del lado menor del mapa")]
    public Vector2 PlateauRadius = new Vector2(0.06f, 0.13f);

    [Tooltip("Altura de cada nivel de meseta; el desnivel queda como acantilado no caminable")]
    public float LevelHeight = 2.5f;

    [Tooltip("Probabilidad de que una meseta tenga un segundo nivel encima")]
    [Range(0f, 1f)] public float SecondLevelChance = 0.3f;

    [Tooltip("Rampas de acceso por meseta (mín, máx). Con 0 la meseta es solo un obstáculo")]
    public Vector2Int RampsPerPlateau = new Vector2Int(1, 2);

    public float RampWidth = 5f;

    [Tooltip("Pendiente de las rampas (alto / largo)")]
    public float RampGradient = 0.3f;

    [Tooltip("Pendiente máxima en grados que el generador considera caminable (más estricta que la del NavMesh)")]
    public float MaxWalkableSlope = 35f;
}

[System.Serializable]
public class MapWaterRules
{
    [Tooltip("Altura del plano de agua del mapa; el suelo base está en 0")]
    [Range(-8f, -0.5f)] public float WaterLevel = -1f;

    public float NoneWeight = 1f;
    public float LakesWeight = 1.5f;
    public float RiverWeight = 1.5f;
    public float CoastWeight = 1f;

    [Tooltip("Profundidad de los cuerpos de agua por debajo del nivel de agua")]
    public float Depth = 2f;

    public Vector2Int LakeCount = new Vector2Int(1, 3);

    [Tooltip("Radio de cada lago como fracción del lado menor del mapa")]
    public Vector2 LakeRadius = new Vector2(0.05f, 0.1f);

    [Tooltip("Ancho del río en celdas")]
    public Vector2 RiverWidth = new Vector2(5f, 9f);

    [Tooltip("Vados del río (mín, máx). El camino cruza por uno si le queda a mano; si no, abre el suyo")]
    public Vector2Int Fords = new Vector2Int(1, 3);

    [Tooltip("Ancho en celdas de los vados")]
    public float FordWidth = 6f;

    [Tooltip("Ancho de la franja de mar como fracción del lado menor del mapa")]
    public Vector2 CoastDepth = new Vector2(0.08f, 0.16f);

    [Tooltip("Altura sobre el nivel de agua a la que quedan los vados")]
    public float FordClearance = 0.15f;
}

[System.Serializable]
public class MapPathRules
{
    [Tooltip("Ancho del camino en celdas")]
    public Vector2 Width = new Vector2(4f, 6f);

    [Tooltip("Cuánto se desvía el camino de la línea recta (peso del ruido en el costo del A*)")]
    public Vector2 Wander = new Vector2(1f, 4f);

    [Tooltip("Tamaño en celdas de las curvas que produce el desvío")]
    public float WanderScale = 14f;

    [Tooltip("Costo extra de cruzar un acantilado: cuanto más alto, más rodea el camino antes de cortar una meseta")]
    public float CliffCost = 25f;

    [Tooltip("Costo extra por celda de agua cruzada")]
    public float WaterCost = 12f;

    [Tooltip("Pendiente máxima a lo largo del camino (alto / largo) después de nivelarlo")]
    public float MaxGradient = 0.2f;

    [Tooltip("Celdas a cada lado del camino que quedan no edificables")]
    public float BuildMargin = 1f;

    [Tooltip("Separación en celdas entre waypoints del MapPath")]
    public int WaypointSpacing = 10;
}

[System.Serializable]
public class MapResourceRule
{
    public ResourceType Type;

    [Tooltip("Nodos del depósito inicial pegado al HQ (mín, máx)")]
    public Vector2Int StarterNodes = new Vector2Int(3, 4);

    [Tooltip("Distancia en celdas del depósito inicial al HQ")]
    public Vector2 StarterDistance = new Vector2(9f, 13f);

    [Tooltip("Cantidad de depósitos grandes lejos del HQ (mín, máx). El mínimo es obligatorio")]
    public Vector2Int ExpansionClusters = new Vector2Int(2, 4);

    [Tooltip("Nodos por depósito grande (mín, máx)")]
    public Vector2Int ClusterSize = new Vector2Int(8, 14);

    [Tooltip("Distancia de los depósitos grandes al HQ como fracción del lado menor. El primero cae en el tercio más cercano")]
    public Vector2 ExpansionDistance = new Vector2(0.2f, 0.55f);

    [Tooltip("Celdas libres entre el borde del depósito y el camino")]
    public float PathClearance = 4f;
}

[System.Serializable]
public class MapScatterRule
{
    public string Name;
    public List<GameObject> Prefabs = new List<GameObject>();

    [Tooltip("Distancia mínima entre objetos de esta capa (radio del muestreo de Poisson)")]
    public float Spacing = 3f;

    [Tooltip("Fracción de las muestras válidas que efectivamente se colocan")]
    [Range(0f, 1f)] public float Coverage = 0.5f;

    [Tooltip("0 reparte parejo; 1 concentra los objetos en manchones")]
    [Range(0f, 1f)] public float Clumping = 0.5f;

    [Tooltip("Tamaño en celdas de los manchones")]
    public float ClumpScale = 18f;

    [Tooltip("Celdas libres entre el borde del camino y los objetos de esta capa")]
    public float PathClearance = 2f;

    [Tooltip("Distancia mínima en celdas al HQ")]
    public float StartClearance = 14f;

    [Tooltip("Pendiente máxima en grados donde se colocan")]
    public float MaxSlope = 25f;
}

[System.Serializable]
public class MapLandmarkRule
{
    public string Name;
    public List<GameObject> Prefabs = new List<GameObject>();

    [Tooltip("Cantidad de conjuntos en el mapa (mín, máx)")]
    public Vector2Int Count = new Vector2Int(0, 2);

    [Tooltip("Piezas por conjunto, dispuestas en anillo mirando al centro (mín, máx)")]
    public Vector2Int Pieces = new Vector2Int(3, 6);

    [Tooltip("Radio del anillo en celdas")]
    public Vector2 Radius = new Vector2(5f, 8f);

    [Tooltip("Distancia mínima al HQ como fracción del lado menor del mapa")]
    public float MinStartDistance = 0.25f;
}

// Reglas de generación procedural de mapas: qué estructura tiene que cumplir todo mapa (inicio, rutas, recursos,
// área jugable) y dentro de qué rangos puede variar cada semilla (layout, relieve, agua, camino, decoración).
// Un asset por arquetipo de mapa; el generador solo lo lee.
[CreateAssetMenu(fileName = "New Map Generation Rules", menuName = "ScriptableObjects/MapGenerationRulesSO")]
public class MapGenerationRulesSO : ScriptableObject
{
    [field: SerializeField] public MapLayoutRules Layout { get; private set; } = new MapLayoutRules();
    [field: SerializeField] public MapStartRules Start { get; private set; } = new MapStartRules();
    [field: SerializeField] public MapTerrainRules Terrain { get; private set; } = new MapTerrainRules();
    [field: SerializeField] public MapWaterRules Water { get; private set; } = new MapWaterRules();
    [field: SerializeField] public MapPathRules Path { get; private set; } = new MapPathRules();

    [field: SerializeField] public List<MapResourceRule> Resources { get; private set; } = new List<MapResourceRule>
    {
        new MapResourceRule { Type = ResourceType.Wood },
        new MapResourceRule { Type = ResourceType.Food }
    };

    [field: SerializeField] public List<MapScatterRule> Scatter { get; private set; } = new List<MapScatterRule>();
    [field: SerializeField] public List<MapLandmarkRule> Landmarks { get; private set; } = new List<MapLandmarkRule>();

    [field: Tooltip("Intentos por semilla: si un intento rompe una regla obligatoria se descarta y se prueba el siguiente")]
    [field: SerializeField] public int MaxAttempts { get; private set; } = 12;

    [field: Tooltip("Fracción mínima del mapa que tiene que ser alcanzable desde el HQ y edificable")]
    [field: SerializeField, Range(0f, 1f)] public float MinPlayableArea { get; private set; } = 0.3f;
}
