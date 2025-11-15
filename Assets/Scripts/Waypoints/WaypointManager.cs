using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using static Waypoint;

public class WaypointManager : LocalSingleton<WaypointManager>
{
	public GameObject WaypointPrefab;
	[SerializeField] private float waypointsSpacing;
	[SerializeField] private LayerMask waypointLayer;

	public List<Waypoint> waypoints = new();

	public Waypoint RobotsClosestWaypoint;
	public Waypoint DogsClosestWaypoint;
	public Waypoint LastPlacedWaypoint;

	public static event Action<List<Waypoint>> MakeDogFollowWaypoint;

	private bool blockBasicWaypointPlacing;

	private float robotsClosestWaypointTimer;
	private float dogsClosestWaypointTimer;
	
	//waypoint pathfinding
	public List<Waypoint> GetWaypointPath()
	{
		Vector2 dogsPosition = GameManager.Instance.DogController.transform.position;
		Vector2 robotsPosition = GameManager.Instance.RobotController.transform.position;

		UpdateDogsClosestWaypoint(dogsPosition);
		UpdateRobotsClosestWaypoint(robotsPosition);

		List<Waypoint> path = FindShortestWaypointPath(DogsClosestWaypoint, RobotsClosestWaypoint);

		if (path == null)
			Debug.LogError("found no path searching forwards and backwards");

		return path;
	}
	public List<Waypoint> FindShortestWaypointPath(Waypoint currentWaypoint, Waypoint targetWaypoint)
	{
		if (currentWaypoint == targetWaypoint)
			return new List<Waypoint> { currentWaypoint };

		//Queue waypoints for pathing BFS, record waypoints found
		Queue<Waypoint> queue = new Queue<Waypoint>();
		Dictionary<Waypoint, Waypoint> cameFrom = new Dictionary<Waypoint, Waypoint>();
		HashSet<Waypoint> visited = new HashSet<Waypoint>();

		queue.Enqueue(currentWaypoint);
		visited.Add(currentWaypoint);

		while (queue.Count > 0)
		{
			Waypoint current = queue.Dequeue();

			foreach (WaypointLink waypointLink in current.waypointLinks)
			{
				Waypoint neighbor = waypointLink.waypoint;

				if (visited.Contains(neighbor))
					continue;

				//return found path
				if (neighbor == targetWaypoint)
				{
					cameFrom[neighbor] = current;
					return ReconstructPath(cameFrom, currentWaypoint, targetWaypoint);
				}

				//add visited waypoints
				visited.Add(neighbor);
				cameFrom[neighbor] = current;
				queue.Enqueue(neighbor);
			}
		}

		Debug.LogError("Error found no valid path");
		return null;
	}
	private List<Waypoint> ReconstructPath(Dictionary<Waypoint, Waypoint> cameFrom, Waypoint start, Waypoint goal)
	{
		List<Waypoint> path = new List<Waypoint>();
		Waypoint current = goal;

		while (current != start)
		{
			path.Insert(0, current);  // Insert at the front of the list
			current = cameFrom[current];
		}
		path.Insert(0, start);  // Add the start waypoint at the beginning

		return path;
	}

	//waypoint creation
	public void BasicWaypointPlacing(Vector2 robotCurrentPos, bool isGrounded)
	{
		if (blockBasicWaypointPlacing) return;

		Waypoint newWaypoint;
		WaypointLink newWaypointLink = new(LastPlacedWaypoint, WaypointLink.LinkType.basic);

		if (LastPlacedWaypoint == null)
		{
			newWaypoint = CreateWaypoint(robotCurrentPos);
			newWaypoint.InitilizeWaypoint(WaypointType.start);
			UpdateInfoOnWaypointCreation(newWaypoint);
		}

		float distance = Vector2.Distance(RobotsClosestWaypoint.transform.position, robotCurrentPos);

		if (distance <= waypointsSpacing || !isGrounded) return; //dont place waypoints too close or floating ones
		if (CheckSurroundingsForWaypoints(robotCurrentPos, waypointsSpacing)) return;

		newWaypoint = CreateWaypoint(robotCurrentPos);
		newWaypoint.InitilizeWaypoint(WaypointType.basic, LastPlacedWaypoint);
		UpdateInfoOnWaypointCreation(newWaypoint);
	}
	//NEEDS UPDATING TO ADD EXTRA LINKS TO SURROUNDING JUMP WAYPOINTS
	public void CreateJumpWaypointPair(Vector2 playerLeftGroundPosition, Vector2 playerTouchedGroundPosition)
	{
		Waypoint jumpStartWaypoint = CheckForDuplicateJumpWaypoints(WaypointType.jumpStart, playerLeftGroundPosition, 2f);
		Waypoint jumpEndWaypoint = CheckForDuplicateJumpWaypoints(WaypointType.jumpEnd, playerTouchedGroundPosition, 2f);

		List<Waypoint> jumpStartWaypointsToLink = new();
		List<Waypoint> jumpEndWaypointsToLink = new();

		if (jumpStartWaypoint == null)
		{
			jumpStartWaypoint = CreateWaypoint(playerLeftGroundPosition);
			jumpStartWaypointsToLink.Add(LastPlacedWaypoint);
		}
		if (jumpEndWaypoint == null)
		{
			jumpEndWaypoint = CreateWaypoint(playerTouchedGroundPosition);
			jumpEndWaypointsToLink.Add(jumpStartWaypoint);
		}

		//initilize jump waypoints
		jumpStartWaypoint.InitilizeWaypoint(WaypointType.jumpStart, jumpStartWaypointsToLink);
		jumpEndWaypoint.InitilizeWaypoint(WaypointType.jumpEnd, jumpEndWaypointsToLink);

		UpdateInfoOnWaypointCreation(jumpStartWaypoint);
		UpdateInfoOnWaypointCreation(jumpEndWaypoint);
	}
	private Waypoint CreateWaypoint(Vector2 position)
	{
		return Instantiate(WaypointPrefab, position, Quaternion.identity).GetComponent<Waypoint>();
	}

