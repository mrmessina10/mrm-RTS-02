# StateMachine

`Assets/_Scripts/StateMachines/StateMachine.cs`

Motor genérico de la FSM: no es un `MonoBehaviour`, es una clase plana que guarda el `IState` actual y expone `ChangeState(newState)` (llama `Exit()` del estado viejo, `Enter()` del nuevo) y `Update()` (llama `Tick()` del estado actual cada frame).

Instanciada una vez por unidad dentro de [UnitController](../Unit%20Scripts/UnitController.md); ningún estado conoce a `StateMachine` directamente, transicionan llamando `unit.ChangeState(...)`, que delega acá.
