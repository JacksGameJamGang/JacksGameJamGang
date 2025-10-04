using DG.Tweening;
using UnityEngine;

public class OneSwitchDoor : MonoBehaviour
{
    [SerializeField] private PressurePlate plate; // Only one plate now

    [SerializeField] private Transform door;
    [SerializeField] private float openDistance = 2f;
    [SerializeField] private float openTime = 0.5f;

    private bool platePressed; // Only one boolean now
    private Vector3 initialDoorPos;

    private void Awake()
    {
        initialDoorPos = door.position;
    }

    private void OnEnable()
    {
        plate.OnPressurePlateTriggered += HandlePlate; // Only one subscription
    }

    private void OnDisable()
    {
        plate.OnPressurePlateTriggered -= HandlePlate; // Only one unsubscription
    }

    private void HandlePlate(bool pressed) // Single handler
    {
        platePressed = pressed;
        CheckDoorState();
    }

    private void CheckDoorState()
    {
        if (platePressed) // Simple check - if plate is pressed, open door
            OpenDoor();
        else
            CloseDoor();
    }

    private void OpenDoor()
    {
        door.DOMove(initialDoorPos + Vector3.up * openDistance, openTime).SetEase(Ease.OutQuad);
    }

    private void CloseDoor()
    {
        door.DOMove(initialDoorPos, openTime).SetEase(Ease.OutQuad);
    }
}