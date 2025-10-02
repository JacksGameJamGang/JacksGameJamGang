using UnityEngine;

public class PlayerCharacter : MonoBehaviour
{
    public void Start()
    {
        GameManager.Instance.SetPlayerToFollow(this);
        //GameManager.Instance.OnSetPlayerToFollow?.Invoke(this);
        //GameStateManager.Instance.OnGameStateChange += HandleGameStateChange; 
    }

    private void HandleGameStateChange(GameState obj)
    {
        
    }
}
