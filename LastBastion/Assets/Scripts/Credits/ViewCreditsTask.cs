using UnityEngine;

public class ViewCreditsTask : Task {


	/////////////////////////////////////////////
	/// Fields
	/////////////////////////////////////////////


	//the camera's starting position and rotation
	private Vector3 camStartPos = new Vector3(0.0f, 0.0f, 0.0f);
	private Quaternion camStartRot;


	//the camera's position and rotation while reading the credits
	private Vector3 camFinalPos = new Vector3(45.0f, 10.1f, 24.3f);
	private readonly Vector3 camFinalAngle = new Vector3(59.2f, 96.43f, 0.0f);
	private Quaternion camFinalRot;


	//movement speed
	private const float moveSpeed = 0.1f;
	private const float rotSpeed = 0.1f;


	//the rulebook and the credits to display
	private MeshRenderer creditsPageLeft;
	private MeshRenderer creditsPageRight;
	private const string CREDITS_LEFT_OBJ = "Mag_1_-_Open_Page_2";
	private const string CREDITS_RIGHT_OBJ = "Mag_1_-_Open_Page_3";
	private Material creditsPage1;
	private Material creditsPage2;
	private const string TITLE_PATH = "Title/";
	private const string CREDITS_1 = "Credits page 1";
	private const string CREDITS_2 = "Credits page 2";


	/////////////////////////////////////////////
	/// Functions
	/////////////////////////////////////////////


	//constructor
	public ViewCreditsTask(){
		camStartPos = Camera.main.transform.position;
		camStartRot = Camera.main.transform.rotation;
		camFinalRot = Quaternion.Euler(camFinalAngle);
		creditsPageLeft = GameObject.Find(CREDITS_LEFT_OBJ).GetComponent<MeshRenderer>();
		creditsPageRight = GameObject.Find(CREDITS_RIGHT_OBJ).GetComponent<MeshRenderer>();
		creditsPage1 = Resources.Load<Material>(TITLE_PATH + CREDITS_1);
		creditsPage2 = Resources.Load<Material>(TITLE_PATH + CREDITS_2);
	}


	protected override void Init(){
		creditsPageLeft.material = creditsPage1;
		creditsPageRight.material = creditsPage2;
	}
}
