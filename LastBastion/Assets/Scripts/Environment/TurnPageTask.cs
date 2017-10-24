using UnityEngine;

public class TurnPageTask : Task {


	/////////////////////////////////////////////
	/// Fields
	/////////////////////////////////////////////


	//the transform that this task rotates to turn pages
	private Transform bookBinding;
	private const string BOOK_BINDING_OBJ = "Book binding";


	//rotate from start angle to end angle on the Y-axis
	private const float START_Y_ROT = 0.0f;
	private const float END_Y_ROT = -180.0f;
	private float currentRot = START_Y_ROT;
	private Vector3 eulerRot = new Vector3(0.0f, 0.0f, 0.0f);


	//rotation speed, in degrees/second
	private float rotSpeed = 180.0f;


	/////////////////////////////////////////////
	/// Functions
	/////////////////////////////////////////////


	//constructor
	public TurnPageTask(){
		bookBinding = GameObject.Find(BOOK_BINDING_OBJ).transform;
	}


	/// <summary>
	/// Put the book binding at the starting point, page unturned.
	/// </summary>
	protected override void Init(){
		eulerRot.y = currentRot;
		bookBinding.localRotation = Quaternion.Euler(eulerRot);
	}


	/// <summary>
	/// Each frame, rotate the page toward being fully turned.
	/// </summary>
	public override void Tick(){
		eulerRot.y -= rotSpeed * Time.deltaTime;

		bookBinding.localRotation = Quaternion.Euler(eulerRot);

		if (eulerRot.y <= END_Y_ROT) SetStatus(TaskStatus.Success);
	}
}
