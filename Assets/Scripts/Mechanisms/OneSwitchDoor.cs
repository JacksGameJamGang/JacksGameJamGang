using UnityEngine;

public class OneSwitchDoor : MonoBehaviour
{
    [SerializeField] private MonoBehaviour triggerSource; // Assign either PressurePlate or LeverMechanism
    [SerializeField] private Animator doorAnimator;       // Animator for open/close animations
    [SerializeField] private Collider2D doorCollider;     // The door's physical barrier collider

    private ISwitch switchSource;

    private void Awake()
    {
        switchSource = triggerSource as ISwitch;
        if (switchSource == null)
            Debug.LogError($"{name}: Assigned object does not implement ISwitch!");
    }

    private void OnEnable()
    {
        if (switchSource != null)
            switchSource.OnSwitchToggled += HandleSwitch;
    }

    private void OnDisable()
    {
        if (switchSource != null)
            switchSource.OnSwitchToggled -= HandleSwitch;
    }

    private void HandleSwitch(bool active)
    {
        doorAnimator.SetBool("IsOpen", active);
        if (doorCollider) doorCollider.enabled = !active;
    }

}
