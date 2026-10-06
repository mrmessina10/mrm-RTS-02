using UnityEngine;
using System.Collections.Generic;

// Etapa de objetos del generador, en orden de prioridad: depósitos iniciales junto al HQ, depósitos grandes de
// expansión con su claro para el drop-off, conjuntos en anillo (aldeas, ruinas) y por último las capas de
// decoración con muestreo de Poisson. Cada objeto respeta el espaciado de su entrada de paleta y las zonas despejadas.
public class MapObjectScatterer
{
    private const int Stage = 4;
    private const int BucketSize = 4;
    private const int ClusterPlacementTries = 80;
    private const int LandmarkPlacementTries = 40;
    private const int DartsPerNode = 25;
    private const int PoissonCandidates = 20;
    private const float ClusterRadiusFactor = 0.85f;
    private const float OpenGroundRatio = 0.85f;
    private const float StarterSeparation = 7f;
    private const float MinClusterSeparation = 5f;
    private const float NearestClusterBand = 0.35f;
    private const float DropOffRadius = 3.5f;
    private const float LandmarkMaxSlope = 30f;

    private readonly MapGenerationContext context;
    private readonly MapRandom random;
    private readonly List<int>[] buckets;
    private readonly int bucketsWide;
    private readonly int bucketsDeep;
    private readonly List<Vector2> placedPositions = new List<Vector2>();
    private readonly List<float> placedSpacings = new List<float>();
    private float maxPlacedSpacing;

    public MapObjectScatterer(MapGenerationContext context)
    {
        this.context = context;
        random = context.CreateRandom(Stage);

        bucketsWide = context.Width / BucketSize + 1;
        bucketsDeep = context.Depth / BucketSize + 1;
        buckets = new List<int>[bucketsWide * bucketsDeep];
    }

    public void Populate()
    {
        PlaceStarterResources();
        PlaceExpansionClusters();
        PlaceLandmarks();
        PlaceScatterLayers();
    }

    private void PlaceStarterResources()
    {
        Vector2 start = context.Layout.PlayerStart;

        foreach (MapResourceRule rule in context.Request.Resources)
        {
            List<int> entries = context.Request.Catalog.GetResourceEntries(rule.Type);
            if (entries.Count == 0)
            {
                context.MissingResourceTypes.Add(rule.Type);
                continue;
            }

            int entryIndex = entries[random.Index(entries.Count)];
            int nodeCount = random.Range(rule.StarterNodes);
            float radius = GetClusterRadius(entryIndex, nodeCount);

            for (int attempt = 0; attempt < ClusterPlacementTries; attempt++)
            {
                Vector2 center = start + random.Direction() * random.Range(rule.StarterDistance);
                if (!IsOpenGround(center, radius + 1f)) continue;
                if (GetPathDistance(center) < context.PathWidth / 2f + radius + 1.5f) continue;
                if (context.Clusters.Exists(cluster => Vector2.Distance(cluster.Center, center) < cluster.Radius + radius + StarterSeparation)) continue;

                int placed = PlaceClump(entries, center, radius, nodeCount);
                context.Clusters.Add(new MapResourceCluster { Type = rule.Type, IsStarter = true, Center = center, Radius = radius, NodeCount = placed, DropOffSpot = start });
                break;
            }
        }
    }

    // Reparte por rondas (un depósito de cada recurso por vuelta) para que ningún recurso se quede sin lugar porque
    // otro ocupó antes los mejores sitios; si un depósito no entra con su tamaño sorteado, prueba con el mínimo.
    private void PlaceExpansionClusters()
    {
        List<MapResourceRule> rules = context.Request.Resources;
        int[] clusterCounts = new int[rules.Count];
        int rounds = 0;

        for (int i = 0; i < rules.Count; i++)
        {
            clusterCounts[i] = random.Range(rules[i].ExpansionClusters);
            rounds = Mathf.Max(rounds, clusterCounts[i]);
        }

        for (int clusterIndex = 0; clusterIndex < rounds; clusterIndex++)
        {
            for (int i = 0; i < rules.Count; i++)
            {
                List<int> entries = context.Request.Catalog.GetResourceEntries(rules[i].Type);
                if (entries.Count == 0 || clusterIndex >= clusterCounts[i]) continue;

                int entryIndex = entries[random.Index(entries.Count)];
                if (TryPlaceExpansionCluster(rules[i], entries, entryIndex, random.Range(rules[i].ClusterSize), clusterIndex == 0)) continue;

                TryPlaceExpansionCluster(rules[i], entries, entryIndex, rules[i].ClusterSize.x, clusterIndex == 0);
            }
        }
    }

