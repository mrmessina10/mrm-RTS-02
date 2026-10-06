using UnityEngine;
using System.Collections.Generic;

// Etapa de caminos del generador: traza cada ruta con A* sobre la grilla de celdas, con un costo que mezcla ruido
// (curvas orgánicas), pendiente, agua y cercanía al borde. La ruta principal va primero; las de los spawns extra
// apuntan al mismo destino pagando menos por las celdas que ya son camino, así se le unen y comparten el tramo final.
public static class MapPathRouter
{
    private const int Stage = 3;
    private const float SlopeCost = 1.5f;
    private const float EdgeCost = 8f;
    private const float StartAvoidCost = 40f;
    private const float RoadReuseFactor = 0.25f;
    private const float Diagonal = 1.41421356f;

    private static readonly int[] NeighborX = { 1, -1, 0, 0, 1, 1, -1, -1 };
    private static readonly int[] NeighborZ = { 0, 0, 1, -1, 1, -1, 1, -1 };

    public static void Route(MapGenerationContext context)
    {
        MapRandom random = context.CreateRandom(Stage);
        MapLayout layout = context.Layout;

        context.PathWidth = random.Range(context.Request.Path.Width);

        float[] cost = BuildCostGrid(context, random);
        bool[] isRoad = new bool[cost.Length];
        int[] mainCellIndex = new int[cost.Length];
        for (int i = 0; i < mainCellIndex.Length; i++)
        {
            mainCellIndex[i] = -1;
        }

        MapRoute main = new MapRoute { SpawnIndex = 0 };
        List<Vector2Int> accessCells = null;

        if (layout.Type == MapLayoutType.PathToStart)
        {
            main.Cells = FindPath(context, cost, 1f, layout.Spawns[0], layout.PlayerStart);
            TrimNearStart(context, main.Cells);
            context.DefenseCellIndex = main.Cells.Count - 1;
        }
        else
        {
            float[] mainCost = (float[])cost.Clone();
            AddDiscCost(context, mainCost, layout.PlayerStart, context.Request.Start.ClearRadius + 4f, StartAvoidCost);

            main.Cells = FindPath(context, mainCost, 1f, layout.Spawns[0], layout.Junction);
            context.DefenseCellIndex = main.Cells.Count - 1;

            List<Vector2Int> exitCells = FindPath(context, mainCost, 1f, layout.Junction, layout.WaveEnd);
            main.Cells.AddRange(exitCells.GetRange(1, exitCells.Count - 1));

            accessCells = FindPath(context, cost, 1f, layout.Junction, layout.PlayerStart);
            TrimNearStart(context, accessCells);
        }

        main.LengthToDefense = context.DefenseCellIndex;
        main.SharedStretch = context.DefenseCellIndex;
        BuildWaypoints(context, main, 0, main.Cells.Count - 1);
        main.Waypoints[0] = layout.Spawns[0];
        if (layout.HasWaveEnd) main.Waypoints[main.Waypoints.Count - 1] = layout.WaveEnd;
        context.Routes.Add(main);

        MarkRoad(context, main.Cells, cost, isRoad);
        for (int i = 0; i < main.Cells.Count; i++)
        {
            MarkMainCell(context, mainCellIndex, main.Cells[i], i);
        }

        for (int spawnIndex = 1; spawnIndex < layout.Spawns.Count; spawnIndex++)
        {
            Vector2 defensePoint = MapGenerationContext.CellCenter(main.Cells[context.DefenseCellIndex]);
            List<Vector2Int> branchCells = FindPath(context, cost, RoadReuseFactor, layout.Spawns[spawnIndex], defensePoint);

            MapRoute branch = BuildBranch(context, main, mainCellIndex, branchCells, spawnIndex);
            branch.Waypoints[0] = layout.Spawns[spawnIndex];
            context.Routes.Add(branch);

            MarkRoad(context, branchCells, cost, isRoad);
        }

        if (accessCells != null)
        {
            MapRoute access = new MapRoute { Cells = accessCells };
            BuildWaypoints(context, access, 0, accessCells.Count - 1);
            access.Waypoints[0] = main.Waypoints[FindNearestWaypoint(main, layout.Junction)];
            context.Routes.Add(access);
        }

        foreach (MapRoute route in context.Routes)
        {
            List<Vector3> waypoints = new List<Vector3>(route.Waypoints.Count);
            foreach (Vector2 waypoint in route.Waypoints)
            {
                waypoints.Add(new Vector3(waypoint.x, 0f, waypoint.y));
            }
            route.Smoothed = MapTerrainShaper.ToPlanar(MapPath.GetSmoothedPoints(waypoints, 1f));
        }
    }

