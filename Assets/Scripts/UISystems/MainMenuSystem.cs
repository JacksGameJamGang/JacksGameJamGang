using UnityEngine;

public class MainMenuSystem : MonoBehaviour
{
    public void OnStartButtonClick()
    {
        SceneManager.Instance.LoadSceneAdditive("GameScene", true);
    }
    
    public void OnOptionsButtonClick()
    {
        // will we have options?
    }
    
    public void OnQuitButtonClick()
    {
        Application.Quit();
    }
}
