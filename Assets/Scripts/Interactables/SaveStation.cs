using UnityEngine;

public class SaveStation : MonoBehaviour, IInteractable
{
	protected Collider2D _Collider;

	[Header("Interact notif text")]
	public GameObject interactCanvasObject;

	private void Awake()
	{
		_Collider = GetComponent<Collider2D>();
		interactCanvasObject.SetActive(false);
	}

	public void Interact()
	{
		Debug.Log("Save Station Interact");
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