    private bool TryPlaceExpansionCluster(MapResourceRule rule, List<int> entries, int entryIndex, int nodeCount, bool isNearest)
    {
        MapLayout layout = context.Layout;
        Vector2 start = layout.PlayerStart;
        float separation = Mathf.Max(MinClusterSeparation, context.Extent * 0.08f);
        float spawnClearance = context.Request.Start.SpawnClearRadius + 6f;
        float radius = GetClusterRadius(entryIndex, nodeCount);

        float minDistance = rule.ExpansionDistance.x * context.Extent;
        float maxDistance = rule.ExpansionDistance.y * context.Extent;
        if (isNearest) maxDistance = Mathf.Lerp(minDistance, maxDistance, NearestClusterBand);

        for (int attempt = 0; attempt < ClusterPlacementTries; attempt++)
        {
            Vector2 center = start + random.Direction() * random.Range(minDistance, maxDistance);
            if (!context.IsInsideMargin(center, context.Request.Layout.EdgeMargin + radius)) continue;
            if (!IsOpenGround(center, radius + 2f)) continue;
            if (GetPathDistance(center) < context.PathWidth / 2f + radius + rule.PathClearance) continue;
            if (context.Clusters.Exists(cluster => Vector2.Distance(cluster.Center, center) < cluster.Radius + radius + separation)) continue;
            if (layout.Spawns.Exists(spawn => Vector2.Distance(spawn, center) < radius + spawnClearance)) continue;

            Vector2 dropOffSpot = center + (start - center).normalized * (radius + DropOffRadius + 1f);
            if (!IsOpenGround(dropOffSpot, DropOffRadius)) continue;
            if (context.GetDiscRatio(dropOffSpot, DropOffRadius, index => !context.NotBuildable[index]) < OpenGroundRatio) continue;

            int placed = PlaceClump(entries, center, radius, nodeCount);
            context.MarkDisc(context.KeepClear, dropOffSpot, DropOffRadius);
            context.Clusters.Add(new MapResourceCluster { Type = rule.Type, Center = center, Radius = radius, NodeCount = placed, DropOffSpot = dropOffSpot });
            return true;
        }

        return false;
    }

    private void PlaceLandmarks()
    {
        MapLayout layout = context.Layout;
        List<MapLandmarkRule> rules = context.Request.Landmarks;

        for (int ruleIndex = 0; ruleIndex < rules.Count; ruleIndex++)
        {
            MapLandmarkRule rule = rules[ruleIndex];
            List<int> entries = context.Request.Catalog.LandmarkEntries[ruleIndex];
            if (entries.Count == 0) continue;

            int count = random.Range(rule.Count);
            for (int i = 0; i < count; i++)
            {
                for (int attempt = 0; attempt < LandmarkPlacementTries; attempt++)
                {
                    float radius = random.Range(rule.Radius);
                    float margin = context.Request.Layout.EdgeMargin + radius;
                    if (context.Width <= margin * 2f || context.Depth <= margin * 2f) break;

                    Vector2 center = new Vector2(random.Range(margin, context.Width - margin), random.Range(margin, context.Depth - margin));
                    if (Vector2.Distance(center, layout.PlayerStart) < rule.MinStartDistance * context.Extent) continue;
                    if (!IsOpenGround(center, radius + 1f)) continue;
                    if (GetPathDistance(center) < context.PathWidth / 2f + radius + 2f) continue;
                    if (context.Clusters.Exists(cluster => Vector2.Distance(cluster.Center, center) < cluster.Radius + radius + DropOffRadius * 2f + 2f)) continue;
                    if (layout.Spawns.Exists(spawn => Vector2.Distance(spawn, center) < radius + context.Request.Start.SpawnClearRadius + 4f)) continue;

                    PlaceRing(entries, center, radius, random.Range(rule.Pieces));
                    context.TexturePatches.Add(new GeneratedTexturePatch { Ground = MapGroundPatch.Dirt, Center = context.ToWorld(center), Radius = radius * 1.2f, Opacity = 0.7f });
                    break;
                }
            }
        }
    }

