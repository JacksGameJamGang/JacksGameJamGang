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

	/// <summary>
	/// DYNAMIC WAYPOINT PLACEMENT ISSUE FIXES:
	/// link start and jump waypoints to other waypoints within waypointsSpacing limit
	/// define a dictionary for linked waypoints including a ref to said waypoint + a new waypoint link type eg: basic, jump etc...
	/// 
	/// manually placing waypoints can work but comes with its own issues, eg: platform types complicate this (they move/dissapear)
	/// </summary>
	
	//waypoint path generation
	public List<Waypoint> GetWaypointPath()
	{
		Vector2 dogsPosition = GameManager.Instance.DogController.transform.position;
		Vector2 robotsPosition = GameManager.Instance.RobotController.transform.position;

		UpdateDogsClosestWaypoint(dogsPosition);
		UpdateRobotsClosestWaypoint(robotsPosition);

		List<Waypoint> path = FindPathToTargetWaypoint(DogsClosestWaypoint, RobotsClosestWaypoint, true)
			?? FindPathToTargetWaypoint(DogsClosestWaypoint, RobotsClosestWaypoint, false); //find path by looking at next/previous waypoints

		if (path == null)
			Debug.LogError("found no path searching forwards and backwards");

		return path;
	}
	private List<Waypoint> FindPathToTargetWaypoint(Waypoint currentWaypoint, Waypoint targetWaypoint, bool searchNext)
	{
		var waypoints = new List<Waypoint>();

		if (currentWaypoint == null)
			return null; //end of waypoint chain

		waypoints.Add(currentWaypoint);

		if (currentWaypoint == targetWaypoint)
			return waypoints; //return waypoint path

		List<Waypoint> nextPath = FindPathToTargetWaypoint(
			searchNext ? currentWaypoint.GetNextWaypoint() : currentWaypoint.GetPreviousWaypoint(), targetWaypoint, searchNext);

		if (nextPath != null)
			waypoints.AddRange(nextPath);
		else
			return null; //end of path waypint chain

		return waypoints;
	}

	//waypoint creation
	public void BasicWaypointPlacing(Vector2 robotCurrentPos, bool isGrounded)
	{
		if (blockBasicWaypointPlacing) return;

		Waypoint newWaypoint;

		if (LastPlacedWaypoint == null)
		{
			newWaypoint = CreateWaypoint(robotCurrentPos);
			newWaypoint.InitilizeWaypoint(WaypointType.start, LastPlacedWaypoint, null);
			UpdateInfoOnWaypointCreation(newWaypoint);
		}

		float distance = Vector2.Distance(RobotsClosestWaypoint.transform.position, robotCurrentPos);

		if (distance <= waypointsSpacing || !isGrounded) return; //dont place waypoints too close or floating ones
		if (CheckSurroundingsForWaypoints(robotCurrentPos, waypointsSpacing)) return;

		newWaypoint = CreateWaypoint(robotCurrentPos);
		newWaypoint.InitilizeWaypoint(WaypointType.basic, LastPlacedWaypoint, null);
		UpdateInfoOnWaypointCreation(newWaypoint);
	}
	public void CreateJumpWaypointPair(Vector2 playerLeftGroundPosition, Vector2 playerTouchedGroundPosition)
	{
		if (CheckForDuplicateJumpWaypoints(WaypointType.jumpStart, playerLeftGroundPosition, 2f) && 
			CheckForDuplicateJumpWaypoints(WaypointType.jumpEnd, playerTouchedGroundPosition, 2f)) return; //stop dupe jump points for same jump

		Waypoint previousWaypoint = CreateWaypoint(playerLeftGroundPosition);
		Waypoint nextWaypoint = CreateWaypoint(playerTouchedGroundPosition);

		previousWaypoint.InitilizeWaypoint(WaypointType.jumpStart, LastPlacedWaypoint, nextWaypoint);
		nextWaypoint.InitilizeWaypoint(WaypointType.jumpEnd, previousWaypoint, null);

		UpdateInfoOnWaypointCreation(previousWaypoint);
		UpdateInfoOnWaypointCreation(nextWaypoint);
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
		if (LastPlacedWaypoint != null)
			LastPlacedWaypoint.AddNextWaypoint(newWaypoint);

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
	private bool CheckForDuplicateJumpWaypoints(WaypointType waypointType, Vector2 checkPosition, float checkDistance)
	{
		Collider2D[] hits = Physics2D.OverlapCircleAll(checkPosition, checkDistance, waypointLayer);

		foreach (Collider2D hit in hits)
		{
			Waypoint waypoint = hit.GetComponent<Waypoint>();

			if (waypoint.GetWaypointType() == waypointType)
				return true;
		}

		return false;
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
