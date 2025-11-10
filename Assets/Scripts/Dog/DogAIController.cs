using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static WaypointManager;

[RequireComponent(typeof(Rigidbody2D))]
public class DogAIController : MonoBehaviour, IInteractable
{
    private PlayerController playerController;

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
        WaypointManager.MakeDogFollowWaypoint += GetWaypointPath;

        PlayerController.OnPlayerJump += CopyPlayerJump;
    }

    private void OnDestroy()
    {
        GameStateManager.Instance.OnGameStateChange -= OnGameStateChange;
        GameManager.Instance.OnSetPlayerToFollow -= HandleSetPlayer;
		WaypointManager.MakeDogFollowWaypoint -= GetWaypointPath;

		PlayerController.OnPlayerJump -= CopyPlayerJump;
	}

    private void Update()
    {
        if (currentState == DogAIState.ControlledByPlayer)
            return;

		playerController.DogTouchingGroundCheck();
		playerController.UpdateFallSpeed();

		InteractDistanceCheck();

        DogTooFarFromPlayerCheck();
		PathStillValidCheck();
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

        //find waypoint path to player, move to 1st waypoint, depending on waypoint type do x, then move to next waypoint
        //continue above till next waypoint is empty then switch to following player normally
        if (waypointPath.Count == 0)
            ChangeDogState(DogAIState.Following);

        if (waypointTarget.ReachedWaypoint(transform))
        {
            Waypoint.WaypointType waypointType = waypointTarget.GetWaypointType();

            if (waypointType == Waypoint.WaypointType.jumpStart)
			{
				Debug.LogError("jump start");
				playerController.DogAiForceJump();
			}
            else if (waypointType == Waypoint.WaypointType.jumpEnd)
            {
				Debug.LogError("jump end");
            }

            waypointPath.Remove(waypointTarget);

            if (waypointPath.Count == 0)
				ChangeDogState(DogAIState.Following);
            else
			    waypointTarget = waypointPath[0];
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
    private void GetWaypointPath(List<Waypoint> waypointPath)
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
        playerController.DogAiForceJump();
    }

	//AI pathfinding checks
	private void DogTooFarFromPlayerCheck()
    {
		if (RobotVisibleCheck()) return;

        Vector2 distance = transform.position - player.position;

		if (Mathf.Abs(distance.x) > 8 || Mathf.Abs(distance.y) > 2)
		{
			ChangeDogState(DogAIState.Idle);
			WaypointManager.Instance.GetWaypointPath();
		}
	}
	private bool RobotVisibleCheck()
	{
		Vector2 raycastStartPos = new(transform.position.x, transform.position.y + 0.5f);
		RaycastHit2D hit = Physics2D.Linecast(raycastStartPos, player.position, robotVisibleCheck);

		if (hit.rigidbody.GetComponent<RobotController>() != null)
		{
			return true;
		}
		else
		{
			return false;
		}
	}

	/// <summary>
	/// fix dog getting stuck endlessly checking and regrabbing path every x seconds
	/// </summary>

	private void PathStillValidCheck()
	{
		if (currentState != DogAIState.WaypointFollowing || waypointTarget == null) return;
		if (!playerController.isGrounded) return;

		Vector2 distance = transform.position - player.position;

		Debug.LogError("path valid check: " + distance);

		if (Mathf.Abs(distance.y) > 2)
		{
			ChangeDogState(DogAIState.Idle);
			WaypointManager.Instance.GetWaypointPath();
		}
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
	}
}
