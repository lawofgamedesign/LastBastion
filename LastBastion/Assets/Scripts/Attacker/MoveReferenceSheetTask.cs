using UnityEngine;

public class MoveReferenceSheetTask : Task {


	/////////////////////////////////////////////
	/// Fields
	/////////////////////////////////////////////


	//the reference sheet's positions on the screen
	private readonly Vector3 hiddenLoc = new Vector3(47.98f, -1.5f, 23.7f);
	private readonly Vector3 hiddenRot = new Vector3(90.0f, 1.4f, 0.0f);
	private readonly Vector3 displayLoc = new Vector3(19.7f, 11.38f, 4.2f);
	private readonly Vector3 displayRot = new Vector3(79.5f, 0.0f, 0.0f);


	private Vector3 startLoc;
	private Vector3 endLoc;
	private Quaternion endRot;


	//is the player picking up the character sheet, or putting it down?
	public enum Move { Pick_up, Put_down };


	//the recttransform this task moves
	private RectTransform sheet;
	private const string SHEET_CANVAS = "Warlord reference canvas";


	//movement speeds and direction
	private const float MOVE_SPEED = 100.0f;
	private const float ROT_SPEED = 5.0f;
	private Vector3 moveDir;


	//get this close, then stop
	private const float TOLERANCE = 0.5f;


	/////////////////////////////////////////////
	/// Functions
	/////////////////////////////////////////////


	public MoveReferenceSheetTask(Move actionToTake){
		switch(actionToTake){
			case Move.Pick_up:
				startLoc = hiddenLoc;
				endLoc = displayLoc;
				endRot = Quaternion.Euler(displayRot);
				break;
			case Move.Put_down:
				startLoc = displayLoc;
				endLoc = hiddenLoc;
				endRot = Quaternion.Euler(hiddenRot);
				break;
			default:
				Debug.Log("Illegal action to take: " + actionToTake);
				break;
		}
	}


	/// <summary>
	/// Initialize variables.
	/// </summary>
	protected override void Init (){
		sheet = GameObject.Find(SHEET_CANVAS).GetComponent<RectTransform>();

		Debug.Assert(sheet != null, "Can't find the reference sheet. Was it deactivated?");

		moveDir = (endLoc - startLoc).normalized;
	}


	/// <summary>
	/// Each frame, move and rotate the reference sheet until it reaches its final position.
	/// </summary>
	public override void Tick(){
		if (Vector3.Distance(sheet.position, endLoc) <= TOLERANCE){ //don't overshoot
			sheet.position = endLoc;
		} else {
			sheet.Translate(moveDir * MOVE_SPEED * Time.deltaTime, Space.World);
		}

		if (Quaternion.Angle(sheet.rotation, endRot) <= TOLERANCE){ //don't overshoot on the rotation either
			sheet.rotation = endRot;
		} else {
			sheet.rotation = Quaternion.RotateTowards(sheet.rotation, endRot, ROT_SPEED);
		}

		if (Vector3.Distance(sheet.position, endLoc) <= TOLERANCE &&
			Quaternion.Angle(sheet.rotation, endRot) <= TOLERANCE){
			SetStatus(TaskStatus.Success);
		} else if ((sheet.position.x > hiddenLoc.x && //sanity check; don't let the sheet fly off into space if it skips past the tolerance
			sheet.position.y < hiddenLoc.y) ||
			(sheet.position.x < displayLoc.x &&
				sheet.position.y > displayLoc.y)){

			SetStatus(TaskStatus.Success);
		}
	}


	/// <summary>
	/// Don't let the sheet drift over time; make sure it's at the final location when the task is done.
	/// </summary>
	protected override void Cleanup (){
		sheet.position = endLoc;
	}
}
