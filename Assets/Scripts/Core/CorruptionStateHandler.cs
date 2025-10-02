using UnityEngine;

public class CorruptionStateHandler : MonoBehaviour
{
    [SerializeField] private float corruptionTimer = 30f;
    private float timeRemaining;
    private bool corruptionActive = false;
    
    void Start()
    {
        GameStateManager.Instance.OnGameStateChange += HandleGameStateChange;
    }

    private void OnDestroy()
    {
        GameStateManager.Instance.OnGameStateChange -= HandleGameStateChange;
    }

    
    void Update()
    {
        if (!corruptionActive) return;

        timeRemaining -= Time.deltaTime;

        if (timeRemaining <= 0)
        {
            GameStateManager.Instance.ChangeState(GameState.GameOver);
            corruptionActive = false;
        }
    }

    private void HandleGameStateChange(GameState newState)
    {
        if (newState == GameState.Corruption)
        {
            StartCorruption();
        }
        else
        {
            corruptionActive = false;
        }
    }

    private void StartCorruption()
    {
        // Start timer
        timeRemaining = corruptionTimer;
        corruptionActive = true;

        // Switch control to Dog
        GameManager.Instance.SetControlledCharacter(GameManager.Instance.dogPrefab);
    }

    private void EndCorruption()
    {
        Debug.Log("Corruption ended early!");

        corruptionActive = false;  // stop the timer

        // Reset player back to robot

        GameManager.Instance.SetControlledCharacter(GameManager.Instance.robotPrefab);

        // Change game state back to Playing
        GameStateManager.Instance.ChangeState(GameState.Playing);
    }
}
