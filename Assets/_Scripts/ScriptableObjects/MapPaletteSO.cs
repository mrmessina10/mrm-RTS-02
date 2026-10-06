using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class MapPaletteEntry
{
    public string DisplayName;
    public GameObject Prefab;

    [Tooltip("Prefabs alternativos funcionalmente iguales al principal (misma lógica, otro aspecto). Al colocar se elige uno al azar entre el principal y estos")]
    public List<GameObject> Variants = new List<GameObject>();

    public MapObjectCategory Category;

    [Tooltip("Distancia mínima (XZ) a cualquier otro objeto del mapa al colocarlo con el pincel")]
    public float Spacing = 1.5f;

    [Tooltip("Rango de escala uniforme aleatoria aplicada sobre la escala del prefab")]
    public Vector2 ScaleRange = Vector2.one;

    [Tooltip("Altura sobre el terreno a la que queda el pivot (para prefabs con pivot en el centro)")]
    public float VerticalOffset;

    public bool RandomYaw = true;

    [Tooltip("Alinea la posición al centro de la celda de grilla (1 unidad = 1 celda)")]
    public bool SnapToGrid;

    public List<GameObject> GetPrefabs()
    {
        List<GameObject> prefabs = new List<GameObject>();
        if (Prefab != null) prefabs.Add(Prefab);

        foreach (GameObject variant in Variants)
        {
            if (variant != null && !prefabs.Contains(variant)) prefabs.Add(variant);
        }
        return prefabs;
    }

    public bool Contains(GameObject prefab)
    {
        return prefab != null && (Prefab == prefab || Variants.Contains(prefab));
    }
}

// Catálogo de prefabs que el editor de mapas puede colocar, agrupados por categoría, con los parámetros de pincel
// de cada uno. Una entrada puede tener varios prefabs intercambiables (el principal y sus variantes de aspecto).
[CreateAssetMenu(fileName = "New Map Palette", menuName = "ScriptableObjects/MapPaletteSO")]
public class MapPaletteSO : ScriptableObject
{
    [field: SerializeField] public List<MapPaletteEntry> Entries { get; private set; } = new List<MapPaletteEntry>();

    public MapPaletteEntry FindEntry(GameObject prefab)
    {
        return Entries.Find(entry => entry.Contains(prefab));
    }
}
