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


	//needed to determine whether to lift the wall
	private readonly int column;
	private readonly int startZ;
	private readonly int endZ;


	/////////////////////////////////////////////
	/// Functions
	/////////////////////////////////////////////


	//constructor
	public MoveTask(Transform obj, int endX, int startZ, int endZ, float moveSpeed){
		this.obj = obj;
		this.column = endX;
		this.startZ = startZ;
		this.endZ = endZ;
		this.moveSpeed = moveSpeed;
		destination = Services.Board.GetWorldLocation(endX, endZ);
		direction = (destination - obj.position).normalized;
	}


	/// <summary>
	/// Lift the wall, if this move will take the attacker through it.
	/// </summary>
	protected override void Init (){
		if (startZ > Services.Board.WallZPos && endZ <= Services.Board.WallZPos){
			Debug.Log("Lifting wall " + column);
			Services.Board.LiftWall(column);
		}
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
