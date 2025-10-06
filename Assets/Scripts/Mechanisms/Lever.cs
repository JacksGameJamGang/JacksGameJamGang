using UnityEngine;
using System;

public class LeverMechanisms : MonoBehaviour, IMechanism
{
    private string mechanismName = "Platform Lever";

    [Header("Lever Settings")]
    [SerializeField] private Animator leverAnimator;

    private string leverBoolName = "IsOn";

	public event Action<IMechanism, bool> OnToggleMechanism;
	public bool IsActive => isActive;
    private bool isActive;

	public void Activate()
	{
		isActive = true;
        leverAnimator?.SetBool(leverBoolName, true);
		OnToggleMechanism?.Invoke(this, true);
    }

    public void Deactivate()
    {
		isActive = false;
		leverAnimator?.SetBool(leverBoolName, false);
		OnToggleMechanism?.Invoke(this, false);
    }

    public string GetMechanismName() => mechanismName;
}
