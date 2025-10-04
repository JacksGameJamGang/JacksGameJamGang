using UnityEngine;

public class Chip : MonoBehaviour
{
    [Header("Chip Settings")]
    [SerializeField] private float floatHeight = 1f;
    [SerializeField] private float floatSpeed = 2f;
    [SerializeField] private float pickupDistance = 1f;
    [SerializeField] private float reviveDistance = 2f;

    [Header("References")]
    [SerializeField] private Transform dogTransform;
    [SerializeField] private Transform robotTransform;

    private bool isPickedUp = false;
    private bool isFollowingDog = false;

    private void Start()
    {
        // Find dog and robot if not assigned
        if (dogTransform == null)
        {
            dogTransform = GameManager.Instance.DogController;
        }
        if (robotTransform == null)
        {
            robotTransform = GameManager.Instance.RobotController;
        }
    }

    private void Update()
    {
        if (isPickedUp)
        {
            FollowDog();
        }
        else
        {
            CheckForPickup();
        }

        CheckForRevive();
    }

    private void CheckForPickup()
    {
        if (dogTransform == null) return;

        float distanceToDog = Vector2.Distance(transform.position, dogTransform.position);

        if (distanceToDog <= pickupDistance)
        {
            PickupChip();
        }
    }

    private void PickupChip()
    {
        isPickedUp = true;
        isFollowingDog = true;
        Debug.Log("Chip picked up by dog!");
    }

    private void FollowDog()
    {
        if (dogTransform == null) return;

        // Calculate position above dog's head
        Vector3 targetPosition = dogTransform.position + Vector3.up * floatHeight;

        // Smoothly move to target position
        transform.position = Vector3.Lerp(transform.position, targetPosition, floatSpeed * Time.deltaTime);
    }

    private void CheckForRevive()
    {
        if (!isPickedUp || dogTransform == null || robotTransform == null) return;

        float distanceToRobot = Vector2.Distance(dogTransform.position, robotTransform.position);

        if (distanceToRobot <= reviveDistance)
        {
            ReviveRobot();
        }
    }

    private void ReviveRobot()
    {
        Debug.Log("Robot revived by chip!");

        // Reset chip
        isPickedUp = false;
        isFollowingDog = false;

        GameStateManager.Instance.ChangeState(GameState.Playing);

        Destroy(gameObject);
    }
}