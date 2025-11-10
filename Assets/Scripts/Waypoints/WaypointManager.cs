using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using static Waypoint;

public class WaypointManager : LocalSingleton<WaypointManager>
{
	public GameObject WaypointPrefab;
	[SerializeField] private float waypointsSpacing;
	[SerializeField] private LayerMask waypointLayer;

	private List<Waypoint> waypoints = new();

	public Waypoint RobotsClosestWaypoint;
	public Waypoint DogsClosestWaypoint;
	public Waypoint LastPlacedWaypoint;

	public static event Action<List<Waypoint>> MakeDogFollowWaypoint;

	private bool blockBasicWaypointPlacing;

	private void OnDestroy()
	{
		UnityEngine.SceneManagement.SceneManager.sceneLoaded -= CleanUpOldWaypoints;
	}
	private void Start()
	{
		UnityEngine.SceneManagement.SceneManager.sceneLoaded += CleanUpOldWaypoints;
	}

	private void CleanUpOldWaypoints(Scene scene, LoadSceneMode loadSceneMode)
	{
		for (int i = waypoints.Count - 1; i > 0; i--)
			Destroy(waypoints[i].gameObject);

		waypoints.Clear();
		Debug.LogError("clearing waypoints");
	}

	//waypoint path generation
	public void GetWaypointPath()
	{
		Vector2 dogsPosition = GameManager.Instance.DogController.transform.position;
		Vector2 robotsPosition = GameManager.Instance.RobotController.transform.position;

		DogsClosestWaypoint = FindClosestWaypoint(dogsPosition);
		RobotsClosestWaypoint = FindClosestWaypoint(robotsPosition);

		Debug.LogError("(new path) dog pos: " + dogsPosition + " | closeset: " + DogsClosestWaypoint.transform.position);
		Debug.LogError("(new path) robot pos: " + robotsPosition + " | closest: " + RobotsClosestWaypoint.transform.position);

		List<Waypoint> path = FindPathToTargetWaypoint(DogsClosestWaypoint, RobotsClosestWaypoint, true)
			?? FindPathToTargetWaypoint(DogsClosestWaypoint, RobotsClosestWaypoint, false); //find path by looking at next/previous waypoints

		if (path == null)
			Debug.LogError("found no path searching forwards and backwards");

		MakeDogFollowWaypoint?.Invoke(path);
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

		Debug.LogError("duplicate jump waypoints check passed");

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
	private void UpdateInfoOnWaypointCreation(Waypoint newWaypoint)
	{
		if (LastPlacedWaypoint != null)
			LastPlacedWaypoint.AddNextWaypoint(newWaypoint);

		LastPlacedWaypoint = newWaypoint;
		RobotsClosestWaypoint = newWaypoint;
		waypoints.Add(newWaypoint);
		newWaypoint.name = $"Waypoint {waypoints.Count}";
	}
	private Waypoint FindClosestWaypoint(Vector2 position)
	{
		Collider2D[] hits = Physics2D.OverlapCircleAll(position, waypointsSpacing, waypointLayer);
		Waypoint closestWaypoint = null;
		float closestWaypointDistance = 100;

		foreach (Collider2D hit in hits)
		{
			//Debug.LogError(hit.name + "for pos: " + position);

			float waypointDistance = Vector2.Distance(position, hit.transform.position);
			if (waypointDistance > closestWaypointDistance) continue;

			closestWaypoint = hit.GetComponent<Waypoint>();
			closestWaypointDistance = waypointDistance;
		}
		return closestWaypoint;
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
