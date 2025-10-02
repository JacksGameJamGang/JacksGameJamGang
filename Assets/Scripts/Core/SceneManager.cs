using UnityEngine.SceneManagement;

public class SceneManager : Singleton<SceneManager>
{
    private void Start()
    {
        LoadScene("MainMenuScene");
    }

    public void LoadScene(string sceneName)
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
    }

    public void ReloadScene()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name, LoadSceneMode.Single);
    }
}
