using System.Collections;
using UnityEngine;

public class RobotController : MonoBehaviour
{
    [Header("Robot Special Abilities")]
    [SerializeField] private KeyCode interactKey = KeyCode.F;
    
    [Header("Interaction Settings")]
    [SerializeField] private float interactionRange = 2f;
    [SerializeField] private LayerMask interactableLayers;

    IInteractable nearestInteractable;
    
    private void Update()
    {
		if (!GameStateManager.IsInPlayableState()) return;

		if (Input.GetKeyDown(interactKey))
        {
            OnInteractButtonPress();
		}
    }
    
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