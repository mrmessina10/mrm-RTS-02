using UnityEngine;

// Estado compartido que la ventana del editor de mapas le pasa a la herramienta activa en cada evento
public class MapEditorContext
{
    public MapRoot Root;
    public Terrain Terrain;
    public MapDataSO MapData;
    public MapPaletteSO Palette;
    public bool HasHit;
    public Vector3 HitPoint;
    public bool Shift;
    public bool Control;

    public TerrainData TerrainData => Terrain.terrainData;

    public float SampleHeight(Vector3 worldPosition)
    {
        return Terrain.SampleHeight(worldPosition) + Terrain.transform.position.y;
    }

    public Vector3 SnapToTerrain(Vector3 worldPosition)
    {
        return new Vector3(worldPosition.x, SampleHeight(worldPosition), worldPosition.z);
    }
}

// Base de cada herramienta del editor de mapas. La ventana resuelve el raycast contra el terreno, el cursor de
// pincel y el ciclo de trazo (click, arrastre con pasos a ritmo fijo, soltar); cada herramienta solo implementa
// sus parámetros y qué hace en cada momento del trazo.
[System.Serializable]
public abstract class MapEditorTool
{
    public abstract string DisplayName { get; }
    public abstract string Hint { get; }

    public virtual float GetBrushRadius(MapEditorContext context) => 0f;
    public virtual Color GetBrushColor(MapEditorContext context) => Color.white;

    public virtual void OnActivated(MapEditorContext context) { }
    public virtual void OnDeactivated() { }
    public virtual void OnUndoRedo() { }
    public virtual void OnToolGUI(MapEditorContext context) { }
    public virtual void OnSceneGUI(MapEditorContext context) { }
    public virtual void OnStrokeBegin(MapEditorContext context) { }
    public virtual void OnStrokeStep(MapEditorContext context, float deltaTime) { }
    public virtual void OnStrokeEnd(MapEditorContext context) { }
}
