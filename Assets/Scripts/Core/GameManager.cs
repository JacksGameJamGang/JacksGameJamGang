using System;
using UnityEngine;

public class GameManager : LocalSingleton<GameManager>
{
    [SerializeField] private string nextSceneName = "Level_1";
    [SerializeField] private PlayerController robotController;
    [SerializeField] private PlayerController dogController;
    private PlayerController currentCharacter;
    
    public PlayerController RobotController => robotController;
    public PlayerController DogController => dogController;
    public PlayerController CurrentCharacter => currentCharacter;
    public PlayerCharacter PlayerCharacter { get; private set; }
    
    public event Action<PlayerCharacter> OnSetPlayerToFollow;
    
    private void Start()
    {
        GameStateManager.Instance.OnGameStateChange += HandleGameStateChange;
        SetControlledCharacter(robotController);
    }
    
    private void OnDestroy()
    {
        GameStateManager.Instance.OnGameStateChange -= HandleGameStateChange;
    }

    private void HandleGameStateChange(GameState newState)
    {
        if (newState == GameState.Playing)
        {
            SetPlayerToFollow(RobotController.GetComponent<PlayerCharacter>());
            SetControlledCharacter(RobotController);
        }
        else if (newState == GameState.RobotTempDeath)
        {
            SetControlledCharacter(DogController);
        }
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

    public void SwitchToNextScene()
    {
        SceneManager.Instance.LoadSceneAdditive(nextSceneName, true);
    }
}
