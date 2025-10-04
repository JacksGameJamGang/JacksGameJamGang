public interface IMechanism
{
    void Activate();
    void Deactivate();
    bool IsActive { get; }
    string GetMechanismName();
}
