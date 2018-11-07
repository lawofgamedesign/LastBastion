using UnityEngine;

public class DefenderMoveTask : Task {


	/////////////////////////////////////////////
	/// Fields
	/////////////////////////////////////////////


	//the defender that the player has picked up and will move
	private readonly DefenderSandbox defender;
	private readonly Transform mini;


	//used to pick up the defender's model
	private const float PICK_UP_HEIGHT = 4.0f;
	private const string MODEL_OBJ = "Model";


	//moving the defender
	private Vector3 lastMousePos;
	private Vector3 currentMousePos;
	private float moveMax = 0.1f; //maximum movement per frame, to translate screen coordinate movement into reasonable-speed world space movement


	//operate the unmoving model that stays behind as the defender moves


	/////////////////////////////////////////////
	/// Functions
	/////////////////////////////////////////////


	//constructor
	public DefenderMoveTask(DefenderSandbox defender){
		this.defender = defender;
		mini = this.defender.transform.Find(MODEL_OBJ);
	}


	//pick up the defender's model
	protected override void Init (){
		mini.localPosition += new Vector3(0.0f, PICK_UP_HEIGHT, 0.0f);
		defender.ToggleUnmovingModel();
		Services.Cursor.ToggleCursor(CursorManager.OnOrOff.Off);
		currentMousePos = Input.mousePosition;
		lastMousePos = currentMousePos;
	}


	//trace out the player's intended move. When the player lets the model go, put it down and resolve the move
	public override void Tick (){
		if (!Input.GetMouseButton(0)){
			SetStatus(TaskStatus.Success);
			return;
		}


		mini.localPosition += DragDefender();

		TwoDLoc miniLoc = Services.Board.GetGridLocation(mini.position.x, mini.position.z);

		if (miniLoc != null) defender.TryPlanMove(miniLoc);
	}


	private Vector3 DragDefender(){
		lastMousePos = currentMousePos;
		currentMousePos = Input.mousePosition;

		float x = currentMousePos.x - lastMousePos.x;
		float z = currentMousePos.y - lastMousePos.y;

		return new Vector3(x, 0.0f, z) * moveMax;
	}


	protected override void OnSuccess (){
		ChooseMoveAction();
		mini.localPosition = Vector3.zero;
		defender.ToggleUnmovingModel();
		Services.Cursor.ToggleCursor(CursorManager.OnOrOff.On);
	}


	/// <summary>
	/// Choose an action--undo the planned move or execute the planned move--based on the mini's final location.
	/// </summary>
	private void ChooseMoveAction(){
		TwoDLoc miniFinalPos = Services.Board.GetGridLocation(mini.position.x, mini.position.z);

		//check if the player brought the mini back to the defender's start location
		if (miniFinalPos != null){ //separate null check to avoid null references; if this is null, the mini is off the board
			if (miniFinalPos.x == defender.ReportGridLoc().x && miniFinalPos.z == defender.ReportGridLoc().z){ //the mini is on the board; is it at the defender's location?
				defender.UndoMove();
			}
		}
		if (defender.CheckIfMovePlanned()) defender.Move();
	}
}
