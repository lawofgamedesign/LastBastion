using System.Collections.Generic;
using UnityEngine;

public class MoveDefenderTask : Task {


	/////////////////////////////////////////////
	/// Fields
	/////////////////////////////////////////////


	private readonly Rigidbody defender;
	private readonly float speed;
	private readonly List<TwoDLoc> waypoints;


	private float distTolerance = 0.05f; //the defender must be at least this close to each waypoint before moving on to the next


	/////////////////////////////////////////////
	/// Fields
	/////////////////////////////////////////////


	//constructor
	public MoveDefenderTask(Rigidbody defender, float speed, List<TwoDLoc> waypoints){
		this.defender = defender;
		this.speed = speed;
		this.waypoints = new List<TwoDLoc>();
		foreach (TwoDLoc point in waypoints){
//			Debug.Log("Adding " + point.x + ", " + point.z);
			this.waypoints.Add(point);
		}
		for (int i = 0; i < this.waypoints.Count; i++){
			Debug.Log ("waypoints[" + i + "].x == " + this.waypoints[i].x + ", z == " + this.waypoints[i].z);
		}
		Debug.Log("waypoints.Count == " + waypoints.Count);
	}
		

	/// <summary>
	/// Each update loop, move toward the next waypoint.
	/// </summary>
	public override void Tick(){
		Debug.Log("waypoints[0] is currently " + waypoints[0].x + ", " + waypoints[0].z);
		Vector3 nextWaypointLoc = Services.Board.GetWorldLocation(waypoints[0].x, waypoints[0].z);

		//if you're at the current waypoint, discard it
		if (Vector3.Distance(defender.position, nextWaypointLoc) <= distTolerance){ 
//			Debug.Log("Removing waypoints[0].x == " + waypoints[0].x + ", z == " + waypoints[0].z);
			waypoints.RemoveAt(0);
//			Debug.Log("waypoints[0] is now x == " + waypoints[0].x + ", z == " + waypoints[0].z);
		}

		//so long as there's still a waypoint, move toward it. If there are no more waypoints, this task is complete
		if (waypoints.Count >= 1){
			Debug.Log("Moving toward x == " + waypoints[0].x + " z == " + waypoints[0].z);
			if (Vector3.Distance(defender.position, nextWaypointLoc) <= speed * Time.deltaTime){ //sanity check; don't overshoot
				defender.MovePosition(nextWaypointLoc);
			} else {
				defender.MovePosition(defender.position + (nextWaypointLoc - defender.position).normalized * speed * Time.deltaTime);
			}
		} else {
			SetStatus(TaskStatus.Success);
		}
	}
}
