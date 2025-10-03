using UnityEngine;

public class ControlManager : MonoBehaviour
{
    private PlayerController localController;
    
    public void Start()
    {
        localController = GetComponentInParent<PlayerController>();
        GameStateManager.Instance.OnGameStateChange += HandleGameStateChange;
    }

    private void OnDestroy()
    {
        GameStateManager.Instance.OnGameStateChange -= HandleGameStateChange;
    }

    private void HandleGameStateChange(GameState newState)
    {
        if (newState == GameState.Playing)
            GameManager.Instance.SetPlayerToFollow(GameManager.Instance.RobotController.GetComponent<PlayerCharacter>());
        else if (newState == GameState.RobotTempDeath)
            GameManager.Instance.SetPlayerToFollow(GameManager.Instance.DogController.GetComponent<PlayerCharacter>());

        if (GameManager.Instance.CurrentCharacter == localController)
            GetComponentInParent<PlayerController>().enabled = true;
        else
            GetComponentInParent<PlayerController>().enabled = false;
    }
}