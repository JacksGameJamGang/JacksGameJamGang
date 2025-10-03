using UnityEngine;

public class RobotDeathStateHandler : MonoBehaviour
{
    [SerializeField] private float downTimeBeforeTermination = 30f;
    private bool isRobotDown = false;
    private float timeRemainingTimer;

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

        timeRemainingTimer -= Time.deltaTime;

        if (timeRemainingTimer <= 0)
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
        // else
        // {
        //     isRobotDown = false;
        // }
    }

    private void KillRobot()
    {
        // Start timer
        timeRemainingTimer = downTimeBeforeTermination;
        isRobotDown = true;

        // Switch control to Dog
        GameManager.Instance.SetControlledCharacter(GameManager.Instance.DogController);
    }

    private void ReviveRobot()
    {
        isRobotDown = false;  // stop the timer

        // Reset player back to robot
        GameManager.Instance.SetControlledCharacter(GameManager.Instance.RobotController);

        // Change game state back to Playing
        GameStateManager.Instance.ChangeState(GameState.Playing);
    }
}
