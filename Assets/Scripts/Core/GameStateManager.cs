using System;
using UnityEngine;

public enum GameState
{
    Loading,
    Playing,
    RobotTempDeath,
    Paused,
    GameOver,
}

/// <summary>
/// This will control the flow of the game states.
/// It will transmit events when the game state changes so other systems can react accordingly.
/// such as when the game is paused, we want to stop player movement and show the pause menu.
/// </summary>
public class GameStateManager : LocalSingleton<GameStateManager>
{
    public GameState CurrentGameState {get; private set;}
    
    public Action<GameState> OnGameStateChange;

    private void Start()
    {
        ChangeState(GameState.Playing); // Loading
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Debug.Log("Escape");
            if (CurrentGameState == GameState.Playing)
                PauseGame();
            else if (CurrentGameState == GameState.Paused)
                ResumeGame();
        }
    }
    
    private void PauseGame()
    {
        Time.timeScale = 0f;
        ChangeState(GameState.Paused);
    }

    private void ResumeGame()
    {
        Time.timeScale = 1f;
        ChangeState(GameState.Playing);
    }

    public void ChangeState(GameState newState)
    {
        if (CurrentGameState != newState)
        {
            CurrentGameState = newState;
            OnGameStateChange?.Invoke(newState);
            Debug.Log($"Game State changed to: {newState}");
        }
    }
}
