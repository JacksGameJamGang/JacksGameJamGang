using System;
using DG.Tweening;
using UnityEngine;

public class PressurePlate : MonoBehaviour
{
    [SerializeField] private Transform topPart;
    [SerializeField] private float pressDistance = 0.1f;
    [SerializeField] private float pressTime = 0.1f;

    public Action<bool> OnPressurePlateTriggered;

    private Vector3 initialPosition;
    private Tween activeTween;
    private int objectsOnPlate = 0;

    private void Awake()
    {
        initialPosition = topPart.position;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player") && !other.CompareTag("Dog")) return;

        objectsOnPlate++;
        if (objectsOnPlate == 1) // just in case if both the player and dog step on it at the same time
        {
            AnimatePlate(initialPosition + Vector3.down * pressDistance);
            OnPressurePlateTriggered?.Invoke(true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player") && !other.CompareTag("Dog")) return;

        objectsOnPlate = Mathf.Max(0, objectsOnPlate - 1);
        if (objectsOnPlate == 0)
        {
            AnimatePlate(initialPosition);
            OnPressurePlateTriggered?.Invoke(false);
        }
    }

    private void AnimatePlate(Vector3 targetPos)
    {
        activeTween?.Kill();
        activeTween = topPart.DOMove(targetPos, pressTime).SetEase(Ease.OutQuad);
    }
}
