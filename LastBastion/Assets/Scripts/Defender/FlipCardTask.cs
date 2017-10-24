using UnityEngine;
using UnityEngine.UI;

public class FlipCardTask : Task {


	/////////////////////////////////////////////
	/// Fields
	/////////////////////////////////////////////


	//the card that's being flipped
	private RectTransform cardTransform;
	private GameObject cardBack;
	private const string CARD_BACK_OBJ = "Card back";


	//angles
	private const float FACE_DOWN_Y_ROT = 180.0f;
	private const float FACE_UP_Y_ROT = 0.0f;
	private const float HALFWAY_Y_ROT = 90.0f;
	private float currentRot;
	private float startRot;
	private const float ROT_SPEED = 180.0f;
	private float currentSpeed;


	//are we flipping face-up, or face-down?
	public enum UpOrDown { Up, Down };
	private UpOrDown flipDir;


	//get this close to the target rotation, then stop
	private const float TOLERANCE = 0.5f;



	/////////////////////////////////////////////
	/// Functions
	/////////////////////////////////////////////


	//constructor
	public FlipCardTask(RectTransform cardTransform, UpOrDown flipDir){
		this.cardTransform = cardTransform;
		this.flipDir = flipDir;
	}


	/// <summary>
	/// Set starting and ending Y-rotations (in degrees), as well as rotation speed.
	/// </summary>
	protected override void Init(){
		cardBack = cardTransform.Find(CARD_BACK_OBJ).gameObject;

		switch(flipDir){
			case UpOrDown.Up:
				startRot = FACE_DOWN_Y_ROT;
				currentSpeed = -ROT_SPEED;
				cardBack.SetActive(true);
				break;
			case UpOrDown.Down:
				startRot = FACE_UP_Y_ROT;
				currentSpeed = ROT_SPEED;
				cardBack.SetActive(false);
				break;
			default:
				Debug.Log("Illegal flip direction: " + flipDir.ToString());
				break;
		}

		currentRot = startRot;

		cardTransform.localRotation = Quaternion.Euler(0.0f, currentRot, 0.0f);
	}


	/// <summary>
	/// Each frame, do the following in this order:
	/// 
	/// 1. Find the next y-axis angle.
	/// 2. Check whether that angle crosses the threshold for displaying or hiding the card back; if so, do that.
	/// 3. Update the y-axis angle.
	/// 4. Check to see whether the card is fully rotated.
	/// </summary>
	public override void Tick(){
		currentRot += currentSpeed * Time.deltaTime;

		CheckCardBackStatus(currentRot);

		cardTransform.localRotation = Quaternion.Euler(0.0f, currentRot, 0.0f);

		if (CheckFinishedRotating()) SetStatus(TaskStatus.Success);
	}


	/// <summary>
	/// Deactivate the card back if the card is going across 90 degrees on the y-axis toward zero; activate it if it's crossing toward 180.
	/// </summary>
	/// <param name="newY">The y-axis angle the card is moving toward.</param>
	private void CheckCardBackStatus(float newY){
		if ((cardTransform.localRotation.eulerAngles.y > HALFWAY_Y_ROT && newY <= HALFWAY_Y_ROT) ||
			(cardTransform.localRotation.eulerAngles.y < HALFWAY_Y_ROT && newY >= HALFWAY_Y_ROT)){
			cardBack.SetActive(!cardBack.activeInHierarchy);
		}
	}


	/// <summary>
	/// A card is done rotating when it's within TOLERANCE of its final y-axis angle.
	/// </summary>
	/// <returns><c>true</c> if the y-axis angle is within TOLERANCE, <c>false</c> otherwise.</returns>
	private bool CheckFinishedRotating(){
		switch(flipDir){
			case UpOrDown.Up:
				if (currentRot <= TOLERANCE) return true;
				return false;
			case UpOrDown.Down:
				if (currentRot >= FACE_DOWN_Y_ROT - TOLERANCE) return true;
				return false;
			default:
				Debug.Log("Illegal flip direction: " + flipDir.ToString());
				return false;
		}
	}
}
