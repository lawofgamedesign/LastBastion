using Title;
using UnityEngine;

public class DoneViewingCreditsTask : Task {

	/////////////////////////////////////////////
	/// Fields
	/////////////////////////////////////////////


	//the camera's positions, while reading the credits and the final position it should return to
	private readonly Vector3 camReadPos;
	private readonly Vector3 camReturnPos = new Vector3(-3.66f, 10.1f, -4.0f);
	private readonly Quaternion camReturnRot;
	private Vector3 camReturnAngles = new Vector3(35.0f, 33.9f, 0.0f);
	
	
	//movement
	private const float maxMoveSpeed = 10.0f;
	private const float startMoveSpeed = 0.0f;
	private float currentMoveSpeed = 0.0f;
	private CommonAnimationCurves curveSource;
	private float totalDist = 0.0f;
	private Vector3 moveVector;
	private const float stopDist = 0.1f; //camera stops moving when this close to the destination
	
	
	//the center of the board, which the rotating camera looks at
	private readonly Transform boardCenter;
	private const string BOARD_CENTER_OBJ = "Board center";
	
	
	//the center of the rulebook
	private readonly Transform rulebook;
	private const string RULEBOOK_OBJ = "Rulebook center"; //the rulebook's center is different from its pivot, look at this instead
	
	
	//menu button text while looking at the credits, with associated variables
	private const string DONE_CREDITS_BUTTON_TEXT = "Who made this?";
	private const string TITLE_MENU_OBJ = "Menu";
	


	/////////////////////////////////////////////
	/// Functions
	/////////////////////////////////////////////


	//constructor
	public DoneViewingCreditsTask(Vector3 camReadPos){
		this.camReadPos = camReadPos;
		camReturnRot = Quaternion.Euler(camReturnAngles);
		boardCenter = GameObject.Find(BOARD_CENTER_OBJ).transform;
		rulebook = GameObject.Find(RULEBOOK_OBJ).transform;
	}


	protected override void Init(){
		totalDist = Vector3.Distance(rulebook.position, camReturnPos);
		moveVector = (camReturnPos - camReadPos).normalized;
		curveSource = Services.ScriptableObjs.curveSource;
		GameObject.Find(TITLE_MENU_OBJ).GetComponent<TitleMenuBehavior>().SetButtonText(TitleMenuBehavior.TitleMenuButtons.Credits,
																						DONE_CREDITS_BUTTON_TEXT);
		Services.Events.Register<CreditsButtonEvent>(ListenForCredits);
	}


	public override void Tick(){
		if (Vector3.Distance(Camera.main.transform.position, camReturnPos) > stopDist){
			currentMoveSpeed = Mathf.Lerp(startMoveSpeed,
										  maxMoveSpeed,
										  curveSource.easeOutSudden.Evaluate(1.0f - Vector3.Distance(Camera.main.transform.position,
																									 camReturnPos)/totalDist));
			Camera.main.transform.Translate(moveVector * currentMoveSpeed * Time.deltaTime, Space.World);
			Camera.main.transform.LookAt(rulebook);
		}
		else {
			SetStatus(TaskStatus.Success);
		}
	}


	/// <summary>
	/// If the player presses the credits button again while this task is running, let go of control so that the
	/// new ViewCreditsTask started by TitleMenuBehavior can move the camera.
	/// </summary>
	/// <param name="e">A CreditsButtonEvent fired by TitleMenuBehavior</param>
	private void ListenForCredits(global::Event e){
		Debug.Assert(e.GetType() == typeof(CreditsButtonEvent), "Non-CreditsButtonEvent in ListenForCredits");
		SetStatus(TaskStatus.Aborted);
	}


	/// <summary>
	/// When the camera gets back to where it belongs, notify TitleManager to start the camera rotating again.
	/// </summary>
	protected override void OnSuccess(){
		Camera.main.transform.LookAt(boardCenter);
		Services.Events.Fire(new ToggleCamRotEvent());
	}


	/// <summary>
	/// Whether the camera gets all the way back or the player aborts this task to see the credits again, unregister for
	/// events.
	/// </summary>
	protected override void Cleanup(){
		Services.Events.Unregister<CreditsButtonEvent>(ListenForCredits);
	}
}
