public interface ISwitch
{
    event System.Action<bool> OnSwitchToggled;
    bool IsActive { get; }
}