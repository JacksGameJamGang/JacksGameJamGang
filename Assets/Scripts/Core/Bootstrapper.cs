using System.Collections;
using UnityEngine;

public class Bootstrapper : MonoBehaviour
{
    private static string GLOBAL_UI_SCENE_NAME = "GlobalUIScene";
    
    [SerializeField] private string initialSceneName = "MainMenuScene";
    
    private void Start()
    {
        StartCoroutine(LoadInitialSceneCoroutine());
    }
    
    private IEnumerator LoadInitialSceneCoroutine()
    {
        SceneManager.Instance.LoadScene(GLOBAL_UI_SCENE_NAME);
        SceneManager.Instance.LoadSceneAdditive(initialSceneName, false);
        yield return null;
    }
}