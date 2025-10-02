using UnityEngine;

/// <summary>
/// Has to be refactored
/// Subscribe on Awake
/// cant have that mess of a distance check
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class DogAIController : MonoBehaviour
{
    private enum DogAIState
    {
        Idle,
        Following,
        Sitting,
    };
    
    [Header("Follow Settings")]
    [SerializeField] private float followDistance = 4.0f;
    [SerializeField] private float followSpeed = 3.0f;
    [SerializeField] private float idleTime = 2.0f;
    
    [Header("Interact Settings")]
    [SerializeField] private float interactDistance = 3.0f;
    [SerializeField] private Canvas interactCanvas;

    private DogAIState currentState = DogAIState.Idle;
    private PlayerCharacter player;
    private Rigidbody2D rb;
    private float idleTimer;

    // just for testing
    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        GameManager.Instance.OnSetPlayerToFollow += HandleSetPlayer;
    }

    private void OnDestroy()
    {
        GameManager.Instance.OnSetPlayerToFollow -= HandleSetPlayer;
    }

    private void Update()
    {
        DistanceCheck();
    }

    private void FixedUpdate()
    {
        if (player == null)
        {
            if (GameManager.Instance.PlayerCharacter == null || GameManager.Instance.PlayerIsDead)
                return;
            HandleSetPlayer(GameManager.Instance.PlayerCharacter);
        }

        switch (currentState)
        {
            case DogAIState.Idle:
                HandleIdle();
                break;
            case DogAIState.Following:
                HandleFollowing();
                break;
            case DogAIState.Sitting:
                break;
            default:
                break;
        }
    }

    private void DistanceCheck()
    {
        if (player == null) 
            return;
        
        float distanceToPlayer = Vector2.Distance(transform.position, player.transform.position);
        
        if (interactDistance >= distanceToPlayer && currentState != DogAIState.Sitting)
        {
            interactCanvas.enabled = true;
            if (Input.GetKeyDown(KeyCode.E))
            {
                Debug.Log("Dog is now sitting.");
                currentState = DogAIState.Sitting;
                spriteRenderer.color = Color.blue; // just for testing
                interactCanvas.enabled = false;
                return;
            }
        } 
        else
        {
            interactCanvas.enabled = false;
        }
            
        if (currentState == DogAIState.Sitting) // just for now
            return;
        
        if (distanceToPlayer > followDistance)
        {
            idleTimer += Time.fixedDeltaTime;
            spriteRenderer.color = Color.yellow; // just for testing
            if (idleTimer >= idleTime)
            {
                spriteRenderer.color = Color.red; // just for testing
                currentState = DogAIState.Following;
            }
        }
        else
        {
            spriteRenderer.color = Color.green; // just for testing
            idleTimer = 0f;
            currentState = DogAIState.Idle;
        }
    }

    private void HandleIdle()
    {
        // Dog is idle, maybe play an idle animation or look around
        Debug.Log("Dog is idling.");
    }

    private void HandleFollowing()
    {
        if (currentState == DogAIState.Idle && player == null)
            return;

        Debug.Log("Dog is following.");
        
        float distanceToPlayer = Vector2.Distance(transform.position, player.transform.position);
        if (distanceToPlayer > followDistance)
        {
            float differenceOnX = player.transform.position.x - transform.position.x;
            Vector2 direction = new Vector2(differenceOnX, 0f).normalized;

            rb.MovePosition(rb.position + direction * followSpeed * Time.deltaTime);
        }
    }

    private void HandleSetPlayer(PlayerCharacter obj)
    {
        player = obj;
        currentState = DogAIState.Following;
    }
}
    