	//update info
	public void UpdateRobotsClosestWaypoint(Vector2 position)
	{
		if (robotsClosestWaypointTimer > 0)
			robotsClosestWaypointTimer -= Time.deltaTime;
		else
		{
			Waypoint closestWaypoint = null;
			float closestWaypointDistance = 100;

			foreach (Waypoint waypoint in waypoints)
			{
				float waypointDistance = Vector2.Distance(position, waypoint.transform.position);
				if (waypointDistance > closestWaypointDistance) continue;

				closestWaypoint = waypoint;
				closestWaypointDistance = waypointDistance;
			}

			RobotsClosestWaypoint = closestWaypoint;
			robotsClosestWaypointTimer = 0.5f;
		}
	}
	public void UpdateDogsClosestWaypoint(Vector2 position)
	{
		if (dogsClosestWaypointTimer > 0)
			dogsClosestWaypointTimer -= Time.deltaTime;
		else
		{
			Waypoint closestWaypoint = null;
			float closestWaypointDistance = 100;

			foreach (Waypoint waypoint in waypoints)
			{
				float waypointDistance = Vector2.Distance(position, waypoint.transform.position);
				if (waypointDistance > closestWaypointDistance) continue;

				closestWaypoint = waypoint;
				closestWaypointDistance = waypointDistance;
			}

			DogsClosestWaypoint = closestWaypoint;
			dogsClosestWaypointTimer = 0.5f;
		}
	}
	private void UpdateInfoOnWaypointCreation(Waypoint newWaypoint)
	{
		LastPlacedWaypoint = newWaypoint;
		RobotsClosestWaypoint = newWaypoint;
		waypoints.Add(newWaypoint);
		newWaypoint.name = $"Waypoint {waypoints.Count}";
	}

	//waypoint checking
	private bool CheckSurroundingsForWaypoints(Vector2 checkPosition, float checkDistance)
	{
		return Physics2D.OverlapCircle(checkPosition, checkDistance - 1, waypointLayer);
	}
	private Waypoint CheckForDuplicateJumpWaypoints(WaypointType waypointType, Vector2 checkPosition, float checkDistance)
	{
		Collider2D[] hits = Physics2D.OverlapCircleAll(checkPosition, checkDistance, waypointLayer);

		foreach (Collider2D hit in hits)
		{
			Waypoint waypoint = hit.GetComponent<Waypoint>();

			if (waypoint.GetWaypointType() == waypointType)
				return waypoint;
		}

		return null;
	}

	//basic waypoint blocking after jumping
	public void BlockBasicWaypointSpacing()
	{
		blockBasicWaypointPlacing = true;
	}
	public void AllowBasicWaypointSpacing()
	{
		StopCoroutine(FinishedJumpingTimer());
		StartCoroutine(FinishedJumpingTimer());
	}
	private IEnumerator FinishedJumpingTimer()
	{
		yield return new WaitForSeconds(5f);
		blockBasicWaypointPlacing = false;
	}

	private void OnDrawGizmos()
	{
		Gizmos.color = Color.red;
		Gizmos.DrawWireSphere(transform.position, waypointsSpacing - 1);
	}
}
