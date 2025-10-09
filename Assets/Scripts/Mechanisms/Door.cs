using UnityEngine;

public class Doors : MechanismListener
{
	protected override void HandleMechanismTrigger(IMechanism sender, bool isActive)
    {
		MechanismStates mechanism = null;

		foreach (var linkedMechanism in linkedMechanisms)
		{
			if (linkedMechanism.mechanism != sender) continue;
			linkedMechanism.isActive = isActive;
			mechanism = linkedMechanism;
		}

		if (MechanismShouldOpen(mechanism, isActive))
			OpenDoor(true);
		else
			OpenDoor(false);
	}

    void OpenDoor(bool shouldOpen)
    {
		_Animator.SetBool("IsOpen", shouldOpen);
		if (_Collider) _Collider.enabled = !shouldOpen;
	}
}
