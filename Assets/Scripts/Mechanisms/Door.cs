using UnityEngine;
using System.Collections.Generic;
using System;

public enum DoorMode { Any, Specific, Ordered, All }

public class Doors : MonoBehaviour
{
	[Header("Mechanism Settings")]
	[SerializeField] private DoorMode doorMode = DoorMode.Any;
	[Tooltip("link mechanisms here (Lever, Plate, etc.)")]
	[SerializeField] private List<MechanismStates> linkedMechanisms;
	private MechanismStates lastToggledMechanism;

	[Header("Door Components")]
    [SerializeField] private Animator doorAnimator;
    [SerializeField] private Collider2D doorCollider;

	private void Awake()
    {
        foreach (var linkedMechanism in linkedMechanisms)
			linkedMechanism.Initilize();

		lastToggledMechanism = new();
    }

    private void OnEnable()
    {
        foreach (var linkedMechanism in linkedMechanisms)
			linkedMechanism.mechanism.OnToggleMechanism += HandleSwitchChanged;
    }

    private void OnDisable()
    {
		foreach (var linkedMechanism in linkedMechanisms)
			linkedMechanism.mechanism.OnToggleMechanism -= HandleSwitchChanged;
	}

	private void HandleSwitchChanged(IMechanism sender, bool isActive)
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

    private bool MechanismShouldOpen(MechanismStates newToggledMechanism, bool isActive)
    {
        if (doorMode == DoorMode.Any)
        {
            foreach (var linkedMechanism in linkedMechanisms)
			{
                if (linkedMechanism.isActive) //any state match open mech
					return true;
            }
            return false;
        }
        else if (doorMode == DoorMode.Specific)
        {
			foreach (var linkedMechanism in linkedMechanisms)
			{
                if (linkedMechanism.isActive != linkedMechanism.forceStateActive) //state doesnt match forced mech stays closed
					return false;
			}
			return true;
		}
        else if (doorMode == DoorMode.Ordered && isActive) //only check when toggling to active
        {
			if (newToggledMechanism == null)
			{
				Debug.LogError("Mechanism ref null, this shouldnt occur");
				return false;
			}

			if (lastToggledMechanism == null) //always allow toggling of 1st mechanism despite wrong order
			{
				Debug.LogError("1st mechanism trigger (always works)");
				lastToggledMechanism = newToggledMechanism;
				return false;
			}

			if (newToggledMechanism.orderOfMechanism == lastToggledMechanism.orderOfMechanism + 1) //correct order
			{
				Debug.LogError("correct order");
				lastToggledMechanism = newToggledMechanism;

				foreach (var linkedMechanism in linkedMechanisms)
				{
					if (!linkedMechanism.isActive) //any state not active mech stays closed
						return false;
				}
				return true;
			}
			else
			{
				Debug.LogError("incorrect order");
				lastToggledMechanism.linkedMechanism = null;

				foreach (var linkedMechanism in linkedMechanisms) //reset all mechanisms
					linkedMechanism.mechanism.Deactivate();

				return false;
			}
		}
        else
        {
			foreach (var linkedMechanism in linkedMechanisms)
			{
				if (!linkedMechanism.isActive) //any state not active mech stays closed
					return false;
			}
			return true;
		}
    }

    void OpenDoor(bool shouldOpen)
    {
		doorAnimator.SetBool("IsOpen", shouldOpen);
		if (doorCollider) doorCollider.enabled = !shouldOpen;
	}
}
