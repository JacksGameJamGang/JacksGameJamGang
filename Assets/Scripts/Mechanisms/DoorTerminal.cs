using System;
using System.Collections;
using UnityEngine;

public class RoomDoorTerminal : MonoBehaviour, IMechanism, IInteractable
{
    private string mechanismName = "Final Door Terminal (in the room)";

	public event Action<IMechanism, bool> OnToggleMechanism;
	public bool IsActive => isActive;
	private bool isActive;

	// IInteract
	public void Interact()
	{
		Activate();
	}

	// IMechanism
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
		yield return 0f;
		Deactivate();
	}

	public string GetMechanismName()
    {
        return mechanismName;
    }
}