using System;
using System.Collections;
using UnityEngine;

public class RobotController : MonoBehaviour
{
    PlayerController playerController;

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

    //waypoint + events
    private WaypointManager waypointManager;

	private void Awake()
	{
        playerController = GetComponent<PlayerController>();
        waypointManager = GetComponent<WaypointManager>();
		PlayerController.OnPlayerJump += OnPlayerJump;

		playerLeftGroundPosition = transform.position;
		playerTouchedGroundPosition = transform.position;
	}
	private void OnDestroy()
	{
		PlayerController.OnPlayerJump -= OnPlayerJump;
	}

	private void Update()
	{
		if (!GameStateManager.IsInPlayableState()) return;

		if (Input.GetKeyDown(interactKey))
		{
			OnInteractButtonPress();
		}
	}

	private void OnCollisionEnter2D(Collision2D collision)
	{
		TouchingNewGround(collision);
    }

	//ground checks
	private void TouchingNewGround(Collision2D collision)
	{
		if (!CollisionIsGroundLayer(collision)) return;
		if (!playerController.TouchingGroundCheck()) return;

		playerTouchedGroundPosition = transform.position;
		waypointManager.AllowBasicWaypointSpacing();

		if (StandingOnDifferentGroundOrLevel(collision.gameObject))
		{
			objectPlayerStoodOn = collision.gameObject;
			waypointManager.CreateJumpWaypointPair(playerLeftGroundPosition, playerTouchedGroundPosition);
		}
	}
	private bool CollisionIsGroundLayer(Collision2D collision)
	{
		if (((1 << collision.gameObject.layer) & playerController.GetGroundLayerMask()) != 0)
			return true;
		else
			return false;
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
	private void OnPlayerJump()
	{
		playerLeftGroundPosition = transform.position;
		waypointManager.BlockBasicWaypointSpacing();
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