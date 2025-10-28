using UnityEngine;
using System;
using System.Collections;

public class LeverMechanisms : MonoBehaviour, IMechanism
{
    [SerializeField] private Animator leverAnimator;

	[Header("Lever Settings")]
	private string mechanismName = "Platform Lever";
	private string leverBoolName = "IsOn";

	public event Action<IMechanism, bool> OnToggleMechanism;
	public bool IsActive => isActive;
    private bool isActive;

	private void Awake()
	{
		leverAnimator = GetComponent<Animator>();
	}

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

	public IEnumerator FailActivate()
	{
		leverAnimator?.SetBool(leverBoolName, true);
		yield return new WaitForSeconds(0.2f);
		Deactivate();
	}

	public string GetMechanismName() => mechanismName;
}