    private static float[] BuildCostGrid(MapGenerationContext context, MapRandom random)
    {
        MapPathRules rules = context.Request.Path;
        MapNoise noise = new MapNoise(random);
        float wander = random.Range(rules.Wander);
        float frequency = 1f / Mathf.Max(1f, rules.WanderScale);
        float maxSlope = context.Request.Terrain.MaxWalkableSlope;
        float waterLevel = context.Request.Water.WaterLevel;
        float margin = Mathf.Max(1f, context.Request.Layout.EdgeMargin);

        float[] cost = new float[context.Width * context.Depth];
        for (int z = 0; z < context.Depth; z++)
        {
            for (int x = 0; x < context.Width; x++)
            {
                int index = context.CellIndex(x, z);
                float centerX = x + 0.5f;
                float centerZ = z + 0.5f;

                float cellCost = 1f + wander * noise.Fractal(centerX * frequency, centerZ * frequency, 2);
                cellCost += context.CellSlope[index] > maxSlope ? rules.CliffCost : context.CellSlope[index] / maxSlope * SlopeCost;
                if (context.CellHeight[index] < waterLevel) cellCost += rules.WaterCost;

                float borderDistance = Mathf.Min(Mathf.Min(centerX, centerZ), Mathf.Min(context.Width - centerX, context.Depth - centerZ));
                if (borderDistance < margin) cellCost += EdgeCost * (1f - borderDistance / margin);

                cost[index] = cellCost;
            }
        }
        return cost;
    }

    private static void AddDiscCost(MapGenerationContext context, float[] cost, Vector2 center, float radius, float extraCost)
    {
        bool[] disc = new bool[cost.Length];
        context.MarkDisc(disc, center, radius);

        for (int i = 0; i < cost.Length; i++)
        {
            if (disc[i]) cost[i] += extraCost;
        }
    }

    private static void MarkRoad(MapGenerationContext context, List<Vector2Int> cells, float[] cost, bool[] isRoad)
    {
        foreach (Vector2Int cell in cells)
        {
            int index = context.CellIndex(cell.x, cell.y);
            if (isRoad[index]) continue;

            isRoad[index] = true;
            cost[index] *= RoadReuseFactor;
        }
    }

    private static void MarkMainCell(MapGenerationContext context, int[] mainCellIndex, Vector2Int cell, int routeIndex)
    {
        for (int z = Mathf.Max(0, cell.y - 1); z <= Mathf.Min(context.Depth - 1, cell.y + 1); z++)
        {
            for (int x = Mathf.Max(0, cell.x - 1); x <= Mathf.Min(context.Width - 1, cell.x + 1); x++)
            {
                int index = context.CellIndex(x, z);
                if (mainCellIndex[index] < 0) mainCellIndex[index] = routeIndex;
            }
        }
    }

    private static void TrimNearStart(MapGenerationContext context, List<Vector2Int> cells)
    {
        float stopDistance = context.Request.Start.PathStopDistance;
        while (cells.Count > 2 && Vector2.Distance(MapGenerationContext.CellCenter(cells[cells.Count - 1]), context.Layout.PlayerStart) < stopDistance)
        {
            cells.RemoveAt(cells.Count - 1);
        }
    }

    // Arma la ruta completa de un spawn extra: su tramo propio hasta tocar el camino principal y, desde ahí, los
    // mismos waypoints del principal, para que el tramo compartido sea exactamente el mismo camino.
    private static MapRoute BuildBranch(MapGenerationContext context, MapRoute main, int[] mainCellIndex, List<Vector2Int> branchCells, int spawnIndex)
    {
        int contact = branchCells.Count - 1;
        int mainIndex = context.DefenseCellIndex;
        for (int i = 0; i < branchCells.Count; i++)
        {
            int touched = mainCellIndex[context.CellIndex(branchCells[i].x, branchCells[i].y)];
            if (touched < 0) continue;

            contact = i;
            mainIndex = touched;
            break;
        }

        MapRoute branch = new MapRoute { SpawnIndex = spawnIndex };
        branch.Cells.AddRange(branchCells.GetRange(0, contact + 1));
        branch.Cells.AddRange(main.Cells.GetRange(mainIndex, main.Cells.Count - mainIndex));
        branch.SharedStretch = context.DefenseCellIndex - mainIndex;
        branch.LengthToDefense = contact + branch.SharedStretch;

        int spacing = Mathf.Max(2, context.Request.Path.WaypointSpacing);
        for (int i = 0; i < contact - spacing / 2; i += spacing)
        {
            branch.Waypoints.Add(MapGenerationContext.CellCenter(branchCells[i]));
            branch.WaypointCells.Add(i);
        }
        if (branch.Waypoints.Count == 0)
        {
            branch.Waypoints.Add(MapGenerationContext.CellCenter(branchCells[0]));
            branch.WaypointCells.Add(0);
        }

        for (int i = 0; i < main.Waypoints.Count; i++)
        {
            if (main.WaypointCells[i] < mainIndex + spacing / 2 && i < main.Waypoints.Count - 1) continue;

            branch.Waypoints.Add(main.Waypoints[i]);
            branch.WaypointCells.Add(contact + main.WaypointCells[i] - mainIndex);
        }

        return branch;
    }

