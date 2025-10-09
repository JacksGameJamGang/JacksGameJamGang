using JetBrains.Annotations;
using System.Collections;
using UnityEngine;

public interface IMechanism
{
	event System.Action<IMechanism, bool> OnToggleMechanism;
	bool IsActive { get; }

	void Activate();
    void Deactivate();

	IEnumerator FailActivate();

    string GetMechanismName();
}

[System.Serializable]
public class MechanismStates
{
	public MonoBehaviour linkedMechanism;
	public IMechanism mechanism;

	[Tooltip("toggle to force mechanism to match specific state (requires DoorMode == Specific)")]
	public bool forceStateActive;

	[Tooltip("give order to mechanisms (requires DoorMode == Ordered)")]
	[Range(1,10)] public int orderOfMechanism;

	public bool isActive;

	public MechanismStates()
	{
		orderOfMechanism = 0;
	}

	public void Initilize()
	{
		mechanism = linkedMechanism.GetComponent<IMechanism>();
	}
}
