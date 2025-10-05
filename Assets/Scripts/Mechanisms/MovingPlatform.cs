using System.Collections;
using UnityEngine;

public class MovingPlatform : MonoBehaviour
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

    public void Activate()
    {
        // If platform is not moving, start it
        if (!isMoving)
        {
            moveRoutine = StartCoroutine(MovePlatform());
        }
        else
        {
            // If it's already moving, reverse direction immediately
            ReverseDirection();
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

    private void ReverseDirection()
    {
        // Kill the current coroutine
        if (moveRoutine != null)
            StopCoroutine(moveRoutine);

        // Flip direction immediately
        movingToEnd = !movingToEnd;

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
