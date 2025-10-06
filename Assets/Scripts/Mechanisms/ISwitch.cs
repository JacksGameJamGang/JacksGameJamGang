public interface ISwitch
{
    event System.Action<ISwitch, bool> OnSwitchToggled;
    bool IsActive { get; }
}