# MapGeneratorPanel

`Assets/_Scripts/Editor/MapEditor/MapGeneratorPanel.cs`

Sección **Generación procedural** de [MapEditorWindow](MapEditorWindow.md). Es la interfaz del generador ([MapGenerator](../../Map/Generation/MapGenerator.md)) sobre el mapa activo de la escena; el tamaño del mapa generado es el del `MapData` activo.

Flujo de uso:

1. **Reglas** — el asset [MapGenerationRulesSO](../../ScriptableObjects/MapGenerationRulesSO.md). Si no hay ninguno, "Crear" genera el de por defecto.
2. **Semilla** — se escribe a mano, o "Al azar" / "Siguiente" cambian la semilla y regeneran la vista previa en el mismo click, para recorrer mapas rápido.
3. **Vista previa** — genera sin tocar la escena y muestra la imagen de [MapGenerationPreview](../../Map/Generation/MapGenerationPreview.md), un resumen (layout, agua, mesetas, spawns, objetos) y el informe de reglas: las que fallan como error o aviso, y el detalle completo en un desplegable.
4. **Generar en la escena** — pide confirmación (avisa si la semilla no cumple las reglas obligatorias) y vuelca el mapa con [MapGenerationApplier](MapGenerationApplier.md). Reemplaza relieve, texturas, objetos, marcadores, caminos y zona edificable del mapa activo; es un solo paso de undo. Con "Bake NavMesh al generar" dispara el bake a continuación.

Después de generar corresponde **Validar mapa** (sección Mapa de la ventana) cuando termina el bake: las reglas del generador se chequean sin NavMesh.

**Probar semillas** corre N semillas consecutivas desde la actual, sin tocar la escena, e informa cuántas cumplen las reglas, cuántos intentos hicieron falta en promedio, qué reglas descartaron intentos y qué variantes (layout + agua) salieron. Sirve para ajustar un asset de reglas: si una regla descarta la mayoría de los intentos, o está muy exigente o los rangos no le dejan lugar.

La vista previa y el informe no se serializan: se pierden en un domain reload y hay que volver a generarlos. Reglas, semilla y opciones sí persisten con la ventana.
