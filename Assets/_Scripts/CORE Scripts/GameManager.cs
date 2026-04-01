using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private VoidEventChannelSO onGameOverEvent;

    private void OnEnable()
    {
        onGameOverEvent.OnEventRaised += HandleGameOver;
    }

    private void OnDisable()
    {
        onGameOverEvent.OnEventRaised -= HandleGameOver;
    }

    private void HandleGameOver()
    {
        Debug.Log("Game Over! Returning to main menu...");
        // pause the game, show game over screen, etc.
        // Here you can add code to transition to the main menu scene, e.g.:
        // SceneManager.LoadScene("MainMenu");
    }
}
