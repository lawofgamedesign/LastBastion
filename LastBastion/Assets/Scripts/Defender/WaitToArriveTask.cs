using UnityEngine;

public class WaitToArriveTask : Task {

	/////////////////////////////////////////////
	/// Fields
	/////////////////////////////////////////////


	//the object that's moving
	private readonly Transform obj;


	//where the object is going
	private readonly TwoDLoc end;
	private Vector3 endVec;


	//the object is considered to have arrived when it gets this close to the destination
	private float tolerance = 0.5f;


	/////////////////////////////////////////////
	/// Functions
	/////////////////////////////////////////////


	//constructor
	public WaitToArriveTask(Transform obj, TwoDLoc end){
		this.obj = obj;
		this.end = end;
	}


	/// <summary>
	/// Turn the end location into a Vector3 that can be compared against.
	/// </summary>
	protected override void Init (){
		endVec = Services.Board.GetWorldLocation(end.x, end.z);

		Debug.Assert(endVec != Services.Board.illegalLoc, "Illegal end location while waiting to arrive.");
	}


	/// <summary>
	/// Keep track of the object's location. Declare success when the object is close to its destination.
	/// </summary>
	public override void Tick (){
		if (Vector3.Distance(obj.position, endVec) <= tolerance) SetStatus(TaskStatus.Success);
	}
}
