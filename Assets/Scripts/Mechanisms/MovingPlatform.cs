using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
	[Header("Mechanism Settings")]
	[SerializeField] private DoorMode doorMode = DoorMode.Any;
	[SerializeField] private List<MonoBehaviour> mechanismSources; // Any IMechanism (Lever, Plate, etc.)
	[SerializeField] private List<MechanismStates> mechanismStates; //internal states of all related mechanisms

	[SerializeField] private Transform platform;    // The moving platform
    [SerializeField] private Transform startPoint;  // One end of the track
    [SerializeField] private Transform endPoint;    // The other end of the track
    [SerializeField] private float moveSpeed = 2f;  // Movement speed

    private bool movingToEnd = true;                // Direction flag
    private Coroutine moveRoutine;                  // Reference to running coroutine
    private bool isMoving = false;

    private Rigidbody2D platformRb;

    private void Start()
    {
        platform.position = startPoint.position;

        platformRb = platform.GetComponent<Rigidbody2D>();
        if (platformRb == null)
        {
            platformRb = platform.gameObject.AddComponent<Rigidbody2D>();
        }
    }

	private void Awake()
	{
		mechanismStates = new List<MechanismStates>();
		foreach (var mechanism in mechanismSources)
			mechanismStates.Add(new MechanismStates(mechanism.GetComponent<IMechanism>(), false));
	}

	private void OnEnable()
	{
		foreach (var mechanismState in mechanismStates)
			mechanismState.mechanism.OnToggleMechanism += HandleSwitchChanged;
	}

	private void OnDisable()
	{
		foreach (var mechanismState in mechanismStates)
			mechanismState.mechanism.OnToggleMechanism -= HandleSwitchChanged;
	}

	private void HandleSwitchChanged(IMechanism sender, bool isActive)
	{
		foreach (var mechanismState in mechanismStates)
		{
			if (mechanismState.mechanism != sender) continue;
			mechanismState.isActive = isActive;
		}

		if (MechanismShouldOpen())
			ReverseDirection(true);
		else
			ReverseDirection(false);
	}

	private bool MechanismShouldOpen()
	{
		if (doorMode == DoorMode.Any)
		{
			foreach (var mechanismState in mechanismStates)
			{
				if (mechanismState.isActive)
					return true;
			}
			return false;
		}
		else
		{
			foreach (var mechanismState in mechanismStates)
			{
				if (!mechanismState.isActive)
					return false;
			}
			return true;
		}
	}

	private IEnumerator MovePlatform()
    {
        isMoving = true;

        Transform target = movingToEnd ? endPoint : startPoint;

        while (true)
        {
            Vector2 newPos = Vector2.MoveTowards(
                platformRb.position,
                target.position,
                moveSpeed * Time.deltaTime
            );
            platformRb.MovePosition(newPos);

            if (Vector2.Distance(platformRb.position, target.position) < 0.01f)
            {
                platformRb.MovePosition(target.position);
                break;
            }

            yield return null;
        }

        // When finished moving, flip direction and mark idle
        movingToEnd = !movingToEnd;
        isMoving = false;
    }
    private void ReverseDirection(bool moveToEnd)
    {
        // Kill the current coroutine
        if (moveRoutine != null)
            StopCoroutine(moveRoutine);

        // Flip direction immediately
        movingToEnd = moveToEnd;

        // Restart movement in new direction
        moveRoutine = StartCoroutine(MovePlatform());
    }

    private void OnDrawGizmos()
    {
        if (startPoint && endPoint)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(startPoint.position, endPoint.position);
        }
    }
}
