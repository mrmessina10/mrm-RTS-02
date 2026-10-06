using UnityEngine;
using UnityEditor;

// Herramienta de marcadores del loop de juego: spawn de enemigos (varios), fin de oleada e inicio del jugador
// (únicos: colocar otro mueve el existente).
[System.Serializable]
public class MarkerTool : MapEditorTool
{
    private const string MarkerTypeProperty = "<MarkerType>k__BackingField";
    private const float HandleSizeFactor = 0.15f;
    private const float CursorRadius = 1.5f;

    private static readonly string[] TypeLabels = { "Spawn de enemigos", "Fin de oleada", "Inicio del jugador" };
    private static readonly string[] TypeDescriptions =
    {
        "Punto por donde entran al mapa las oleadas y las caravanas. Puede haber varios.",
        "Punto donde el tráfico que no fue detenido sale del mapa (extremo B del camino). Único. Si no existe, el camino termina en el HQ.",
        "Posición del City Center (HQ) y del grupo inicial de workers/soldados; la cámara arranca centrada acá. Único."
    };

    [SerializeField] private MapMarkerType markerType = MapMarkerType.EnemySpawn;

    public override string DisplayName => "Marcadores";
    public override string Hint => "Click coloca el marcador elegido. Arrastrar un marcador lo mueve. Shift + click sobre uno lo borra.";

    public override float GetBrushRadius(MapEditorContext context) => CursorRadius;
    public override Color GetBrushColor(MapEditorContext context) => MapMarker.GetColor(markerType);

    public override void OnToolGUI(MapEditorContext context)
    {
        markerType = (MapMarkerType)GUILayout.SelectionGrid((int)markerType, TypeLabels, 1);
        EditorGUILayout.HelpBox(TypeDescriptions[(int)markerType], MessageType.None);
        EditorGUILayout.Space();

        foreach (MapMarker marker in context.Root.GetMarkers())
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(marker.name);
            if (GUILayout.Button("Ver", GUILayout.Width(40f)) && SceneView.lastActiveSceneView != null)
            {
                SceneView.lastActiveSceneView.LookAt(marker.Position);
            }
            if (GUILayout.Button("Borrar", GUILayout.Width(60f)))
            {
                Undo.DestroyObjectImmediate(marker.gameObject);
                GUIUtility.ExitGUI();
            }
            EditorGUILayout.EndHorizontal();
        }
    }

    public override void OnSceneGUI(MapEditorContext context)
    {
        foreach (MapMarker marker in context.Root.GetMarkers())
        {
            Vector3 position = marker.Position;
            float handleSize = HandleUtility.GetHandleSize(position) * HandleSizeFactor;

            if (context.Shift)
            {
                Handles.color = Color.red;
                if (Handles.Button(position, Quaternion.identity, handleSize, handleSize * 1.5f, Handles.SphereHandleCap))
                {
                    Undo.DestroyObjectImmediate(marker.gameObject);
                    break;
                }
                continue;
            }

            Handles.color = MapMarker.GetColor(marker.MarkerType);
            EditorGUI.BeginChangeCheck();
            Handles.FreeMoveHandle(position, handleSize, Vector3.zero, Handles.SphereHandleCap);
            if (EditorGUI.EndChangeCheck() && context.HasHit)
            {
                Undo.RecordObject(marker.transform, "Mapa: mover marcador");
                marker.transform.position = context.HitPoint;
            }
        }
    }

    public override void OnStrokeBegin(MapEditorContext context)
    {
        if (context.Shift) return;

        if (markerType != MapMarkerType.EnemySpawn)
        {
            MapMarker existing = context.Root.GetMarker(markerType);
            if (existing != null)
            {
                Undo.RecordObject(existing.transform, "Mapa: mover marcador");
                existing.transform.position = context.HitPoint;
                return;
            }
        }

        CreateMarker(context.Root, markerType, context.HitPoint, Quaternion.identity);
    }

    public static MapMarker CreateMarker(MapRoot root, MapMarkerType type, Vector3 position, Quaternion rotation)
    {
        int sameTypeCount = 0;
        foreach (MapMarker marker in root.GetMarkers())
        {
            if (marker.MarkerType == type) sameTypeCount++;
        }

        GameObject markerObject = new GameObject(type == MapMarkerType.EnemySpawn ? $"{type} {sameTypeCount + 1}" : type.ToString());
        markerObject.transform.SetParent(root.MarkersContainer, false);
        markerObject.transform.SetPositionAndRotation(position, rotation);

        MapMarker created = markerObject.AddComponent<MapMarker>();
        SerializedObject serializedMarker = new SerializedObject(created);
        serializedMarker.FindProperty(MarkerTypeProperty).enumValueIndex = (int)type;
        serializedMarker.ApplyModifiedPropertiesWithoutUndo();

        Undo.RegisterCreatedObjectUndo(markerObject, "Mapa: colocar marcador");
        return created;
    }
}