    private void PlaceRing(List<int> entries, Vector2 center, float radius, int pieceCount)
    {
        float startAngle = random.Range(0f, 360f);

        for (int i = 0; i < pieceCount; i++)
        {
            float angle = (startAngle + i * 360f / pieceCount + random.Range(-12f, 12f)) * Mathf.Deg2Rad;
            Vector2 offset = new Vector2(Mathf.Sin(angle), Mathf.Cos(angle)) * (radius * random.Range(0.6f, 1f));
            float yaw = Mathf.Atan2(-offset.x, -offset.y) * Mathf.Rad2Deg;

            TryPlace(entries[random.Index(entries.Count)], center + offset, yaw, false, LandmarkMaxSlope, true);
        }
    }

    private void PlaceScatterLayers()
    {
        Vector2 start = context.Layout.PlayerStart;
        List<MapScatterRule> rules = context.Request.Scatter;

        for (int ruleIndex = 0; ruleIndex < rules.Count; ruleIndex++)
        {
            MapScatterRule rule = rules[ruleIndex];
            List<int> entries = context.Request.Catalog.ScatterEntries[ruleIndex];
            if (entries.Count == 0 || rule.Spacing <= 0f) continue;

            MapNoise clumpNoise = new MapNoise(random);
            float clumpThreshold = rule.Clumping > 0f ? Mathf.Lerp(0.2f, 0.65f, rule.Clumping) : 0f;
            float clumpFrequency = 1f / Mathf.Max(1f, rule.ClumpScale);

            foreach (Vector2 sample in SamplePoissonDisk(rule.Spacing))
            {
                if (clumpNoise.Fractal(sample.x * clumpFrequency, sample.y * clumpFrequency, 2) < clumpThreshold) continue;
                if (!random.Chance(rule.Coverage)) continue;
                if (Vector2.Distance(sample, start) < rule.StartClearance) continue;
                if (GetPathDistance(sample) < context.PathWidth / 2f + rule.PathClearance) continue;

                TryPlace(entries[random.Index(entries.Count)], sample, 0f, true, rule.MaxSlope, true);
            }
        }
    }

    // Muestreo de disco de Poisson (Bridson): puntos al azar que nunca quedan a menos de radius entre sí
    private List<Vector2> SamplePoissonDisk(float radius)
    {
        float cellSize = radius / Mathf.Sqrt(2f);
        int gridWidth = Mathf.CeilToInt(context.Width / cellSize);
        int gridDepth = Mathf.CeilToInt(context.Depth / cellSize);
        int[] grid = new int[gridWidth * gridDepth];
        for (int i = 0; i < grid.Length; i++)
        {
            grid[i] = -1;
        }

        List<Vector2> samples = new List<Vector2>();
        List<int> active = new List<int>();

        Vector2 first = new Vector2(random.Range(0f, context.Width), random.Range(0f, context.Depth));
        samples.Add(first);
        active.Add(0);
        grid[Mathf.Min((int)(first.y / cellSize), gridDepth - 1) * gridWidth + Mathf.Min((int)(first.x / cellSize), gridWidth - 1)] = 0;

        while (active.Count > 0)
        {
            int activeIndex = random.Index(active.Count);
            Vector2 origin = samples[active[activeIndex]];
            bool found = false;

            for (int k = 0; k < PoissonCandidates; k++)
            {
                Vector2 candidate = origin + random.Direction() * random.Range(radius, radius * 2f);
                if (candidate.x < 0f || candidate.y < 0f || candidate.x >= context.Width || candidate.y >= context.Depth) continue;

                int gridX = Mathf.Min((int)(candidate.x / cellSize), gridWidth - 1);
                int gridZ = Mathf.Min((int)(candidate.y / cellSize), gridDepth - 1);
                if (HasNeighborWithin(samples, grid, gridWidth, gridDepth, gridX, gridZ, candidate, radius)) continue;

                grid[gridZ * gridWidth + gridX] = samples.Count;
                active.Add(samples.Count);
                samples.Add(candidate);
                found = true;
                break;
            }

            if (found) continue;

            active[activeIndex] = active[active.Count - 1];
            active.RemoveAt(active.Count - 1);
        }

        return samples;
    }

    private static bool HasNeighborWithin(List<Vector2> samples, int[] grid, int gridWidth, int gridDepth, int gridX, int gridZ, Vector2 candidate, float radius)
    {
        for (int z = Mathf.Max(0, gridZ - 2); z <= Mathf.Min(gridDepth - 1, gridZ + 2); z++)
        {
            for (int x = Mathf.Max(0, gridX - 2); x <= Mathf.Min(gridWidth - 1, gridX + 2); x++)
            {
                int sampleIndex = grid[z * gridWidth + x];
                if (sampleIndex >= 0 && Vector2.Distance(samples[sampleIndex], candidate) < radius) return true;
            }
        }
        return false;
    }

