using System;
using UnityEngine;

public class DoorToScenes : MonoBehaviour, IInteractable
{
	protected Collider2D _Collider;

	[Header("Loading Next Scene")]
	public string nameOfSceneToLoad;

	[Header("Interact notif text")]
	public GameObject interactCanvasObject;

	public static event Action OnEnterNewScene;

	private void Start()
	{
		_Collider = GetComponent<Collider2D>();
		interactCanvasObject.SetActive(false);
	}

	public void Interact()
	{
		Debug.Log("Door To Different Scene Interact");

		SceneManager.Instance.LoadSceneAdditive(nameOfSceneToLoad, true);
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
