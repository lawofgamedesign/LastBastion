using UnityEngine;

public class BlockFeedbackTask : Task {


	/////////////////////////////////////////////
	/// Fields
	/////////////////////////////////////////////


	//the marker that this task puts on the board to indicate that a column is blocked
	private readonly Transform blockMarker;
	private const string BLOCK_MARKER_OBJ = "Line held marker";


	//the column to be marked
	private readonly int column;


	//everything needed to drop the marker
	private float startHeight = 21.5f;
	private Vector3 dropSpeed = new Vector3(0.0f, 2.0f, 0.0f);
	private Vector3 startLoc = new Vector3(0.0f, 0.0f, 0.0f);
	private Vector3 endLoc = new Vector3(0.0f, 0.0f, 0.0f);


	private int clickedRow = 0;
	private const string BOARD_TAG = "Board";


	/////////////////////////////////////////////
	/// Functions
	////////////////////////////////////////////


	//constructor
	public BlockFeedbackTask(int column){
		this.column = column;
		blockMarker = GameObject.Find(BLOCK_MARKER_OBJ).transform;
	}



	/// <summary>
	/// Put the marker in its starting position, and establish the ending position for the marker--beneath the starting location at
	/// y == 0.0f
	/// </summary>
	protected override void Init (){
		int row = Services.Board.GetFirstEmptyInColumn(column);

		if (row == Services.Board.NOT_FOUND) SetStatus(TaskStatus.Aborted); //if there's no empty space in the chosen row, do nothing
		else startLoc = Services.Board.GetWorldLocation(column, row);

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
