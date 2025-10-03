using UnityEngine;

public class MainMenuSystem : MonoBehaviour
{
    [SerializeField] private string firstSceneName = "Level_0";
    
    public void OnStartButtonClick()
    {
        SceneManager.Instance.LoadSceneAdditive(firstSceneName, true);
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
