using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;

public class TwoSwitchesDoor : MonoBehaviour
{
    [SerializeField] private PressurePlate plate1;
    [SerializeField] private PressurePlate plate2;

    [SerializeField] private Animator doorAnimator;       // Animator for open/close animations
    [SerializeField] private Collider2D doorCollider;     // The door's physical barrier collider

    private bool plate1Pressed;
    private bool plate2Pressed;

    private void OnEnable()
    {
        plate1.OnPressurePlateTriggered += HandlePlate1;
        plate2.OnPressurePlateTriggered += HandlePlate2;
    }

    private void OnDisable()
    {
        plate1.OnPressurePlateTriggered -= HandlePlate1;
        plate2.OnPressurePlateTriggered -= HandlePlate2;
    }

    private void HandlePlate1(bool pressed)
    {
        plate1Pressed = pressed;
        UpdateDoorState();
    }

    private void HandlePlate2(bool pressed)
    {
        plate2Pressed = pressed;
        UpdateDoorState();
    }


    private void UpdateDoorState()
    {
        bool isOpen = plate1Pressed || plate2Pressed;
        // Update animation
        doorAnimator.SetBool("IsOpen", isOpen);

        // Enable or disable collider based on door state
        if (doorCollider != null)
            doorCollider.enabled = !isOpen;
    }
}
