using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class DogAIController : MonoBehaviour
{
    private enum DogAIState
    {
        Idle,
        Following,
        Sitting,
        ControlledByPlayer,
        InDistress,
    };

    [Header("Follow Settings")]
    [SerializeField] private float followDistance = 4.0f;
    [SerializeField] private float followSpeed = 3.0f;
    [SerializeField] private float idleTime = 2.0f;

    [Header("Interact Settings")]
    [SerializeField] private float interactDistance = 3.0f;
    [SerializeField] private Canvas interactCanvas;

    private DogAIState currentState = DogAIState.Idle;
    private Transform player;
    private Rigidbody2D rb;
    private float idleTimer;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private float lastHorizontalDir;

    private void Start()
    {
        GameStateManager.Instance.OnGameStateChange += OnGameStateChange;
        GameManager.Instance.OnSetPlayerToFollow += HandleSetPlayer;
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
    }

    private void OnDestroy()
    {
        GameStateManager.Instance.OnGameStateChange -= OnGameStateChange;
        GameManager.Instance.OnSetPlayerToFollow -= HandleSetPlayer;
    }

    private void Update()
    {
        if (currentState == DogAIState.ControlledByPlayer)
            return;

        DistanceCheck();
    }

    private void FixedUpdate()
    {
        if (currentState == DogAIState.ControlledByPlayer)
            return;

        if (player == null)
        {
            if (GameManager.Instance.PlayerToFollow == null)
                return;
            HandleSetPlayer(GameManager.Instance.PlayerToFollow);
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
                HandleSitting();
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
                SitDog();
                return;
            }
        }
        else
        {
            interactCanvas.enabled = false;
        }

        if (currentState == DogAIState.Sitting)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                StandUpDog();
            }
            return;
        }

        if (distanceToPlayer > followDistance)
        {
            idleTimer += Time.fixedDeltaTime;
            if (idleTimer >= idleTime)
                currentState = DogAIState.Following;
        }
        else
        {
            idleTimer = 0f;
            currentState = DogAIState.Idle;
        }
    }

    private void HandleIdle()
    {
        rb.linearVelocity = Vector2.zero;
        animator?.SetBool("IsRunning", false);
        animator?.SetBool("IsSitting", false);
    }

    private void HandleFollowing()
    {
        if (player == null)
            return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.transform.position);
        if (distanceToPlayer > followDistance)
        {
            float differenceOnX = player.transform.position.x - transform.position.x;
            Vector2 direction = new Vector2(differenceOnX, 0f).normalized;

            rb.linearVelocity = new Vector2(direction.x * followSpeed, rb.linearVelocity.y);

            FlipDog(-direction.x);
            animator?.SetBool("IsRunning", Mathf.Abs(direction.x) > 0.01f);
            animator?.SetBool("IsSitting", false);

            lastHorizontalDir = direction.x;
        }
        else
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            animator?.SetBool("IsRunning", false);
        }
    }

    private void HandleSitting()
    {
        rb.linearVelocity = Vector2.zero;
        animator?.SetBool("IsRunning", false);
        animator?.SetBool("IsSitting", true);
    }

    private void SitDog()
    {
        Debug.Log("Dog is now sitting.");
        currentState = DogAIState.Sitting;
        rb.linearVelocity = Vector2.zero;
        animator?.SetBool("IsRunning", false);
        animator?.SetBool("IsSitting", true);
        interactCanvas.enabled = false;
    }

    private void StandUpDog()
    {
        Debug.Log("Dog is now following.");
        currentState = DogAIState.Following;
        animator?.SetBool("IsSitting", false);
    }

    private void HandleSetPlayer(Transform obj)
    {
        player = obj;
        currentState = DogAIState.Following;
    }

    private void OnGameStateChange(GameState obj)
    {
        if (obj == GameState.RobotTempDeath)
        {
            currentState = DogAIState.ControlledByPlayer;
        }
        else if (obj == GameState.Playing)
        {
            currentState = DogAIState.Following;
        }
    }

    private void FlipDog(float horizontalInput)
    {
        if (horizontalInput > 0.05f)
            spriteRenderer.flipX = false;
        else if (horizontalInput < -0.05f)
            spriteRenderer.flipX = true;
    }
}
