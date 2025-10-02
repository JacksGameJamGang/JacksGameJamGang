using System;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    public int Lives { get; private set; }
    
    [SerializeField] private int initialLives = 3;
    
    public event Action<PlayerCharacter> OnSetPlayerToFollow;
    public PlayerCharacter PlayerCharacter { get; private set; }
    
    private GameManager()
    {
        Lives = initialLives;
    }

    public void AddLife(int value)
    {
        Lives += value;
        Debug.Log($"Lives updated: {Lives}");
    }

    public void LoseLife()
    {
        Lives--;
        // switch to PlayerDead?
        Debug.Log($"Lives remaining: {Lives}");
        if (Lives <= 0)
            GameStateManager.Instance.ChangeState(GameState.GameOver);
    }

    public void ResetGame()
    {
        Lives = 3;
        Debug.Log("Game reset.");
        GameStateManager.Instance.ChangeState(GameState.MainMenu);
    }
    
    public void SetPlayerToFollow(PlayerCharacter player)
    {
        PlayerCharacter = player;
        OnSetPlayerToFollow?.Invoke(player);
    }
}