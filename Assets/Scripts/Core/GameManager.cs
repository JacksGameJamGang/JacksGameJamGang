using System;
using UnityEngine;
using Unity.Cinemachine;

public class GameManager : LocalSingleton<GameManager>
{
    [SerializeField] private string nextSceneName = "Level_1";
    [SerializeField] private Transform robotController;
    [SerializeField] private Transform dogController;
    [SerializeField] private Unity.Cinemachine.CinemachineCamera virtualCamera;

    private Transform currentCharacter;
    
    public Transform RobotController => robotController;
    public Transform DogController => dogController;
    public Transform CurrentCharacter => currentCharacter;
    public Transform PlayerToFollow { get; private set; }
    
    public event Action<Transform> OnSetPlayerToFollow;
    
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
            SetPlayerToFollow(RobotController.transform);
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
    
    public void SetPlayerToFollow(Transform player)
    {
        PlayerToFollow = player;
        OnSetPlayerToFollow?.Invoke(player);
    }

    public void SetControlledCharacter(Transform character)
    {
        currentCharacter = character;

        // Disable all PlayerController components
        RobotController.GetComponent<PlayerController>().enabled = false;
        DogController.GetComponent<PlayerController>().enabled = false;

        currentCharacter.GetComponent<PlayerController>().enabled = true;

        if (currentCharacter == robotController)
        {
            RobotController.GetComponent<RobotController>().enabled = true;
            DogController.GetComponent<DogAIController>().enabled = true;
        }
        else if (currentCharacter == dogController)
        {
            RobotController.GetComponent<RobotController>().enabled = false;
            DogController.GetComponent<DogAIController>().enabled = false;
        }

        // Update Cinemachine target
        if (virtualCamera != null)
        {
            virtualCamera.Follow = currentCharacter;
        }
    }

    public void SwitchToNextScene()
    {
        SceneManager.Instance.LoadSceneAdditive(nextSceneName, true);
    }
}
