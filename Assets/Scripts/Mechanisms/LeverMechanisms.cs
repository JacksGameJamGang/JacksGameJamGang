using UnityEngine;
using System;

public class LeverMechanisms : MonoBehaviour, IMechanism, ISwitch
{
    private string mechanismName = "Platform Lever";

    [Header("Lever Settings")]
    [SerializeField] private Animator leverAnimator;
    [SerializeField] private MovingPlatform movingPlatform;

    private string leverBoolName = "IsOn";
    public event Action<ISwitch, bool> OnSwitchToggled;

    private bool isActive = false;

    public void Activate()
    {
        isActive = !isActive;
        leverAnimator?.SetBool(leverBoolName, isActive);

        // Notify listeners (doors, platforms, etc.)
        OnSwitchToggled?.Invoke(this, isActive);

        // Optional: still directly activate platform
        movingPlatform?.Activate();
    }

    public void Deactivate()
    {
        isActive = false;
        leverAnimator?.SetBool(leverBoolName, isActive);
        OnSwitchToggled?.Invoke(this, false);
    }

    public bool IsActive => isActive;

    public string GetMechanismName() => mechanismName;
}
