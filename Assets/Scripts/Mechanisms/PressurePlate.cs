using System;
using System.Collections;
using UnityEngine;

public class PressurePlate : MonoBehaviour, IMechanism
{
	private string mechanismName = "Pressure Plate";

	[Header("Sprites (visual change)")]
    [SerializeField] private SpriteRenderer plateRenderer;
    [SerializeField] private Sprite unpressedSprite;
    [SerializeField] private Sprite pressedSprite;

    [Header("Tags that can trigger the plate")]
    [SerializeField] private string[] validTags = { "Player", "Dog", "Box" };

	public event Action<IMechanism, bool> OnToggleMechanism;
	public bool IsActive => objectsOnPlate > 0;
	private int objectsOnPlate = 0;

	public Action<bool> OnPressurePlateTriggered { get; internal set; }

    private void Awake()
    {
		plateRenderer = GetComponent<SpriteRenderer>();

        if (plateRenderer == null)
            plateRenderer = GetComponent<SpriteRenderer>();
    }

	//plate trigger activation
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!IsValidTag(other.tag)) return;
		Activate();
    }
	public void Activate()
	{
		objectsOnPlate++;
		if (objectsOnPlate == 1)
		{
			SetPlateSprite(true);
			OnToggleMechanism?.Invoke(this, true);
		}
	}

	//plate trigger deactivation
	private void OnTriggerExit2D(Collider2D other)
    {
        if (!IsValidTag(other.tag)) return;
        Deactivate();
    }
	public void Deactivate()
	{
		objectsOnPlate = Mathf.Max(0, objectsOnPlate - 1);
		if (objectsOnPlate == 0)
		{
			SetPlateSprite(false);
			OnToggleMechanism?.Invoke(this, false);
		}
	}

	public IEnumerator FailActivate()
	{
		yield return null;
		//noop
	}

	private void SetPlateSprite(bool pressed)
	{
		if (plateRenderer == null) return;
		plateRenderer.sprite = pressed ? pressedSprite : unpressedSprite;
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

	public string GetMechanismName() => mechanismName;
}
