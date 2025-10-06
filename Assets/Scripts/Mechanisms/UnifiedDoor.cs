using UnityEngine;
using System.Collections.Generic;

public enum DoorMode { Any, All }

public class UnifiedDoor : MonoBehaviour
{
    [Header("Switch Settings")]
    [SerializeField] private List<MonoBehaviour> switchSources; // Any ISwitch (Lever, Plate, etc.)
    [SerializeField] private DoorMode doorMode = DoorMode.Any;

    [Header("Door Components")]
    [SerializeField] private Animator doorAnimator;
    [SerializeField] private Collider2D doorCollider;

    private readonly List<ISwitch> switches = new();
    private readonly Dictionary<ISwitch, bool> switchStates = new();

    private void Awake()
    {
        foreach (var src in switchSources)
        {
            if (src is ISwitch sw)
            {
                switches.Add(sw);
                switchStates[sw] = sw.IsActive;
            }
            else
            {
                Debug.LogError($"{name}: {src.name} does not implement ISwitch!");
            }
        }
    }

    private void OnEnable()
    {
        foreach (var sw in switches)
            sw.OnSwitchToggled += HandleSwitchChanged;
    }

    private void OnDisable()
    {
        foreach (var sw in switches)
            sw.OnSwitchToggled -= HandleSwitchChanged;
    }

    private void HandleSwitchChanged(ISwitch sender, bool isActive)
    {
        if (switchStates.ContainsKey(sender))
            switchStates[sender] = isActive;

        UpdateDoorState();
    }

    private void UpdateDoorState()
    {
        bool shouldOpen;

        if (doorMode == DoorMode.Any)
        {
            // Open if ANY switch is active
            shouldOpen = false;
            foreach (var state in switchStates.Values)
            {
                if (state)
                {
                    shouldOpen = true;
                    break;
                }
            }
        }
        else // DoorMode.All
        {
            // Open only if ALL switches are active
            shouldOpen = true;
            foreach (var state in switchStates.Values)
            {
                if (!state)
                {
                    shouldOpen = false;
                    break;
                }
            }
        }

        // Update animation and collider
        doorAnimator.SetBool("IsOpen", shouldOpen);
        if (doorCollider) doorCollider.enabled = !shouldOpen;
    }
}
