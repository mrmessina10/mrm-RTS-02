using UnityEngine;

public interface IState
{
    void Enter(); // Método para ejecutar acciones al entrar al estado
    void Tick();// Método para ejecutar acciones cada frame mientras se está en el estado
    void Exit();// Método para ejecutar acciones al salir del estado

}
