using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;

public class Waypoint : MonoBehaviour
{
	public List<WaypointLink> waypointLinks = new();

	[SerializeField] private WaypointType waypointType;

	public enum WaypointType
	{
		start, basic, end, jumpStart, jumpEnd
	}

	public void InitilizeWaypoint(WaypointType waypointType)
	{
		this.waypointType = waypointType;
	}
	public void InitilizeWaypoint(WaypointType waypointType, List<Waypoint> previousWaypoints)
	{
		this.waypointType = waypointType;

		foreach (Waypoint previousWaypoint in previousWaypoints)
		{
			if (previousWaypoint.GetWaypointType() == WaypointType.jumpStart && waypointType == WaypointType.jumpEnd)
			{
				AddNewWaypointLink(new WaypointLink(previousWaypoint, WaypointLink.LinkType.jump));
				previousWaypoint.AddNewWaypointLink(new WaypointLink(this, WaypointLink.LinkType.jump));
			}
			else
			{
				AddNewWaypointLink(new WaypointLink(previousWaypoint, WaypointLink.LinkType.basic));
				previousWaypoint.AddNewWaypointLink(new WaypointLink(this, WaypointLink.LinkType.basic));
			}
		}
	}
	public void InitilizeWaypoint(WaypointType waypointType, Waypoint previousWaypoint)
	{
		this.waypointType = waypointType;

		if (previousWaypoint.GetWaypointType() == WaypointType.jumpStart && waypointType == WaypointType.jumpEnd)
		{
			AddNewWaypointLink(new WaypointLink(previousWaypoint, WaypointLink.LinkType.jump));
			previousWaypoint.AddNewWaypointLink(new WaypointLink(this, WaypointLink.LinkType.jump));
		}
		else
		{
			AddNewWaypointLink(new WaypointLink(previousWaypoint, WaypointLink.LinkType.basic));
			previousWaypoint.AddNewWaypointLink(new WaypointLink(this, WaypointLink.LinkType.basic));
		}
	}

	private void AddNewWaypointLink(WaypointLink waypointLink)
	{
		waypointLinks.Add(waypointLink);
	}

	public WaypointType GetWaypointType()
	{
		return waypointType;
	}
	public bool ReachedWaypoint(Transform otherTransform)
	{
		float distance = Vector2.Distance(otherTransform.position, transform.position);

		if (distance < 1.5f)
			return true;
		else return false;
	}

	private void OnDrawGizmos()
	{
		Gizmos.color = Color.blue;
		Gizmos.DrawWireSphere(transform.position, 1f);

		foreach (var link in waypointLinks)
		{
			if (link.waypoint.GetWaypointType() == WaypointType.jumpStart || link.waypoint.GetWaypointType() == WaypointType.jumpEnd)
				Gizmos.color = Color.yellow;
			else
				Gizmos.color = Color.blue;

			Gizmos.DrawLine(transform.position, link.waypoint.transform.position);
		}
	}
}

[Serializable]
public class WaypointLink
{
	public Waypoint waypoint;
	public LinkType linkType;

	public enum LinkType
	{
		basic, jump
	}

	public WaypointLink(Waypoint waypoint, LinkType linkType)
	{
		this.waypoint = waypoint;
		this.linkType = linkType;
	}
}
