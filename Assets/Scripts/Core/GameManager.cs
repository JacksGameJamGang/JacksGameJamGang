using System;
using UnityEngine;

public class GameManager : LocalSingleton<GameManager>
{
    [SerializeField] private PlayerController robotController;
    [SerializeField] private PlayerController dogController;
    private PlayerController currentCharacter;
    
    public PlayerController RobotController => robotController;
    public PlayerController DogController => dogController;
    public PlayerController CurrentCharacter => currentCharacter;
    public PlayerCharacter PlayerCharacter { get; private set; }
    public bool PlayerIsDead { get; private set; } = false;
    
    public event Action<PlayerCharacter> OnSetPlayerToFollow;

    private void Start()
    {
        SetControlledCharacter(robotController);
    }

    public void ResetLevel()
    {
        Debug.Log("Game reset.");
        //GameStateManager.Instance.ChangeState(GameState.MainMenu);
        //SceneManager.ReloadScene();
    }
    
    public void SetPlayerToFollow(PlayerCharacter player)
    {
        PlayerCharacter = player;
        OnSetPlayerToFollow?.Invoke(player);
    }

    public void SetControlledCharacter(PlayerController character)
    {
        currentCharacter = character;
        // A CameraFollow script should be added later to the camera component
        // Camera.main.GetComponent<CameraFollow>().target = currentCharacter;
    }
}