    // Cada nodo del depósito sortea su variante entre todos los prefabs del recurso (varios árboles distintos en un mismo bosque)
    private int PlaceClump(List<int> entries, Vector2 center, float radius, int nodeCount)
    {
        int placed = 0;
        for (int dart = 0; dart < nodeCount * DartsPerNode && placed < nodeCount; dart++)
        {
            Vector2 position = center + random.InsideUnitCircle() * radius;
            int entryIndex = entries.Count == 1 ? entries[0] : entries[random.Index(entries.Count)];

            if (TryPlace(entryIndex, position, 0f, true, context.Request.Terrain.MaxWalkableSlope, false)) placed++;
        }
        return placed;
    }

    private bool TryPlace(int entryIndex, Vector2 position, float yaw, bool useEntryYaw, float maxSlope, bool respectKeepClear)
    {
        MapGenerationCatalogEntry entry = context.Request.Catalog.Entries[entryIndex];
        if (!context.IsInsideMargin(position, context.Request.Layout.EdgeMargin)) return false;

        int cellIndex = context.CellIndex(position);
        if (context.TerrainBlocked[cellIndex] || context.CellSlope[cellIndex] > maxSlope) return false;
        if (respectKeepClear && context.KeepClear[cellIndex]) return false;
        if (IsTooClose(position, entry.Spacing)) return false;

        float scale = random.Range(entry.ScaleRange);
        if (useEntryYaw) yaw = entry.RandomYaw ? random.Range(0f, 360f) : 0f;

        context.Objects.Add(new GeneratedMapObject
        {
            CatalogIndex = entryIndex,
            Position = context.ToWorld(position) + Vector3.up * (entry.VerticalOffset * scale),
            Yaw = yaw,
            Scale = scale
        });

        int bucket = GetBucket(position);
        if (buckets[bucket] == null) buckets[bucket] = new List<int>();
        buckets[bucket].Add(placedPositions.Count);
        placedPositions.Add(position);
        placedSpacings.Add(entry.Spacing);
        maxPlacedSpacing = Mathf.Max(maxPlacedSpacing, entry.Spacing);

        if (entry.BlocksMovement) context.MarkDisc(context.Obstacle, position, Mathf.Max(0.75f, entry.Spacing * 0.5f));
        return true;
    }

    private bool IsTooClose(Vector2 position, float spacing)
    {
        float searchRadius = Mathf.Max(spacing, maxPlacedSpacing);
        int minX = Mathf.Max(0, (int)((position.x - searchRadius) / BucketSize));
        int maxX = Mathf.Min(bucketsWide - 1, (int)((position.x + searchRadius) / BucketSize));
        int minZ = Mathf.Max(0, (int)((position.y - searchRadius) / BucketSize));
        int maxZ = Mathf.Min(bucketsDeep - 1, (int)((position.y + searchRadius) / BucketSize));

        for (int z = minZ; z <= maxZ; z++)
        {
            for (int x = minX; x <= maxX; x++)
            {
                List<int> bucket = buckets[z * bucketsWide + x];
                if (bucket == null) continue;

                foreach (int placedIndex in bucket)
                {
                    if (Vector2.Distance(placedPositions[placedIndex], position) < Mathf.Max(spacing, placedSpacings[placedIndex])) return true;
                }
            }
        }
        return false;
    }

    private int GetBucket(Vector2 position)
    {
        int x = Mathf.Clamp((int)(position.x / BucketSize), 0, bucketsWide - 1);
        int z = Mathf.Clamp((int)(position.y / BucketSize), 0, bucketsDeep - 1);
        return z * bucketsWide + x;
    }

    private float GetClusterRadius(int entryIndex, int nodeCount)
    {
        return context.Request.Catalog.Entries[entryIndex].Spacing * Mathf.Sqrt(nodeCount) * ClusterRadiusFactor;
    }

    private float GetPathDistance(Vector2 position)
    {
        return context.PathDistance[context.CellIndex(position)];
    }

    private bool IsOpenGround(Vector2 center, float radius)
    {
        if (!context.IsInsideMargin(center, 1f) || !context.Reachable[context.CellIndex(center)]) return false;
        return context.GetDiscRatio(center, radius, index => context.Reachable[index] && !context.Obstacle[index]) >= OpenGroundRatio;
    }
}
