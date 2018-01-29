using UnityEngine;

public class MoveCharSheetTask : Task {


	/////////////////////////////////////////////
	/// Fields
	/////////////////////////////////////////////


	//the character sheet's positions on the screen
	private readonly Vector3 hiddenLoc = new Vector3(47.6f, 0.1f, 1.7f);
	private readonly Vector3 displayedLoc = new Vector3(20.0f, 24.0f, 2.3f);
	private readonly Vector3 hiddenRot = new Vector3(90.0f, -0.9f, 0.0f);
	private readonly Vector3 displayedRot = new Vector3(70.69f, 0.0f, 0.0f);


	private Vector3 startLoc;
	private Vector3 endLoc;
	private Quaternion endRot;


	//is the player picking up the character sheet, or putting it down?
	public enum Move { Pick_up, Put_down };
	private Move intendedMove;


	//the recttransform this task moves
	private RectTransform sheet;
	private const string SHEET_CANVAS = "Defender sheet canvas";


	//movement speeds and direction
	private const float MOVE_SPEED = 100.0f;
	private const float ROT_SPEED = 5.0f;
	private Vector3 moveDir;


	//get this close, then stop
	private const float TOLERANCE = 0.5f;


	/////////////////////////////////////////////
	/// Functions
	/////////////////////////////////////////////


	//constructor
	public MoveCharSheetTask(Move actionToTake){
		intendedMove = actionToTake;

		switch(intendedMove){
			case Move.Pick_up:
				startLoc = hiddenLoc;
				endLoc = displayedLoc;
				endRot = Quaternion.Euler(displayedRot);
				break;
			case Move.Put_down:
				startLoc = displayedLoc;
				endLoc = hiddenLoc;
				endRot = Quaternion.Euler(hiddenRot);
				break;
			default:
				Debug.Log("Illegal action to take: " + actionToTake);
				break;
		}
	}


	//generic constructor for type checks in CharacterSheetBehavior
	public MoveCharSheetTask(){
		SetStatus(TaskStatus.Aborted);
	}


	/// <summary>
	/// Initialize variables.
	/// </summary>
	protected override void Init (){
		sheet = GameObject.Find(SHEET_CANVAS).GetComponent<RectTransform>();

		Debug.Assert(sheet != null, "Can't find the character sheet. Was it deactivated?");


		moveDir = (endLoc - startLoc).normalized;
	}


	/// <summary>
	/// Each frame, move and rotate the character sheet until it reaches its final position.
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
		} else if ((sheet.position.x > hiddenLoc.x && //sanity check; don't let the character sheet fly off into space if it skips past the tolerance
					sheet.position.y < hiddenLoc.y) ||
				   (sheet.position.x < displayedLoc.x &&
					sheet.position.y > displayedLoc.y)){
			
			SetStatus(TaskStatus.Success);
		}
	}


	/// <summary>
	/// Don't let the sheet drift over time; make sure it's at the final location when the task is done.
	/// Then change the sheet's state.
	/// </summary>
	protected override void Cleanup (){
		sheet.position = endLoc;

		CharacterSheetBehavior sheetBehavior = sheet.GetComponent<CharacterSheetBehavior>();
		if (intendedMove == Move.Pick_up) sheetBehavior.ChangeCurrentStatus(CharacterSheetBehavior.SheetStatus.Displayed);
		else sheetBehavior.ChangeCurrentStatus(CharacterSheetBehavior.SheetStatus.Hidden);
	}
}
