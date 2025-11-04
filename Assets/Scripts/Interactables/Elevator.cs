using DG.Tweening;
using System.Collections;
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
		Debug.Log("Elevator Interact");

		if (GameStateManager.Instance.CurrentGameState == GameState.Loading) return;
		StartCoroutine(TeleportPlayer());
	}

	IEnumerator TeleportPlayer()
	{
		GameStateManager.Loading();
		yield return GlobalUIManager.Instance.FadeIn(0.5f).WaitForCompletion();

		GameManager.Instance.RobotController.transform.position = linkedElevator.transform.position;

		yield return new WaitForSeconds(0.25f);
		yield return GlobalUIManager.Instance.FadeOut(0.5f).WaitForCompletion();
		GameStateManager.Playing();
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
