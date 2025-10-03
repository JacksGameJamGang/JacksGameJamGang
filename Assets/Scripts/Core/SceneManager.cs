using System.Collections;
using DG.Tweening;
using UnityEngine.SceneManagement;

public class SceneManager : Singleton<SceneManager>
{
    private string currentActiveSceneName = "";

    // only used to load the global UI scene
    public void LoadScene(string sceneName)
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
    }

    public void LoadSceneAdditive(string sceneName, bool withFade)
    {
        if (withFade)
            StartCoroutine(LoadSceneAdditiveWithFadeCo(sceneName));
        else
            StartCoroutine(LoadSceneAdditiveFastCo(sceneName));
    }

    private IEnumerator LoadSceneAdditiveWithFadeCo(string sceneName)
    {
        yield return GlobalUIManager.Instance.FadeIn(1f).WaitForCompletion();
        yield return UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);

        if (!string.IsNullOrEmpty(currentActiveSceneName))
            yield return UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(currentActiveSceneName);

        currentActiveSceneName = sceneName;
        yield return null; // wait a frame to ensure the scene is fully loaded
        yield return GlobalUIManager.Instance.FadeOut(0.5f).WaitForCompletion();
    }

    private IEnumerator LoadSceneAdditiveFastCo(string sceneName)
    {
        yield return UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);

        if (!string.IsNullOrEmpty(currentActiveSceneName))
            yield return UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(currentActiveSceneName);

        currentActiveSceneName = sceneName;
        yield return null; // wait a frame to ensure the scene is fully loaded
    }

    public void ReloadScene()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(currentActiveSceneName, LoadSceneMode.Single);
    }
}