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

	private int index = 0;


	///////////////////////////////////////////// 
	/// Fields 
	///////////////////////////////////////////// 


	//constructor 
	public MoveDefenderTask(Rigidbody defender, float speed, List<TwoDLoc> waypoints){ 
		this.defender = defender; 
		this.speed = speed; 
		this.waypoints = new List<TwoDLoc>();
		foreach (TwoDLoc point in waypoints) this.waypoints.Add(point);
	} 



	/// <summary> 
	/// Each update loop, move toward the next waypoint. 
	/// </summary> 
	public override void Tick(){
		Vector3 nextWaypointLoc = Services.Board.GetWorldLocation(waypoints[index].x, waypoints[index].z); 

		//if you're at the current waypoint, discard it 
		if (Vector3.Distance(defender.position, nextWaypointLoc) <= distTolerance) index++; 

		//so long as there's still a waypoint, move toward it. If there are no more waypoints, this task is complete 
		if (index <= waypoints.Count - 1){ 
			if (Vector3.Distance(defender.position, nextWaypointLoc) <= speed * Time.deltaTime) { //sanity check; don't overshoot 
				defender.MovePosition(nextWaypointLoc); 
			} else { 
				defender.MovePosition(defender.position + (nextWaypointLoc - defender.position).normalized * speed * Time.deltaTime); 
			} 
		} else { 
			SetStatus(TaskStatus.Success); 
		} 
	} 
} 
