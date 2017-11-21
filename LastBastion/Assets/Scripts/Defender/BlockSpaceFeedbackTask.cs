using UnityEngine;

public class BlockSpaceFeedbackTask : Task {

	/////////////////////////////////////////////
	/// Fields
	/////////////////////////////////////////////


	//the marker that this task puts on the board to indicate that a column is blocked
	private readonly Transform blockMarker;


	//everything needed to drop the marker
	private float startHeight = 21.5f;
	private Vector3 dropSpeed = new Vector3(0.0f, 2.0f, 0.0f);
	private Vector3 startLoc = new Vector3(0.0f, 0.0f, 0.0f);
	private Vector3 endLoc = new Vector3(0.0f, 0.0f, 0.0f);
	private TwoDLoc gridSpace = new TwoDLoc(-1, -1);


	/////////////////////////////////////////////
	/// Functions
	/////////////////////////////////////////////


	//constructor
	public BlockSpaceFeedbackTask(int x, int z, string markerName){
		gridSpace.x = x;
		gridSpace.z = z;
		blockMarker = GameObject.Find(markerName).transform;
	}


	/// <summary>
	/// Put the marker in its starting position, and determine where it will end
	/// </summary>
	protected override void Init(){
		startLoc = Services.Board.GetWorldLocation(gridSpace.x, gridSpace.z);
		startLoc.y += startHeight;

		blockMarker.position = startLoc;

		//set up the end location
		endLoc = startLoc;
		endLoc.y = 0.0f;
	}


	/// <summary>
	/// Each frame, drop the marker.
	/// </summary>
	public override void Tick (){
		if (blockMarker.position.y - dropSpeed.y <= 0.0f){ //don't overshoot
			blockMarker.position = endLoc;
			SetStatus(TaskStatus.Success);
		}
		else blockMarker.position -= dropSpeed;
	}
}
