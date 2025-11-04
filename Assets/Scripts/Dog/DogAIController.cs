using System.Collections;
using Unity.Mathematics;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class DogAIController : MonoBehaviour, IInteractable
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
	[SerializeField] private float followSpeed = 6f;
	[SerializeField] private float followDistanceFromIdle = 1.75f;
	[SerializeField] private float followDistanceCatchup = 1.5f;
	[SerializeField] private float followDistance = 1.25f;
    [SerializeField] private float followDelay = 0.25f;

    [Header("Interact Settings")]
    [SerializeField] private float interactDistance = 3.0f;
    [SerializeField] private Canvas interactCanvas;

    private DogAIState currentState = DogAIState.Idle;
    private Transform player;
    private Rigidbody2D rb;
    private float followDelayTimer;
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
			//no op
			break;
			case DogAIState.Following:
			HandleFollowing();
			break;
			case DogAIState.Sitting:
			//no op
			break;
		}
	}

    // IInteract
	public void Interact()
	{
		StartCoroutine(PetDogInteraction());
	}
	private IEnumerator PetDogInteraction()
	{
		animator.SetBool("IsSitting", true);
		yield return new WaitForSeconds(2f); // Wait 2 seconds (adjust duration as needed)
		animator.SetBool("IsSitting", false);

		Debug.Log("Robot pets the dog!");
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
                ChangeDogState(DogAIState.Sitting);
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
				ChangeDogState(DogAIState.Idle);
			}
            return;
        }

        if (distanceToPlayer > followDistanceFromIdle)
        {
            followDelayTimer += Time.fixedDeltaTime;
            if (followDelayTimer >= followDelay)
				ChangeDogState(DogAIState.Following);
		}
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


            if (distanceToPlayer > followDistanceCatchup)
				rb.linearVelocity = new Vector2(direction.x * (followSpeed + 2), rb.linearVelocity.y); //catch upto player
            else
				rb.linearVelocity = new Vector2(direction.x * followSpeed, rb.linearVelocity.y);

			FlipDog(-direction.x);
            lastHorizontalDir = direction.x;
        }
        else
        {
            ChangeDogState(DogAIState.Idle);
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        }
    }

    private void ChangeDogState(DogAIState state)
    {
        currentState = state;

        switch (currentState)
        {
            case DogAIState.Idle:
			Debug.Log("Dog is now idle.");
            followDelayTimer = 0;
			rb.linearVelocity = Vector2.zero;
			animator?.SetBool("IsRunning", false);
			animator?.SetBool("IsSitting", false);
			interactCanvas.enabled = true;
			break;

            case DogAIState.Following:
			Debug.Log("Dog is now following.");
			animator?.SetBool("IsRunning", true);
			animator?.SetBool("IsSitting", false);
			interactCanvas.enabled = true;
			break;

            case DogAIState.Sitting:
			Debug.Log("Dog is now sitting.");
			followDelayTimer = 0;
			rb.linearVelocity = Vector2.zero;
			animator?.SetBool("IsRunning", false);
			animator?.SetBool("IsSitting", true);
			interactCanvas.enabled = false;
			break;
        }
	}

	private void HandleSetPlayer(Transform obj)
    {
        player = obj;
        currentState = DogAIState.Following;
    }

    private void OnGameStateChange(GameState obj)
    {
        if (obj == GameState.RobotDowned)
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
