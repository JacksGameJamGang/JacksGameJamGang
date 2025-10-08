using UnityEngine;

public class Doors : MechanismListener
{
	[Header("Door Components")]
    [SerializeField] private Animator doorAnimator;
    [SerializeField] private Collider2D doorCollider;

	protected override void HandleSwitchChanged(IMechanism sender, bool isActive)
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
		doorAnimator.SetBool("IsOpen", shouldOpen);
		if (doorCollider) doorCollider.enabled = !shouldOpen;
	}
}
