using System;
using UnityEngine;

public enum GameState
{
    MainMenu,
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
public class GameStateManager : Singleton<GameStateManager>
{
    public GameState CurrentGameState {get; private set;}
    
    public Action<GameState> OnGameStateChange;

    private void Start()
    {
        ChangeState(GameState.MainMenu);
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
