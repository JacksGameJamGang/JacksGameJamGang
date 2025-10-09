using System.Collections;
using UnityEngine;

public class RobotController : MonoBehaviour
{
    [Header("Robot Special Abilities")]
    [SerializeField] private KeyCode petDogKey = KeyCode.P;
    [SerializeField] private KeyCode activateMechanismKey = KeyCode.I;
    
    [Header("Interaction Settings")]
    [SerializeField] private float interactionRange = 2f;
    [SerializeField] private LayerMask dogLayer;
    [SerializeField] private LayerMask mechanismLayer;
    
    private DogAIController nearbyDog;
    private GameObject nearbyMechanism;
    
    private void Update()
    {
        FindNearbyInteractables();

        if (Input.GetKeyDown(petDogKey))
        {
            StartCoroutine(PetDog());
        }

        if (Input.GetKeyDown(activateMechanismKey))
        {
            ActivateMechanism();
        }
    }
    
    private void FindNearbyInteractables()
    {
        Collider2D dogCollider = Physics2D.OverlapCircle(transform.position, interactionRange, dogLayer);
        nearbyDog = dogCollider?.GetComponent<DogAIController>();
        
        Collider2D mechanismCollider = Physics2D.OverlapCircle(transform.position, interactionRange, mechanismLayer);
        nearbyMechanism = mechanismCollider?.gameObject;
    }

    private IEnumerator PetDog()
    {
        if (nearbyDog != null)
        {
            Debug.Log("Robot pets the dog!");
            Animator dogAnimator = nearbyDog.GetComponent<Animator>();

            dogAnimator.SetBool("IsSitting", true);
            yield return new WaitForSeconds(2f); // Wait 2 seconds (adjust duration as needed)
            dogAnimator.SetBool("IsSitting", false);
        }
        else
        {
            Debug.Log("No dog nearby to pet!");
            yield break; // Stops the coroutine early
        }
    }


    private void ActivateMechanism()
    {
        var mechanism = nearbyMechanism?.GetComponent<IMechanism>();
        if (mechanism != null)
        {
            if (!mechanism.IsActive)
            {
				Debug.Log($"Robot activates {mechanism.GetMechanismName()}!");
				mechanism.Activate();
			}
            else
            {
				Debug.Log($"Robot deactivates {mechanism.GetMechanismName()}!");
				mechanism.Deactivate();
			}
        }
        else
        {
            Debug.Log("No mechanism nearby to activate!");
        }
    }


}