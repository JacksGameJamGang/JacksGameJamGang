using UnityEngine;

public class PlayerCharacter : MonoBehaviour
{
    public void Start()
    {
        GameManager.Instance.SetPlayerToFollow(this); // This will be handled in HandleGameStateChange() once it's functional and this line should be removed
        //GameManager.Instance.OnSetPlayerToFollow?.Invoke(this);
    }

    private void OnDestroy()
    {
    }
}