    private static void BuildWaypoints(MapGenerationContext context, MapRoute route, int firstCell, int lastCell)
    {
        int spacing = Mathf.Max(2, context.Request.Path.WaypointSpacing);

        for (int i = firstCell; i < lastCell - spacing / 2; i += spacing)
        {
            route.Waypoints.Add(MapGenerationContext.CellCenter(route.Cells[i]));
            route.WaypointCells.Add(i);
        }

        route.Waypoints.Add(MapGenerationContext.CellCenter(route.Cells[lastCell]));
        route.WaypointCells.Add(lastCell);
    }

    private static int FindNearestWaypoint(MapRoute route, Vector2 point)
    {
        int nearest = 0;
        for (int i = 1; i < route.Waypoints.Count; i++)
        {
            if (Vector2.Distance(route.Waypoints[i], point) < Vector2.Distance(route.Waypoints[nearest], point)) nearest = i;
        }
        return nearest;
    }

    private static List<Vector2Int> FindPath(MapGenerationContext context, float[] cost, float heuristicScale, Vector2 from, Vector2 to)
    {
        int width = context.Width;
        int depth = context.Depth;
        Vector2Int startCell = context.ToCell(from);
        Vector2Int goalCell = context.ToCell(to);
        int startIndex = context.CellIndex(startCell.x, startCell.y);
        int goalIndex = context.CellIndex(goalCell.x, goalCell.y);

        float[] bestCost = new float[cost.Length];
        int[] parent = new int[cost.Length];
        bool[] closed = new bool[cost.Length];
        for (int i = 0; i < bestCost.Length; i++)
        {
            bestCost[i] = float.MaxValue;
            parent[i] = -1;
        }

        CellHeap open = new CellHeap(256);
        bestCost[startIndex] = 0f;
        open.Push(startIndex, 0f);

        while (open.Count > 0)
        {
            int current = open.Pop();
            if (closed[current]) continue;
            closed[current] = true;

            if (current == goalIndex) break;

            int currentX = current % width;
            int currentZ = current / width;

            for (int n = 0; n < NeighborX.Length; n++)
            {
                int nextX = currentX + NeighborX[n];
                int nextZ = currentZ + NeighborZ[n];
                if (nextX < 0 || nextZ < 0 || nextX >= width || nextZ >= depth) continue;

                int next = nextZ * width + nextX;
                if (closed[next]) continue;

                float stepCost = (cost[current] + cost[next]) * 0.5f * (n >= 4 ? Diagonal : 1f);
                float candidate = bestCost[current] + stepCost;
                if (candidate >= bestCost[next]) continue;

                bestCost[next] = candidate;
                parent[next] = current;

                int deltaX = Mathf.Abs(goalCell.x - nextX);
                int deltaZ = Mathf.Abs(goalCell.y - nextZ);
                float heuristic = (Mathf.Max(deltaX, deltaZ) + (Diagonal - 1f) * Mathf.Min(deltaX, deltaZ)) * heuristicScale;
                open.Push(next, candidate + heuristic);
            }
        }

        List<Vector2Int> cells = new List<Vector2Int>();
        for (int index = goalIndex; index >= 0; index = parent[index])
        {
            cells.Add(new Vector2Int(index % width, index / width));
        }
        cells.Reverse();
        return cells;
    }

    private class CellHeap
    {
        private int[] cells;
        private float[] priorities;

        public int Count { get; private set; }

        public CellHeap(int capacity)
        {
            cells = new int[capacity];
            priorities = new float[capacity];
        }

        public void Push(int cell, float priority)
        {
            if (Count == cells.Length)
            {
                System.Array.Resize(ref cells, Count * 2);
                System.Array.Resize(ref priorities, Count * 2);
            }

            int index = Count++;
            while (index > 0)
            {
                int parentIndex = (index - 1) / 2;
                if (priorities[parentIndex] <= priority) break;

                cells[index] = cells[parentIndex];
                priorities[index] = priorities[parentIndex];
                index = parentIndex;
            }

            cells[index] = cell;
            priorities[index] = priority;
        }

        public int Pop()
        {
            int result = cells[0];
            Count--;

            int lastCell = cells[Count];
            float lastPriority = priorities[Count];
            int index = 0;

            while (true)
            {
                int child = index * 2 + 1;
                if (child >= Count) break;
                if (child + 1 < Count && priorities[child + 1] < priorities[child]) child++;
                if (priorities[child] >= lastPriority) break;

                cells[index] = cells[child];
                priorities[index] = priorities[child];
                index = child;
            }

            cells[index] = lastCell;
            priorities[index] = lastPriority;
            return result;
        }
    }
}
