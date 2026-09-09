# CoreManagerSceneSetup

`Assets/_Scripts/Editor/CoreManagerSceneSetup.cs`

Script de Editor. Menú `Tools/RTS/Ensure Core Managers In Scene`: si [ResourceManager](../_CORE%20Scripts/ResourceManager.md) y/o [BuildingManager](../_CORE%20Scripts/BuildingManager.md) no existen todavía como GameObject en la escena abierta, los crea (uno cada uno) como hijos de `GameArchitecture` — el mismo contenedor donde ya viven `GlobalUnitManager`/`GameManager`/`SelectionManager` — y marca la escena como modificada (`EditorSceneManager.MarkSceneDirty`) para recordar guardarla.

Si ambos ya existen (`FindFirstObjectByType<T>()` los encuentra), no crea nada y solo loguea que ya estaban.

**Por qué hizo falta**: ambos son singletons (`Instance` estático seteado en `Awake`) pero solo funcionan si su componente vive en algún GameObject de la escena — no se auto-instancian. `SampleScene.unity` nunca tuvo ninguno de los dos, así que `DropOffBuilding.OnEnable()` (necesita `BuildingManager.Instance`) y `DropOffBuilding.Deposit()` (necesita `ResourceManager.Instance`) fallaban en silencio (null-check) o iban a tirar `NullReferenceException` en cuanto se alcanzara el deposit real.
