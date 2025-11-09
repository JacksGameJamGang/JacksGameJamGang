using UnityEngine;

public class Waypoint : MonoBehaviour
{
	[SerializeField] private Waypoint previousWaypoint;
	[SerializeField] private Waypoint nextWaypoint;

	[SerializeField] private WaypointType waypointType;

	public enum WaypointType
	{
		start, basic, end, jumpStart, jumpEnd
	}

	public void InitilizeWaypoint(WaypointType waypointType, Waypoint previousWaypoint, Waypoint nextWaypoint)
	{
		this.waypointType = waypointType;
		this.previousWaypoint = previousWaypoint;
		this.nextWaypoint = nextWaypoint;
	}
	public void AddNextWaypoint(Waypoint nextWaypoint)
	{
		this.nextWaypoint = nextWaypoint;
	}

	public bool ReachedWaypoint(Transform otherTransform)
	{
		float distance = Vector2.Distance(otherTransform.position, transform.position);

		if (distance < 1.5f)
			return true;
		else return false;
	}

	public WaypointType GetWaypointType()
	{
		return waypointType;
	}

	public Waypoint GetPreviousWaypoint()
	{
		return previousWaypoint;
	}
	public Waypoint GetNextWaypoint()
	{
		return nextWaypoint;
	}

	private void OnDrawGizmos()
	{
		Gizmos.color = Color.blue;
		Gizmos.DrawWireSphere(transform.position, 1f);
	}
}
