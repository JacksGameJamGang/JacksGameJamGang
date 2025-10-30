using UnityEngine;

public class DoorToScenes : MonoBehaviour, IInteractable
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
		Debug.LogError("Door To Different Scene Interact");
		//code to load other scenes
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
