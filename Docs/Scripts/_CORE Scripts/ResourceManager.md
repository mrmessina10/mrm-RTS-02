# ResourceManager

`Assets/_Scripts/_CORE Scripts/ResourceManager.cs`

Singleton que lleva el inventario global de recursos del jugador: un diccionario `ResourceType → int` inicializado con todos los valores del enum en 0 (`Awake`). `AddResource(type, amount)` suma al inventario y dispara `OnResourceChanged` (para HUD/UI).

Todavía no tiene `SpendResource`/`TryConsume` ni validación de fondos — solo el lado de "ingreso" de la economía está implementado. Quien le aporta recursos hoy es [DropOffBuilding.Deposit](../Buildings/DropOffBuilding.md), aunque esa llamada todavía es un mock (`Debug.Log`) y no invoca `ResourceManager.AddResource` — es el próximo cable a conectar.
