using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering.Universal;

[RequireComponent(typeof(Rigidbody2D))]
public class DogAIController : MonoBehaviour, IInteractable
{
    private PlayerController playerController;
	private PlayerController robotPlayerController;

	private enum DogAIState
    {
        Idle,
        Following,
        WaypointFollowing,
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

    [Header("Sight Settings")]
	[SerializeField] private LayerMask robotVisibleCheck;

    [Header("Waypoint Pathing")]
    public List<Waypoint> waypointPath = new();
    public List<Waypoint> tempWaypointPath = new();
	public Waypoint waypointTarget;

	[SerializeField] private DogAIState currentState = DogAIState.Idle;
    private Transform player;
    private Rigidbody2D rb;
    private float followDelayTimer;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private float lastHorizontalDir;

	private void Start()
    {
        playerController = GetComponent<PlayerController>();
		rb = GetComponent<Rigidbody2D>();
		spriteRenderer = GetComponent<SpriteRenderer>();
		animator = GetComponent<Animator>();

		GameStateManager.Instance.OnGameStateChange += OnGameStateChange;
        GameManager.Instance.OnSetPlayerToFollow += HandleSetPlayer;

        PlayerController.OnPlayerJump += CopyPlayerJump;
    }

    private void OnDestroy()
    {
        GameStateManager.Instance.OnGameStateChange -= OnGameStateChange;
        GameManager.Instance.OnSetPlayerToFollow -= HandleSetPlayer;

		PlayerController.OnPlayerJump -= CopyPlayerJump;
	}

    private void Update()
    {
        if (currentState == DogAIState.ControlledByPlayer)
            return;

		playerController.DogTouchingGroundCheck();
		playerController.UpdateFallSpeed();

		WaypointManager.Instance.UpdateDogsClosestWaypoint(transform.position);

		InteractDistanceCheck();
		DogsNeedsWaypointPath();
		DogsPathStillValid();
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
            case DogAIState.WaypointFollowing:
            HandleWaypointFollowing();
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
	private void InteractDistanceCheck()
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

		if (waypointPath.Count == 00 && distanceToPlayer > followDistanceFromIdle)
		{
			followDelayTimer += Time.fixedDeltaTime;
			if (followDelayTimer >= followDelay)
				ChangeDogState(DogAIState.Following);
		}
	}

