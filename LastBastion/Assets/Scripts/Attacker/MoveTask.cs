using UnityEngine;

public class MoveTask : Task {


	/////////////////////////////////////////////
	/// Fields
	/////////////////////////////////////////////


	//the object to move
	private readonly Transform obj;


	//movement speed
	private float moveSpeed;


	//direction of movement
	private readonly Vector3 direction;


	//destination, in world space
	private readonly Vector3 destination;


	//how close does this object get to its destination to end the task?
	private float tolerance = 0.1f;


	/////////////////////////////////////////////
	/// Functions
	/////////////////////////////////////////////


	//constructor
	public MoveTask(Transform obj, int endX, int endZ, float moveSpeed){
		this.obj = obj;
		this.moveSpeed = moveSpeed;
		destination = Services.Board.GetWorldLocation(endX, endZ);
		direction = (destination - obj.position).normalized;
	}


	/// <summary>
	/// Move directly toward the end coordinates. Stop when within [tolerance] of them.
	/// </summary>
	public override void Tick(){
		if (Vector3.Distance(obj.transform.position, destination) < moveSpeed * Time.deltaTime){ //prevent overshooting the destination
			obj.transform.position = destination;
		} else {
			obj.Translate(direction * moveSpeed * Time.deltaTime);
		}

		if (Vector3.Distance(obj.transform.position, destination) < tolerance){
			SetStatus(TaskStatus.Success);
		}
	}
}
