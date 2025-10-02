using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class DogAIController : MonoBehaviour
{
    [SerializeField] private float followDistance = 4.0f;
    [SerializeField] private float followSpeed = 3.0f;
    [SerializeField] private float idleTime = 2.0f;
 
    private PlayerCharacter player;
    private bool isFollowingPlayer;
    private Rigidbody2D rb;
    private float idleTimer;
    
    // just for testing
    private SpriteRenderer spriteRenderer;
    
    private void Awake()
    {
        GameManager.Instance.OnSetPlayerToFollow += HandleSetPlayerToFollow;
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void FixedUpdate()
    {
        if (player == null)
        {
            if (GameManager.Instance.PlayerCharacter == null)
                return;
            HandleSetPlayerToFollow(GameManager.Instance.PlayerCharacter);
        }

        DistanceCheck();
        FollowPlayer();
    }

    private void DistanceCheck()
    {
        if (player == null)
            return;
        
        float distanceToPlayer = Vector2.Distance(transform.position, player.transform.position);
        if (distanceToPlayer > followDistance)
        {
            idleTimer += Time.fixedDeltaTime;
            spriteRenderer.color = Color.yellow; // just for testing
            if (idleTimer >= idleTime)
            {
                spriteRenderer.color = Color.red; // just for testing
                isFollowingPlayer = true;
            }
        }
        else
        {
            spriteRenderer.color = Color.green; // just for testing
            idleTimer = 0f;
            isFollowingPlayer = false;
        }
    }

    private void FollowPlayer()
    {
        if (!isFollowingPlayer && player == null)
            return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.transform.position);
        if (distanceToPlayer > followDistance)
        {
            float differenceOnX = player.transform.position.x - transform.position.x;
            Vector2 direction = new Vector2(differenceOnX, 0f).normalized;

            rb.MovePosition(rb.position + direction * followSpeed * Time.deltaTime);
        }
    }

    private void HandleSetPlayerToFollow(PlayerCharacter obj)
    {
        player = obj;
        isFollowingPlayer = true;
    }
}
