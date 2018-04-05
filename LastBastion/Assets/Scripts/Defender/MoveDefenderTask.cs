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
		//also check whether it's necessary to lift the wall to pass through
		if (Vector3.Distance(defender.position, nextWaypointLoc) <= distTolerance){
			if (CheckForWallPass()) Services.Board.LiftWall(waypoints[index].x);
			index++; 
		}

		//so long as there's still a waypoint, move toward it. If there are no more waypoints, this task is complete 
		if (index <= waypoints.Count - 1){ 
			if (Vector3.Distance(defender.position, nextWaypointLoc) <= speed * Time.deltaTime) { //sanity check; don't overshoot 
				defender.MovePosition(nextWaypointLoc);
			} else { 
				defender.MovePosition(defender.position + (nextWaypointLoc - defender.position).normalized * speed * Time.deltaTime); 
			} 
		} else {
			Services.Events.Fire(new MoveEvent(defender.transform, new TwoDLoc(waypoints[waypoints.Count - 1].x,
																			   waypoints[waypoints.Count - 1].z)));
			SetStatus(TaskStatus.Success); 
		} 
	}


	/// <summary>
	/// Will the defender's next move take them through the wall?
	/// </summary>
	/// <returns><c>true</c> if the defender will move from south of the wall to north of it, or vice versa; <c>false</c> otherwise.</returns>
	private bool CheckForWallPass(){
		if (index <= waypoints.Count - 2){ //only see if the next move will pass through the wall if there will be a next move
			if ((waypoints[index].z <= Services.Board.WallZPos &&
				 waypoints[index + 1].z > Services.Board.WallZPos) ||
				(waypoints[index].z > Services.Board.WallZPos &&
				 waypoints[index + 1].z <= Services.Board.WallZPos)){

				return true;
			}
		}

		return false;
	}
} 
