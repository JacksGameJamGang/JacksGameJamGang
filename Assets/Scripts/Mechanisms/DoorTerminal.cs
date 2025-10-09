using System;
using System.Collections;
using UnityEngine;

public class RoomDoorTerminal : MonoBehaviour, IMechanism
{
    private string mechanismName = "Final Door Terminal (in the room)";

	public event Action<IMechanism, bool> OnToggleMechanism;
	public bool IsActive => isActive;
	private bool isActive;

	// IMechanism implementation
	public void Activate()
    {
        isActive = true;
		GameStateManager.Instance.ChangeState(GameState.RobotTempDeath);
	}

    public void Deactivate()
    {
        isActive = true; //cant deactivate
    }

	public IEnumerator FailActivate()
	{
		isActive = false;
		yield return null;
		//noop
	}

	public string GetMechanismName()
    {
        return mechanismName;
    }
}