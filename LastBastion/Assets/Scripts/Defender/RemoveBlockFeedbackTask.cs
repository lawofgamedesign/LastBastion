using UnityEngine;

public class RemoveBlockFeedbackTask : Task {


	/////////////////////////////////////////////
	/// Fields
	/////////////////////////////////////////////


	//the marker that will move
	private readonly Transform blockMarker;


	//how far and fast the marker moves
	private Vector3 pickUpSpeed = new Vector3(0.0f, 5.0f, 0.0f);
	private float offScreenHeight = 90.0f; //high enough to get even the shadow out of view


	//the marker's position at the very beginning of the game; if the marker is at this location, it's not on the board and can just be dropped
	private Vector3 startOfGameLoc = new Vector3(-100.0f, 0.0f, 0.0f);
	private const float TOLERANCE = 5.0f;


	/////////////////////////////////////////////
	/// Functions
	/////////////////////////////////////////////


	//constructor
	public RemoveBlockFeedbackTask(string marker) {
		blockMarker = GameObject.Find(marker).transform;
	}


	/// <summary>
	/// If the marker is off the board, it doesn't need to be picked up. Abort this task so that it can be dropped.
	/// </summary>
	protected override void Init (){
		if (Vector3.Distance(blockMarker.position, startOfGameLoc) <= TOLERANCE) SetStatus(TaskStatus.Success);
	}


	/// <summary>
	/// Each frame, pick the block up
	/// </summary>
	public override void Tick (){
		blockMarker.position += pickUpSpeed;

		if (blockMarker.position.y >= offScreenHeight) SetStatus(TaskStatus.Success);
	}
}
