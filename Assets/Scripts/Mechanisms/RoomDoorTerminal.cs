using UnityEngine;

public class RoomDoorTerminal : MonoBehaviour, IMechanism
{
    private string mechanismName = "Final Door Terminal (in the room)";
    private bool isActive = false;

    // IMechanism implementation
    public void Activate()
    {
        if (!isActive)
        {
            Debug.Log("Terminal activated! Robot entering death state...");
            isActive = true;

            // Kill the robot
            GameStateManager.Instance.ChangeState(GameState.RobotTempDeath);
        }
    }

    public void Deactivate()
    {
        
    }

    public bool IsActive => isActive;

    public string GetMechanismName()
    {
        return mechanismName;
    }
}