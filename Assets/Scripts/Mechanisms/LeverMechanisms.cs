using UnityEngine;
using System;

public class LeverMechanisms : MonoBehaviour, IMechanism, ISwitch
{
    private string mechanismName = "Platform Lever";

    [Header("Lever Settings")]
    [SerializeField] private Animator leverAnimator; // Animator for lever visual
    [SerializeField] private MovingPlatform movingPlatform; // Reference moving platform
    private string leverBoolName = "IsOn"; // Animator bool parameter
    public event Action<bool> OnSwitchToggled;
    private bool isActive = false;

    // IMechanism implementation
    public void Activate()
    {
        isActive = !isActive;
        leverAnimator?.SetBool(leverBoolName, isActive);

        // Notify listeners (doors, platforms, etc.)
        OnSwitchToggled?.Invoke(isActive);

        // Optional: still directly activate platform
        movingPlatform?.Activate();
    }

    public void Deactivate()
    {
        isActive = false;

        isActive = false;
        leverAnimator?.SetBool(leverBoolName, isActive);
        OnSwitchToggled?.Invoke(isActive);
    }

    public bool IsActive => isActive;

    public string GetMechanismName()
    {
        return mechanismName;
    }
}
