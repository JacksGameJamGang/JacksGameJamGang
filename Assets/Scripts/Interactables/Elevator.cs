using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Elevator : MonoBehaviour, IInteractable
{
	protected Collider2D _Collider;

	public Elevator linkedElevator;

	[Header("Interact notif text")]
	public GameObject interactCanvasObject;

	private void Start()
	{
		_Collider = GetComponent<Collider2D>();
		interactCanvasObject.SetActive(false);

		if (linkedElevator == null)
			Debug.LogError("no linked elevator, assign one in inspector");
	}

	public void Interact()
	{
		Debug.LogError("Elevator Interact");

		GameManager.Instance.RobotController.transform.position = linkedElevator.transform.position;
	}

	private void OnTriggerEnter2D(Collider2D collision)
	{
		if (!collision.CompareTag("Player")) return;

		interactCanvasObject.SetActive(true);
	}
	private void OnTriggerExit2D(Collider2D collision)
	{
		if (!collision.CompareTag("Player")) return;

		interactCanvasObject.SetActive(false);
	}
}
