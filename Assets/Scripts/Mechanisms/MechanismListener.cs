using System.Collections.Generic;
using UnityEngine;

public enum DoorMode { Any, Specific, Ordered, All }

public class MechanismListener : MonoBehaviour
{
	[Header("Mechanism Settings")]
	[SerializeField] protected DoorMode doorMode = DoorMode.Any;
	[Tooltip("link mechanisms here (Lever, Plate, etc.)")]
	[SerializeField] protected List<MechanismStates> linkedMechanisms;
	protected MechanismStates lastToggledMechanism;

	protected void Awake()
	{
		foreach (var linkedMechanism in linkedMechanisms)
			linkedMechanism.Initilize();

		lastToggledMechanism = new();
	}

	protected void OnEnable()
	{
		foreach (var linkedMechanism in linkedMechanisms)
			linkedMechanism.mechanism.OnToggleMechanism += HandleSwitchChanged;
	}

	protected void OnDisable()
	{
		foreach (var linkedMechanism in linkedMechanisms)
			linkedMechanism.mechanism.OnToggleMechanism -= HandleSwitchChanged;
	}

	protected virtual void HandleSwitchChanged(IMechanism sender, bool isActive)
	{
		Debug.LogError("Function needs implimenting or base.HandleSwitchChanged(sender, isActive); needs removing");
	}

	protected bool MechanismShouldOpen(MechanismStates newToggledMechanism, bool isActive)
	{
		if (doorMode == DoorMode.Any)
			return DoorModeAnyCheck();
		else if (doorMode == DoorMode.Specific)
			return DoorModeSpecificCheck();
		else if (doorMode == DoorMode.Ordered && isActive) //only check when toggling to active
			return DoorModeOrderedCheck(newToggledMechanism);
		else 
			return DoorModeAllCheck();
	}

	//door mode any check
	bool DoorModeAnyCheck()
	{
		foreach (var linkedMechanism in linkedMechanisms)
		{
			if (linkedMechanism.isActive) //any state match open mech
				return true;
		}
		return false;
	}

	//door mode specific check
	bool DoorModeSpecificCheck()
	{
		foreach (var linkedMechanism in linkedMechanisms)
		{
			if (linkedMechanism.isActive != linkedMechanism.forceStateActive) //state doesnt match forced mech stays closed
				return false;
		}
		return true;
	}

	//door mode ordered check
	bool DoorModeOrderedCheck(MechanismStates newToggledMechanism)
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

	//door mode all check
	bool DoorModeAllCheck()
	{
		foreach (var linkedMechanism in linkedMechanisms)
		{
			if (!linkedMechanism.isActive) //any state not active mech stays closed
				return false;
		}
		return true;
	}
}
