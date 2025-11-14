using System;
using System.Collections;
using UnityEngine;

public class RobotController : MonoBehaviour
{
	private PlayerController playerController;

    [Header("Robot Special Abilities")]
    [SerializeField] private KeyCode interactKey = KeyCode.F;
    
    [Header("Interaction Settings")]
    [SerializeField] private float interactionRange = 2f;
    [SerializeField] private LayerMask interactableLayers;

    IInteractable nearestInteractable;

	//track ground info
    private GameObject objectPlayerStoodOn;
    private Vector3 playerLeftGroundPosition;
	private Vector3 playerTouchedGroundPosition;

	private void Awake()
	{
		playerController = GetComponent<PlayerController>();
		PlayerController.OnPlayerJump += OnPlayerJump;
		PlayerController.OnPlayerTouchGround += TouchingNewGround;

		playerLeftGroundPosition = transform.position;
		playerTouchedGroundPosition = transform.position;
	}
	private void OnDestroy()
	{
		PlayerController.OnPlayerJump -= OnPlayerJump;
		PlayerController.OnPlayerTouchGround -= TouchingNewGround;
	}

	private void Update()
	{
		if (!GameStateManager.IsInPlayableState()) return;

		playerController.UpdateFallSpeed();

		WaypointManager.Instance.UpdateRobotsClosestWaypoint(transform.position);
		WaypointManager.Instance.BasicWaypointPlacing(transform.position, playerController.isGrounded);

		if (Input.GetKeyDown(interactKey))
		{
			OnInteractButtonPress();
		}
	}

	//robot player jumping
	private void OnPlayerJump()
	{
		playerLeftGroundPosition = transform.position;
		WaypointManager.Instance.BlockBasicWaypointSpacing();
	}
	private void TouchingNewGround(Collider2D collider)
	{
		playerTouchedGroundPosition = transform.position;
		WaypointManager.Instance.AllowBasicWaypointSpacing();

		if (StandingOnDifferentGroundOrLevel(collider.gameObject))
		{
			objectPlayerStoodOn = collider.gameObject;
			WaypointManager.Instance.CreateJumpWaypointPair(playerLeftGroundPosition, playerTouchedGroundPosition);
		}
	}
	private bool StandingOnDifferentGroundOrLevel(GameObject gameObject)
	{
        if (objectPlayerStoodOn == null) //should be first time loading in so ignore and set it
        {
            objectPlayerStoodOn = gameObject;
			return false;
		}

		if (objectPlayerStoodOn != gameObject)
			return true;
		else
		{
			float distance = transform.position.y - GameManager.Instance.DogController.transform.position.y - 0.5f; //0.5 offset

			if (distance >= 0.25f)
                return true;
			else
                return false;
		}
	}

	//interactions
	private void OnInteractButtonPress()
    {
		Collider2D[] interactableCollidersArray = Physics2D.OverlapCircleAll(transform.position, interactionRange, interactableLayers);

        if (interactableCollidersArray.Length == 0)
        {
            Debug.Log("no nearby interactables found");
            return;
        }
        else
        {
			Debug.Log($"found {interactableCollidersArray.Length} interactables, grabbing closest");
			nearestInteractable = FindClosestInteractable(interactableCollidersArray);
		}

        nearestInteractable.Interact();
	}
    private IInteractable FindClosestInteractable(Collider2D[] interactableColliders)
    {
        IInteractable closestInteractable = null;
        float shortestDistance = 100;

        foreach (Collider2D collider in interactableColliders)
        {
            float distance = Vector2.Distance(gameObject.transform.position, collider.gameObject.transform.position);

            if (distance > shortestDistance) continue;

            shortestDistance = distance;
            closestInteractable = collider.GetComponent<IInteractable>();
        }

        return closestInteractable;
    }
}