using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static Waypoint;

public class WaypointManager : MonoBehaviour
{
	private PlayerController playerController;

	public GameObject WaypointPrefab;
	[SerializeField] private float waypointsSpacing;
	[SerializeField] private LayerMask waypointLayer;

	private List<Waypoint> waypoints = new();

	public Waypoint ClosestWaypoint;
	public Waypoint LastPlacedWaypoint;

	public static event Action<List<Waypoint>> MakeDogFollowWaypoint;

	private bool blockBasicWaypointPlacing;

	private void Awake()
	{
		playerController = GetComponent<PlayerController>();
	}
	private void Start()
	{
		CreateSpacedWaypoint();
	}

	public void Update()
	{
		BasicWaypointPlacing();
	}

	//waypoint path generation
	public List<Waypoint> GetWaypointPath(Vector2 positionToPathTo)
	{
		Debug.LogError("dog getting new path");

		ClosestWaypoint = FindClosestWaypoint(transform.position);
		Waypoint waypointToFind = FindClosestWaypoint(positionToPathTo);

		List<Waypoint> path = FindPathToTargetWaypoint(ClosestWaypoint, waypointToFind, true)
			?? FindPathToTargetWaypoint(ClosestWaypoint, waypointToFind, false); //find path by looking at next/previous waypoints

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
	private void BasicWaypointPlacing()
	{
		if (blockBasicWaypointPlacing) return;

		float distance = Vector2.Distance(ClosestWaypoint.transform.position, transform.position);

		if (distance <= waypointsSpacing || !playerController.TouchingGroundCheck()) return; //dont place waypoints too close or floating ones
		if (CheckSurroundingsForWaypoints(WaypointType.basic, waypointsSpacing)) return;

		CreateSpacedWaypoint();
	}
	private void CreateSpacedWaypoint()
	{
		Waypoint nextWaypont = CreateWaypoint(transform.position);

		if (LastPlacedWaypoint == null)
			nextWaypont.InitilizeWaypoint(WaypointType.start, LastPlacedWaypoint, null);
		else
			nextWaypont.InitilizeWaypoint(WaypointType.basic, LastPlacedWaypoint, null);

		UpdateInfoOnWaypointCreation(nextWaypont);
	}
	public void CreateJumpWaypointPair(Vector2 playerLeftGroundPosition, Vector2 playerTouchedGroundPosition)
	{
		if (CheckSurroundingsForWaypoints(WaypointType.jumpEnd, 2f)) return; //should stop multiple jump points for the same jump

		Waypoint previousWaypoint = CreateWaypoint(playerLeftGroundPosition);
		Waypoint nextWaypoint = CreateWaypoint(playerTouchedGroundPosition);

		previousWaypoint.InitilizeWaypoint(WaypointType.jumpStart, LastPlacedWaypoint, nextWaypoint);
		nextWaypoint.InitilizeWaypoint(WaypointType.jumpEnd, previousWaypoint, null);

		UpdateInfoOnWaypointCreation(previousWaypoint);
		UpdateInfoOnWaypointCreation(nextWaypoint);

		MakeDogFollowWaypoint?.Invoke(GetWaypointPath(GameManager.Instance.DogController.transform.position));
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
		ClosestWaypoint = newWaypoint;
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
	private bool CheckSurroundingsForWaypoints(WaypointType typeToCheckFor, float checkDistance)
	{
		if (typeToCheckFor == WaypointType.jumpEnd)
		{
			Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, checkDistance, waypointLayer);

			foreach (Collider2D hit in hits)
			{
				if (hit.GetComponent<Waypoint>().GetWaypointType() == WaypointType.jumpEnd)
					return true;
			}

			return false;
		}
		else
		{
			return Physics2D.OverlapCircle(transform.position, checkDistance - 1, waypointLayer);
		}
	}

	//basic waypoint blocking after jumping
	public void BlockBasicWaypointSpacing()
	{
		blockBasicWaypointPlacing = true;
	}
	public void AllowBasicWaypointSpacing()
	{
		blockBasicWaypointPlacing = true;
		StopCoroutine(FinishedJumpingTimer());
		StartCoroutine(FinishedJumpingTimer());
	}
	private IEnumerator FinishedJumpingTimer()
	{
		yield return new WaitForSeconds(0.1f);
		blockBasicWaypointPlacing = false;
	}

	private void OnDrawGizmos()
	{
		Gizmos.color = Color.red;
		Gizmos.DrawWireSphere(transform.position, waypointsSpacing - 1);
	}
}
