using UnityEngine;
using System.Collections.Generic;
using System;

public enum DoorMode { Any, All }

public class Doors : MonoBehaviour
{
	[Header("Mechanism Settings")]
	[SerializeField] private DoorMode doorMode = DoorMode.Any;
	[SerializeField] private List<MonoBehaviour> mechanismSources; // Any IMechanism (Lever, Plate, etc.)
	[SerializeField] private List<MechanismStates> mechanismStates; //internal states of all related mechanisms

	[Header("Door Components")]
    [SerializeField] private Animator doorAnimator;
    [SerializeField] private Collider2D doorCollider;

	private void Awake()
    {
		mechanismStates = new List<MechanismStates>();
        foreach (var mechanism in mechanismSources)
			mechanismStates.Add(new MechanismStates(mechanism.GetComponent<IMechanism>(), false));
    }

    private void OnEnable()
    {
        foreach (var mechanismState in mechanismStates)
			mechanismState.mechanism.OnToggleMechanism += HandleSwitchChanged;
    }

    private void OnDisable()
    {
		foreach (var mechanismState in mechanismStates)
			mechanismState.mechanism.OnToggleMechanism -= HandleSwitchChanged;
	}

	private void HandleSwitchChanged(IMechanism sender, bool isActive)
    {
        foreach (var mechanismState in mechanismStates)
        {
            if (mechanismState.mechanism != sender) continue;
            mechanismState.isActive = isActive;
        }

        if (MechanismShouldOpen())
			OpenDoor(true);
        else
			OpenDoor(false);
    }

    private bool MechanismShouldOpen()
    {
        if (doorMode == DoorMode.Any)
        {
            foreach (var mechanismState in mechanismStates)
            {
                if (mechanismState.isActive)
                    return true;
            }
            return false;
        }
        else
        {
			foreach (var mechanismState in mechanismStates)
			{
				if (!mechanismState.isActive)
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
