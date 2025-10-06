using System;
using UnityEngine;

public class PressurePlate : MonoBehaviour, ISwitch
{
    [Header("Sprites (visual change)")]
    [SerializeField] private SpriteRenderer plateRenderer;
    [SerializeField] private Sprite unpressedSprite;
    [SerializeField] private Sprite pressedSprite;

    [Header("Tags that can trigger the plate")]
    [SerializeField] private string[] validTags = { "Player", "Dog", "Box" };

    public event Action<ISwitch, bool> OnSwitchToggled;

    private int objectsOnPlate = 0;

    private void Awake()
    {
        if (plateRenderer == null)
            plateRenderer = GetComponent<SpriteRenderer>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!IsValidTag(other.tag)) return;

        objectsOnPlate++;
        if (objectsOnPlate == 1)
        {
            SetPlateSprite(true);
            OnSwitchToggled?.Invoke(this, true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!IsValidTag(other.tag)) return;

        objectsOnPlate = Mathf.Max(0, objectsOnPlate - 1);
        if (objectsOnPlate == 0)
        {
            SetPlateSprite(false);
            OnSwitchToggled?.Invoke(this, false);
        }
    }

    private bool IsValidTag(string tag)
    {
        foreach (var validTag in validTags)
        {
            if (tag == validTag)
                return true;
        }
        return false;
    }

    private void SetPlateSprite(bool pressed)
    {
        if (plateRenderer == null) return;
        plateRenderer.sprite = pressed ? pressedSprite : unpressedSprite;
    }

    public bool IsActive => objectsOnPlate > 0;

    public Action<bool> OnPressurePlateTriggered { get; internal set; }
}
