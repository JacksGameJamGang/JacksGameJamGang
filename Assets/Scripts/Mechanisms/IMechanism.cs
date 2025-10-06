using JetBrains.Annotations;

public interface IMechanism
{
	event System.Action<IMechanism, bool> OnToggleMechanism;
	bool IsActive { get; }

	void Activate();
    void Deactivate();
    string GetMechanismName();
}

[System.Serializable]
public class MechanismStates
{
	public IMechanism mechanism;
	public bool isActive;

	public MechanismStates(IMechanism mechanism, bool isActive)
	{
		this.mechanism = mechanism;
		this.isActive = isActive;
	}
}
