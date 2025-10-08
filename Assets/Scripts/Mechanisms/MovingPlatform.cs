using System.Collections;
using System.Collections.Generic;
using Unity.AppUI.UI;
using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
	[Header("Mechanism Settings")]
	[SerializeField] private DoorMode doorMode = DoorMode.Any;
	[Tooltip("link mechanisms here (Lever, Plate, etc.)")]
	[SerializeField] private List<MechanismStates> linkedMechanisms;
	private MechanismStates lastToggledMechanism;

	[SerializeField] private Transform platform;    // The moving platform
    [SerializeField] private Transform startPoint;  // One end of the track
    [SerializeField] private Transform endPoint;    // The other end of the track
    [SerializeField] private float moveSpeed = 2f;  // Movement speed

    private bool movingToEnd = true;                // Direction flag
    private Coroutine moveRoutine;                  // Reference to running coroutine
    private bool isMoving = false;

    private Rigidbody2D platformRb;

	private void Awake()
	{
		foreach (var linkedMechanism in linkedMechanisms)
			linkedMechanism.Initilize();

		lastToggledMechanism = new();
	}

	private void Start()
    {
        platform.position = startPoint.position;

        platformRb = platform.GetComponent<Rigidbody2D>();
        if (platformRb == null)
        {
            platformRb = platform.gameObject.AddComponent<Rigidbody2D>();
        }
    }

	private void OnEnable()
	{
		foreach (var linkedMechanism in linkedMechanisms)
			linkedMechanism.mechanism.OnToggleMechanism += HandleSwitchChanged;
	}

	private void OnDisable()
	{
		foreach (var linkedMechanism in linkedMechanisms)
			linkedMechanism.mechanism.OnToggleMechanism -= HandleSwitchChanged;
	}

	private void HandleSwitchChanged(IMechanism sender, bool isActive)
	{
		MechanismStates mechanism = null;

		foreach (var linkedMechanism in linkedMechanisms)
		{
			if (linkedMechanism.mechanism != sender) continue;
			linkedMechanism.isActive = isActive;
			mechanism = linkedMechanism;
		}

		if (MechanismShouldOpen(mechanism, isActive))
			ReverseDirection(true);
		else
			ReverseDirection(false);
	}

	private bool MechanismShouldOpen(MechanismStates newToggledMechanism, bool isActive)
	{
		if (doorMode == DoorMode.Any)
		{
			foreach (var linkedMechanism in linkedMechanisms)
			{
				if (linkedMechanism.isActive) //any state match open mech
					return true;
			}
			return false;
		}
		else if (doorMode == DoorMode.Specific)
		{
			foreach (var linkedMechanism in linkedMechanisms)
			{
				if (linkedMechanism.isActive != linkedMechanism.forceStateActive) //state doesnt match forced mech stays closed
					return false;
			}
			return true;
		}
		else if (doorMode == DoorMode.Ordered && isActive) //only check when toggling to active
		{
			if (newToggledMechanism == null)
			{
				Debug.LogError("Mechanism ref null, this shouldnt occur");
				return false;
			}

			if (lastToggledMechanism == null) //always allow toggling of 1st mechanism despite wrong order
			{
				Debug.LogError("1st mechanism trigger (always works)");
				lastToggledMechanism = newToggledMechanism;
				return false;
			}

			if (newToggledMechanism.orderOfMechanism == lastToggledMechanism.orderOfMechanism + 1) //correct order
			{
				Debug.LogError("correct order");
				lastToggledMechanism = newToggledMechanism;

				foreach (var linkedMechanism in linkedMechanisms)
				{
					if (!linkedMechanism.isActive) //any state not active mech stays closed
						return false;
				}
				return true;
			}
			else
			{
				Debug.LogError("incorrect order");
				lastToggledMechanism.linkedMechanism = null;

				foreach (var linkedMechanism in linkedMechanisms) //reset all mechanisms
					linkedMechanism.mechanism.Deactivate();

				return false;
			}
		}
		else
		{
			foreach (var linkedMechanism in linkedMechanisms)
			{
				if (!linkedMechanism.isActive) //any state not active mech stays closed
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
