using Unity.VisualScripting;
using UnityEngine;

public class Elevator : MonoBehaviour, IInteractable
{
	protected Collider2D _Collider;

	[Header("Interact notif text")]
	public GameObject interactCanvasObject;

	private void Start()
	{
		_Collider = GetComponent<Collider2D>();
		interactCanvasObject.SetActive(false);
	}

	public void Interact()
	{
		Debug.LogError("Elevator Interact");
		//code to teleport robot/dog to other elevators
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
