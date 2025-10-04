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
            PetDog();
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
    
    private void PetDog()
    {
        if (nearbyDog != null)
        {
            Debug.Log("Robot pets the dog!");
            // Add petting animation/effect here
            // Maybe trigger a happy animation on the dog
        }
        else
        {
            Debug.Log("No dog nearby to pet!");
        }
    }


    private void ActivateMechanism()
    {
        var mechanism = nearbyMechanism?.GetComponent<IMechanism>();
        if (mechanism != null)
        {
            Debug.Log($"Robot activates {mechanism.GetMechanismName()}!");
            mechanism.Activate();
        }
        else
        {
            Debug.Log("No mechanism nearby to activate!");
        }
    }


}