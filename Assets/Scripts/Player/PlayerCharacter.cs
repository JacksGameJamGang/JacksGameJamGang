using UnityEngine;

public class PlayerCharacter : MonoBehaviour
{
    public void Start()
    {
        GameStateManager.Instance.OnGameStateChange += HandleGameStateChange;
        //GameManager.Instance.SetPlayerToFollow(this); // This will be handled in HandleGameStateChange() once it's functional and this line should be removed
        //GameManager.Instance.OnSetPlayerToFollow?.Invoke(this);
    }

    private void OnDestroy()
    {
        GameStateManager.Instance.OnGameStateChange -= HandleGameStateChange;
    }

    private void HandleGameStateChange(GameState newState)
    {
        if (newState == GameState.Playing)
            GameManager.Instance.SetPlayerToFollow(GameManager.Instance.robotPrefab.GetComponent<PlayerCharacter>());
        else if (newState == GameState.Corruption)
            GameManager.Instance.SetPlayerToFollow(GameManager.Instance.dogPrefab.GetComponent<PlayerCharacter>());

        if (GameManager.Instance.GetControlledCharacter() == GetComponentInParent<GameObject>())
            GetComponentInParent<PlayerController>().enabled = true;
        else
            GetComponentInParent<PlayerController>().enabled = false;
    }
}
