using UnityEngine;

public class RobotDeathStateHandler : MonoBehaviour
{
    [SerializeField] private float downTimeBeforeTermination = 30f;
    private float timeRemaining;
    private bool isRobotDown = false;

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
        if (!isRobotDown) return;

        timeRemaining -= Time.deltaTime;

        if (timeRemaining <= 0)
        {
            GameStateManager.Instance.ChangeState(GameState.GameOver);
            isRobotDown = false;
        }
    }

    private void HandleGameStateChange(GameState newState)
    {
        if (newState == GameState.RobotTempDeath)
        {
            KillRobot();
        }
        else
        {
            isRobotDown = false;
        }
    }

    private void KillRobot()
    {
        // Start timer
        timeRemaining = downTimeBeforeTermination;
        isRobotDown = true;

        // Switch control to Dog
        GameManager.Instance.SetControlledCharacter(GameManager.Instance.dogPrefab);
    }

    private void ReviveRobot()
    {
        isRobotDown = false;  // stop the timer

        // Reset player back to robot

        GameManager.Instance.SetControlledCharacter(GameManager.Instance.robotPrefab);

        // Change game state back to Playing
        GameStateManager.Instance.ChangeState(GameState.Playing);
    }
}