	//follow behaviour
	private void HandleFollowing()
    {
		if (player == null)
			return;

		float distanceToPlayer = Vector2.Distance(transform.position, player.transform.position);

		if (distanceToPlayer > followDistance)
		{
			float differenceOnX = player.transform.position.x - transform.position.x;

			if (Mathf.Abs(differenceOnX) < 0.1f) return;

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
    private void HandleWaypointFollowing()
    {
		if (player == null || waypointTarget == null)
			return;

        if (waypointPath.Count == 0)
            ChangeDogState(DogAIState.Following);

        if (waypointTarget.ReachedWaypoint(transform))
        {
            Waypoint.WaypointType waypointType = waypointTarget.GetWaypointType();

            if (waypointType == Waypoint.WaypointType.jumpStart)
			{
				if (playerController.isGrounded) //jump only when grounded
				{
					playerController.DogAiForceJump();
					waypointPath.Remove(waypointTarget);
					waypointTarget = waypointPath[0];
				}
			}
			else
			{
				waypointPath.Remove(waypointTarget);

				if (waypointPath.Count == 0)
					ChangeDogState(DogAIState.Following);
				else
					waypointTarget = waypointPath[0];
			}
		}
        else
        {
			float differenceOnX = waypointTarget.transform.position.x - transform.position.x;

			if (Mathf.Abs(differenceOnX) < 0.1f) return;

			Vector2 direction = new Vector2(differenceOnX, 0f).normalized;
			rb.linearVelocity = new Vector2(direction.x * (followSpeed + 2), rb.linearVelocity.y); //catch upto player

			FlipDog(-direction.x);
			lastHorizontalDir = direction.x;
		}
	}
    private void SetWaypointPath(List<Waypoint> waypointPath)
    {
        this.waypointPath = waypointPath;
        waypointTarget = waypointPath[0]; 
        ChangeDogState(DogAIState.WaypointFollowing);
    }
	private void FlipDog(float horizontalInput)
	{
		if (horizontalInput > 0.05f)
			spriteRenderer.flipX = false;
		else if (horizontalInput < -0.05f)
			spriteRenderer.flipX = true;
	}

    private void CopyPlayerJump()
    {
        if (playerController.isActiveAndEnabled) return; //currently dog so dont double jump

        StopCoroutine(DelayCopyPlayerJump());
        StartCoroutine(DelayCopyPlayerJump());
    }
    private IEnumerator DelayCopyPlayerJump()
    {
        yield return new WaitForSeconds(0.25f);

		if (playerController.isGrounded)
			playerController.DogAiForceJump();
    }

	//AI pathfinding checks
	public void DogsNeedsWaypointPath()
	{
		if (currentState != DogAIState.Following) return;
		if (!playerController.isGrounded) return;

		if (RobotNotVisible() || RobotTooFar())
			SetWaypointPath(WaypointManager.Instance.GetWaypointPath());
	}
	private bool RobotTooFar()
	{
		float yDistance = transform.position.y - player.position.y;
		float distance = Vector2.Distance(transform.position, player.position);

		if (Mathf.Abs(distance) > 8 || robotPlayerController.isGrounded && Mathf.Abs(yDistance) > 2)
			return true;
		else
			return false;
	}
	private bool RobotNotVisible()
	{
		Vector2 raycastStartPos = new(transform.position.x, transform.position.y + 0.5f);
		RaycastHit2D hit = Physics2D.Linecast(raycastStartPos, player.position, robotVisibleCheck);

		if (hit.rigidbody.GetComponent<RobotController>() == null)
			return true;
		else
			return false;
	}
	private void DogsPathStillValid()
	{
		if (currentState != DogAIState.WaypointFollowing) return;
		if (!playerController.isGrounded) return;

		List<Waypoint> newWaypointPath = WaypointManager.Instance.GetWaypointPath();
		tempWaypointPath = newWaypointPath;

		if (TargetWaypointDifferent() || WaypointPathDifferent(waypointPath, newWaypointPath))
			SetWaypointPath(newWaypointPath);
	}	
	private bool TargetWaypointDifferent()
	{
		if (WaypointManager.Instance.RobotsClosestWaypoint != waypointPath[^1])
			return true;
		else
			return false;
	}
	private bool WaypointPathDifferent(List<Waypoint> current, List<Waypoint> next)
	{
		if (current == null || next == null) return true;

		int indexOffset = current.Count - next.Count;
		if (Mathf.Abs(indexOffset) > 1) return true; //allow minimum 1 offset

		int minCount = Mathf.Min(current.Count, next.Count);

		if (indexOffset == 1) //compare path waypoint + account for 1 possible offset
		{
			for (int i = 0; i < minCount; i++)
			{
				if (current[i + 1] != next[i])
				{
					//Debug.LogError($"Different at current[{i + 1}] != next[{i}] : {current[i + 1].name} != {next[i].name}");
					return true;
				}
			}
		}
		else if (indexOffset == -1)
		{
			for (int i = 0; i < minCount; i++)
			{
				if (current[i] != next[i + 1])
				{
					//Debug.LogError($"Different at current[{i}] != next[{i + 1}] : {current[i].name} != {next[i + 1].name}");
					return true;
				}
			}
		}
		else
		{
			for (int i = 0; i < minCount; i++)
			{
				if (current[i] != next[i])
				{
					//Debug.LogError($"Different at index {i}: {current[i].name} != {next[i].name}");
					return true;
				}
			}
		}

		return false;
	}

	//state changes
	private void ChangeDogState(DogAIState state)
    {
        if (currentState == state) return;

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

			case DogAIState.WaypointFollowing:
			Debug.Log("Dog is now waypoint following.");
			animator?.SetBool("IsRunning", true);
			animator?.SetBool("IsSitting", false);
			interactCanvas.enabled = false;
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
		robotPlayerController = player.GetComponent<PlayerController>();
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

	private void OnDrawGizmos()
	{
        if (player == null) return;

		Vector2 raycastStartPos = new Vector2(transform.position.x, transform.position.y + 0.5f);

		Gizmos.color = Color.yellow;
		Gizmos.DrawLine(raycastStartPos, player.position);

		if (currentState == DogAIState.WaypointFollowing)
		{
			Gizmos.color = Color.blue;

			for (int i = 0; i < waypointPath.Count; i++)
			{
				if (i == 0)
					Gizmos.DrawLine(raycastStartPos, waypointPath[i].transform.position);
				else if (i == waypointPath.Count - 1)
					return;
				else
					Gizmos.DrawLine(waypointPath[i].transform.position, waypointPath[i + 1].transform.position);
			}
		}
	}
}
