using System;
using UnityEngine;
using UnityEngine.XR;

public class GameStateManager : MonoBehaviour
{
    public enum GameState
    {
        Initializing,
        Playing,
        Pause,
        GameOver
    }
    public static GameStateManager Instance { get; private set; }

    public GameState CurrentState { get; private set; }
    public event Action<GameState> OnGameStateChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        ChangeState(GameState.Initializing);
        //logica aca para inicializar el juego, cargar recursos, etc.
        ChangeState(GameState.Playing);
        
    }

    public void ChangeState(GameState newState)
    {
        CurrentState = newState;
        OnGameStateChanged?.Invoke(CurrentState);
    }
}
