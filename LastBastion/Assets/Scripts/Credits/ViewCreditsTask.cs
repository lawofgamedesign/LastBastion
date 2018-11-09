using Title;
using UnityEngine;

public class ViewCreditsTask : Task {


	/////////////////////////////////////////////
	/// Fields
	/////////////////////////////////////////////


	//the camera's starting position
	private Vector3 camStartPos = new Vector3(0.0f, 0.0f, 0.0f);


	//the camera's position while reading the credits
	private Vector3 camFinalPos = new Vector3(45.0f, 10.1f, 24.3f);
	private readonly Vector3 camFinalAngle = new Vector3(59.2f, 96.43f, 0.0f);


	//movement
	private float maxMoveSpeed = 10.0f;
	private const float startMoveSpeed = 0.0f;
	private float currentMoveSpeed = 0.0f;
	private CommonAnimationCurves curveSource;
	private float totalDist = 0.0f;
	private Vector3 moveVector;
	private const float STOP_DIST = 0.1f; //camera stops moving when this close to the destination


	//the rulebook and the credits to display
	private readonly Transform rulebook;
	private const string RULEBOOK_OBJ = "Rulebook center"; //the rulebook's center is different from its pivot, look at this instead
	private MeshRenderer creditsPageLeft;
	private MeshRenderer creditsPageRight;
	private const string CREDITS_LEFT_OBJ = "Mag_1_-_Open_Page_2";
	private const string CREDITS_RIGHT_OBJ = "Mag_1_-_Open_Page_3";
	private Material creditsPage1;
	private Material creditsPage2;
	private const string TITLE_PATH = "Title/";
	private const string CREDITS_1 = "Credits page 1";
	private const string CREDITS_2 = "Credits page 2";
	
	
	//menu button text while looking at the credits, with associated variables
	private const string LOOK_CREDITS_BUTTON_TEXT = "Neat.";
	private const string TITLE_MENU_OBJ = "Menu";


	/////////////////////////////////////////////
	/// Functions
	/////////////////////////////////////////////


	//constructor
	public ViewCreditsTask(){
		rulebook = GameObject.Find(RULEBOOK_OBJ).transform;
		creditsPageLeft = GameObject.Find(CREDITS_LEFT_OBJ).GetComponent<MeshRenderer>();
		creditsPageRight = GameObject.Find(CREDITS_RIGHT_OBJ).GetComponent<MeshRenderer>();
	}


	protected override void Init(){
		camStartPos = Camera.main.transform.position;
		creditsPage1 = Resources.Load<Material>(TITLE_PATH + CREDITS_1);
		creditsPage2 = Resources.Load<Material>(TITLE_PATH + CREDITS_2);
		creditsPageLeft.material = creditsPage1;
		creditsPageRight.material = creditsPage2;
		totalDist = Vector3.Distance(camStartPos, camFinalPos);
		moveVector = (camFinalPos - camStartPos).normalized;
		curveSource = Services.ScriptableObjs.curveSource;
		GameObject.Find(TITLE_MENU_OBJ).GetComponent<TitleMenuBehavior>().SetButtonText(TitleMenuBehavior.TitleMenuButtons.Credits,
																						LOOK_CREDITS_BUTTON_TEXT);
		Services.Events.Fire(new ToggleCamRotEvent()); //let TitleManager know to stop the camera's rotation
		Services.Events.Register<CreditsButtonEvent>(DoneViewing);

		//if the camera is already close to the rulebook, reduce max speed so it can't jump past
		if (Vector3.Distance(Camera.main.transform.position, camFinalPos) < maxMoveSpeed)
			maxMoveSpeed = Vector3.Distance(Camera.main.transform.position, camFinalPos);
	}


	public override void Tick(){
		if (Vector3.Distance(Camera.main.transform.position, camFinalPos) > STOP_DIST){
			currentMoveSpeed = Mathf.Lerp(startMoveSpeed,
									      maxMoveSpeed,
									      curveSource.easeOutSudden.Evaluate(1.0f - Vector3.Distance(Camera.main.transform.position,
																						             camFinalPos)/totalDist));
			Camera.main.transform.Translate(moveVector * currentMoveSpeed * Time.deltaTime, Space.World);
			Camera.main.transform.LookAt(rulebook);
		}
	}


	private void DoneViewing(global::Event e){
		SetStatus(TaskStatus.Success);
	}


	protected override void OnSuccess(){
		Services.Tasks.AddTask(new DoneViewingCreditsTask(Camera.main.transform.position));
	}

	protected override void Cleanup(){
		Services.Events.Unregister<CreditsButtonEvent>(DoneViewing);
	}
}
