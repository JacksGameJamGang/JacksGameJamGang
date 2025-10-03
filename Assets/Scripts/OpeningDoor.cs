using DG.Tweening;
using UnityEngine;

public class OpeningDoor : MonoBehaviour
{
    [SerializeField] private PressurePlate plate1;
    [SerializeField] private PressurePlate plate2;
    
    [SerializeField] private Transform door;
    [SerializeField] private float openDistance = 2f; 
    [SerializeField] private float openTime = 0.5f;

    private bool plate1Pressed;
    private bool plate2Pressed;
    private Vector3 initialDoorPos;

    private void Awake()
    {
        initialDoorPos = door.position;
    }

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
        CheckDoorState();
    }

    private void HandlePlate2(bool pressed)
    {
        plate2Pressed = pressed;
        CheckDoorState();
    }

    private void CheckDoorState()
    {
        if (plate1Pressed || plate2Pressed)
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
