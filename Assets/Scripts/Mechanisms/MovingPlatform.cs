using System.Collections;
using UnityEngine;

public class MovingPlatform : MechanismListener
{
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

	protected override void HandleSwitchChanged(IMechanism sender, bool isActive)
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
