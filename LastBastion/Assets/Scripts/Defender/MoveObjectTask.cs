/// <summary>
/// Use this task to move objects on the board, as opposed to moving defenders.
/// </summary>

using UnityEngine;

public class MoveObjectTask : Task {

	/////////////////////////////////////////////
	/// Fields
	/////////////////////////////////////////////


	//the object to move
	private readonly Transform obj;


	//where it's moving from and to, in grid coordinates
	private readonly TwoDLoc start;
	private readonly TwoDLoc end;


	//movement locations as Vector3s, as well as direction of movement
	private Vector3 startVec;
	private Vector3 endVec;
	private Vector3 direction;


	//how quickly the object moves
	private float speed = 5.0f;


	//the object stops when it gets this close
	private float tolerance = 0.5f;


	/////////////////////////////////////////////
	/// Functions
	/////////////////////////////////////////////


	//constructor
	public MoveObjectTask(Transform obj, TwoDLoc start, TwoDLoc end){
		this.obj = obj;
		this.start = start;
		this.end = end;
	}


	/// <summary>
	/// Turn the start and end locations into Vector3s that can be used to move. 
	/// </summary>
	protected override void Init (){
		startVec = Services.Board.GetWorldLocation(start.x, start.z);
		endVec = Services.Board.GetWorldLocation(end.x, end.z);

		Debug.Assert(startVec != Services.Board.illegalLoc, "Illegal starting location.");
		Debug.Assert(endVec != Services.Board.illegalLoc, "Illegal ending location.");

		direction = (endVec - startVec).normalized;
	}


	/// <summary>
	/// Move toward the end location until the object arrives.
	/// </summary>
	public override void Tick (){
		if (Vector3.Distance(obj.position, endVec) <= speed * Time.deltaTime) obj.position = endVec; //sanity check; don't overshoot
		else obj.Translate(direction * speed * Time.deltaTime, Space.World);

		if (Vector3.Distance(obj.position, endVec) <= tolerance) SetStatus(TaskStatus.Success);
	}
}
